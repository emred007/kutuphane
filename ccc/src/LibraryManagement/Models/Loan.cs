using System.Text.Json.Serialization;

namespace LibraryManagement.Models;

public class Loan
{
    [JsonPropertyName("Id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonPropertyName("KullaniciId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("AdSoyad")]
    public string UserFullName { get; set; } = string.Empty;

    [JsonPropertyName("KitapId")]
    public Guid BookId { get; init; }

    [JsonPropertyName("AlinmaTarihi")]
    public DateTime BorrowedAt { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("SonIadeTarihi")]
    public DateTime DueDate { get; set; }

    [JsonPropertyName("IadeTarihi")]
    public DateTime? ReturnedAt { get; set; }

    [JsonIgnore]
    public bool IsReturned => ReturnedAt.HasValue;

    [JsonIgnore]
    public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;
}
