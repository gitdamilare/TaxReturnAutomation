using System.Collections.Concurrent;
using System.Security.Cryptography;
using Azure.AI.DocumentIntelligence;
using Newtonsoft.Json;
using TaxReturnAutomation.Infrastructure.Parsing;

namespace Infrastructure.IntegrationTests.Utilities;
internal class JsonBasedAnalyzeResultCache : IAnalyzeResultCache
{
    private readonly string _cacheDirectory;
    private readonly TimeSpan _defaultTTL = TimeSpan.FromDays(30);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore
    };

    public JsonBasedAnalyzeResultCache()
    {
        _cacheDirectory = Path.Combine(AppContext.BaseDirectory, "cache", "analyresult");
        _ = Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task<AnalyzeResult?> GetAsync(byte[] fileData, string modelId)
    {
        var cacheFile = GetCacheFilePath(fileData, modelId);
        var semahore = _locks.GetOrAdd(cacheFile, _ => new SemaphoreSlim(1, 1));

        await semahore.WaitAsync();
        try
        {
            if (!File.Exists(cacheFile)) return default;

            var lastWriteTime = File.GetLastWriteTime(cacheFile);
            if (lastWriteTime.Add(_defaultTTL) > DateTime.UtcNow)
            {
                var json = await File.ReadAllTextAsync(cacheFile);
                var result = JsonConvert.DeserializeObject<AnalyzeResult>(json, _jsonSerializerSettings);
                return result;
            }
            else
            {
                File.Delete(cacheFile);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            semahore.Release();
            _locks.TryRemove(cacheFile, out _);
        }

        return default;
    }

    public async Task SetAsync(byte[] fileData, string modelId, AnalyzeResult result, TimeSpan? timespan)
    {
        var cacheFile = GetCacheFilePath(fileData, modelId);
        var semahore = _locks.GetOrAdd(cacheFile, _ => new SemaphoreSlim(1, 1));

        await semahore.WaitAsync();
        try
        {
            var json = JsonConvert.SerializeObject(result);

            if(File.Exists(cacheFile))
            {
                var lastWriteTime = File.GetLastWriteTime(cacheFile);
                if (lastWriteTime.Add(_defaultTTL) > DateTime.UtcNow)
                {
                    return;
                }
            }

            var tempFile = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(tempFile, json);
                File.Move(tempFile, cacheFile, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            semahore.Release();
            _locks.TryRemove(cacheFile, out _);
        }
    }

    private string GetCacheFilePath(byte[] fileData, string modelId)
    {
        var cacheKey = GenerateCacheKey(fileData, modelId);
        return Path.Combine(_cacheDirectory, cacheKey + ".json");
    }

    private static string GenerateCacheKey(byte[] fileData, string modelId)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(fileData);
        var key = Convert.ToHexStringLower(hash);

        return $"{modelId}-{key}";
    }
}
