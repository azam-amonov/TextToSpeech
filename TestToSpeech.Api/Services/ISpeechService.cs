using Microsoft.CognitiveServices.Speech;

namespace TestToSpeech.Api.Services;

public interface ISpeechService
{
    Task<SpeechSynthesisResult> GetSpeechSynthesisResult(string text);
}