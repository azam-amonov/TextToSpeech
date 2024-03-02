using Azure.Storage.Blobs;

namespace SpeechToText.Blobs;

public class BlobService
{
    static string downloadFilePath = "/Users/azamamonov/RiderProjects/TextToSpeach/SpeechToText/wwwroot";
    static string connectionString = "your_connection_string";
    static string containerName = "your_container_name";
    static string blobName = "your_blob_name";

    public static async Task CreateBlob()
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        // Get a reference to the container
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // Get a reference to the blob
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DownloadToAsync(downloadFilePath);

        return ;

    }

}