using AocSlackBot.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AocSlackBot.Clients
{
    public class AzureBlobClient
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureBlobClient> _logger;
        private readonly string _containerName;
        private readonly string _leaderboardBlobFileName;

        public AzureBlobClient(ILogger<AzureBlobClient> logger, IConfiguration configuration)
        {
            _logger = logger;
            _containerName = configuration["BlobContainerName"];
            _leaderboardBlobFileName = $"leaderboard{DateTime.Now.Year}.json";
            _blobServiceClient = new BlobServiceClient(configuration["StorageAccountConnectionString"]);
        }

        public async Task<Leaderboard> DownloadLeaderboardBlobAsync(string localFilePath)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(_leaderboardBlobFileName);
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Blob '{LeaderboardFileName}' does not exist in container '{ContainerName}'.", _leaderboardBlobFileName, _containerName);
                return null;
            }
            await blobClient.DownloadToAsync(localFilePath);
            _logger.LogInformation("Downloaded Leaderboard successfully to {LocalFilePath}.", localFilePath);
            var jsonContent = await File.ReadAllTextAsync(localFilePath);
            return JsonConvert.DeserializeObject<Leaderboard>(jsonContent);
        }

        public async Task UploadLeaderboardBlobAsync(Leaderboard leaderboard, string localFilePath)
        {
            var jsonContent = JsonConvert.SerializeObject(leaderboard);
            await File.WriteAllTextAsync(localFilePath, jsonContent);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(_leaderboardBlobFileName);

            using (var stream = new FileStream(localFilePath, FileMode.Open))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            _logger.LogInformation("Uploaded Leaderboard successfully from {LocalFilePath}.", localFilePath);
        }
    }
}
