using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagement.Web.Pages.Settings;

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
    public int LoanDurationDays { get; set; }

    public string DatabasePath => _bootstrap.DatabasePath;

    public void OnGet() => LoanDurationDays = _library.LoanDurationDays;

    public IActionResult OnPostSave()
    {
        try
        {
            _library.SetLoanDurationDays(LoanDurationDays);
            _bootstrap.ExportAllReports();
            TempData["Message"] = $"Ödünç süresi {LoanDurationDays} gün olarak kaydedildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public IActionResult OnPostBackup()
    {
        try
        {
            var path = _bootstrap.CreateBackup();
            TempData["Message"] = $"Yedek alındı: {path}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }
}
