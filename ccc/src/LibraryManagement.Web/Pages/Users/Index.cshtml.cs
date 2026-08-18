using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagement.Web.Pages.Users;

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
    public string FullName { get; set; } = "";

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string PhoneNumber { get; set; } = "";

    [BindProperty]
    public Guid? EditId { get; set; }

    public IReadOnlyList<User> Users { get; private set; } = [];

    public void OnGet() => Users = _library.GetAllUsers();

    public IActionResult OnPostAdd()
    {
        try
        {
            _library.AddUser(FullName, Email, PhoneNumber);
            _bootstrap.ExportAllReports();
            TempData["Message"] = "Kullanıcı eklendi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public IActionResult OnPostEdit()
    {
        if (EditId is null)
        {
            TempData["Error"] = "Düzenlenecek kullanıcı seçilmedi.";
            return RedirectToPage();
        }

        try
        {
            _library.UpdateUser(EditId.Value, FullName, Email, PhoneNumber);
            _bootstrap.ExportAllReports();
            TempData["Message"] = "Kullanıcı güncellendi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }
}
