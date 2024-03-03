using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Api.Models;
using TextToSpeech.Api.Services;

namespace TextToSpeech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeechController(ISpeechService speechService) : ControllerBase
{
    
    [HttpPost]
    public async ValueTask<IActionResult> GetSpeech(SpeechText speechText)
    {
        var result = await speechService.GetSpeechSynthesisResult(speechText.Text);
        return this.Ok(result);
    }
}