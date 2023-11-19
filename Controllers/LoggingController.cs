using MartenApp.LoggingMediatr;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace MartenApp.Controllers;

[ApiController]
[Route("logging")]
public class LoggingController : ControllerBase
{
	private readonly IMediator mediator;
	private readonly TimeProvider timeProvider;

	public LoggingController(IMediator mediator, TimeProvider timeProvider)
	{
		this.mediator = mediator;
		this.timeProvider = timeProvider;
	}

	[HttpGet]
	public async Task<IActionResult> Get()
	{
		var command = new BestCommandEver();
		await mediator.Send(command);

		return Ok(":)");
	}

	[HttpGet]
	[Route("Etag")]
	public IActionResult GetWithEtag()
	{
		var utcNow = timeProvider.GetUtcNow();
		var localNow = timeProvider.GetLocalNow();
		var localTZ = timeProvider.LocalTimeZone;

		var utcNow1 = DateTimeOffset.UtcNow;
		var localNow1 = DateTimeOffset.Now;
		var localNow2 = utcNow1.LocalDateTime;
		var localTZ1 = TimeZoneInfo.Local;

		var local = timeProvider.GetLocalNow();
		var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
		var converted = TimeZoneInfo.ConvertTime(local, cstZone);

		return Ok(1231231);
	}

	[HttpGet]
	[Route("error :)")]
	public IActionResult GetThatJuicyError()
	{
		throw new Exception("-_-");
	}
}
