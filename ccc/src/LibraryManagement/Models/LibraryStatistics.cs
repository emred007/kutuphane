namespace LibraryManagement.Models;

public class LibraryStatistics
{
    public int UserCount { get; init; }
    public int BookCount { get; init; }
    public int TotalCopies { get; init; }
    public int AvailableCopies { get; init; }
    public int ActiveLoanCount { get; init; }
    public int OverdueCount { get; init; }
    public int ReservationCount { get; init; }
    public int ReadyReservationCount { get; init; }
    public int LoanDurationDays { get; init; }
    public IReadOnlyList<BookStatistic> MostBorrowedBooks { get; init; } = [];
    public IReadOnlyList<UserStatistic> MostActiveUsers { get; init; } = [];
}

public class BookStatistic
{
    public required string Title { get; init; }
    public required string Author { get; init; }
    public int BorrowCount { get; init; }
}

public class UserStatistic
{
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public int BorrowCount { get; init; }
}
