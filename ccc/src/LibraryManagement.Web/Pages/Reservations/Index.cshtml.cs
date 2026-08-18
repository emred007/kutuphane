using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Web.Pages.Reservations;

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

    public IReadOnlyList<ReservationRecord> Reservations { get; private set; } = [];
    public SelectList Users { get; private set; } = null!;
    public SelectList UnavailableBooks { get; private set; } = null!;

    public void OnGet()
    {
        Reservations = _library.GetActiveReservationRecords();
        Users = new SelectList(_library.GetAllUsers(), nameof(Models.User.Id), nameof(Models.User.FullName));
        UnavailableBooks = new SelectList(
            _library.GetAllBooks().Where(b => !b.IsAvailable),
            nameof(Book.Id),
            nameof(Book.Title));
    }

    public IActionResult OnPostAdd()
    {
        try
        {
            var reservation = _library.AddReservation(UserId, BookId);
            var position = _library.GetReservationQueuePosition(reservation);
            _bootstrap.ExportAllReports();
            TempData["Message"] = position > 0
                ? $"Rezervasyon eklendi. Sıra: {position}."
                : "Rezervasyon eklendi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public IActionResult OnPostCancel(Guid id)
    {
        try
        {
            _library.CancelReservation(id);
            _bootstrap.ExportAllReports();
            TempData["Message"] = "Rezervasyon iptal edildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }
}
