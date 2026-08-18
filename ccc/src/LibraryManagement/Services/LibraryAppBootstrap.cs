using LibraryManagement.Persistence;

namespace LibraryManagement.Services;

public sealed class LibraryAppBootstrap
{
    public LibraryAppBootstrap(string dataDirectory)
    {
        DataDirectory = dataDirectory;
        DatabasePath = Path.Combine(dataDirectory, "kutuphane.db");
        ExportJsonPath = Path.Combine(dataDirectory, "kutuphane-verileri.json");
        ExportTextPath = Path.Combine(dataDirectory, "kutuphane-raporu.txt");
        ExportExcelPath = Path.Combine(dataDirectory, "kutuphane-raporu.xlsx");
        ExportHtmlPath = Path.Combine(dataDirectory, "kutuphane-raporu.html");
        ExportPdfPath = Path.Combine(dataDirectory, "kutuphane-raporu.pdf");
        BackupDirectory = Path.Combine(dataDirectory, "yedekler");
    }

    public string DataDirectory { get; }
    public string DatabasePath { get; }
    public string ExportJsonPath { get; }
    public string ExportTextPath { get; }
    public string ExportExcelPath { get; }
    public string ExportHtmlPath { get; }
    public string ExportPdfPath { get; }
    public string BackupDirectory { get; }
    public LibraryService Library { get; private set; } = null!;

    public void Initialize()
    {
        var repository = new SqliteLibraryRepository(DatabasePath);
        MigrateFromJsonIfNeeded(repository);
        Library = new LibraryService(repository);

        if (!Library.HasData)
        {
            SampleDataSeeder.Seed(Library);
        }

        ClassicBooksSeeder.ApplyMissingClassics(Library);
        ExportAllReports();
    }

    public void ExportAllReports()
    {
        var data = Library.GetAllData();
        var stats = Library.GetStatistics();
        DataViewer.ExportReadableFiles(data, ExportJsonPath, ExportTextPath, stats);
        ReportExporter.ExportToExcel(data, ExportExcelPath);
        ReportExporter.ExportToPdfHtml(data, stats, ExportHtmlPath);
        ReportExporter.ExportToPdf(data, stats, ExportPdfPath);
    }

    public string CreateBackup()
        => BackupService.CreateBackup(DatabasePath, BackupDirectory);

    private void MigrateFromJsonIfNeeded(SqliteLibraryRepository repository)
    {
        if (repository.Exists() || !File.Exists(ExportJsonPath)) return;

        var data = new JsonLibraryRepository(ExportJsonPath).Load();
        if (data.Users.Count == 0 && data.Books.Count == 0 && data.Loans.Count == 0) return;

        repository.Save(data);
    }
}
