using MartenApp.LoggingMediatr;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace MartenApp.Controllers;

[ApiController]
[Route("logging")]
public class LoggingController : ControllerBase
{
	private readonly IMediator mediator;

	public LoggingController(IMediator mediator)
	{
		this.mediator = mediator;
	}

	[HttpGet]
	public async Task<IActionResult> Get()
	{
		var command = new BestCommandEver();
		await mediator.Send(command);

		return Ok(":)");
	}
}
