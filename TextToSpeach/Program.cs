using Microsoft.CognitiveServices.Speech;


 static class Program
{
    const string SubscriptionKey = "YOUR_KEY_HERE";
    const string Region = "YOUR_REGION_HERE";

    static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        switch (speechSynthesisResult.Reason)
        {
            case ResultReason.SynthesizingAudioCompleted:
                Console.WriteLine($"Speech synthesized for text : {text}");
                break;
            case ResultReason.Canceled:
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                }

                break;
        }
    }

    async static Task Main(string[] args)
    {
        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey: SubscriptionKey, region: Region);
        speechConfig.SpeechSynthesisVoiceName = "en-US-GuyNeural";

        using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
        {
            Console.WriteLine("Enter text");
            var userText = Console.ReadLine();

            var speechResult = await speechSynthesizer.SpeakTextAsync(text: userText);
            OutputSpeechSynthesisResult(speechResult, text: userText);
        }
        
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}