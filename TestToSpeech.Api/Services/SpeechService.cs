using Microsoft.CognitiveServices.Speech;

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
        // Change to speech voice 
        using (var speechSynthesizer = new SpeechSynthesizer(speechConfig, null))
        {
            var speechResult = await speechSynthesizer.SpeakTextAsync(text: text);
            OutputSpeechSynthesisResult(speechResult, text);
            
            try
            {
                var filePath = Path.Combine(this.wwwRootPath, $"{FileNameIdDateTime()}.wav");
               
                SaveSpeechSynthesisResultToLocalDirectory(
                    speechSynthesisResult: speechResult,
                    filePath: filePath);
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

    private static async void SaveSpeechSynthesisResultToLocalDirectory(
        SpeechSynthesisResult speechSynthesisResult, 
        string filePath)
    {
        if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            var audioStream = AudioDataStream.FromResult(speechSynthesisResult);
            await audioStream.SaveToWaveFileAsync(filePath);
            Console.WriteLine($"Audio saved successfully to {filePath}");
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