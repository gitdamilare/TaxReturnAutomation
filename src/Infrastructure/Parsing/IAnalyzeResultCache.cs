using Azure.AI.DocumentIntelligence;

namespace TaxReturnAutomation.Infrastructure.Parsing;
public interface IAnalyzeResultCache
{
    Task<AnalyzeResult?> GetAsync(byte[] fileData, string modelId);
    Task SetAsync(byte[] fileData, string modelId, AnalyzeResult result, TimeSpan? timeSpan = null);
}
