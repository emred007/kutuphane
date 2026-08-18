using LibraryManagement.Models;
using LibraryManagement.Persistence;

namespace LibraryManagement.Services;

public sealed partial class LibraryService
{
    private DateTime _lastKnownDbWriteTime;
    private int _dataVersion;

    public bool ReloadIfChanged()
    {
        var versionBefore = _dataVersion;
        EnsureFreshData();
        return _dataVersion != versionBefore;
    }

    private void EnsureFreshData()
    {
        if (_repository is not SqliteLibraryRepository sqlite || !File.Exists(sqlite.FilePath))
        {
            return;
        }

        var writeTime = File.GetLastWriteTimeUtc(sqlite.FilePath);
        if (writeTime <= _lastKnownDbWriteTime)
        {
            return;
        }

        ReloadFromRepository();
    }

    private void ReloadFromRepository()
    {
        var data = _repository.Load();

        _users.Clear();
        _users.AddRange(data.Users);
        _books.Clear();
        _books.AddRange(data.Books);
        _loans.Clear();
        _loans.AddRange(data.Loans);
        _reservations.Clear();
        _reservations.AddRange(data.Reservations);
        _settings = data.Settings;

        CaptureDbWriteTime();
        _dataVersion++;
    }

    private void InitializeFromRepository()
    {
        var data = _repository.Load();

        _users.Clear();
        _users.AddRange(data.Users);
        _books.Clear();
        _books.AddRange(data.Books);
        _loans.Clear();
        _loans.AddRange(data.Loans);
        _reservations.Clear();
        _reservations.AddRange(data.Reservations);
        _settings = data.Settings;

        SyncLoanUserNames();
        SyncBookCopies();
        EnsureLoanDueDates();
        SyncReservationStatuses();
        PromoteEligibleReservations();

        if (SamplePhoneSeeder.ApplyMissingPhones(_users))
        {
            Persist();
            return;
        }

        CaptureDbWriteTime();
        _dataVersion++;
    }

    private void CaptureDbWriteTime()
    {
        if (_repository is SqliteLibraryRepository sqlite && File.Exists(sqlite.FilePath))
        {
            _lastKnownDbWriteTime = File.GetLastWriteTimeUtc(sqlite.FilePath);
        }
    }
}
