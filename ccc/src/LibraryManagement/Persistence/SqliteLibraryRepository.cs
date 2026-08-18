using System.Globalization;
using LibraryManagement.Models;
using Microsoft.Data.Sqlite;

namespace LibraryManagement.Persistence;

public class SqliteLibraryRepository : ILibraryRepository
{
    private readonly string _filePath;

    public SqliteLibraryRepository(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    public string FilePath => _filePath;

    public bool Exists() => File.Exists(_filePath);

    public LibraryData Load()
    {
        EnsureSchema();

        if (!Exists())
        {
            return new LibraryData();
        }

        using var connection = OpenConnection();
        connection.Open();

        return new LibraryData
        {
            Users = ReadUsers(connection),
            Books = ReadBooks(connection),
            Loans = ReadLoans(connection),
            Reservations = ReadReservations(connection),
            Settings = ReadSettings(connection)
        };
    }

    public void Save(LibraryData data)
    {
        EnsureSchema();

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = OpenConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                DELETE FROM OduncKayitlari;
                DELETE FROM Rezervasyonlar;
                DELETE FROM Kitaplar;
                DELETE FROM Kullanicilar;
                DELETE FROM Ayarlar;
                """;
            command.ExecuteNonQuery();
        }

        foreach (var user in data.Users) InsertUser(connection, transaction, user);
        foreach (var book in data.Books) InsertBook(connection, transaction, book);
        foreach (var loan in data.Loans) InsertLoan(connection, transaction, loan);
        foreach (var reservation in data.Reservations) InsertReservation(connection, transaction, reservation);
        InsertSettings(connection, transaction, data.Settings);

        transaction.Commit();
    }

    private void EnsureSchema()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = OpenConnection();
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS Kullanicilar (
                    Id TEXT PRIMARY KEY,
                    AdSoyad TEXT NOT NULL,
                    Eposta TEXT NOT NULL UNIQUE,
                    KayitTarihi TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Kitaplar (
                    Id TEXT PRIMARY KEY,
                    Baslik TEXT NOT NULL,
                    Yazar TEXT NOT NULL,
                    Isbn TEXT NOT NULL UNIQUE,
                    ToplamKopya INTEGER NOT NULL DEFAULT 1,
                    MusaitKopya INTEGER NOT NULL DEFAULT 1,
                    MusaitMi INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS OduncKayitlari (
                    Id TEXT PRIMARY KEY,
                    KullaniciId TEXT NOT NULL,
                    AdSoyad TEXT NOT NULL,
                    KitapId TEXT NOT NULL,
                    AlinmaTarihi TEXT NOT NULL,
                    SonIadeTarihi TEXT NOT NULL,
                    IadeTarihi TEXT
                );

                CREATE TABLE IF NOT EXISTS Rezervasyonlar (
                    Id TEXT PRIMARY KEY,
                    KullaniciId TEXT NOT NULL,
                    AdSoyad TEXT NOT NULL,
                    KitapId TEXT NOT NULL,
                    RezervasyonTarihi TEXT NOT NULL,
                    Durum TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Ayarlar (
                    Anahtar TEXT PRIMARY KEY,
                    Deger TEXT NOT NULL
                );
                """;
            command.ExecuteNonQuery();
        }

        MigrateSchema(connection);
    }

