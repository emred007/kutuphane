namespace LibraryManagement.Services;

public static class LibraryPaths
{
    public static string ResolveDataDirectory()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "data"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "LibraryManagement.Console", "data"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "LibraryManagement.Console", "data"))
        };

        foreach (var directory in candidates)
        {
            if (File.Exists(Path.Combine(directory, "kutuphane.db")))
            {
                return directory;
            }
        }

        foreach (var directory in candidates)
        {
            if (Directory.Exists(directory))
            {
                return directory;
            }
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "data");
    }
}
