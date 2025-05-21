using System.Security.Cryptography;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Caching.Memory;

namespace TaxReturnAutomation.Infrastructure.Parsing;
public class MemoryAnalyzeResultCache : IAnalyzeResultCache
{
    private readonly TimeSpan _defaultTTL = TimeSpan.FromDays(30);
    private readonly IMemoryCache _memoryCache;

    public MemoryAnalyzeResultCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<AnalyzeResult?> GetAsync(byte[] fileData, string modelId)
    {
        var key = GenerateCacheKey(fileData, modelId);
        return _memoryCache.TryGetValue(key, out AnalyzeResult? result) ?
            Task.FromResult(result) :
            Task.FromResult<AnalyzeResult?>(null);
    }

    public Task SetAsync(byte[] fileData, string modelId, AnalyzeResult result, TimeSpan? timeSpan)
    {
        var key = GenerateCacheKey(fileData, modelId);
        _memoryCache.Set(key, result, timeSpan ?? _defaultTTL);
        return Task.CompletedTask;
    }

    private static string GenerateCacheKey(byte[] fileData, string modelId)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(fileData);
        var key = Convert.ToHexStringLower(hash);

        return $"{modelId}-{key}";
    }
}
