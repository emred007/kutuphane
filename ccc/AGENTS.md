# AGENTS.md — Kütüphane Yönetim Sistemi

Bu dosya, bu depoda çalışan AI agent'lar için proje rehberidir.  
**Sunum / hoca soruları için:** `SUNUM.md` · `SUNUM-HAZIRLIK.md` · **`NE-NEREDE.md`** (dosya + satır numarası)

## Proje Özeti

**Kütüphane Yönetim Sistemi**, C# / .NET 10 ile yazılmış çok katmanlı bir kütüphane uygulamasıdır. Kullanıcı, kitap, ödünç/iade, rezervasyon ve raporlama işlemlerini yönetir. Üç arayüz aynı veritabanını paylaşır:

| Proje | Tür | Rol |
|-------|-----|-----|
| `src/LibraryManagement` | Class Library | Modeller, iş mantığı, SQLite, raporlama |
| `src/LibraryManagement.Console` | Console | Metin menülü CLI (0–19) |
| `src/LibraryManagement.WinForms` | WinForms | Masaüstü GUI (10 sekme) |
| `src/LibraryManagement.Web` | ASP.NET Razor Pages | Web arayüzü |

**Tek doğruluk kaynağı:** `LibraryService` + `kutuphane.db`

## Mimari

```
UI (Console / WinForms / Web)
    ↓
LibraryAppBootstrap.Initialize()
    ↓
LibraryService (in-memory List<> + kurallar)
    ↓
ILibraryRepository
    ├── SqliteLibraryRepository  (ana depo)
    └── JsonLibraryRepository    (export + JSON göçü)
    ↓
data/kutuphane.db
    ↓
ReportExporter + DataViewer → JSON, TXT, XLSX, HTML, PDF
```

### Katmanlar

- **Models** (`src/LibraryManagement/Models/`): `User`, `Book`, `Loan`, `Reservation`, `LoanRecord`, `ReservationRecord`, `LibrarySettings`, `LibraryStatistics`, `OverdueFineRecord`
- **Services** (`src/LibraryManagement/Services/`): `LibraryService` (partial + Sync), `LibraryAppBootstrap`, doğrulama, rapor, seed
- **Persistence** (`src/LibraryManagement/Persistence/`): `ILibraryRepository`, `SqliteLibraryRepository`, `JsonLibraryRepository`, `LibraryData`
- **UI**: Her proje kendi `Program.cs` / sayfaları; iş mantığına doğrudan DB erişimi yok

### Çoklu İstemci Senkronizasyonu

Web ve WinForms aynı anda açıkken veri tutarlılığı dosya zaman damgasıyla sağlanır:

- `LibraryService.Sync.cs`: `ReloadIfChanged()` / `EnsureFreshData()` — DB değiştiyse belleği yeniden yükler
- **Web:** `Program.cs` middleware — her istekte `library.ReloadIfChanged()`
- **WinForms:** `MainForm.cs` — 2 sn timer → `SyncFromDatabase()` → `RefreshDataViews()`

Yeni özellik eklerken tüm public `LibraryService` metotlarında `EnsureFreshData()` çağrısını koruyun; yazma sonrası `Persist()` zaten `CaptureDbWriteTime()` çağırır.

## Veri Konumu

`LibraryPaths.ResolveDataDirectory()` şu sırayla `kutuphane.db` arar:

1. `./data`
2. `./src/LibraryManagement.Console/data`
3. App base directory göreli yollar

**Ana veri klasörü:** `src/LibraryManagement.Console/data/`

| Dosya | Açıklama |
|-------|----------|
| `kutuphane.db` | SQLite veritabanı |
| `kutuphane-verileri.json` | JSON export |
| `kutuphane-raporu.txt` | Metin rapor |
| `kutuphane-raporu.xlsx` | Excel |
| `kutuphane-raporu.html` | HTML |
| `kutuphane-raporu.pdf` | PDF (QuestPDF) |
| `yedekler/` | DB yedekleri |

### SQLite Tabloları (Türkçe kolon adları)

- `Kullanicilar` — Id, AdSoyad, Eposta, Telefon, KayitTarihi
- `Kitaplar` — Id, Baslik, Yazar, Isbn, ToplamKopya, MusaitKopya, MusaitMi
- `OduncKayitlari` — Id, KullaniciId, AdSoyad, KitapId, AlinmaTarihi, SonIadeTarihi, IadeTarihi
- `Rezervasyonlar` — Id, KullaniciId, AdSoyad, KitapId, RezervasyonTarihi, Durum
- `Ayarlar` — Anahtar/Deger çiftleri (`OduncSuresiGun`, `GunlukCezaTutari`)

**Not:** C# property'leri İngilizce (`FullName`), DB/JSON anahtarları Türkçe (`AdSoyad`). Yeni alan eklerken her iki katmanı güncelleyin.

### Kaydetme Stratejisi

`SqliteLibraryRepository.Save()` tüm tabloları DELETE edip yeniden INSERT yapar (full rewrite). Bu bilinçli bir basitlik tercihidir; incremental update yoktur.

## Özellikler

### Temel CRUD
- Kullanıcı: ekle, düzenle (sil yok)
- Kitap: ekle, düzenle, sil (ödünçte/rezervasyonda değilse)
- Çoklu kopya: `TotalCopies` / `AvailableCopies`

### Ödünç / İade
- `BorrowBook`, `ReturnBook`
- Ödünç süresi: `LibrarySettings.LoanDurationDays` (varsayılan 14, 1–365)
- Gecikme: `Loan.IsOverdue` (hesaplanmış property)
- **Gecikme cezası:** `OverdueFineCalculator`, `GetOverdueFines()`, günlük ceza `FinePerDay` (varsayılan 5 TL)

