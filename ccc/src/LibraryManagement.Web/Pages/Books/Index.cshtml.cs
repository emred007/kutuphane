using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagement.Web.Pages.Books;

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
    public string Title { get; set; } = "";

    [BindProperty]
    public string Author { get; set; } = "";

    [BindProperty]
    public string Isbn { get; set; } = "";

    [BindProperty]
    public int TotalCopies { get; set; } = 1;

    [BindProperty]
    public Guid? EditId { get; set; }

    public IReadOnlyList<Book> Books { get; private set; } = [];

    public void OnGet() => Books = _library.GetAllBooks();

    public IActionResult OnPostAdd()
    {
        try
        {
            _library.AddBook(Title, Author, Isbn, TotalCopies);
            _bootstrap.ExportAllReports();
            TempData["Message"] = "Kitap eklendi.";
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
            TempData["Error"] = "Düzenlenecek kitap seçilmedi.";
            return RedirectToPage();
        }

        try
        {
            _library.UpdateBook(EditId.Value, Title, Author, Isbn, TotalCopies);
            _bootstrap.ExportAllReports();
            TempData["Message"] = "Kitap güncellendi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public IActionResult OnPostDelete(Guid id)
    {
        try
        {
            _library.DeleteBook(id);
            _bootstrap.ExportAllReports();
            TempData["Message"] = "Kitap silindi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }
}
