namespace LibraryManagement.Services;

public static class SampleDataSeeder
{
    public static void Seed(LibraryService library)
    {
        var ayse = library.AddUser("Ayşe Yılmaz", "ayse@example.com", "05321110001");
        var mehmet = library.AddUser("Mehmet Demir", "mehmet@example.com", "05332220002");
        var zeynep = library.AddUser("Zeynep Kaya", "zeynep@example.com", "05343330003");
        var ali = library.AddUser("Ali Çelik", "ali@example.com", "05354440004");
        var fatma = library.AddUser("Fatma Öztürk", "fatma@example.com", "05365550005");

        var sucVeCeza = library.AddBook("Suç ve Ceza", "Fyodor Dostoyevski", "978-975-08-0001", 2);
        var simyaci = library.AddBook("Simyacı", "Paulo Coelho", "978-975-08-0002", 3);
        var saatler = library.AddBook("Saatleri Ayarlama Enstitüsü", "Ahmet Hamdi Tanpınar", "978-975-08-0003", 1);
        var kar = library.AddBook("Kar", "Orhan Pamuk", "978-975-08-0004", 2);
        var kitap1984 = library.AddBook("1984", "George Orwell", "978-975-08-0005", 2);
        var kucukPrens = library.AddBook("Küçük Prens", "Antoine de Saint-Exupéry", "978-975-08-0006", 4);
        var sefiller = library.AddBook("Sefiller", "Victor Hugo", "978-975-08-0007", 1);
        var inceMemed = library.AddBook("İnce Memed", "Yaşar Kemal", "978-975-08-0008", 2);

        var loan1 = library.BorrowBook(ayse.Id, sucVeCeza.Id);
        library.ReturnBook(loan1.Id);

        library.BorrowBook(ayse.Id, saatler.Id);
        library.BorrowBook(mehmet.Id, simyaci.Id);
        library.BorrowBook(zeynep.Id, kitap1984.Id);
        library.BorrowBook(ali.Id, sefiller.Id);

        var loan6 = library.BorrowBook(fatma.Id, inceMemed.Id);
        library.ReturnBook(loan6.Id);

        var loan7 = library.BorrowBook(mehmet.Id, kar.Id);
        library.ReturnBook(loan7.Id);

        var loan8 = library.BorrowBook(zeynep.Id, kucukPrens.Id);
        library.ReturnBook(loan8.Id);

        library.BorrowBook(mehmet.Id, sucVeCeza.Id, DateTime.UtcNow.AddDays(-20));
        library.AddReservation(ayse.Id, kitap1984.Id);
    }
}
