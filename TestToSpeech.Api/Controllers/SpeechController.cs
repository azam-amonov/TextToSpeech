using Microsoft.AspNetCore.Mvc;
using TestToSpeech.Api.Models;
using TestToSpeech.Api.Services;

namespace TestToSpeech.Api.Controllers;

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