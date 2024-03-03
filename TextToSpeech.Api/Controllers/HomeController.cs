using Microsoft.AspNetCore.Mvc;

namespace TextToSpeech.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]

public class HomeController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> GetHomeMessage() => Ok("Hi there");
}