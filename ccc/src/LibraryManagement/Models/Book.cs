using System.Text.Json.Serialization;

namespace LibraryManagement.Models;

public class Book
{
    [JsonPropertyName("Id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonPropertyName("Baslik")]
    public required string Title { get; set; }

    [JsonPropertyName("Yazar")]
    public required string Author { get; set; }

    [JsonPropertyName("Isbn")]
    public required string Isbn { get; set; }

    [JsonPropertyName("ToplamKopya")]
    public int TotalCopies { get; set; } = 1;

    [JsonPropertyName("MusaitKopya")]
    public int AvailableCopies { get; set; } = 1;

    [JsonPropertyName("MusaitMi")]
    public bool IsAvailable
    {
        get => AvailableCopies > 0;
        set => AvailableCopies = value ? Math.Max(AvailableCopies, 1) : 0;
    }
}
