# SUNUM REHBERİ — Kütüphane Yönetim Sistemi

Bu dosya, projeyi hocaya sunarken kod sorularına hazırlanmanız içindir.  
Tüm terimler Türkçe açıklanmıştır; önemli kod bloklarının **ne iş yaptığı** satır satır anlatılmıştır.

---

## 1. Projeyi Tek Cümleyle Anlat

> "Kütüphanedeki kullanıcıları, kitapları, ödünç verme/iade işlemlerini ve rezervasyonları yöneten; verileri SQLite veritabanında saklayan; konsol, WinForms ve web arayüzünden kullanılabilen bir C# uygulaması."

---

## 2. İsim Sözlüğü (Hoca İngilizce Sorarsa)

| Kodda Geçen İsim | Türkçe Anlamı | Ne İş Yapar? |
|------------------|---------------|--------------|
| `LibraryService` | Kütüphane Servisi | Tüm iş kuralları burada (ekle, sil, ödünç ver) |
| `LibraryAppBootstrap` | Uygulama Başlatıcı | Program açılınca DB'yi yükler, raporları üretir |
| `User` | Kullanıcı | Kütüphane üyesi (ad, e-posta, telefon) |
| `Book` | Kitap | Başlık, yazar, ISBN, kopya sayısı |
| `Loan` | Ödünç Kaydı | Kim, hangi kitabı, ne zaman aldı, ne zaman iade edecek |
| `LoanRecord` | Ödünç Detayı | Ödünç + kullanıcı + kitap bilgisi bir arada |
| `Reservation` | Rezervasyon | Müsait olmayan kitap için sıraya girme |
| `Persist()` | Kaydet | Bellekteki veriyi veritabanına yazar |
| `EnsureFreshData()` | Veriyi Tazele | Başka uygulama DB'yi değiştirdiyse yeniden yükle |
| `BorrowBook` | Ödünç Ver | Kitabı kullanıcıya ödünç olarak ver |
| `ReturnBook` | İade Al | Ödünç kitabı geri al |
| `ILibraryRepository` | Veri Deposu Arayüzü | Veriyi okuma/yazma sözleşmesi |
| `SqliteLibraryRepository` | SQLite Deposu | `kutuphane.db` dosyasıyla çalışır |
| `IndexModel` | Sayfa Modeli | Web sayfasının arka plan kodu |
| `OnPostBorrow` | Ödünç Ver (POST) | "Ödünç Ver" butonuna basılınca çalışır |
| `TempData` | Geçici Mesaj | "Kitap ödünç verildi" gibi bildirimler |
| `ComboBox` | Açılır Liste | WinForms'ta kullanıcı/kitap seçimi |
| `DataGridView` | Tablo | WinForms'ta listeleri gösterir |

---

## 3. Proje Yapısı (Hocaya Gösterebileceğiniz Harita)

```
ccc/
├── AGENTS.md          → Geliştirici / AI rehberi
├── SUNUM.md           → Bu dosya (sunum hazırlığı)
├── README.md          → Genel proje açıklaması
└── src/
    ├── LibraryManagement/           → ÇEKİRDEK (modeller + iş mantığı + DB)
    ├── LibraryManagement.Console/   → Konsol menüsü
    ├── LibraryManagement.WinForms/  → Masaüstü arayüz
    └── LibraryManagement.Web/       → Web arayüzü
```

**Veritabanı yolu:** `src/LibraryManagement.Console/data/kutuphane.db`

---

## 4. Mimari — 4 Katman (Ezber Cümle)

1. **Arayüz** → Kullanıcı butona basar, form doldurur  
2. **İş Mantığı (`LibraryService`)** → Kuralları kontrol eder, listeyi günceller  
3. **Model (`User`, `Book`, `Loan`)** → Verinin şekli  
4. **Veri Erişimi (`SqliteLibraryRepository`)** → `kutuphane.db` dosyasına yazar/okur  

**Akış:** Buton → Sayfa kodu → LibraryService → Veritabanı → Rapor dosyaları

---

## 5. ÖRNEK: Kullanıcıya Kitap Ödünç Verilmesi (TAM AKIŞ)

Bu bölüm sunumun en önemli kısmıdır. Hoca büyük ihtimalle "Ödünç ver butonuna basınca ne oluyor?" diye sorar.

---

### ADIM 1 — Kullanıcı Arayüzde Ne Yapar?

**Web:** `/Loans` sayfasında kullanıcı ve kitap seçilir → **"Ödünç Ver"** butonuna basılır.

**WinForms:** "Ödünç İşlemleri" sekmesinde iki açılır listeden seçim yapılır → **"Ödünç Ver"** butonuna basılır.

---

### ADIM 2 — Web: Butonun HTML Formu

