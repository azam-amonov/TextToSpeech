using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;

class Program 
{
    // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
    static string speechKey = "f1ab29f30ea24d0e85b60360b9f3cf5e";
    static string speechRegion = "eastus";

    static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
    {
        switch (speechRecognitionResult.Reason)
        {
            case ResultReason.RecognizedSpeech:
                Console.WriteLine($"RECOGNIZED: Text= \n{speechRecognitionResult.Text}");
                break;
            case ResultReason.NoMatch:
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                break;
            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                }
                break;
        }
    }
    static double GetAudioFileDuration(string audioFilePath)
    {
        using (var audioFileReader = new AudioFileReader(audioFilePath))
        {
            // Calculate the duration in seconds
            double durationInSeconds = audioFileReader.TotalTime.TotalSeconds;
            return durationInSeconds;
        }
    }
    
    async static Task Main(string[] args)
    {
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);        
        speechConfig.SpeechRecognitionLanguage = "en-US";

        var auidioPath = "/Users/azamamonov/RiderProjects/TextToSpeach/TestToSpeech.Api/wwwroot/speeches/welcome.wav";
       
        using var audioConfig = AudioConfig.FromWavFileInput(auidioPath);
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        Console.WriteLine("Speak into your microphone.");

        
        // Define a timer for 2 minutes (120 seconds)
        var durationOfAudioFile = (int)GetAudioFileDuration(auidioPath);
        var duration = TimeSpan.FromSeconds(durationOfAudioFile);
        var startTime = DateTime.Now;
        
        while(DateTime.Now - startTime < duration)
        {
            var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            OutputSpeechRecognitionResult(speechRecognitionResult);
        }

        // var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
        // OutputSpeechRecognitionResult(speechRecognitionResult);
        // If you want to signal the end of listening after 2 minutes, you can add a message here.
        Console.WriteLine("Listening ended after 2 minutes.");
    }

}