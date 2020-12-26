

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Services;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{

	private readonly GeoDb _context;
	private readonly IConfiguration _configuration;
	private IBlobService _blobService;

	public ItemsController(GeoDb db, IConfiguration configuration, IBlobService blobService)
	{
		_context = db;
		_configuration = configuration;
		_blobService = blobService;
	}


	[HttpGet("nearby")]
	public ActionResult<IEnumerable<Item>> Nearby(double lng=0, double lat=0, double radius=10000)
	{
		var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
		var myLocation = geometryFactory.CreatePoint(new Coordinate(lng, lat));

		var items = _context.Items
		.Where(x => x.Location.IsWithinDistance(myLocation, radius))
		.OrderBy(c => c.Location.Distance(myLocation)).ToList();

		return items;
	}

	// [HttpGet("inmap")]
	// public ActionResult<IEnumerable<Item>> InMap(double se=0, double ne=0, double sw=0, double nw=0)
	// {
	// 	// var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
	// 	// var myLocation = geometryFactory.CreatePoint(new Coordinate(lng, lat));
	// 	// var myLocation = geometryFactory.CreatePolygon()



	// 	// var coo = new CoordinateSequence(se, ne, sw, nw);
	// 	// var geo = geometryFactory.CreatePolygon(

	// 	// )

	// 	// var items = _context.Items
	// 	// .Where(x => x.Location.Within())
	// 	// .Where(x => x.Location.IsWithinDistance(myLocation, 2000))
	// 	// .OrderBy(c => c.Location.Distance(myLocation)).ToList();

	// 	return items;
	// }


	// [Route("/items2")]
	// [HttpGet]
	// public async Task Item2()
	// {
	// 	return new HttpResponseMessage(HttpStatusCode.OK)
	// 					{
	// 									Content = new JsonContent("{}"),
	// 									RequestMessage = Request
	// 					};
	// 	}
	// 	// return _context.Items;
	// }


	// [Route("/items")]
	// [HttpGet]
	// public async IAsyncEnumerable<Item> Item()
	// {
	// 	var items = _context.Items;

	// 	await foreach (var item in items)
	// 	{
	// 		yield return item;
	// 	}
	// }





	[HttpPost]
	public async Task<ActionResult<Item>> Create([FromForm] UploadItem item)
	{
		var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
		var myLocation = geometryFactory.CreatePoint(new Coordinate(item.Long, item.Lat));
		
		var newItem = new Item
		{
			Location = myLocation,
			Created = DateTime.Now
		};

		_context.Items.Add(newItem);
		await _context.SaveChangesAsync();

		if (Request.Form.Files.Count() == 1) {
			IFormFile file = Request.Form.Files[0];
			if (file != null)
			{
				var result = await _blobService.UploadFileBlobAsync(_configuration["BlobStorage:Containername"], file.OpenReadStream(), file.ContentType, newItem.Id.ToString());
				var toReturn = result.AbsoluteUri;

				newItem.Image = toReturn;
				await _context.SaveChangesAsync();
			}
		}

		Console.WriteLine("Thank you!");

		return CreatedAtAction(nameof(Item), new { id = newItem.Id }, newItem);

		// return Ok(new { path = toReturn });
	}
}
