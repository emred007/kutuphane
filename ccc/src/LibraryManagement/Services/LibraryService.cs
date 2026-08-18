using LibraryManagement.Models;
using LibraryManagement.Persistence;

namespace LibraryManagement.Services;

public partial class LibraryService
{
    private readonly ILibraryRepository _repository;
    private readonly List<User> _users = new();
    private readonly List<Book> _books = new();
    private readonly List<Loan> _loans = new();
    private readonly List<Reservation> _reservations = new();
    private LibrarySettings _settings = new();

    public LibraryService(ILibraryRepository repository)
    {
        _repository = repository;
        InitializeFromRepository();
    }

    public int LoanDurationDays
    {
        get
        {
            EnsureFreshData();
            return _settings.LoanDurationDays;
        }
    }

    public decimal FinePerDay
    {
        get
        {
            EnsureFreshData();
            return _settings.FinePerDay;
        }
    }

    public bool HasData
    {
        get
        {
            EnsureFreshData();
            return _users.Count > 0 || _books.Count > 0 || _loans.Count > 0;
        }
    }

    public LibraryData GetAllData()
    {
        EnsureFreshData();
        return new()
        {
            Users = _users.ToList(),
            Books = _books.ToList(),
            Loans = _loans.ToList(),
            Reservations = _reservations.ToList(),
            Settings = _settings
        };
    }

    public void SetLoanDurationDays(int days)
    {
        EnsureFreshData();
        if (days is < 1 or > 365)
        {
            throw new InvalidOperationException("Ödünç süresi 1-365 gün arasında olmalıdır.");
        }

        _settings.LoanDurationDays = days;
        Persist();
    }

    public void SetFinePerDay(decimal amount)
    {
        EnsureFreshData();
        if (amount < 0 || amount > 1000)
        {
            throw new InvalidOperationException("Günlük ceza tutarı 0-1000 TL arasında olmalıdır.");
        }

        _settings.FinePerDay = amount;
        Persist();
    }

