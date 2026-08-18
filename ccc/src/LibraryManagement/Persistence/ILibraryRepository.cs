namespace LibraryManagement.Persistence;

public interface ILibraryRepository
{
    bool Exists();
    LibraryData Load();
    void Save(LibraryData data);
}
