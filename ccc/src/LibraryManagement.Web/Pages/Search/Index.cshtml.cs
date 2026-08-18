using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagement.Web.Pages.Search;

public class IndexModel : PageModel
{
    private readonly LibraryService _library;

    public IndexModel(LibraryService library) => _library = library;

    [BindProperty(SupportsGet = true)]
    public string Query { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string Type { get; set; } = "user";

    [BindProperty(SupportsGet = true)]
    public Guid? UserId { get; set; }

    public IReadOnlyList<User> Users { get; private set; } = [];
    public IReadOnlyList<Book> Books { get; private set; } = [];
    public User? SelectedUser { get; private set; }
    public IReadOnlyList<LoanRecord> SelectedUserActiveLoans { get; private set; } = [];

    public void OnGet()
    {
        if (string.IsNullOrWhiteSpace(Query)) return;

        if (Type == "book")
        {
            Books = _library.SearchBooks(Query);
            return;
        }

        Users = _library.SearchUsers(Query);

        if (UserId is Guid userId)
        {
            SelectedUser = _library.GetAllUsers().FirstOrDefault(u => u.Id == userId);
            if (SelectedUser is not null)
            {
                SelectedUserActiveLoans = _library.GetUserActiveLoans(userId);
            }
        }
    }
}
