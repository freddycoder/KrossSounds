using System.ComponentModel.DataAnnotations;
using KrossSounds.Data;

namespace KrossSounds.Models;

public class Sound
{
    public int Id { get; set; }

    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    [MaxLength(15)]
    public string? Location { get; set; } // ex: "prog ou combi"

    /// <summary>
    /// Numéro du preset pour le retrouver dans le PCG, selon sa catégorisation (Locations, Tags)
    /// Ex: Location: prog, Tags Piano, Number 50
    /// </summary>
    public int? Number { get; set; }

    public string FilePath { get; set; } = default!; // ex: "uploads/1234.pcg"

    [MaxLength(32)]
    public string? Tags { get; set; } // "piano,pad,EDM"

    [MaxLength(32)]
    public string? KrossModel { get; set; } // "Kross 2-61", "Kross 2-88"

    [YoutubeUrl]
    [MaxLength(256)]
    public string? YoutubeUrl { get; set; }

    public int DownloadCount { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public List<Review> Reviews { get; set; } = new();

    public double AverageRating =>
        Reviews.Count == 0 ? 0 : Reviews.Average(r => r.Rating);

    public string? GetEmbedYoutubeUrl()
    {
        if (string.IsNullOrWhiteSpace(YoutubeUrl))
            return null;

        try
        {
            // Format youtu.be/xxxx
            if (YoutubeUrl.Contains("youtu.be/"))
            {
                var id = YoutubeUrl.Split("youtu.be/")[1];
                return $"https://www.youtube.com/embed/{id}";
            }

            // Format youtube.com/watch?v=xxxx
            var uri = new Uri(YoutubeUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var id2 = query["v"];

            return id2 is null ? null : $"https://www.youtube.com/embed/{id2}";
        }
        catch
        {
            return null;
        }
    }

}

public class YoutubeUrlAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
            return ValidationResult.Success;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var host = uri.Host.ToLower();

            if (host.Contains("youtube.com") || host.Contains("youtu.be"))
                return ValidationResult.Success;
        }

        return new ValidationResult("Veuillez entrer une URL YouTube valide.");
    }
}