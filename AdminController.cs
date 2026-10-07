using InsaatProjeleri.Models;
using InsaatProjeleri.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsaatProjeleri.Controllers;

[Route("yonetim-8f42")]
public class AdminController : Controller
{
    private readonly DatabaseService _db;
    public AdminController(DatabaseService db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
        => View(await _db.GetProjectsAsync());

    [HttpGet("yeni")]
    public IActionResult Create() => View(new ProjectForm());

    [HttpPost("yeni")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Create(ProjectForm model, List<IFormFile> fotograflar)
    {
        if (!ModelState.IsValid) return View(model);
        await _db.CreateProjectAsync(model, fotograflar);
        TempData["Ok"] = "Proje başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("duzenle/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.GetProjectAsync(id);
        if (p is null) return NotFound();
        ViewBag.Project = p;
        return View(new ProjectForm {
            Id=p.Id, ProjeAdi=p.ProjeAdi, ProjeAciklamasi=p.ProjeAciklamasi, ProjeDegeri=p.ProjeDegeri
        });
    }

    [HttpPost("duzenle/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Edit(int id, ProjectForm model, List<IFormFile> fotograflar)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            ViewBag.Project = await _db.GetProjectAsync(id);
            return View(model);
        }
        await _db.UpdateProjectAsync(model, fotograflar);
        TempData["Ok"] = "Proje güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("sil/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.DeleteProjectAsync(id);
        TempData["Ok"] = "Proje silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("foto-sil/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhoto(int id, int projectId)
    {
        await _db.DeletePhotoAsync(id, projectId);
        TempData["Ok"] = "Fotoğraf silindi.";
        return RedirectToAction(nameof(Edit), new { id = projectId });
    }
}
