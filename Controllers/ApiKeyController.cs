using Microsoft.AspNetCore.Mvc;
using MartenApp.Auth;

namespace MartenApp.Controllers;

[ApiController]
[Route("key")]
[ApiKey]
public class ApiKeyController : ControllerBase
{
	[HttpGet]
	public IActionResult Get()
	{

		return Ok();
	}
}
