using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagement.Web.Pages;

public class IndexModel : PageModel
{
    private readonly LibraryService _library;

    public IndexModel(LibraryService library) => _library = library;

    public LibraryStatistics Stats { get; private set; } = null!;
    public IReadOnlyList<LoanRecord> OverdueLoans { get; private set; } = [];

    public void OnGet()
    {
        Stats = _library.GetStatistics();
        OverdueLoans = _library.GetOverdueLoans();
    }
}
