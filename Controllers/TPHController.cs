using Microsoft.AspNetCore.Mvc;
using Marten;

namespace MartenApp.Controllers;

// why do i even need this? 
/// <summary>
/// ╲╲╲╭┓┏╮╲╱╭┓┏╮╱╱╱
/// ╲╲╲┃╰╯┃╲╱┃╰╯┃╱╱╱
/// ╲╲╲╰━┳╯ⓄⓄ╰┳━╯╱╱╱
/// ╲╲╲╲╲┃╭┻┻╮┃╱╱╱╱╱
/// ╲╲╲╲╲╰┫┏┓┣╯╱╱╱╱╱
/// ┈┈┈┈┏┳╋╰╯╋┳┓┈┈┈┈
/// </summary>
[ApiController]
[Route("TPH")]
public class TPHController : ControllerBase
{
	private readonly IDocumentSession ctx;

	public TPHController(IDocumentSession ctx)
	{
		this.ctx = ctx;
	}

	[HttpGet]
	public async Task<IActionResult> Get()
	{
		var query = ctx.CreateBatchQuery();
		var queriedUser1 = query.Query<BaseClass>().ToList();
		var queriedUser2 = query.Query<FirstSubClass>().ToList();
		var queriedUser3 = query.Query<SecondSubClass>().ToList();

		await query.Execute();

		var dict = new Dictionary<string, object>()
		{
			{ nameof(BaseClass), queriedUser1.Result },
			{ nameof(FirstSubClass), queriedUser2.Result },
			{ nameof(SecondSubClass), queriedUser3.Result }
		};

		return Ok(dict);
	}

	[HttpPost]
	public async Task<IActionResult> Create()
	{
		var user1 = new BaseClass(Guid.NewGuid());
		var user2 = new FirstSubClass(Guid.NewGuid());
		var user3 = new SecondSubClass(Guid.NewGuid());
		ctx.Store<object>(user1, user2, user3);
		await ctx.SaveChangesAsync();

		return Ok();
	}
}

public record BaseClass(Guid Id);
public record FirstSubClass : BaseClass
{
	public FirstSubClass(Guid id) : base(id)
	{ }
}
public record SecondSubClass : BaseClass
{
	public SecondSubClass(Guid id) : base(id)
	{ }
}
