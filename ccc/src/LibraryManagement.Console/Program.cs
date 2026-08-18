using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Console;

public static class Program
{
    private static LibraryAppBootstrap Bootstrap = null!;

    public static void Main(string[] args)
    {
        Bootstrap = new LibraryAppBootstrap(LibraryPaths.ResolveDataDirectory());
        Bootstrap.Initialize();

        if (args.Contains("--seed-only"))
        {
            System.Console.WriteLine($"Toplam kitap: {Bootstrap.Library.GetAllBooks().Count}");
            return;
        }

        var running = true;
        while (running)
        {
            PrintMenu();
            var choice = System.Console.ReadLine()?.Trim();

            try
            {
                running = choice switch
                {
                    "1" => HandleAddUser(),
                    "2" => HandleAddBook(),
                    "3" => HandleBorrowBook(),
                    "4" => HandleReturnBook(),
                    "5" => HandleListUsers(),
                    "6" => HandleListBooks(),
                    "7" => HandleActiveLoans(),
                    "8" => HandleUserHistory(),
                    "9" => HandleBookHistory(),
                    "10" => HandleViewAndExportData(),
                    "11" => HandleOverdueLoans(),
                    "12" => HandleSearch(),
                    "13" => HandleStatistics(),
                    "14" => HandleEditBook(),
                    "15" => HandleDeleteBook(),
                    "16" => HandleReservation(),
                    "17" => HandleLoanDuration(),
                    "18" => HandleBackup(),
                    "19" => HandleEditUser(),
                    "0" => Exit(),
                    _ => InvalidChoice()
                };
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }

            if (running)
            {
                WaitForContinue();
            }
        }
    }

    private static LibraryService Library => Bootstrap.Library;

    private static void PrintMenu()
    {
        ClearConsole();
        System.Console.WriteLine("=== Kütüphane Yönetim Sistemi (SQLite) ===");
        System.Console.WriteLine($"Veritabanı: {Bootstrap.DatabasePath}");
        System.Console.WriteLine();
        System.Console.WriteLine("1) Kullanıcı ekle");
        System.Console.WriteLine("2) Kitap ekle");
        System.Console.WriteLine("3) Kitap ödünç ver");
        System.Console.WriteLine("4) Kitap iade al");
        System.Console.WriteLine("5) Kullanıcıları listele");
        System.Console.WriteLine("6) Kitapları listele");
        System.Console.WriteLine("7) İade edilmemiş kitapları göster");
        System.Console.WriteLine("8) Kullanıcının okuduğu kitapları göster");
        System.Console.WriteLine("9) Kitabı kimler okudu göster");
        System.Console.WriteLine("10) SQL kayıtlarını göster ve dosyalara aktar");
        System.Console.WriteLine("11) Geciken iadeleri göster");
        System.Console.WriteLine("12) Arama (kullanıcı/kitap)");
        System.Console.WriteLine("13) İstatistikleri göster");
        System.Console.WriteLine("14) Kitap düzenle");
        System.Console.WriteLine("15) Kitap sil");
        System.Console.WriteLine("16) Rezervasyon işlemleri");
        System.Console.WriteLine("17) Ödünç süresi ayarla");
        System.Console.WriteLine("18) Veritabanı yedeği al");
        System.Console.WriteLine("19) Kullanıcı düzenle");
        System.Console.WriteLine("0) Çıkış");
        System.Console.WriteLine();
        System.Console.Write("Seçiminiz: ");
    }

    private static bool HandleAddUser()
    {
        System.Console.Write("Ad Soyad: ");
        var name = ReadRequiredInput();
        System.Console.Write("E-posta: ");
        var email = ReadRequiredInput();
        System.Console.Write("Telefon (05XXXXXXXXX): ");
        var phone = ReadRequiredInput();

        var user = Library.AddUser(name, email, phone);
        AfterChange();
        PrintSuccess($"Kullanıcı eklendi: {user.FullName} ({user.Email}, {user.PhoneNumber})");
        return true;
    }

    private static bool HandleEditUser()
    {
        var users = Library.GetAllUsers();
        if (users.Count == 0) { PrintError("Henüz kullanıcı yok."); return true; }

        PrintUsers(users);
        var user = SelectUser(users);
        if (user is null) return true;

        System.Console.Write($"Yeni ad soyad [{user.FullName}]: ");
        var name = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(name)) name = user.FullName;

