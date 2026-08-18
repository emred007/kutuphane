namespace LibraryManagement.Models;

public class LoanRecord
{
    public required Loan Loan { get; init; }
    public required User User { get; init; }
    public required Book Book { get; init; }
}