    public User AddUser(string fullName, string email, string phoneNumber)
    {
        EnsureFreshData();
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedPhone = PhoneNumberValidator.NormalizeAndValidate(phoneNumber);

        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"'{email}' e-posta adresine sahip kullanıcı zaten mevcut.");
        }

        if (_users.Any(u => u.PhoneNumber == normalizedPhone))
        {
            throw new InvalidOperationException($"'{normalizedPhone}' telefon numarası zaten kayıtlı.");
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            PhoneNumber = normalizedPhone
        };
        _users.Add(user);
        Persist();
        return user;
    }

    public User UpdateUser(Guid userId, string fullName, string email, string phoneNumber)
    {
        EnsureFreshData();
        var user = GetUserOrThrow(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedPhone = PhoneNumberValidator.NormalizeAndValidate(phoneNumber);

        if (_users.Any(u => u.Id != userId && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"'{email}' e-posta adresi başka kullanıcıda kayıtlı.");
        }

        if (_users.Any(u => u.Id != userId && u.PhoneNumber == normalizedPhone))
        {
            throw new InvalidOperationException($"'{normalizedPhone}' telefon numarası başka kullanıcıda kayıtlı.");
        }

        user.FullName = fullName.Trim();
        user.Email = email.Trim();
        user.PhoneNumber = normalizedPhone;
        SyncUserNamesInRecords(user);
        Persist();
        return user;
    }

    public Book AddBook(string title, string author, string isbn, int totalCopies = 1)
    {
        EnsureFreshData();
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(author);
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);

        if (totalCopies < 1)
        {
            throw new InvalidOperationException("Kopya sayısı en az 1 olmalıdır.");
        }

        if (_books.Any(b => b.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"'{isbn}' ISBN numarası zaten mevcut.");
        }

        var book = new Book
        {
            Title = title.Trim(),
            Author = author.Trim(),
            Isbn = isbn.Trim(),
            TotalCopies = totalCopies,
            AvailableCopies = totalCopies
        };

        _books.Add(book);
        Persist();
        return book;
    }

    public Book UpdateBook(Guid bookId, string title, string author, string isbn, int totalCopies)
    {
        EnsureFreshData();
        var book = GetBookOrThrow(bookId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(author);
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);

        if (totalCopies < 1)
        {
            throw new InvalidOperationException("Kopya sayısı en az 1 olmalıdır.");
        }

        if (_books.Any(b => b.Id != bookId && b.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"'{isbn}' ISBN başka kitapta kullanılıyor.");
        }

        var borrowed = ActiveLoanCount(bookId);
        if (totalCopies < borrowed)
        {
            throw new InvalidOperationException($"Toplam kopya ödünçteki sayıdan ({borrowed}) az olamaz.");
        }

        book.Title = title.Trim();
        book.Author = author.Trim();
        book.Isbn = isbn.Trim();
        book.TotalCopies = totalCopies;
        book.AvailableCopies = totalCopies - borrowed;
        Persist();
        return book;
    }

    public void DeleteBook(Guid bookId)
    {
        EnsureFreshData();
        var book = GetBookOrThrow(bookId);

        if (ActiveLoanCount(bookId) > 0)
        {
            throw new InvalidOperationException("Ödünçte olan kitap silinemez.");
        }

        if (_reservations.Any(r => r.BookId == bookId && ReservationStatus.IsOpen(r.Status)))
        {
            throw new InvalidOperationException("Bekleyen rezervasyonu olan kitap silinemez.");
        }

        _books.Remove(book);
        _reservations.RemoveAll(r => r.BookId == bookId);
        Persist();
    }

    public Loan BorrowBook(Guid userId, Guid bookId, DateTime? borrowedAt = null)
    {
        EnsureFreshData();
        var user = GetUserOrThrow(userId);
        var book = GetBookOrThrow(bookId);

        if (book.AvailableCopies <= 0)
        {
            throw new InvalidOperationException($"'{book.Title}' için müsait kopya yok.");
        }

        var readyReservations = GetReadyReservationsForBook(bookId);
        if (readyReservations.Count > 0 && readyReservations.All(r => r.UserId != userId))
        {
            var next = readyReservations[0];
            throw new InvalidOperationException(
                $"Bu kitap rezervasyon sırasında. Şu an sıra: {next.UserFullName} (Hazır).");
        }

        var borrowedDate = borrowedAt ?? DateTime.UtcNow;
        var loan = new Loan
        {
            UserId = user.Id,
            UserFullName = user.FullName,
            BookId = book.Id,
            BorrowedAt = borrowedDate,
            DueDate = borrowedDate.AddDays(_settings.LoanDurationDays)
        };

        book.AvailableCopies--;
        _loans.Add(loan);
        CompleteReservationIfExists(user.Id, book.Id);
        Persist();
        return loan;
    }

    public Loan ReturnBook(Guid loanId)
    {
        EnsureFreshData();
        var loan = _loans.FirstOrDefault(l => l.Id == loanId)
            ?? throw new InvalidOperationException("Ödünç kaydı bulunamadı.");

        if (loan.IsReturned)
        {
            throw new InvalidOperationException("Bu kitap zaten iade edilmiş.");
        }

        var book = GetBookOrThrow(loan.BookId);
        loan.ReturnedAt = DateTime.UtcNow;

        if (book.AvailableCopies < book.TotalCopies)
        {
            book.AvailableCopies++;
        }

        PromoteEligibleReservations(book.Id);
        Persist();
        return loan;
    }

    public Reservation AddReservation(Guid userId, Guid bookId)
    {
        EnsureFreshData();
        var user = GetUserOrThrow(userId);
        var book = GetBookOrThrow(bookId);

        if (book.AvailableCopies > 0)
        {
            throw new InvalidOperationException("Müsait kopya varken rezervasyon yapılamaz.");
        }

        if (_reservations.Any(r => r.UserId == userId && r.BookId == bookId && ReservationStatus.IsOpen(r.Status)))
        {
            throw new InvalidOperationException("Bu kitap için zaten aktif rezervasyonunuz var.");
        }

        var reservation = new Reservation
        {
            UserId = user.Id,
            UserFullName = user.FullName,
            BookId = book.Id
        };

        _reservations.Add(reservation);
        Persist();
        return reservation;
    }

    public void CancelReservation(Guid reservationId)
    {
        EnsureFreshData();
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId)
            ?? throw new InvalidOperationException("Rezervasyon bulunamadı.");

        if (!ReservationStatus.IsOpen(reservation.Status))
        {
            throw new InvalidOperationException("Sadece bekleyen veya hazır rezervasyonlar iptal edilebilir.");
        }

        var bookId = reservation.BookId;
        var wasReady = reservation.Status == ReservationStatus.Ready;
        reservation.Status = ReservationStatus.Cancelled;

        if (wasReady)
        {
            PromoteEligibleReservations(bookId);
        }

        Persist();
    }

    public IReadOnlyList<LoanRecord> GetOverdueLoans()
    {
        EnsureFreshData();
        return _loans.Where(l => l.IsOverdue).Select(ToLoanRecord).OrderBy(r => r.Loan.DueDate).ToList();
    }

    public IReadOnlyList<OverdueFineRecord> GetOverdueFines()
    {
        EnsureFreshData();
        return GetOverdueLoans()
            .Select(record => OverdueFineCalculator.Calculate(record, _settings.FinePerDay))
            .OrderByDescending(f => f.FineAmount)
            .ToList();
    }

    public decimal GetTotalOverdueFineAmount()
        => GetOverdueFines().Sum(f => f.FineAmount);

    public IReadOnlyList<LoanRecord> GetActiveLoans()
    {
        EnsureFreshData();
        return _loans.Where(l => !l.IsReturned).Select(ToLoanRecord).OrderBy(r => r.Loan.BorrowedAt).ToList();
    }

    public IReadOnlyList<LoanRecord> GetUserActiveLoans(Guid userId)
    {
        EnsureFreshData();
        _ = GetUserOrThrow(userId);
        return _loans
            .Where(l => l.UserId == userId && !l.IsReturned)
            .Select(ToLoanRecord)
            .OrderBy(r => r.Loan.BorrowedAt)
            .ToList();
    }

    public IReadOnlyList<LoanRecord> GetUnreturnedBooks() => GetActiveLoans();

    public IReadOnlyList<LoanRecord> GetUserReadingHistory(Guid userId)
    {
        EnsureFreshData();
        _ = GetUserOrThrow(userId);
        return _loans.Where(l => l.UserId == userId).Select(ToLoanRecord).OrderByDescending(r => r.Loan.BorrowedAt).ToList();
    }

    public IReadOnlyList<LoanRecord> GetBookReadingHistory(Guid bookId)
    {
        EnsureFreshData();
        _ = GetBookOrThrow(bookId);
        return _loans.Where(l => l.BookId == bookId).Select(ToLoanRecord).OrderByDescending(r => r.Loan.BorrowedAt).ToList();
    }

    public IReadOnlyList<User> SearchUsers(string query)
    {
        EnsureFreshData();
        if (string.IsNullOrWhiteSpace(query)) return GetAllUsers();
        query = query.Trim();
        return _users.Where(u =>
            u.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            u.PhoneNumber.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(u => u.FullName).ToList();
    }

    public IReadOnlyList<Book> SearchBooks(string query)
    {
        EnsureFreshData();
        if (string.IsNullOrWhiteSpace(query)) return GetAllBooks();
        query = query.Trim();
        return _books.Where(b =>
            b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            b.Isbn.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(b => b.Title).ToList();
    }

    public LibraryStatistics GetStatistics()
    {
        EnsureFreshData();
        return new()
    {
        UserCount = _users.Count,
        BookCount = _books.Count,
        TotalCopies = _books.Sum(b => b.TotalCopies),
        AvailableCopies = _books.Sum(b => b.AvailableCopies),
        ActiveLoanCount = _loans.Count(l => !l.IsReturned),
        OverdueCount = _loans.Count(l => l.IsOverdue),
        ReservationCount = _reservations.Count(r => ReservationStatus.IsOpen(r.Status)),
        ReadyReservationCount = _reservations.Count(r => r.Status == ReservationStatus.Ready),
        LoanDurationDays = _settings.LoanDurationDays,
        MostBorrowedBooks = _loans.GroupBy(l => l.BookId).Select(g =>
        {
            var book = GetBookOrThrow(g.Key);
            return new BookStatistic { Title = book.Title, Author = book.Author, BorrowCount = g.Count() };
        }).OrderByDescending(x => x.BorrowCount).Take(5).ToList(),
        MostActiveUsers = _loans.GroupBy(l => l.UserId).Select(g =>
        {
            var user = GetUserOrThrow(g.Key);
            return new UserStatistic { FullName = user.FullName, Email = user.Email, BorrowCount = g.Count() };
        }).OrderByDescending(x => x.BorrowCount).Take(5).ToList()
        };
    }

    public IReadOnlyList<Reservation> GetActiveReservations()
    {
        EnsureFreshData();
        return _reservations.Where(r => ReservationStatus.IsOpen(r.Status)).OrderBy(r => r.ReservedAt).ToList();
    }

    public IReadOnlyList<ReservationRecord> GetActiveReservationRecords()
    {
        EnsureFreshData();
        return GetActiveReservations()
            .Select(r => new ReservationRecord
            {
                Reservation = r,
                Book = GetBookOrThrow(r.BookId),
                QueuePosition = GetReservationQueuePosition(r)
            })
            .ToList();
    }

    public IReadOnlyList<User> GetAllUsers()
    {
        EnsureFreshData();
        return _users.OrderBy(u => u.FullName).ToList();
    }

    public IReadOnlyList<Book> GetAllBooks()
    {
        EnsureFreshData();
        return _books.OrderBy(b => b.Title).ToList();
    }

    public User? FindUserByEmail(string email)
    {
        EnsureFreshData();
        return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public User GetUserOrThrow(Guid userId)
    {
        EnsureFreshData();
        return _users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");
    }

    public Book GetBookOrThrow(Guid bookId)
    {
        EnsureFreshData();
        return _books.FirstOrDefault(b => b.Id == bookId) ?? throw new InvalidOperationException("Kitap bulunamadı.");
    }

    private int ActiveLoanCount(Guid bookId) => _loans.Count(l => l.BookId == bookId && !l.IsReturned);

    public int GetReservationQueuePosition(Reservation reservation)
    {
        EnsureFreshData();
        if (reservation.Status == ReservationStatus.Ready)
        {
            return 0;
        }

        if (reservation.Status != ReservationStatus.Waiting)
        {
            return 0;
        }

        var waiting = _reservations
            .Where(r => r.BookId == reservation.BookId && r.Status == ReservationStatus.Waiting)
            .OrderBy(r => r.ReservedAt)
            .ToList();

        var index = waiting.FindIndex(r => r.Id == reservation.Id);
        return index >= 0 ? index + 1 : 0;
    }

    private IReadOnlyList<Reservation> GetReadyReservationsForBook(Guid bookId)
        => _reservations
            .Where(r => r.BookId == bookId && r.Status == ReservationStatus.Ready)
            .OrderBy(r => r.ReservedAt)
            .ToList();

    private void CompleteReservationIfExists(Guid userId, Guid bookId)
    {
        var reservation = _reservations
            .Where(r => r.UserId == userId && r.BookId == bookId && ReservationStatus.IsOpen(r.Status))
            .OrderBy(r => r.ReservedAt).FirstOrDefault();

        if (reservation is not null)
        {
            reservation.Status = ReservationStatus.Completed;
            PromoteEligibleReservations(bookId);
        }
    }

    private void PromoteEligibleReservations(Guid? bookId = null)
    {
        var books = bookId is Guid id
            ? new[] { GetBookOrThrow(id) }
            : _books.ToArray();

        foreach (var book in books)
        {
            var readyCount = _reservations.Count(r => r.BookId == book.Id && r.Status == ReservationStatus.Ready);
            var slots = book.AvailableCopies - readyCount;
            if (slots <= 0)
            {
                continue;
            }

            foreach (var waiting in _reservations
                .Where(r => r.BookId == book.Id && r.Status == ReservationStatus.Waiting)
                .OrderBy(r => r.ReservedAt)
                .Take(slots))
            {
                waiting.Status = ReservationStatus.Ready;
            }
        }
    }

    private void SyncReservationStatuses()
    {
        var updated = false;
        foreach (var reservation in _reservations)
        {
            if (reservation.Status is "Tamamlandi")
            {
                reservation.Status = ReservationStatus.Completed;
                updated = true;
            }
            else if (reservation.Status is "Iptal")
            {
                reservation.Status = ReservationStatus.Cancelled;
                updated = true;
            }
        }

        if (updated)
        {
            Persist();
        }
    }

    private void SyncBookCopies()
    {
        var updated = false;
        foreach (var book in _books)
        {
            if (book.TotalCopies < 1) { book.TotalCopies = 1; updated = true; }
            var borrowed = ActiveLoanCount(book.Id);
            var expected = book.TotalCopies - borrowed;
            if (book.AvailableCopies != expected) { book.AvailableCopies = Math.Max(0, expected); updated = true; }
        }
        if (updated) Persist();
    }

    private void EnsureLoanDueDates()
    {
        var updated = false;
        foreach (var loan in _loans)
        {
            if (loan.DueDate == default)
            {
                loan.DueDate = loan.BorrowedAt.AddDays(_settings.LoanDurationDays);
                updated = true;
            }
        }
        if (updated) Persist();
    }

    private void SyncUserNamesInRecords(User user)
    {
        foreach (var loan in _loans.Where(l => l.UserId == user.Id))
        {
            loan.UserFullName = user.FullName;
        }

        foreach (var reservation in _reservations.Where(r => r.UserId == user.Id))
        {
            reservation.UserFullName = user.FullName;
        }
    }

    private void SyncLoanUserNames()
    {
        var updated = false;
        foreach (var loan in _loans.Where(l => string.IsNullOrWhiteSpace(l.UserFullName)))
        {
            var user = _users.FirstOrDefault(u => u.Id == loan.UserId);
            if (user is null) continue;
            loan.UserFullName = user.FullName;
            updated = true;
        }
        if (updated) Persist();
    }

    private void Persist()
    {
        _repository.Save(new LibraryData
        {
            Users = _users,
            Books = _books,
            Loans = _loans,
            Reservations = _reservations,
            Settings = _settings
        });
        CaptureDbWriteTime();
    }

    private LoanRecord ToLoanRecord(Loan loan)
    {
        return new LoanRecord
        {
            Loan = loan,
            User = GetUserOrThrow(loan.UserId),
            Book = GetBookOrThrow(loan.BookId)
        };
    }
}
