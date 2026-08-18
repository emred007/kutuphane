using System.Text.Json.Serialization;

namespace LibraryManagement.Models;

public class Reservation
{
    [JsonPropertyName("Id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonPropertyName("KullaniciId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("AdSoyad")]
    public string UserFullName { get; set; } = string.Empty;

    [JsonPropertyName("KitapId")]
    public Guid BookId { get; init; }

    [JsonPropertyName("RezervasyonTarihi")]
    public DateTime ReservedAt { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("Durum")]
    public string Status { get; set; } = ReservationStatus.Waiting;
}

public static class ReservationStatus
{
    public const string Waiting = "Bekliyor";
    public const string Ready = "Hazır";
    public const string Completed = "Tamamlandı";
    public const string Cancelled = "İptal";

    public static bool IsOpen(string status)
        => status is Waiting or Ready;
}