        System.Console.Write($"Yeni e-posta [{user.Email}]: ");
        var email = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(email)) email = user.Email;

        System.Console.Write($"Yeni telefon (05XXXXXXXXX) [{user.PhoneNumber}]: ");
        var phone = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(phone)) phone = user.PhoneNumber;

        Library.UpdateUser(user.Id, name, email, phone);
        AfterChange();
        PrintSuccess("Kullanıcı güncellendi.");
        return true;
    }

    private static bool HandleAddBook()
    {
        System.Console.Write("Kitap adı: ");
        var title = ReadRequiredInput();
        System.Console.Write("Yazar: ");
        var author = ReadRequiredInput();
        System.Console.Write("ISBN: ");
        var isbn = ReadRequiredInput();
        System.Console.Write("Kopya sayısı (varsayılan 1): ");
        var copiesInput = System.Console.ReadLine()?.Trim();
        var copies = int.TryParse(copiesInput, out var c) && c > 0 ? c : 1;

        var book = Library.AddBook(title, author, isbn, copies);
        AfterChange();
        PrintSuccess($"Kitap eklendi: {book.Title} - {book.Author} ({book.TotalCopies} kopya)");
        return true;
    }

    private static bool HandleBorrowBook()
    {
        var users = Library.GetAllUsers();
        if (users.Count == 0) { PrintError("Önce en az bir kullanıcı eklemelisiniz."); return true; }

        var books = Library.GetAllBooks().Where(b => b.IsAvailable).ToList();
        if (books.Count == 0) { PrintError("Ödünç verilebilecek müsait kitap yok."); return true; }

        PrintUsers(users);
        var user = SelectUser(users);
        if (user is null) return true;

        PrintAvailableBooks(books);
        var book = SelectBook(books);
        if (book is null) return true;

        var loan = Library.BorrowBook(user.Id, book.Id);
        AfterChange();
        PrintSuccess($"{book.Title} kitabı {user.FullName} kullanıcısına ödünç verildi.");
        System.Console.WriteLine($"Son iade tarihi: {FormatDate(loan.DueDate)}");
        return true;
    }

    private static bool HandleReturnBook()
    {
        var activeLoans = Library.GetActiveLoans();
        if (activeLoans.Count == 0) { PrintSuccess("İade edilmemiş kitap bulunmuyor."); return true; }

        PrintActiveLoans(activeLoans);
        System.Console.Write("İade edilecek kayıt numarası (GUID): ");
        if (!Guid.TryParse(System.Console.ReadLine()?.Trim(), out var loanId))
        {
            PrintError("Geçersiz kayıt numarası.");
            return true;
        }

        var loan = Library.ReturnBook(loanId);
        var record = Library.GetBookReadingHistory(loan.BookId).First(r => r.Loan.Id == loan.Id);
        AfterChange();
        PrintSuccess($"{record.Book.Title} kitabı {record.User.FullName} tarafından iade edildi.");
        return true;
    }

    private static bool HandleListUsers()
    {
        var users = Library.GetAllUsers();
        if (users.Count == 0) { PrintError("Henüz kullanıcı yok."); return true; }
        PrintUsers(users);
        return true;
    }

    private static bool HandleListBooks()
    {
        var books = Library.GetAllBooks();
        if (books.Count == 0) { PrintError("Henüz kitap yok."); return true; }
        PrintBooks(books);
        return true;
    }

    private static bool HandleActiveLoans()
    {
        var activeLoans = Library.GetActiveLoans();
        if (activeLoans.Count == 0) { PrintSuccess("Tüm kitaplar iade edilmiş."); return true; }
        PrintActiveLoans(activeLoans);
        return true;
    }

    private static bool HandleUserHistory()
    {
        var users = Library.GetAllUsers();
        if (users.Count == 0) { PrintError("Henüz kullanıcı yok."); return true; }

        PrintUsers(users);
        var user = SelectUser(users);
        if (user is null) return true;

        var history = Library.GetUserReadingHistory(user.Id);
        if (history.Count == 0) { PrintError($"{user.FullName} henüz kitap almamış."); return true; }

        System.Console.WriteLine();
        System.Console.WriteLine($"{user.FullName} okuma geçmişi:");
        PrintLoanHistory(history);
        return true;
    }

    private static bool HandleBookHistory()
    {
        var books = Library.GetAllBooks();
        if (books.Count == 0) { PrintError("Henüz kitap yok."); return true; }

        PrintBooks(books);
        var book = SelectBook(books);
        if (book is null) return true;

        var history = Library.GetBookReadingHistory(book.Id);
        if (history.Count == 0) { PrintError($"'{book.Title}' henüz kimse tarafından alınmamış."); return true; }

        System.Console.WriteLine();
        System.Console.WriteLine($"'{book.Title}' kitabını okuyanlar:");
        PrintLoanHistory(history);
        return true;
    }

    private static bool HandleViewAndExportData()
    {
        Bootstrap.ExportAllReports();
        var data = Library.GetAllData();
        var stats = Library.GetStatistics();
        System.Console.WriteLine();
        System.Console.WriteLine(DataViewer.BuildReadableReport(data, stats));
        PrintSuccess("Kayıtlar güncellendi:");
        System.Console.WriteLine($"  JSON  -> {Bootstrap.ExportJsonPath}");
        System.Console.WriteLine($"  TXT   -> {Bootstrap.ExportTextPath}");
        System.Console.WriteLine($"  Excel -> {Bootstrap.ExportExcelPath}");
        System.Console.WriteLine($"  HTML  -> {Bootstrap.ExportHtmlPath}");
        System.Console.WriteLine($"  PDF   -> {Bootstrap.ExportPdfPath}");
        return true;
    }

    private static bool HandleOverdueLoans()
    {
        var overdue = Library.GetOverdueLoans();
        if (overdue.Count == 0) { PrintSuccess("Geciken iade yok."); return true; }

        System.Console.WriteLine();
        System.Console.WriteLine("Geciken iadeler:");
        foreach (var record in overdue)
        {
            var days = (int)(DateTime.UtcNow - record.Loan.DueDate).TotalDays;
            System.Console.WriteLine($"  {record.User.FullName} -> {record.Book.Title} | Son: {FormatDate(record.Loan.DueDate)} | {days} gün gecikme");
        }
        return true;
    }

    private static bool HandleSearch()
    {
        System.Console.WriteLine("1) Kullanıcı ara  2) Kitap ara");
        System.Console.Write("Seçim: ");
        var type = System.Console.ReadLine()?.Trim();
        System.Console.Write("Aranacak kelime: ");
        var query = System.Console.ReadLine()?.Trim() ?? "";

        if (type == "2")
        {
            var books = Library.SearchBooks(query);
            if (books.Count == 0) { PrintError("Sonuç bulunamadı."); return true; }
            PrintBooks(books);
        }
        else
        {
            var users = Library.SearchUsers(query);
            if (users.Count == 0) { PrintError("Sonuç bulunamadı."); return true; }
            PrintUsers(users);

            System.Console.WriteLine();
            System.Console.Write("Detay için kullanıcı ID (boş=atla): ");
            var userInput = System.Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(userInput) && Guid.TryParse(userInput, out var userId))
            {
                try
                {
                    var user = Library.GetUserOrThrow(userId);
                    var activeLoans = Library.GetUserActiveLoans(userId);
                    System.Console.WriteLine();
                    System.Console.WriteLine($"{user.FullName} — aktif ödünç kitaplar:");
                    if (activeLoans.Count == 0)
                    {
                        PrintSuccess("Aktif ödünç kitap yok.");
                    }
                    else
                    {
                        foreach (var record in activeLoans)
                        {
                            var status = record.Loan.IsOverdue ? "GECİKMİŞ" : "Aktif";
                            System.Console.WriteLine(
                                $"  {record.Book.Title} - {record.Book.Author} | Alınma: {FormatDate(record.Loan.BorrowedAt)} | Son: {FormatDate(record.Loan.DueDate)} | {status}");
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    PrintError("Kullanıcı bulunamadı.");
                }
            }
        }
        return true;
    }

    private static bool HandleStatistics()
    {
        var stats = Library.GetStatistics();
        System.Console.WriteLine();
        System.Console.WriteLine("=== İSTATİSTİKLER ===");
        System.Console.WriteLine($"Kullanıcı: {stats.UserCount} | Kitap: {stats.BookCount} | Kopya: {stats.TotalCopies}");
        System.Console.WriteLine($"Aktif ödünç: {stats.ActiveLoanCount} | Geciken: {stats.OverdueCount} | Rezervasyon: {stats.ReservationCount} (Hazır: {stats.ReadyReservationCount})");
        System.Console.WriteLine($"Ödünç süresi: {stats.LoanDurationDays} gün");
        System.Console.WriteLine();
        System.Console.WriteLine("En çok okunan kitaplar:");
        foreach (var book in stats.MostBorrowedBooks)
            System.Console.WriteLine($"  {book.Title} - {book.Author}: {book.BorrowCount}");
        System.Console.WriteLine();
        System.Console.WriteLine("En aktif kullanıcılar:");
        foreach (var user in stats.MostActiveUsers)
            System.Console.WriteLine($"  {user.FullName}: {user.BorrowCount}");
        return true;
    }

    private static bool HandleEditBook()
    {
        var books = Library.GetAllBooks();
        if (books.Count == 0) { PrintError("Henüz kitap yok."); return true; }

        PrintBooks(books);
        var book = SelectBook(books);
        if (book is null) return true;

        System.Console.Write($"Yeni başlık [{book.Title}]: ");
        var title = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(title)) title = book.Title;

        System.Console.Write($"Yeni yazar [{book.Author}]: ");
        var author = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(author)) author = book.Author;

        System.Console.Write($"Yeni ISBN [{book.Isbn}]: ");
        var isbn = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(isbn)) isbn = book.Isbn;

        System.Console.Write($"Kopya sayısı [{book.TotalCopies}]: ");
        var copiesInput = System.Console.ReadLine()?.Trim();
        var copies = int.TryParse(copiesInput, out var c) && c > 0 ? c : book.TotalCopies;

        Library.UpdateBook(book.Id, title, author, isbn, copies);
        AfterChange();
        PrintSuccess("Kitap güncellendi.");
        return true;
    }

    private static bool HandleDeleteBook()
    {
        var books = Library.GetAllBooks();
        if (books.Count == 0) { PrintError("Henüz kitap yok."); return true; }

        PrintBooks(books);
        var book = SelectBook(books);
        if (book is null) return true;

        System.Console.Write("Silmek istediğinize emin misiniz? (e/h): ");
        if (System.Console.ReadLine()?.Trim().ToLowerInvariant() != "e") return true;

        Library.DeleteBook(book.Id);
        AfterChange();
        PrintSuccess("Kitap silindi.");
        return true;
    }

    private static bool HandleReservation()
    {
        System.Console.WriteLine("1) Rezervasyon ekle  2) Rezervasyon iptal  3) Aktif rezervasyonları listele");
        System.Console.Write("Seçim: ");
        var choice = System.Console.ReadLine()?.Trim();

        if (choice == "3")
        {
            var reservations = Library.GetActiveReservationRecords();
            if (reservations.Count == 0) { PrintSuccess("Aktif rezervasyon yok."); return true; }
            foreach (var record in reservations)
            {
                var queue = record.Reservation.Status == ReservationStatus.Ready
                    ? "Hazır - ödünç alabilir"
                    : $"Sıra: {record.QueuePosition}";
                System.Console.WriteLine($"  [{record.Reservation.Id}] {record.Reservation.UserFullName} -> {record.Book.Title} | {record.Reservation.Status} | {queue}");
            }
            return true;
        }

        if (choice == "2")
        {
            System.Console.Write("Rezervasyon ID (GUID): ");
            if (!Guid.TryParse(System.Console.ReadLine()?.Trim(), out var id)) { PrintError("Geçersiz ID."); return true; }
            Library.CancelReservation(id);
            AfterChange();
            PrintSuccess("Rezervasyon iptal edildi.");
            return true;
        }

        var users = Library.GetAllUsers();
        var books = Library.GetAllBooks();
        if (users.Count == 0 || books.Count == 0) { PrintError("Kullanıcı ve kitap gerekli."); return true; }

        PrintUsers(users);
        var user = SelectUser(users);
        if (user is null) return true;

        PrintBooks(books);
        var book = SelectBook(books);
        if (book is null) return true;

        var reservation = Library.AddReservation(user.Id, book.Id);
        AfterChange();
        var position = Library.GetReservationQueuePosition(reservation);
        PrintSuccess(position > 0 ? $"Rezervasyon eklendi. Sıra: {position}." : "Rezervasyon eklendi.");
        return true;
    }

    private static bool HandleLoanDuration()
    {
        System.Console.Write($"Ödünç süresi (gün) [mevcut: {Library.LoanDurationDays}]: ");
        var input = System.Console.ReadLine()?.Trim();
        if (!int.TryParse(input, out var days) || days < 1 || days > 365)
        {
            PrintError("1-365 arasında geçerli bir sayı girin.");
            return true;
        }

        Library.SetLoanDurationDays(days);
        AfterChange();
        PrintSuccess($"Ödünç süresi {days} gün olarak ayarlandı.");
        return true;
    }

    private static bool HandleBackup()
    {
        var path = Bootstrap.CreateBackup();
        PrintSuccess($"Yedek alındı: {path}");
        return true;
    }

    private static void AfterChange() => Bootstrap.ExportAllReports();

    private static bool Exit()
    {
        System.Console.WriteLine("Çıkılıyor...");
        return false;
    }

    private static bool InvalidChoice()
    {
        PrintError("Geçersiz seçim.");
        return true;
    }

    private static string ReadRequiredInput()
    {
        while (true)
        {
            var value = System.Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value)) return value;
            System.Console.Write("Bu alan zorunludur. Tekrar girin: ");
        }
    }

    private static void PrintUsers(IReadOnlyList<User> users)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Kullanıcılar:");
        foreach (var user in users)
            System.Console.WriteLine($"  [{user.Id}] {user.FullName} - {user.Email} - {user.PhoneNumber}");
    }

    private static void PrintBooks(IReadOnlyList<Book> books)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Kitaplar:");
        foreach (var book in books)
            System.Console.WriteLine($"  [{book.Id}] {book.Title} - {book.Author} | Kopya: {book.AvailableCopies}/{book.TotalCopies}");
    }

    private static void PrintAvailableBooks(IReadOnlyList<Book> books)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Müsait kitaplar:");
        foreach (var book in books)
            System.Console.WriteLine($"  [{book.Id}] {book.Title} - {book.Author}");
    }

    private static void PrintActiveLoans(IReadOnlyList<LoanRecord> loans)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("İade edilmemiş kitaplar:");
        foreach (var record in loans)
        {
            var overdue = record.Loan.IsOverdue ? " [GECİKMİŞ]" : "";
            System.Console.WriteLine($"  Kayıt: {record.Loan.Id} | {record.User.FullName} -> {record.Book.Title} | Alınma: {FormatDate(record.Loan.BorrowedAt)} | Son: {FormatDate(record.Loan.DueDate)}{overdue}");
        }
    }

    private static void PrintLoanHistory(IReadOnlyList<LoanRecord> history)
    {
        foreach (var record in history)
        {
            var status = record.Loan.IsReturned
                ? $"İade: {FormatDate(record.Loan.ReturnedAt!.Value)}"
                : "Henüz iade edilmedi";
            System.Console.WriteLine($"  {record.User.FullName} | {record.Book.Title} | Alınma: {FormatDate(record.Loan.BorrowedAt)} | {status}");
        }
    }

    private static User? SelectUser(IReadOnlyList<User> users)
    {
        System.Console.Write("Kullanıcı ID (GUID): ");
        if (!Guid.TryParse(System.Console.ReadLine()?.Trim(), out var userId)) { PrintError("Geçersiz kullanıcı ID."); return null; }
        try { return Library.GetUserOrThrow(userId); }
        catch (InvalidOperationException) { PrintError("Kullanıcı bulunamadı."); return null; }
    }

    private static Book? SelectBook(IReadOnlyList<Book> books)
    {
        System.Console.Write("Kitap ID (GUID): ");
        if (!Guid.TryParse(System.Console.ReadLine()?.Trim(), out var bookId)) { PrintError("Geçersiz kitap ID."); return null; }
        try { return Library.GetBookOrThrow(bookId); }
        catch (InvalidOperationException) { PrintError("Kitap bulunamadı."); return null; }
    }

    private static string FormatDate(DateTime date) => date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    private static void PrintSuccess(string message) => System.Console.WriteLine($"[OK] {message}");
    private static void PrintError(string message) => System.Console.WriteLine($"[HATA] {message}");

    private static void ClearConsole()
    {
        if (!System.Console.IsOutputRedirected)
        {
            try { System.Console.Clear(); }
            catch (IOException) { }
        }
    }

    private static void WaitForContinue()
    {
        System.Console.WriteLine();
        if (System.Console.IsInputRedirected) return;
        System.Console.WriteLine("Devam etmek için bir tuşa basın...");
        System.Console.ReadKey(intercept: true);
    }
}