### Rezervasyon
- Durumlar (string): `Bekliyor`, `Hazır`, `Tamamlandı`, `İptal`
- FIFO kuyruk; kitap iade edilince sıradaki `Hazır` olur
- `ReservationStatus.IsOpen()` yardımcı metodu

### Arama ve İstatistik
- `SearchUsers`, `SearchBooks`
- `GetStatistics()` — en çok ödünç alınan kitaplar, en aktif kullanıcılar
- `GetUserActiveLoans`, `GetUserReadingHistory`, `GetBookReadingHistory`

### Raporlama ve Yedek
- Her mutasyondan sonra `ExportAllReports()` (web POST handler'larında bootstrap üzerinden)
- `BackupService.CreateBackup()`

### Doğrulama
- `PhoneNumberValidator`: `05` + 11 rakam, harf yok
- ISBN benzersizliği, e-posta benzersizliği
- İş kuralları `InvalidOperationException` ile Türkçe mesaj

### Seed Veriler
- `SampleDataSeeder` — boş DB'de örnek kullanıcı/kitap/ödünç
- `ClassicBooksSeeder` — eksik dünya klasiği kitapları ISBN ile ekler
- `SamplePhoneSeeder` — eksik telefonları doldurur

## Önemli Dosyalar

| Dosya | Ne zaman dokunulur |
|-------|-------------------|
| `LibraryService.cs` | Yeni iş kuralı, CRUD, arama, istatistik |
| `LibraryService.Sync.cs` | Senkronizasyon mantığı |
| `LibraryAppBootstrap.cs` | Başlangıç, export, göç |
| `SqliteLibraryRepository.cs` | Şema, okuma/yazma |
| `MainForm.cs` + `MainForm.Features.cs` | WinForms UI |
| `LibraryManagement.Web/Pages/**` | Web sayfaları |
| `LibraryManagement.Console/Program.cs` | Konsol menüsü |

## Çalıştırma

```bash
# WinForms (önerilen)
dotnet run --project src/LibraryManagement.WinForms

# Web
dotnet run --project src/LibraryManagement.Web --urls "http://localhost:5180"

# Konsol
dotnet run --project src/LibraryManagement.Console

# Sadece seed (klasik kitaplar vb.)
dotnet run --project src/LibraryManagement.Console -- --seed-only
```

**Derleme kilidi:** WinForms/Web çalışırken `LibraryManagement.dll` kilitlenebilir. Build öncesi süreçleri kapatın.

## UI Desenleri

### Web (Razor Pages)
- POST handler: `try { _library.Metot(); _bootstrap.ExportAllReports(); TempData["Message"] } catch { TempData["Error"] }`
- `nameof(Models.User.Id)` kullanın — `User` ClaimsPrincipal ile çakışır
- Sync middleware `Program.cs`'te routing'den sonra

### WinForms
- `_bootstrap.Library` üzerinden tüm işlemler
- `RefreshDataViews()` senkron sonrası; `RefreshAll()` rapor dahil
- Hata: `ShowError(ex.Message)` MessageBox

### Konsol
- `Bootstrap.Library` static referans
- Menü 0–19; üst seviye `catch (Exception ex)`

## Yeni Özellik Ekleme Rehberi

1. Model gerekirse `Models/` altına ekle
2. İş mantığını `LibraryService`'e ekle; `EnsureFreshData()` + `Persist()`
3. SQLite şeması değişiyorsa `SqliteLibraryRepository` Read/Insert güncelle
4. Üç UI'dan en az birine bağla (özellik parity hedeflenir)
5. Web POST sonrası `ExportAllReports()` çağır
6. Türkçe hata mesajları kullan
7. Gereksiz kapsam genişletme yapma — mevcut desenlere uy

## Bağımlılıklar (Core)

- `Microsoft.Data.Sqlite`
- `ClosedXML` (Excel)
- `QuestPDF` (PDF; çıktıda `LatoFont/` dosyaları normal)

## Bilinen Sınırlamalar

- Kullanıcı silme yok
- Kimlik doğrulama / rol yok
- Unit test projesi yok
- `LibraryService` büyük (God Class)
- Web'de okuma geçmişi sayfası yok (servis metotları var)
- Rezervasyon durumu `string` (enum değil)
- Full-table rewrite performansı düşük

## Agent İçin Yap / Yapma

**Yap:**
- Değişiklikleri minimal tut
- Mevcut Türkçe UI metinlerini koru
- `LibraryPaths.ResolveDataDirectory()` ile veri yolunu çöz
- Çoklu istemci senkronunu bozma

**Yapma:**
- `.env` veya gizli anahtar commit etme
- Git commit/push kullanıcı istemeden yapma
- UI'dan doğrudan SQLite erişimi ekleme
- README/AGENTS dışında gereksiz dokümantasyon dosyası oluşturma

## Hızlı Referans — LibraryService API

```
Kullanıcı:  AddUser, UpdateUser, GetAllUsers, SearchUsers
Kitap:      AddBook, UpdateBook, DeleteBook, GetAllBooks, SearchBooks
Ödünç:      BorrowBook, ReturnBook, GetActiveLoans, GetOverdueLoans, GetOverdueFines
Rezervasyon: AddReservation, CancelReservation, GetActiveReservations
Geçmiş:     GetUserReadingHistory, GetBookReadingHistory, GetUserActiveLoans
Ayar:       SetLoanDurationDays, SetFinePerDay, LoanDurationDays, FinePerDay
Diğer:      GetStatistics, GetAllData, ReloadIfChanged
```
