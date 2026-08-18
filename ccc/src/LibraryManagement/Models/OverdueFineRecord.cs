namespace LibraryManagement.Models;

public class OverdueFineRecord
{
    public required LoanRecord LoanRecord { get; init; }
    public int DaysOverdue { get; init; }
    public decimal FineAmount { get; init; }
}
