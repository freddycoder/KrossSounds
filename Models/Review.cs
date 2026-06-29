using System.ComponentModel;

namespace KrossSounds.Models;

public class Review
{
    public int Id { get; set; }

    // Score sur 5 étoiles
    [DisplayName("Note")]
    public int Rating { get; set; }   // 1 à 5

    // Commentaire de l'utilisateur
    [DisplayName("Commentaire")]
    public string Comment { get; set; } = string.Empty;

    // Id de l'utilisateur qui a fait le review
    public string UserId { get; set; } = default!;

    // Relation avec Sound
    public int SoundId { get; set; }
    public Sound Sound { get; set; } = default!;
}