**Dosya:** `LibraryManagement.Web/Pages/Loans/Index.cshtml`

```html
<!-- Bu form sunucuya POST isteği gönderir -->
<form method="post" asp-page-handler="Borrow">
    <!-- Kullanıcı listesi (UserId seçilir) -->
    <select asp-for="UserId" asp-items="Model.Users"></select>

    <!-- Sadece müsait kitaplar (BookId seçilir) -->
    <select asp-for="BookId" asp-items="Model.AvailableBooks"></select>

    <!-- Butona basınca form gönderilir -->
    <button type="submit">Ödünç Ver</button>
</form>
```

**Ne oluyor?**
- `asp-page-handler="Borrow"` → Sunucuda `OnPostBorrow()` metodunu çağırır
- `asp-for="UserId"` → Seçilen kullanıcının kimliğini `UserId` alanına bağlar
- `asp-for="BookId"` → Seçilen kitabın kimliğini `BookId` alanına bağlar

---

### ADIM 3 — Web: Butona Basılınca Çalışan Kod

**Dosya:** `LibraryManagement.Web/Pages/Loans/Index.cshtml.cs`

```csharp
public IActionResult OnPostBorrow()
{
    try
    {
        // 1) Asıl iş: kitabı ödünç ver
        _library.BorrowBook(UserId, BookId);

        // 2) JSON, Excel, PDF raporlarını güncelle
        _bootstrap.ExportAllReports();

        // 3) Kullanıcıya yeşil başarı mesajı
        TempData["Message"] = "Kitap ödünç verildi.";
    }
    catch (Exception ex)
    {
        // Hata olursa program çökmez; kırmızı mesaj gösterilir
        TempData["Error"] = ex.Message;
    }

    // Sayfayı yeniden yükle (POST-Redirect-GET deseni)
    return RedirectToPage();
}
```

**Satır satır açıklama:**

| Satır | Ne yapar? |
|-------|-----------|
| `BorrowBook(UserId, BookId)` | İş mantığına "şu kullanıcıya şu kitabı ver" der |
| `ExportAllReports()` | `kutuphane-verileri.json`, Excel, PDF vb. güncellenir |
| `TempData["Message"]` | Bir sonraki sayfa yüklemesinde yeşil kutuda mesaj görünür |
| `catch` | Müsait kopya yoksa veya rezervasyon sırası varsa hata mesajı gösterir |
| `RedirectToPage()` | Sayfayı yeniler; aktif ödünçler listesi güncellenir |

---

### ADIM 4 — WinForms: Butona Basılınca Çalışan Kod

**Dosya:** `LibraryManagement.WinForms/MainForm.cs`

Buton oluşturulurken tıklama olayı bağlanır:

```csharp
// "Ödünç Ver" butonu oluşturulur; tıklanınca BorrowBook() çalışır
CreateButton("Ödünç Ver", (_, _) => BorrowBook());
```

`BorrowBook()` metodu:

```csharp
private void BorrowBook()
{
    try
    {
        // 1) Açılır listelerden seçim yapılmış mı kontrol et
        if (_borrowUserCombo.SelectedItem is not UserListItem user ||
            _borrowBookCombo.SelectedItem is not BookListItem book)
        {
            ShowError("Kullanıcı ve kitap seçin.");
            return;
        }

        // 2) Aynı iş mantığı metodu (web ile aynı!)
        _bootstrap.Library.BorrowBook(user.Id, book.Id);

        // 3) Raporları güncelle + ekranı yenile + durum çubuğuna yaz
        AfterDataChange("Kitap ödünç verildi.");
    }
    catch (Exception ex)
    {
        ShowError(ex.Message);  // MessageBox ile hata
    }
}
```

**Web ile fark:** WinForms doğrudan `MessageBox` gösterir; web `TempData` kullanır. **İkisi de aynı `LibraryService.BorrowBook` metodunu çağırır.**

---

### ADIM 5 — İş Mantığı: Asıl İş Burada Yapılır

**Dosya:** `LibraryManagement/Services/LibraryService.cs` → `BorrowBook` metodu

```csharp
public Loan BorrowBook(Guid userId, Guid bookId, DateTime? borrowedAt = null)
{
    EnsureFreshData();                          // DB başka yerde değiştiyse yenile
    var user = GetUserOrThrow(userId);        // Kullanıcı var mı?
    var book = GetBookOrThrow(bookId);        // Kitap var mı?

    // KURAL 1: Müsait kopya var mı?
    if (book.AvailableCopies <= 0)
        throw new InvalidOperationException("Müsait kopya yok.");

    // KURAL 2: Rezervasyon sırası başkasında mı?
    if (readyReservations.Count > 0 && sıra başkasında)
        throw new InvalidOperationException("Rezervasyon sırasında...");

    // Ödünç kaydı oluştur
    var loan = new Loan
    {
        UserId = user.Id,
        UserFullName = user.FullName,
        BookId = book.Id,
        BorrowedAt = DateTime.UtcNow,
        DueDate = DateTime.UtcNow.AddDays(_settings.LoanDurationDays)  // +14 gün
    };

    book.AvailableCopies--;   // Müsait kopya 1 azalır
    _loans.Add(loan);         // Ödünç listesine eklenir
    Persist();                // Veritabanına kaydedilir
    return loan;
}
```

