using Microsoft.CognitiveServices.Speech;
using TestToSpeech.Api.Models;

namespace TestToSpeech.Api.Services;

public class SpeechService: ISpeechService
{
    private readonly string SubscriptionKey = "YOUR_KEY";
    private readonly string Region = "YOUR_REGION";
    private const string SpeechVoice = "en-US-NancyNeural";
    private static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


    private readonly IWebHostEnvironment webHostEnvironment;
    private readonly string wwwRootPath;

    public SpeechService(
        IWebHostEnvironment webHostEnvironment)
    {
        this.webHostEnvironment = webHostEnvironment;
        this.wwwRootPath = Path.Combine(webHostEnvironment.WebRootPath, "speeches");
        Directory.CreateDirectory(this.wwwRootPath);
    }

    public async Task<SpeechSynthesisResult> GetSpeechSynthesisResult(string text)
    {
        var speechConfig = SpeechConfig.FromSubscription(
            subscriptionKey: this.SubscriptionKey,
            region: this.Region);

        speechConfig.SpeechSynthesisVoiceName = SpeechVoice;

        using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
        {
            var speechResult = await speechSynthesizer.SpeakTextAsync(text: text);
            OutputSpeechSynthesisResult(speechResult, text);
            SaveSpeechSynthesisResultToLocalDirectory(speechResult, text);
            try
            {
                var filePath = Path.Combine(this.wwwRootPath, $"{FileNameIdDateTime()}.wov");
                await File.WriteAllBytesAsync(filePath, speechResult.AudioData);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sorry I couldn't'");
                throw;
            }
            
            return speechResult;
        }
    }

    private static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
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

    private static async void SaveSpeechSynthesisResultToLocalDirectory(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        
        var guidName = Guid.NewGuid().ToString();
        var pathForFileToSave = Path.Combine(DesktopPath, $"{guidName}.wav");
        if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            var audioStream = AudioDataStream.FromResult(speechSynthesisResult);
            await audioStream.SaveToWaveFileAsync(pathForFileToSave);
            Console.WriteLine($"Audio saved successfully!");
        }
    }

    private static string FileNameIdDateTime()
    {
        var time = DateTimeOffset.Now.ToString();
        var timeClearName = time
            .Replace(" ", "-")
            .Replace(":", "-")
            .Replace("+", "")
            .Replace("/", "-");

        return timeClearName;
    }
}