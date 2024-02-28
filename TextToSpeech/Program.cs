using Azure.Storage.Blobs;
using Microsoft.CognitiveServices.Speech;

static class Program
{
    const string SubscriptionKey = "c471224359ce494499635bc6a10ffd58";
    const string Region = "eastus";
    private static string _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    const string BlobConnectionString = "DefaultEndpointsProtocol=https;AccountName=betarteebblob;AccountKey=SmrTfiJH9P8bHVZvjCSUm7uk776hlO7gY/uJzj0E4i3lmy3DwwwwGftHBNUXGJpFPOceUTALRsmg+AStOu9Jrg==;EndpointSuffix=core.windows.net";
    const string BlobContainerName = "audio";
    static async Task UploadAudioToBlobStorage(AudioDataStream audioStream)
    {
        var blobServiceClient = new BlobServiceClient(BlobConnectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName);
        
        await blobContainerClient.CreateIfNotExistsAsync();
        
        var blobName = $"{Guid.NewGuid()}.wav";
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = (int)audioStream.ReadData(buffer)) > 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            await blobClient.UploadAsync(memoryStream, true);
        }

        Console.WriteLine($"Audio uploaded to Blob Storage. Blob URI: {blobClient.Uri}");
    }

    static async void SaveSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        var guidName = Guid.NewGuid().ToString();
        var pathForFileToSave = Path.Combine($"{_desktopPath}", $"{guidName}.wav");
        if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
        {
                var audioStream = AudioDataStream.FromResult(speechSynthesisResult);
                // await UploadAudioToBlobStorage(audioStream);
                await audioStream.SaveToWaveFileAsync(pathForFileToSave);
                Console.WriteLine($"Audio saved successfully!");
        }
    }
    
    static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        switch (speechSynthesisResult.Reason)
        {
            case ResultReason.SynthesizingAudioCompleted:
                Console.WriteLine($"Speech synthesized for text: {text}");
                Console.WriteLine($"Reason type: {speechSynthesisResult.Reason.GetType()}");
                break;
            case ResultReason.Canceled:
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    Console.WriteLine("CANCELED: Did you set the speech resource key and region values?");
                }
                break;
        }
    }

    static async Task Main(string[] args)
    {
        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey: SubscriptionKey, region: Region);
        speechConfig.SpeechSynthesisVoiceName = "en-US-NancyNeural";

        using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
        {
            Console.WriteLine("Enter text:");
            var userText = Console.ReadLine();

            var speechResult = await speechSynthesizer.SpeakTextAsync(text: userText);
            
            OutputSpeechSynthesisResult(speechResult, userText);
            SaveSpeechSynthesisResult(speechResult, userText);
            
            var duration = speechResult.AudioDuration;
            Console.WriteLine($"Audio duration: {duration}");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
