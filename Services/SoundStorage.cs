using Microsoft.AspNetCore.StaticFiles;

namespace KrossSounds.Services;

public interface ISoundStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct = default);

    Task<(Stream Stream, string FileName, string ContentType)?> GetAsync(
        string relativePath,
        CancellationToken ct = default);

    string GetFullPath(string relativePath);

    Task<List<string>> ListFilesAsync();
}

public class LocalSoundStorageService : ISoundStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly FileExtensionContentTypeProvider _mimeProvider;

    public LocalSoundStorageService(IWebHostEnvironment env)
    {
        _env = env;
        _mimeProvider = new FileExtensionContentTypeProvider();
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var uploadsDir = GetFullPath("uploads");
        Directory.CreateDirectory(uploadsDir);

        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var fullPath = Path.Combine(uploadsDir, uniqueName);

        await using var file = File.Create(fullPath);
        await fileStream.CopyToAsync(file, ct);

        // Chemin relatif pour la base de données
        return Path.Combine("uploads", uniqueName).Replace("\\", "/");
    }

    public async Task<(Stream Stream, string FileName, string ContentType)?> GetAsync(
        string relativePath,
        CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);

        if (!File.Exists(fullPath))
            return null;

        if (!_mimeProvider.TryGetContentType(fullPath, out var mime))
            mime = "application/octet-stream";

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return (stream, Path.GetFileName(fullPath), mime);
    }

    public string GetFullPath(string relativePath)
    {
        return Path.Combine(_env.WebRootPath, relativePath);
    }

    public Task<List<string>> ListFilesAsync()
    {
        var path = Path.Combine(_env.WebRootPath, "uploads");
        return Task.FromResult(Directory.EnumerateFiles(path).Select(p => Path.GetRelativePath(_env.WebRootPath, p)).ToList());
    }
}