    private static void MigrateSchema(SqliteConnection connection)
    {
        AddColumnIfMissing(connection, "Kullanicilar", "Telefon", "TEXT NOT NULL DEFAULT ''");
        AddColumnIfMissing(connection, "Kitaplar", "ToplamKopya", "INTEGER NOT NULL DEFAULT 1");
        AddColumnIfMissing(connection, "Kitaplar", "MusaitKopya", "INTEGER NOT NULL DEFAULT 1");
        AddColumnIfMissing(connection, "OduncKayitlari", "SonIadeTarihi", "TEXT");

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "UPDATE Kitaplar SET ToplamKopya = 1 WHERE ToplamKopya IS NULL OR ToplamKopya = 0";
            command.ExecuteNonQuery();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                UPDATE Kitaplar SET MusaitKopya = CASE WHEN MusaitMi = 1 THEN ToplamKopya ELSE 0 END
                WHERE MusaitKopya IS NULL;
                """;
            command.ExecuteNonQuery();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                UPDATE OduncKayitlari SET SonIadeTarihi = datetime(AlinmaTarihi, '+14 days')
                WHERE SonIadeTarihi IS NULL OR SonIadeTarihi = '';
                """;
            command.ExecuteNonQuery();
        }
    }

    private static void AddColumnIfMissing(SqliteConnection connection, string table, string column, string definition)
    {
        using var check = connection.CreateCommand();
        check.CommandText = $"PRAGMA table_info({table})";
        using var reader = check.ExecuteReader();
        while (reader.Read())
        {
            if (reader.GetString(1).Equals(column, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
        alter.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection() => new($"Data Source={_filePath}");

    private static List<User> ReadUsers(SqliteConnection connection)
    {
        var users = new List<User>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, AdSoyad, Eposta, KayitTarihi, Telefon FROM Kullanicilar ORDER BY AdSoyad";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = Guid.Parse(reader.GetString(0)),
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                CreatedAt = ParseDate(reader.GetString(3)),
                PhoneNumber = reader.FieldCount > 4 && !reader.IsDBNull(4) ? reader.GetString(4) : ""
            });
        }
        return users;
    }

    private static List<Book> ReadBooks(SqliteConnection connection)
    {
        var books = new List<Book>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Baslik, Yazar, Isbn, ToplamKopya, MusaitKopya FROM Kitaplar ORDER BY Baslik";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            books.Add(new Book
            {
                Id = Guid.Parse(reader.GetString(0)),
                Title = reader.GetString(1),
                Author = reader.GetString(2),
                Isbn = reader.GetString(3),
                TotalCopies = reader.GetInt32(4),
                AvailableCopies = reader.GetInt32(5)
            });
        }
        return books;
    }

    private static List<Loan> ReadLoans(SqliteConnection connection)
    {
        var loans = new List<Loan>();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, KullaniciId, AdSoyad, KitapId, AlinmaTarihi, SonIadeTarihi, IadeTarihi
            FROM OduncKayitlari ORDER BY AlinmaTarihi
            """;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            loans.Add(new Loan
            {
                Id = Guid.Parse(reader.GetString(0)),
                UserId = Guid.Parse(reader.GetString(1)),
                UserFullName = reader.GetString(2),
                BookId = Guid.Parse(reader.GetString(3)),
                BorrowedAt = ParseDate(reader.GetString(4)),
                DueDate = ParseDate(reader.GetString(5)),
                ReturnedAt = reader.IsDBNull(6) ? null : ParseDate(reader.GetString(6))
            });
        }
        return loans;
    }

    private static List<Reservation> ReadReservations(SqliteConnection connection)
    {
        var reservations = new List<Reservation>();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, KullaniciId, AdSoyad, KitapId, RezervasyonTarihi, Durum
            FROM Rezervasyonlar ORDER BY RezervasyonTarihi
            """;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            reservations.Add(new Reservation
            {
                Id = Guid.Parse(reader.GetString(0)),
                UserId = Guid.Parse(reader.GetString(1)),
                UserFullName = reader.GetString(2),
                BookId = Guid.Parse(reader.GetString(3)),
                ReservedAt = ParseDate(reader.GetString(4)),
                Status = reader.GetString(5)
            });
        }
        return reservations;
    }

    private static LibrarySettings ReadSettings(SqliteConnection connection)
    {
        var settings = new LibrarySettings();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Anahtar, Deger FROM Ayarlar";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var key = reader.GetString(0);
            var value = reader.GetString(1);
            if (key == "OduncSuresiGun" && int.TryParse(value, out var days))
            {
                settings.LoanDurationDays = days;
            }
            else if (key == "GunlukCezaTutari" && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var fine))
            {
                settings.FinePerDay = fine;
            }
        }
        return settings;
    }

    private static void InsertUser(SqliteConnection connection, SqliteTransaction transaction, User user)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO Kullanicilar (Id, AdSoyad, Eposta, KayitTarihi, Telefon) VALUES ($id, $adSoyad, $eposta, $kayitTarihi, $telefon)";
        command.Parameters.AddWithValue("$id", user.Id.ToString());
        command.Parameters.AddWithValue("$adSoyad", user.FullName);
        command.Parameters.AddWithValue("$eposta", user.Email);
        command.Parameters.AddWithValue("$kayitTarihi", FormatDate(user.CreatedAt));
        command.Parameters.AddWithValue("$telefon", user.PhoneNumber);
        command.ExecuteNonQuery();
    }

    private static void InsertBook(SqliteConnection connection, SqliteTransaction transaction, Book book)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO Kitaplar (Id, Baslik, Yazar, Isbn, ToplamKopya, MusaitKopya, MusaitMi)
            VALUES ($id, $baslik, $yazar, $isbn, $toplamKopya, $musaitKopya, $musaitMi)
            """;
        command.Parameters.AddWithValue("$id", book.Id.ToString());
        command.Parameters.AddWithValue("$baslik", book.Title);
        command.Parameters.AddWithValue("$yazar", book.Author);
        command.Parameters.AddWithValue("$isbn", book.Isbn);
        command.Parameters.AddWithValue("$toplamKopya", book.TotalCopies);
        command.Parameters.AddWithValue("$musaitKopya", book.AvailableCopies);
        command.Parameters.AddWithValue("$musaitMi", book.AvailableCopies > 0 ? 1 : 0);
        command.ExecuteNonQuery();
    }

    private static void InsertLoan(SqliteConnection connection, SqliteTransaction transaction, Loan loan)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO OduncKayitlari (Id, KullaniciId, AdSoyad, KitapId, AlinmaTarihi, SonIadeTarihi, IadeTarihi)
            VALUES ($id, $kullaniciId, $adSoyad, $kitapId, $alinmaTarihi, $sonIadeTarihi, $iadeTarihi)
            """;
        command.Parameters.AddWithValue("$id", loan.Id.ToString());
        command.Parameters.AddWithValue("$kullaniciId", loan.UserId.ToString());
        command.Parameters.AddWithValue("$adSoyad", loan.UserFullName);
        command.Parameters.AddWithValue("$kitapId", loan.BookId.ToString());
        command.Parameters.AddWithValue("$alinmaTarihi", FormatDate(loan.BorrowedAt));
        command.Parameters.AddWithValue("$sonIadeTarihi", FormatDate(loan.DueDate));
        command.Parameters.AddWithValue("$iadeTarihi", loan.ReturnedAt.HasValue ? FormatDate(loan.ReturnedAt.Value) : DBNull.Value);
        command.ExecuteNonQuery();
    }

    private static void InsertReservation(SqliteConnection connection, SqliteTransaction transaction, Reservation reservation)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO Rezervasyonlar (Id, KullaniciId, AdSoyad, KitapId, RezervasyonTarihi, Durum)
            VALUES ($id, $kullaniciId, $adSoyad, $kitapId, $rezervasyonTarihi, $durum)
            """;
        command.Parameters.AddWithValue("$id", reservation.Id.ToString());
        command.Parameters.AddWithValue("$kullaniciId", reservation.UserId.ToString());
        command.Parameters.AddWithValue("$adSoyad", reservation.UserFullName);
        command.Parameters.AddWithValue("$kitapId", reservation.BookId.ToString());
        command.Parameters.AddWithValue("$rezervasyonTarihi", FormatDate(reservation.ReservedAt));
        command.Parameters.AddWithValue("$durum", reservation.Status);
        command.ExecuteNonQuery();
    }

    private static void InsertSettings(SqliteConnection connection, SqliteTransaction transaction, LibrarySettings settings)
    {
        using var loanCommand = connection.CreateCommand();
        loanCommand.Transaction = transaction;
        loanCommand.CommandText = "INSERT INTO Ayarlar (Anahtar, Deger) VALUES ('OduncSuresiGun', $deger)";
        loanCommand.Parameters.AddWithValue("$deger", settings.LoanDurationDays.ToString(CultureInfo.InvariantCulture));
        loanCommand.ExecuteNonQuery();

        using var fineCommand = connection.CreateCommand();
        fineCommand.Transaction = transaction;
        fineCommand.CommandText = "INSERT INTO Ayarlar (Anahtar, Deger) VALUES ('GunlukCezaTutari', $deger)";
        fineCommand.Parameters.AddWithValue("$deger", settings.FinePerDay.ToString(CultureInfo.InvariantCulture));
        fineCommand.ExecuteNonQuery();
    }

    private static string FormatDate(DateTime date)
        => date.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTime ParseDate(string value)
        => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