**Hocaya söyleyebileceğiniz özet:**

1. Kullanıcı ve kitap doğrulanır  
2. Müsait kopya kontrol edilir  
3. Rezervasyon sırası kontrol edilir  
4. Yeni `Loan` kaydı oluşturulur (son iade tarihi = bugün + 14 gün)  
5. Kitabın müsait kopya sayısı 1 azalır  
6. `Persist()` ile SQLite'a yazılır  

---

### ADIM 6 — Veritabanına Kayıt

**Dosya:** `LibraryService.cs` → `Persist()` metodu

```csharp
private void Persist()
{
    _repository.Save(new LibraryData
    {
        Users = _users,
        Books = _books,
        Loans = _loans,
        Reservations = _reservations,
        Settings = _settings
    });
    CaptureDbWriteTime();  // Senkronizasyon için dosya zamanını güncelle
}
```

**Ne oluyor?** Bellekteki tüm listeler `kutuphane.db` dosyasına yazılır. WinForms açıksa 2 saniye içinde bu değişikliği algılar ve ekranı günceller.

---

### Ödünç Verme Akış Şeması (Ezber)

```
[Kullanıcı] Butona basar
      ↓
[Web: OnPostBorrow / WinForms: BorrowBook()]
      ↓
[LibraryService.BorrowBook()]
      ↓  doğrulama + Loan oluştur + kopya azalt
[Persist() → kutuphane.db]
      ↓
[ExportAllReports() → JSON, Excel, PDF]
      ↓
[Ekran yenilenir — aktif ödünçler listesinde görünür]
```

---

## 6. ÖRNEK: Kitap İadesi

**Web:** Aktif ödünçler tablosundaki **"İade Al"** butonu

```html
<form method="post" asp-page-handler="Return">
    <input type="hidden" name="loanId" value="@record.Loan.Id" />
    <button type="submit">İade Al</button>
</form>
```

```csharp
public IActionResult OnPostReturn(Guid loanId)
{
    _library.ReturnBook(loanId);   // İade işlemi
    _bootstrap.ExportAllReports();
    TempData["Message"] = "Kitap iade alındı.";
    return RedirectToPage();
}
```

**LibraryService.ReturnBook ne yapar?**
- Ödünç kaydını bulur
- `ReturnedAt = DateTime.UtcNow` yazar (iade tarihi)
- Kitabın müsait kopya sayısını 1 artırır
- Bekleyen rezervasyon varsa sıradakini "Hazır" yapar
- `Persist()` ile kaydeder

---

## 7. ÖRNEK: Kullanıcı Ekleme

**Web:** `Users/Index.cshtml` → "Ekle" butonu → `OnPostAdd()`

```csharp
_library.AddUser(FullName, Email, PhoneNumber);
```

**LibraryService.AddUser ne yapar?**
1. Ad ve e-posta boş mu kontrol eder  
2. `PhoneNumberValidator` ile telefonu doğrular (05 + 11 rakam)  
3. Aynı e-posta veya telefon var mı bakar  
4. Yeni `User` oluşturup `_users` listesine ekler  
5. `Persist()` ile kaydeder  

**Telefon doğrulama (hoca sorarsa):**

```csharp
if (digits.Length != 11)
    throw new InvalidOperationException("11 hane olmalı.");
if (!digits.StartsWith("05"))
    throw new InvalidOperationException("05 ile başlamalı.");
```

---

## 8. ÖRNEK: Web ↔ WinForms Senkronizasyonu

**Sorun:** Web ve WinForms ayrı programlar; her birinin kendi belleği var.

**Çözüm:** `kutuphane.db` dosyasının değişme zamanına bakılır.

**Web** — her istekte (`Program.cs`):

```csharp
library.ReloadIfChanged();  // DB değiştiyse belleği yenile
```

**WinForms** — 2 saniyede bir (`MainForm.cs`):

```csharp
_syncTimer.Tick += (_, _) => SyncFromDatabase();

private void SyncFromDatabase()
{
    if (!_bootstrap.Library.ReloadIfChanged()) return;
    RefreshDataViews();  // Tabloları yenile
}
```

**Hocaya cümle:** "Web'de kullanıcı ekleyince DB dosyası güncellenir; WinForms timer ile bunu algılar ve tabloları yeniden yükler."

