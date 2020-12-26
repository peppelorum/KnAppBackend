


using Microsoft.AspNetCore.Mvc;

[Controller]
// [Route("map/[controller]")]
public class MapController : Controller
{

	[Route("/map")]
	public IActionResult Index()
	{
		return View();
	}
}