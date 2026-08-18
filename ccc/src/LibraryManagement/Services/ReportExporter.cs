using ClosedXML.Excel;
using LibraryManagement.Models;
using LibraryManagement.Persistence;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LibraryManagement.Services;

public static class ReportExporter
{
    static ReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }
    public static void ExportToExcel(LibraryData data, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        using var workbook = new XLWorkbook();

        var usersSheet = workbook.Worksheets.Add("Kullanicilar");
        usersSheet.Cell(1, 1).Value = "Ad Soyad";
        usersSheet.Cell(1, 2).Value = "E-posta";
        usersSheet.Cell(1, 3).Value = "Telefon";
        usersSheet.Cell(1, 4).Value = "Kayıt Tarihi";
        var row = 2;
        foreach (var user in data.Users.OrderBy(u => u.FullName))
        {
            usersSheet.Cell(row, 1).Value = user.FullName;
            usersSheet.Cell(row, 2).Value = user.Email;
            usersSheet.Cell(row, 3).Value = user.PhoneNumber;
            usersSheet.Cell(row, 4).Value = user.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy");
            row++;
        }

        var booksSheet = workbook.Worksheets.Add("Kitaplar");
        booksSheet.Cell(1, 1).Value = "Başlık";
        booksSheet.Cell(1, 2).Value = "Yazar";
        booksSheet.Cell(1, 3).Value = "ISBN";
        booksSheet.Cell(1, 4).Value = "Toplam Kopya";
        booksSheet.Cell(1, 5).Value = "Müsait Kopya";
        row = 2;
        foreach (var book in data.Books.OrderBy(b => b.Title))
        {
            booksSheet.Cell(row, 1).Value = book.Title;
            booksSheet.Cell(row, 2).Value = book.Author;
            booksSheet.Cell(row, 3).Value = book.Isbn;
            booksSheet.Cell(row, 4).Value = book.TotalCopies;
            booksSheet.Cell(row, 5).Value = book.AvailableCopies;
            row++;
        }

        var loansSheet = workbook.Worksheets.Add("OduncKayitlari");
        loansSheet.Cell(1, 1).Value = "Kullanıcı";
        loansSheet.Cell(1, 2).Value = "Kitap ID";
        loansSheet.Cell(1, 3).Value = "Alınma";
        loansSheet.Cell(1, 4).Value = "Son İade";
        loansSheet.Cell(1, 5).Value = "İade";
        loansSheet.Cell(1, 6).Value = "Gecikmiş";
        row = 2;
        foreach (var loan in data.Loans.OrderByDescending(l => l.BorrowedAt))
        {
            var bookTitle = data.Books.FirstOrDefault(b => b.Id == loan.BookId)?.Title ?? loan.BookId.ToString();
            loansSheet.Cell(row, 1).Value = loan.UserFullName;
            loansSheet.Cell(row, 2).Value = bookTitle;
            loansSheet.Cell(row, 3).Value = loan.BorrowedAt.ToLocalTime().ToString("dd.MM.yyyy");
            loansSheet.Cell(row, 4).Value = loan.DueDate.ToLocalTime().ToString("dd.MM.yyyy");
            loansSheet.Cell(row, 5).Value = loan.ReturnedAt?.ToLocalTime().ToString("dd.MM.yyyy") ?? "-";
            loansSheet.Cell(row, 6).Value = loan.IsOverdue ? "Evet" : "Hayır";
            row++;
        }

        workbook.SaveAs(filePath);
    }

    public static void ExportToPdfHtml(LibraryData data, LibraryStatistics stats, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        var bookRows = string.Join("", stats.MostBorrowedBooks.Select(b =>
            $"<tr><td>{b.Title}</td><td>{b.Author}</td><td>{b.BorrowCount}</td></tr>"));

        var overdueRows = string.Join("", data.Loans.Where(l => l.IsOverdue).Select(l =>
        {
            var title = data.Books.FirstOrDefault(b => b.Id == l.BookId)?.Title ?? "-";
            return $"<tr><td>{l.UserFullName}</td><td>{title}</td><td>{l.DueDate.ToLocalTime():dd.MM.yyyy}</td></tr>";
        }));

        var html = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Kütüphane Raporu</title>" +
            "<style>body{font-family:Arial,sans-serif;margin:24px}h1,h2{color:#1a365d}" +
            "table{border-collapse:collapse;width:100%;margin-bottom:24px}" +
            "th,td{border:1px solid #ccc;padding:8px;text-align:left}th{background:#edf2f7}" +
            ".stats{display:flex;gap:16px;flex-wrap:wrap}.stat{background:#f7fafc;padding:12px 16px;border-radius:8px;border:1px solid #e2e8f0}</style></head><body>" +
            "<h1>Kütüphane Yönetim Raporu</h1>" +
            $"<p>Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}</p>" +
            "<h2>İstatistikler</h2><div class=\"stats\">" +
            $"<div class=\"stat\">Kullanıcı: {stats.UserCount}</div>" +
            $"<div class=\"stat\">Kitap: {stats.BookCount}</div>" +
            $"<div class=\"stat\">Toplam Kopya: {stats.TotalCopies}</div>" +
            $"<div class=\"stat\">Aktif Ödünç: {stats.ActiveLoanCount}</div>" +
            $"<div class=\"stat\">Geciken: {stats.OverdueCount}</div>" +
            $"<div class=\"stat\">Rezervasyon: {stats.ReservationCount} (Hazır: {stats.ReadyReservationCount})</div></div>" +
            "<h2>En Çok Okunan Kitaplar</h2><table><tr><th>Kitap</th><th>Yazar</th><th>Ödünç Sayısı</th></tr>" +
            bookRows + "</table>" +
            "<h2>Geciken İadeler</h2><table><tr><th>Kullanıcı</th><th>Kitap</th><th>Son İade</th></tr>" +
            overdueRows + "</table>" +
            "<p><em>PDF olarak kaydetmek için tarayıcıda Ctrl+P → PDF olarak kaydet</em></p></body></html>";

        File.WriteAllText(filePath, html);
    }

    public static void ExportToPdf(LibraryData data, LibraryStatistics stats, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text("Kütüphane Yönetim Raporu").Bold().FontSize(18);
                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text($"Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}");

                    column.Item().Text("İstatistikler").Bold().FontSize(14);
                    column.Item().Text($"Kullanıcı: {stats.UserCount} | Kitap: {stats.BookCount} | Kopya: {stats.TotalCopies}");
                    column.Item().Text($"Aktif ödünç: {stats.ActiveLoanCount} | Geciken: {stats.OverdueCount}");
                    column.Item().Text($"Rezervasyon: {stats.ReservationCount} (Hazır: {stats.ReadyReservationCount})");

                    column.Item().PaddingTop(10).Text("En Çok Okunan Kitaplar").Bold().FontSize(14);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Kitap").Bold();
                            header.Cell().Text("Yazar").Bold();
                            header.Cell().Text("Sayı").Bold();
                        });
                        foreach (var book in stats.MostBorrowedBooks)
                        {
                            table.Cell().Text(book.Title);
                            table.Cell().Text(book.Author);
                            table.Cell().Text(book.BorrowCount.ToString());
                        }
                    });

                    column.Item().PaddingTop(10).Text("Geciken İadeler").Bold().FontSize(14);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Kullanıcı").Bold();
                            header.Cell().Text("Kitap").Bold();
                            header.Cell().Text("Son İade").Bold();
                        });
                        foreach (var loan in data.Loans.Where(l => l.IsOverdue))
                        {
                            var title = data.Books.FirstOrDefault(b => b.Id == loan.BookId)?.Title ?? "-";
                            table.Cell().Text(loan.UserFullName);
                            table.Cell().Text(title);
                            table.Cell().Text(loan.DueDate.ToLocalTime().ToString("dd.MM.yyyy"));
                        }
                    });

                    var openReservations = data.Reservations.Where(r => ReservationStatus.IsOpen(r.Status)).ToList();
                    if (openReservations.Count > 0)
                    {
                        column.Item().PaddingTop(10).Text("Rezervasyonlar").Bold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Kullanıcı").Bold();
                                header.Cell().Text("Kitap").Bold();
                                header.Cell().Text("Durum").Bold();
                            });
                            foreach (var reservation in openReservations)
                            {
                                var title = data.Books.FirstOrDefault(b => b.Id == reservation.BookId)?.Title ?? "-";
                                table.Cell().Text(reservation.UserFullName);
                                table.Cell().Text(title);
                                table.Cell().Text(reservation.Status);
                            }
                        });
                    }
                });
            });
        }).GeneratePdf(filePath);
    }
}
