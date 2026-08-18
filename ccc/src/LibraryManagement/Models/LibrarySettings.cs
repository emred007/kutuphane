namespace LibraryManagement.Models;

public class LibrarySettings
{
    public int LoanDurationDays { get; set; } = 14;
    public decimal FinePerDay { get; set; } = 5m;
}
