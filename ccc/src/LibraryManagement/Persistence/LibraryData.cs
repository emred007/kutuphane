using System.Text.Json.Serialization;
using LibraryManagement.Models;

namespace LibraryManagement.Persistence;

public class LibraryData
{
    [JsonPropertyName("Kullanicilar")]
    public List<User> Users { get; set; } = [];

    [JsonPropertyName("Kitaplar")]
    public List<Book> Books { get; set; } = [];

    [JsonPropertyName("OduncKayitlari")]
    public List<Loan> Loans { get; set; } = [];

    [JsonPropertyName("Rezervasyonlar")]
    public List<Reservation> Reservations { get; set; } = [];

    [JsonPropertyName("Ayarlar")]
    public LibrarySettings Settings { get; set; } = new();
}
