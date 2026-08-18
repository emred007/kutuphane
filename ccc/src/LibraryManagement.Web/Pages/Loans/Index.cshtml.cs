using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Web.Pages.Loans;

public class IndexModel : PageModel
{
    private readonly LibraryService _library;
    private readonly LibraryAppBootstrap _bootstrap;

    public IndexModel(LibraryService library, LibraryAppBootstrap bootstrap)
    {
        _library = library;
        _bootstrap = bootstrap;
    }

    [BindProperty]
    public Guid UserId { get; set; }

    [BindProperty]
    public Guid BookId { get; set; }

    public IReadOnlyList<LoanRecord> ActiveLoans { get; private set; } = [];
    public IReadOnlyList<LoanRecord> OverdueLoans { get; private set; } = [];
    public IReadOnlyList<OverdueFineRecord> OverdueFines { get; private set; } = [];
    public decimal TotalOverdueFineAmount { get; private set; }
    public decimal FinePerDay { get; private set; }
    public SelectList Users { get; private set; } = null!;
    public SelectList AvailableBooks { get; private set; } = null!;

    public void OnGet()
    {
        ActiveLoans = _library.GetActiveLoans();
        OverdueLoans = _library.GetOverdueLoans();
        OverdueFines = _library.GetOverdueFines();
        TotalOverdueFineAmount = _library.GetTotalOverdueFineAmount();
        FinePerDay = _library.FinePerDay;
        Users = new SelectList(_library.GetAllUsers(), nameof(Models.User.Id), nameof(Models.User.FullName));
        AvailableBooks = new SelectList(_library.GetAllBooks().Where(b => b.IsAvailable), nameof(Book.Id), nameof(Book.Title));
    }

    public IActionResult OnPostBorrow()
    {
        try
        {
            _library.BorrowBook(UserId, BookId);
            _bootstrap.ExportAllReports();
            TempData["Message"] = "Kitap ödünç verildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public IActionResult OnPostReturn(Guid loanId)
    {
        try
        {
            _library.ReturnBook(loanId);
            _bootstrap.ExportAllReports();
            TempData["Message"] = "Kitap iade alındı.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }
}
