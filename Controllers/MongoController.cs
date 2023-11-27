using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;
using Serilog.Extensions.Logging;

namespace MartenApp.Controllers;

[ApiController]
[Route("mongo")]
public class MongoController : ControllerBase
{
	private readonly IMongoCollection<Game> db;
	private static readonly Guid Id = Guid.Parse("82157646-b752-4899-904d-562dfd02f20c");

	public MongoController(IConfiguration cfg)
	{
		var connection = cfg.GetValue<string>("MongoDBLocal");

		// no bueno
		var settings = MongoClientSettings.FromConnectionString(connection);
		settings.ServerApi = new ServerApi(ServerApiVersion.V1);

		var logger = LoggerFactory.Create(options =>
		{
			options.AddProvider(new SerilogLoggerProvider());
			options.SetMinimumLevel(LogLevel.Information);
		});

		settings.LoggingSettings = new LoggingSettings(logger);

		var client = new MongoClient(settings); // or just pass a conn string
		this.db = client.GetDatabase("admin").GetCollection<Game>(name: "Games");
	}

	#region Validation Schema

	private void CreateCollection(IMongoClient client)
	{
		var db = client.GetDatabase("admin");
		db.CreateCollection(name: "Games", new CreateCollectionOptions<Game>()
		{
			Validator = new FilterDefinitionBuilder<Game>().JsonSchema(BsonDocument.Parse("")),
			ValidationAction = DocumentValidationAction.Error,
			ValidationLevel = DocumentValidationLevel.Strict
		});
	}

