using System.Diagnostics;
using Microsoft.CognitiveServices.Speech;

namespace TextToSpeech.Api.Services;

public class SpeechService: ISpeechService
{
    private string SubscriptionKey = "YOUR_KEY_HERE";
    private string Region = "eastus";

    private const string SpeechVoice = "en-IN-RaviNeural";
        // "en-US-ZiraNeural"; 7/10
        // "en-US-JennyNeural"; 7/10
        // "zh-CN-XiaoxiaoNeural"; 4/10 (for joke 8/10)
        // "en-IN-NeerjaNeural"; 4/10 for joke
        //"en-US-AriaNeural"; 8/10
        //"en-AU-TinaNeural"; 7/10
        
    private static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    private readonly IWebHostEnvironment webHostEnvironment;
    private readonly string wwwRootPath;

    static readonly Stopwatch timer = new Stopwatch();
    public SpeechService(
        IWebHostEnvironment webHostEnvironment)
    {
        this.webHostEnvironment = webHostEnvironment;
        this.wwwRootPath = Path.Combine(webHostEnvironment.WebRootPath, "speeches");
        Directory.CreateDirectory(this.wwwRootPath);
    }

    public async Task<SpeechSynthesisResult> GetSpeechSynthesisResult(string text)
    {
        // text = "As an IELTS Writing examiner, I am unable to provide feedback or assign an overall band score to the text you provided, as it does not fit into a specific IELTS Writing Task. To give you a meaningful assessment, I would need a sample of your writing that corresponds to either Task 1 (a report based on visual information for the Academic module, or a letter for the General Training module) or Task 2 (an essay in response to a statement or question for both modules).\n\nHowever, I can give you some general advice based on the IELTS Writing criteria:\n\n1. Task Achievement/Task Response:\n   - For Task 1, ensure you have adequately described all key features of the graph, table, chart, or diagram, and that you have made comparisons where relevant.\n   - For Task 2, you must address all parts of the prompt, present a clear position, and develop an argument supported by relevant examples.\n\n2. Coherence and Cohesion:\n   - Organize your writing in a logical manner using paragraphs.\n   - Use a variety of cohesive devices (linking words, pronoun references, etc.) to connect ideas and paragraphs.\n   - Avoid overusing certain phrases and check that each sentence flows smoothly into the next.\n\n3. Lexical Resource:\n   - Demonstrate a wide range of vocabulary concerning the topic.\n   - Use collocations accurately and with flexibility.\n   - Avoid repeated use of the same words or phrases, and be aware of word forms and appropriateness in terms of style.\n\n4. Grammatical Range and Accuracy:\n   - Employ a mix of simple and complex sentence structures.\n   - Check for subject-verb agreement, correct use of tenses, articles, prepositions, and punctuation.\n   - Aim to be as error-free as possible, but do not worry if there are minor errors as long as they do not impede understanding.\n\nIf you provide a specific IELTS Writing Task sample, I would be able to give you feedback on these criteria and a probable band score. Remember, the ideal sample would be between 150-250 words for Task 1 and 250-400 words for Task 2.";
        timer.Start();
        
        //speech configurations
        var speechConfig = SpeechConfig.FromSubscription(
            subscriptionKey: this.SubscriptionKey,
            region: this.Region
            );

        speechConfig.SpeechSynthesisVoiceName = SpeechVoice;
        
        // if audioConfig is null AI will not rely on web with audio!
        // if u want hear audio on web just remove null value from audioConfig: null
        using (var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig: null))
        {
            
            // synthesizing text to speech
            var speechResult = await speechSynthesizer.SpeakTextAsync(text: text);
            
            OutputSpeechSynthesisResult(speechResult, text);
            
            
            try
            {
                
                var filePath = Path.Combine(this.wwwRootPath, $"{FileNameIdDateTime()}.wav");
               
                // try save audio to storage in local or in blob server
                SaveSpeechSynthesisResultToLocalDirectory(
                    speechSynthesisResult: speechResult,
                    filePath: filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sorry I couldn't'");
                throw;
            }
            
            timer.Stop();
            Console.WriteLine(timer.Elapsed.ToString());
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
