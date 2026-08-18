using LibraryManagement.Models;

namespace LibraryManagement.Services;

public static class OverdueFineCalculator
{
    public static OverdueFineRecord Calculate(LoanRecord record, decimal finePerDay)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (finePerDay < 0)
        {
            throw new InvalidOperationException("Günlük ceza tutarı negatif olamaz.");
        }

        var loan = record.Loan;
        if (!loan.IsOverdue)
        {
            throw new InvalidOperationException(
                $"{record.User.FullName} kullanıcısının '{record.Book.Title}' kitabı için gecikme cezası hesaplanamaz; iade süresi dolmamış veya kitap iade edilmiş.");
        }

        var daysOverdue = (int)Math.Ceiling((DateTime.UtcNow - loan.DueDate).TotalDays);
        var fineAmount = daysOverdue * finePerDay;

        return new OverdueFineRecord
        {
            LoanRecord = record,
            DaysOverdue = daysOverdue,
            FineAmount = fineAmount
        };
    }
}
