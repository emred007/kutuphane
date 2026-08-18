using System.Text;
using LibraryManagement.Models;
using LibraryManagement.Persistence;

namespace LibraryManagement.Services;

public static class DataViewer
{
    public static string BuildReadableReport(LibraryData data, LibraryStatistics? stats = null)
    {
        var report = new StringBuilder();
        report.AppendLine("=== SQLite Veritabanından Çekilen Kayıtlar ===");
        report.AppendLine();

        if (stats is not null)
        {
            report.AppendLine("İSTATİSTİKLER");
            report.AppendLine(new string('-', 60));
            report.AppendLine($"  Kullanıcı: {stats.UserCount} | Kitap: {stats.BookCount} | Kopya: {stats.TotalCopies}");
            report.AppendLine($"  Aktif ödünç: {stats.ActiveLoanCount} | Geciken: {stats.OverdueCount} | Rezervasyon: {stats.ReservationCount} (Hazır: {stats.ReadyReservationCount})");
            report.AppendLine($"  Ödünç süresi: {stats.LoanDurationDays} gün");
            report.AppendLine();
        }

        report.AppendLine($"KULLANICILAR ({data.Users.Count})");
        report.AppendLine(new string('-', 60));
        foreach (var user in data.Users.OrderBy(u => u.FullName))
        {
            report.AppendLine($"  {user.FullName} | {user.Email} | {user.PhoneNumber} | Kayıt: {FormatDate(user.CreatedAt)}");
        }

        report.AppendLine();
        report.AppendLine($"KITAPLAR ({data.Books.Count})");
        report.AppendLine(new string('-', 60));
        foreach (var book in data.Books.OrderBy(b => b.Title))
        {
            report.AppendLine($"  {book.Title} | {book.Author} | Kopya: {book.AvailableCopies}/{book.TotalCopies}");
        }

        report.AppendLine();
        report.AppendLine($"ÖDÜNÇ KAYITLARI ({data.Loans.Count})");
        report.AppendLine(new string('-', 60));
        foreach (var loan in data.Loans.OrderByDescending(l => l.BorrowedAt))
        {
            var bookTitle = data.Books.FirstOrDefault(b => b.Id == loan.BookId)?.Title ?? "Bilinmeyen kitap";
            var status = loan.IsReturned
                ? $"İade: {FormatDate(loan.ReturnedAt!.Value)}"
                : loan.IsOverdue ? "GECİKMİŞ" : "Aktif";

            report.AppendLine($"  {loan.UserFullName} -> {bookTitle} | Alınma: {FormatDate(loan.BorrowedAt)} | Son: {FormatDate(loan.DueDate)} | {status}");
        }

        if (data.Reservations.Count > 0)
        {
            report.AppendLine();
            report.AppendLine($"REZERVASYONLAR ({data.Reservations.Count(r => ReservationStatus.IsOpen(r.Status))} aktif)");
            report.AppendLine(new string('-', 60));
            foreach (var reservation in data.Reservations.Where(r => ReservationStatus.IsOpen(r.Status)).OrderBy(r => r.ReservedAt))
            {
                var bookTitle = data.Books.FirstOrDefault(b => b.Id == reservation.BookId)?.Title ?? "-";
                var queueInfo = reservation.Status == ReservationStatus.Waiting ? " (sırada)" : "";
                report.AppendLine($"  {reservation.UserFullName} -> {bookTitle} | {reservation.Status}{queueInfo} | {FormatDate(reservation.ReservedAt)}");
            }
        }

        return report.ToString();
    }

    public static void ExportToJson(LibraryData data, string jsonFilePath)
    {
        new JsonLibraryRepository(jsonFilePath).Save(data);
    }

    public static void ExportToText(LibraryData data, string textFilePath, LibraryStatistics? stats = null)
    {
        var directory = Path.GetDirectoryName(textFilePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(textFilePath, BuildReadableReport(data, stats));
    }

    public static void ExportReadableFiles(LibraryData data, string jsonFilePath, string textFilePath, LibraryStatistics? stats = null)
    {
        ExportToJson(data, jsonFilePath);
        ExportToText(data, textFilePath, stats);
    }

    private static string FormatDate(DateTime date)
        => date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
}
