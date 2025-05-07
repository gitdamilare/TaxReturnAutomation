using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions.Triggers
{
    public class FileRouterFunction
    {
        private readonly ILogger<FileRouterFunction> _logger;

        public FileRouterFunction(ILogger<FileRouterFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(FileRouterFunction))]
        public async Task Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }
    }
}