---

## 9. ÖRNEK: Gecikme Cezası (Yeni Eklenen Özellik)

**Ne yapar?** Son iade tarihi geçmiş kitaplar için: `gecikme günü × günlük ceza (5 TL)`

```csharp
var gun = (int)(DateTime.UtcNow - loan.DueDate).TotalDays;
var ceza = gun * finePerDay;  // örn. 3 gün × 5 = 15 TL
```

**Nerede görünür?** Web → Ödünç sayfası → "Geciken İadeler" tablosu

---

## 10. OOP Sorularına Hazır Cevaplar

### Encapsulation (Kapsülleme)
> "Kitap listesi `_books` private tutulur; dışarıdan doğrudan erişilemez. `AddBook` ve `DeleteBook` metotları üzerinden kontrollü erişim sağlanır."

### Interface
> "`ILibraryRepository` arayüzü veri okuma/yazma sözleşmesini tanımlar. SQLite ve JSON farklı şekilde uygular ama iş mantığı aynı metotları kullanır."

### Polymorphism (Çok biçimlilik)
> "Aynı `ILibraryRepository` referansı SQLite veya JSON depolama sınıfına bağlanabilir. `BorrowBook` metodu opsiyonel tarih parametresiyle hem bugün hem geçmiş tarihle çalışır."

### List Koleksiyonu
> "Kullanıcılar `List<User>`, kitaplar `List<Book>` ile tutulur. Ekleme: `_books.Add(book)`, silme: `_books.Remove(book)`."

### LINQ
> "En çok ödünç alınan kitaplar `GroupBy` ve `OrderByDescending` ile bulunur."

---

## 11. Hocanın Sorabileceği Sorular — Kısa Cevaplar

| Soru | Cevap |
|------|-------|
| Veriler nerede saklanıyor? | SQLite: `kutuphane.db` |
| Neden 3 arayüz var? | Aynı iş mantığını farklı kullanıcı tiplerine sunmak için |
| Ödünç süresi kaç gün? | Varsayılan 14, Ayarlar'dan 1–365 arası değiştirilebilir |
| Gecikme nasıl anlaşılır? | `Loan.IsOverdue` — bugün > son iade tarihi ve iade edilmemiş |
| Hata olunca program çöker mi? | Hayır; try-catch ile mesaj gösterilir |
| Web'de eklenen kullanıcı WinForms'ta ne zaman görünür? | En geç ~2 saniye (timer senkronizasyonu) |
| Rezervasyon nasıl çalışır? | Kitap yokken sıraya girilir; iade olunca sıradaki "Hazır" olur |
| Raporlar ne zaman güncellenir? | Her veri değişikliğinden sonra `ExportAllReports()` |
| God Class nedir, var mı? | Evet; `LibraryService` çok fazla iş yapıyor, bölünebilir |
| Enum neden kullanılmadı? | Rezervasyon durumu string; enum daha güvenli olurdu |

---

## 12. Sunum Sırası Önerisi (5–7 dk)

1. **Projenin amacı** (30 sn)  
2. **4 katmanlı mimari** — diyagram veya sözlü (1 dk)  
3. **Canlı demo:** Kullanıcı ekle → kitap ödünç ver → WinForms'ta göründüğünü göster (2 dk)  
4. **Kod anlatımı:** Ödünç ver butonunun akışı — Adım 2–5 (2 dk)  
5. **OOP + hata yönetimi** — telefon doğrulama örneği (1 dk)  
6. **Eleştirel değerlendirme** — God Class, enum (30 sn)  

---

## 13. Önemli Dosya Listesi (Hoca "Hangi dosyada?" Derse)

| İşlem | Dosya |
|-------|-------|
| Ödünç ver (web butonu) | `Web/Pages/Loans/Index.cshtml` + `Index.cshtml.cs` |
| Ödünç ver (WinForms butonu) | `WinForms/MainForm.cs` → `BorrowBook()` |
| Asıl iş mantığı | `Services/LibraryService.cs` |
| Veritabanı kayıt | `Services/LibraryService.cs` → `Persist()` |
| SQLite okuma/yazma | `Persistence/SqliteLibraryRepository.cs` |
| Telefon doğrulama | `Services/PhoneNumberValidator.cs` |
| Senkronizasyon | `Services/LibraryService.Sync.cs` |
| Uygulama başlatma | `Services/LibraryAppBootstrap.cs` |
| Modeller | `Models/User.cs`, `Book.cs`, `Loan.cs` |

---

**İpucu:** Hoca bir metodun içini sorarsa her zaman şu sırayı izle: **Doğrulama → İş kuralı → Listeyi güncelle → Persist() → Rapor/Ekran yenile**
