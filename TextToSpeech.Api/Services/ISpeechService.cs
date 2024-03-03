using Microsoft.CognitiveServices.Speech;

namespace TextToSpeech.Api.Services;

public interface ISpeechService
{
    Task<SpeechSynthesisResult> GetSpeechSynthesisResult(string text);
}