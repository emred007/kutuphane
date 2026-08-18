using System.Text.Json;

namespace LibraryManagement.Persistence;

public class JsonLibraryRepository : ILibraryRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public JsonLibraryRepository(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    public bool Exists() => File.Exists(_filePath);

    public LibraryData Load()
    {
        if (!File.Exists(_filePath))
        {
            return new LibraryData();
        }

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<LibraryData>(json, JsonOptions) ?? new LibraryData();
    }

    public void Save(LibraryData data)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
