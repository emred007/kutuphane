namespace LibraryManagement.Services;

public static class BackupService
{
    public static string CreateBackup(string databasePath, string backupDirectory)
    {
        if (!File.Exists(databasePath))
        {
            throw new FileNotFoundException("Veritabanı dosyası bulunamadı.", databasePath);
        }

        Directory.CreateDirectory(backupDirectory);
        var fileName = $"kutuphane_yedek_{DateTime.Now:yyyyMMdd_HHmmss}.db";
        var destination = Path.Combine(backupDirectory, fileName);
        File.Copy(databasePath, destination, overwrite: true);
        return destination;
    }
}
