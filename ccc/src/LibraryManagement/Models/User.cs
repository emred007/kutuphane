using System.Text.Json.Serialization;

namespace LibraryManagement.Models;

public class User
{
    [JsonPropertyName("Id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonPropertyName("AdSoyad")]
    public required string FullName { get; set; }

    [JsonPropertyName("Eposta")]
    public required string Email { get; set; }

    [JsonPropertyName("Telefon")]
    public string PhoneNumber { get; set; } = "";

    [JsonPropertyName("KayitTarihi")]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
