using InsaatProjeleri.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsaatProjeleri.Controllers;

public class HomeController : Controller
{
    private readonly DatabaseService _db;
    public HomeController(DatabaseService db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.GetProjectsAsync());

    [HttpGet("/foto/{id:int}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Photo(int id)
    {
        var photo = await _db.GetPhotoAsync(id);
        if (photo is null) return NotFound();
        return File(photo.Value.Data, photo.Value.Mime);
    }

    public IActionResult Error() => View();
}
