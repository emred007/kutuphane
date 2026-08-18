namespace LibraryManagement.Services;

public static class ClassicBooksSeeder
{
    private static readonly (string Title, string Author, string Isbn, int Copies)[] Classics =
    [
        ("Anna Karenina", "Lev Tolstoy", "978-975-08-0010", 2),
        ("Savaş ve Barış", "Lev Tolstoy", "978-975-08-0011", 2),
        ("Madame Bovary", "Gustave Flaubert", "978-975-08-0012", 2),
        ("Ulysses", "James Joyce", "978-975-08-0013", 1),
        ("Moby Dick", "Herman Melville", "978-975-08-0014", 2),
        ("Bülbülü Öldürmek", "Harper Lee", "978-975-08-0015", 3),
        ("Yabancı", "Albert Camus", "978-975-08-0016", 2),
        ("Gürültü ve Öfke", "William Faulkner", "978-975-08-0017", 1),
        ("Dönüşüm", "Franz Kafka", "978-975-08-0018", 3),
        ("Monte Cristo Kontu", "Alexandre Dumas", "978-975-08-0019", 2)
    ];

    public static int ApplyMissingClassics(LibraryService library)
    {
        var existingIsbns = new HashSet<string>(
            library.GetAllBooks().Select(b => b.Isbn),
            StringComparer.OrdinalIgnoreCase);

        var added = 0;
        foreach (var (title, author, isbn, copies) in Classics)
        {
            if (existingIsbns.Contains(isbn))
            {
                continue;
            }

            library.AddBook(title, author, isbn, copies);
            existingIsbns.Add(isbn);
            added++;
        }

        return added;
    }
}