	private void ChangeSchemaValidation(IMongoClient client)
	{
		var db = client.GetDatabase("admin");
		var jsonSchema = @"{ collMod: ""users"",
								validator: {
									$jsonSchema: {
										bsonType: ""object"",
										required: [ ""username"", ""password"" ],
										properties: {
										username: {
											bsonType: ""string"",
											description: ""must be a string and is required""
										},
										password: {
											bsonType: ""string"",
											minLength: 12,
											description: ""must be a string of at least 12 characters, and is required""
										}
										}
									}
								},
								validationAction: ""error"",
								validationLevel: ""strict"",
								}";
		var command = new JsonCommand<BsonDocument>(jsonSchema);
		db.RunCommand(command);
	}

	#endregion

	[HttpGet]
	[Route("Create")]
	public IActionResult Create()
	{
		var document = MongoExt.CreateDocument();
		db.InsertOne(document);

		return Ok();
	}

	[HttpGet]
	[Route("Replace")]
	public IActionResult Replace([FromQuery] string price)
	{
		// replace commands DO NOT create new resources by default

		var game = db.Find(x => x.Id == Id).FirstOrDefault();
		game.Price = price;

		// returns pre replaced resource
		var replaced = db.FindOneAndReplace(x => x.Id == Id, game);

		// it's a full crudish replace
		var replaceResult = db.ReplaceOne(x => x.Id == Id, game);


		object res1 = null;
		try
		{
			// will fail cause game resource already has an Id which cant be changed to "Some Id" 
			res1 = db.ReplaceOne(x => x.Id == Guid.NewGuid(), game, new ReplaceOptions() { IsUpsert = true });
		}
		catch (Exception)
		{ }

		object res2 = null;
		try
		{
			// will not fail
			// cause Id of the filter and the resource match, new Id = "Whatever Id"
			var id = Guid.NewGuid();
			game.Id = id;
			res2 = db.ReplaceOne(x => x.Id == id, game, new ReplaceOptions() { IsUpsert = true });
		}
		catch (Exception)
		{ }

		object res3 = null;
		try
		{
			// will not fail nor insert a new document
			var id2 = Guid.NewGuid();
			game.Id = id2;
			res3 = db.ReplaceOne(x => x.Id == id2, game, new ReplaceOptions() { IsUpsert = false });
		}
		catch (Exception)
		{ }


		// replaceResult
		// {
		//	"isAcknowledged": true,
		//  "isModifiedCountAvailable": true,
		//  "matchedCount": 0,
		//  "modifiedCount": 0,
		//  "upsertedId": null
		// }

		return Ok(new { replaced, replaceResult, res1, res2, res3 });
	}

	[HttpGet]
	[Route("Update")]
	public IActionResult Update([FromQuery] string price)
	{
		// update commands DO NOT create new resources by default

		var update = Builders<Game>.Update.Set(x => x.Item, "Ori")
			.Set(x => x.Price, "123$");

		var update1 = Builders<Game>.Update.Set(x => x.Qty, 15);

		var combinedUpdate = Builders<Game>.Update.Combine(update, update1);

		// returns pre updated resource
		// if it doesnt find resource by id - new one WILL NOT be created by default
		var updated = db.FindOneAndUpdate(x => x.Id == Id, combinedUpdate);

		// returns meta data
		// if it doesnt find resource by id - new one WILL NOT be created by default
		var updateResult = db.UpdateOne(x => x.Id == Id, update);

		// new resource WILL BE be created
		//var res1 = db.UpdateOne(x => x.Id == "Some Id", update, new UpdateOptions() { IsUpsert = true });

		// updateResult
		// {
		//	"isAcknowledged": true,
		//  "isModifiedCountAvailable": true,
		//  "matchedCount": 0,
		//  "modifiedCount": 0,
		//  "upsertedId": null
		// }

		return Ok(new { updated, updateResult });
	}

	[HttpGet]
	[Route("Delete")]
	public IActionResult Delete()
	{
		// same story
		// var deleted = db.FindOneAndDelete

		var deleteResult = db.DeleteOne(x => x.Id == Id);
		// result = { "deletedCount": 1, "isAcknowledged": true }

		return Ok(deleteResult);
	}

	[HttpGet]
	[Route("Count")]
	public IActionResult Count()
	{
		var filter = Builders<Game>.Filter.Eq("Item", "Ori");

		var estimated = db.EstimatedDocumentCount(); // #1, fastest, uses collection meta data

		var withOptions = db.CountDocuments(_ => true, new CountOptions() { Hint = "_id_" }); // #2, uses only index scan

		var preBuiltFilter = db.CountDocuments(filter); // slow

		var lambdaFilter = db.CountDocuments(x => x.Item == "Ori"); // slow

		return Ok(new { preBuiltFilter, lambdaFilter, estimated, withOptions });
	}

	[HttpGet]
	[Route("Get")]
	public IActionResult Get()
	{
		var filter = Builders<Game>.Filter.Eq("_id", "82157646-b752-4899-904d-562dfd02f20c");
		var emptyFilter = Builders<Game>.Filter.Empty;

		var what = db.Find(filter); // return dog shit

		var gameWithFilter = db.Find(filter).FirstOrDefault(); // return by pre built filter

		var gameDefault = db.Find(x => x.Id == Id) // return by simple labmda
							.FirstOrDefault();

		var gamesByEmptyFilter = db.Find(emptyFilter).ToList();

		var gamesByAnonFilter = db.Find(_ => true).ToList();

		return Ok(new
		{
			gameWithFilter,
			gameDefault
		});
	}

	[HttpGet]
	[Route("Projection")]
	public IActionResult Projection()
	{
		// doesnt work 
		var projection = Builders<Game>.Projection.Include(x => x.Item).Include(x => x.Genres);

		// works
		var expression = Builders<Game>.Projection.Expression(x => new { x.Item, x.Genres });

		var result = db.Find(_ => true).Project(expression).FirstOrDefault();

		return Ok(result);
	}

	[HttpGet]
	[Route("Sorting")]
	public IActionResult Sorting()
	{
		var sorting = Builders<Game>.Sort.Ascending(x => x.Qty);

		var result = db.Find(_ => true).Sort(sorting).ToList();

		return Ok(result);
	}

	[HttpGet]
	[Route("Queryable")]
	public IActionResult Queryable()
	{
		var result = db.AsQueryable()
			.Where(x => x.Item == "Ori")
			.Select(x => new { x.Item, x.Qty, x.Genres }).ToList();

		return Ok(result);
	}

	[HttpGet]
	[Route("Searching")]
	public IActionResult Searching()
	{
		// Aggregation commands only allowed on Atlas
		// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/atlas-search/
		var result = db.Aggregate().Search(Builders<Game>.Search.Phrase(x => x.Item, "r")).ToList();

		return Ok(result);
	}

	[HttpGet]
	[Route("Joining")]
	public IActionResult Joining()
	{
		// Lookup<TForeignDocument, TNewResult>(string foreignCollectionName,
		// FieldDefinition<TResult> localField,
		// FieldDefinition<TForeignDocument> foreignField,
		// FieldDefinition<TNewResult> @as,
		// AggregateLookupOptions<TForeignDocument,
		// TNewResult> options = null);
		//var result = db.Aggregate().Lookup<TForeignDocument, TNewResult>("otherCollection",
		//	game => game.PlayerId, player => player.Id, new { props });

		return Ok();
	}
}

public sealed class Game
{
	// mapped to _id by default
	//public required string Id { get; set; }
	public required Guid Id { get; set; }

	[BsonElement("Item")] // has an effect, not here though, cuz names match
	public required string Item { get; set; }

	public int Qty { get; set; }

	public required string Price { get; set; }

	public required Size Size { get; set; }

	public required List<string> Genres { get; set; } = new();
}

public sealed record Size
{
	public int H { get; set; }
	public int W { get; set; }
	public string Uom { get; set; }
}

public static class MongoExt
{
	private static readonly Guid Id = Guid.Parse("82157646-b752-4899-904d-562dfd02f20c");
	public static Game CreateDocument()
	{
		//var document = new BsonDocument
		//{
		//	{ "_id", Guid.NewGuid().ToString() },
		//	{ "item", "God of War" },
		//	{ "qty", 1 },
		//	{ "price", "69.99$" },
		//	{ "genres", new BsonArray { "Action", "RPG" } },
		//	{ "size", new BsonDocument { { "h", 28 }, { "w", 35.5 }, { "uom", "cm" } } }
		//};

		var document = new Game()
		{
			Id = Id,
			Item = "Ori",
			Qty = 1,
			Price = "69.99$",
			Genres = ["Action", "RPG"],
			Size = new Size() { H = 28, W = 30, Uom = "cm" }
		};

		return document;
	}
}
