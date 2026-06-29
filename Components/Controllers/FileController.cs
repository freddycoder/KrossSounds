using Microsoft.AspNetCore.Mvc;
using KrossSounds.Services;
using KrossSounds.Data;

[Route("download")]
public class DownloadController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ISoundStorageService _storage;

    public DownloadController(ApplicationDbContext db, ISoundStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var sound = _db.Sounds.FirstOrDefault(s => s.Id == id);
        if (sound == null)
            return NotFound("Fichier introuvable dans la base de données.");

        var result = await _storage.GetAsync(sound.FilePath, ct);
        if (result == null)
            return NotFound("Le fichier n'existe plus sur le disque.");

        return File(
            result.Value.Stream, 
            result.Value.ContentType, 
            result.Value.FileName);
    }
}
