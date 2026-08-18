# NE NEREDE? — Kod Konum Rehberi

Sunumda hoca bir özelliği sorduğunda **hangi dosyaya, kaçıncı satıra** gideceğini hızlı bulmak için bu tabloyu kullan.

> **Yol formatı:** `src/...` — proje kökünden (`ccc/`)  
> **Satır:** Dosyayı IDE'de `Ctrl+G` ile satır numarasına git.

---

## HIZLI ARAMA — Özelliğe Göre

| Ne arıyorsun? | Dosya | Satır |
|---------------|-------|-------|
| **Ödünç ver (asıl iş mantığı)** | `LibraryManagement/Services/LibraryService.cs` | **223–257** |
| **İade al (asıl iş mantığı)** | `LibraryManagement/Services/LibraryService.cs` | **259–281** |
| **Kullanıcı ekle** | `LibraryManagement/Services/LibraryService.cs` | **85–111** |
| **Kullanıcı düzenle** | `LibraryManagement/Services/LibraryService.cs` | **113–137** |
| **Kitap ekle** | `LibraryManagement/Services/LibraryService.cs` | **139–168** |
| **Kitap düzenle** | `LibraryManagement/Services/LibraryService.cs` | **170–201** |
| **Kitap sil** | `LibraryManagement/Services/LibraryService.cs` | **203–221** |
| **Rezervasyon ekle** | `LibraryManagement/Services/LibraryService.cs` | **283–309** |
| **Rezervasyon iptal** | `LibraryManagement/Services/LibraryService.cs` | **311–332** |
| **Geciken listesi** | `LibraryManagement/Services/LibraryService.cs` | **334–338** |
| **Gecikme cezası hesabı** | `LibraryManagement/Services/OverdueFineCalculator.cs` | **7–32** |
| **Gecikme cezası listesi** | `LibraryManagement/Services/LibraryService.cs` | **340–350** |
| **Kullanıcı ara (LINQ)** | `LibraryManagement/Services/LibraryService.cs` | **385–395** |
| **Kitap ara (LINQ)** | `LibraryManagement/Services/LibraryService.cs` | **397–407** |
| **İstatistikler** | `LibraryManagement/Services/LibraryService.cs` | **409–434** |
| **Veritabanına kaydet** | `LibraryManagement/Services/LibraryService.cs` | **629–640** |
| **Telefon doğrulama** | `LibraryManagement/Services/PhoneNumberValidator.cs` | **5–31** |
| **Web ↔ WinForms senkron** | `LibraryManagement/Services/LibraryService.Sync.cs` | **11–32** |
| **Rezervasyon sırası (Hazır yapma)** | `LibraryManagement/Services/LibraryService.cs` | **528–548** |

---

## WEB ARAYÜZÜ

| Ne? | Görünüm (.cshtml) | Kod (.cshtml.cs) | Satır (kod) |
|-----|-------------------|------------------|---------------|
| Ana sayfa / istatistik | `Web/Pages/Index.cshtml` | `Web/Pages/Index.cshtml.cs` | OnGet **16–20** |
| Kullanıcı listesi | `Web/Pages/Users/Index.cshtml` | `Web/Pages/Users/Index.cshtml.cs` | OnGet **33** |
| Kullanıcı ekle butonu | `Users/Index.cshtml` (form) | `Users/Index.cshtml.cs` | OnPostAdd **35–48** |
| Kullanıcı düzenle | `Users/Index.cshtml` (modal) | `Users/Index.cshtml.cs` | OnPostEdit **51–71** |
| Kitap listesi | `Web/Pages/Books/Index.cshtml` | `Books/Index.cshtml.cs` | OnGet **36** |
| Kitap ekle | `Books/Index.cshtml` | `Books/Index.cshtml.cs` | OnPostAdd **38–51** |
| Kitap düzenle | `Books/Index.cshtml` | `Books/Index.cshtml.cs` | OnPostEdit **54–73** |
| Kitap sil | `Books/Index.cshtml` | `Books/Index.cshtml.cs` | OnPostDelete **76–89** |
| **Ödünç ver butonu** | `Loans/Index.cshtml` **8–19** | `Loans/Index.cshtml.cs` | OnPostBorrow **45–58** |
| **İade al butonu** | `Loans/Index.cshtml` **34–37** | `Loans/Index.cshtml.cs` | OnPostReturn **61–74** |
| Gecikme cezası tablosu | `Loans/Index.cshtml` | `Loans/Index.cshtml.cs` | OnGet **38–40** |
| Arama | `Web/Pages/Search/Index.cshtml` | `Search/Index.cshtml.cs` | OnGet **28–48** |
| Rezervasyon ekle/iptal | `Web/Pages/Reservations/Index.cshtml` | `Reservations/Index.cshtml.cs` | Add **40–57**, Cancel **59–72** |
| Ödünç süresi / yedek | `Web/Pages/Settings/Index.cshtml` | `Settings/Index.cshtml.cs` | Save **25–39**, Backup **41–52** |
| Menü (navbar) | `Web/Pages/Shared/_Layout.cshtml` | — | **19–26** |
| **Web senkron middleware** | — | `Web/Program.cs` | **22–27** |
| Uygulama başlatma (web) | — | `Web/Program.cs` | **7–11** |

---

## WINFORMS ARAYÜZÜ

| Ne? | Dosya | Satır |
|-----|-------|-------|
| Ana form / timer senkron | `WinForms/MainForm.cs` | Constructor **29–46** |
| Senkron (2 sn) | `WinForms/MainForm.cs` | SyncFromDatabase **199–208** |
| Kullanıcılar sekmesi | `WinForms/MainForm.cs` | CreateUsersTab **63–88** |
| Kullanıcı ekle butonu | `WinForms/MainForm.cs` | AddUser **308–322** |
| Kullanıcı düzenle | `WinForms/MainForm.cs` | EditUser **324–341** |
| Kitaplar sekmesi | `WinForms/MainForm.cs` | CreateBooksTab **90–115** |
| Kitap ekle | `WinForms/MainForm.cs` | AddBook **362–377** |
| **Ödünç ver butonu** | `WinForms/MainForm.cs` | Buton **130**, BorrowBook **379–396** |
| **İade al butonu** | `WinForms/MainForm.cs` | Buton **135**, ReturnSelectedLoan **398–415** |
| Ödünç sekmesi | `WinForms/MainForm.cs` | CreateBorrowTab **117–142** |
| Geçmiş sekmesi | `WinForms/MainForm.cs` | CreateHistoryTab **144–163** |
| Geciken iadeler sekmesi | `WinForms/MainForm.Features.cs` | CreateOverdueTab **25–30**, RefreshOverdue **103–114** |
| Arama sekmesi | `WinForms/MainForm.Features.cs` | CreateSearchTab **32–61**, RunSearch **159–180** |
| İstatistik sekmesi | `WinForms/MainForm.Features.cs` | CreateStatsTab **63–68**, RefreshStats **136–157** |
| Rezervasyon sekmesi | `WinForms/MainForm.Features.cs` | CreateReservationTab **70–89**, AddReservation **262–276** |
| Ayarlar sekmesi | `WinForms/MainForm.Features.cs` | CreateSettingsTab **91–101**, SaveLoanDuration **294–302** |
| Kitap düzenle / sil | `WinForms/MainForm.Features.cs` | EditBook **224–239**, DeleteBook **241–260** |
| Rapor + yedek sekmesi | `WinForms/MainForm.cs` | CreateReportTab **165–180** |
| Veri değişince yenile | `WinForms/MainForm.cs` | AfterDataChange **473–478** |
| Hata mesajı (MessageBox) | `WinForms/MainForm.cs` | ShowError **482–486** |

---

## KONSOL UYGULAMASI

| Menü | Ne yapar? | Dosya | Satır |
|------|-----------|-------|-------|
| — | Menü listesi | `Console/Program.cs` | PrintMenu **68–96** |
| 1 | Kullanıcı ekle | `Console/Program.cs` | HandleAddUser **98–111** |
| 19 | Kullanıcı düzenle | `Console/Program.cs` | HandleEditUser **113–138** |
| 2 | Kitap ekle | `Console/Program.cs` | HandleAddBook **140–156** |
| 3 | **Ödünç ver** | `Console/Program.cs` | HandleBorrowBook **158–179** |
| 4 | **İade al** | `Console/Program.cs` | HandleReturnBook **181–199** |
| 5–6 | Listele | `Console/Program.cs` | **201–215** |
| 7 | Aktif ödünçler | `Console/Program.cs` | HandleActiveLoans **217–223** |
| 8–9 | Okuma geçmişi | `Console/Program.cs` | **225–259** |
| 11 | Geciken iadeler | `Console/Program.cs` | HandleOverdueLoans **277–290** |
| 12 | Arama | `Console/Program.cs` | HandleSearch **292–344** |
| 13 | İstatistik | `Console/Program.cs` | HandleStatistics **346–363** |
| 14–15 | Kitap düzenle/sil | `Console/Program.cs` | **365–412** |
| 16 | Rezervasyon | `Console/Program.cs` | HandleReservation **414–461** |
| 17 | Ödünç süresi | `Console/Program.cs` | HandleLoanDuration **463–477** |
| 18 | Yedek | `Console/Program.cs` | HandleBackup **479–487** |
| — | Program girişi | `Console/Program.cs` | Main **10–19** |

---

## MODELLER (Veri Sınıfları)

| Model | Ne tutar? | Dosya | Satır |
|-------|-----------|-------|-------|
| `User` | Kullanıcı (ad, e-posta, telefon) | `Models/User.cs` | **5–20** |
| `Book` | Kitap (başlık, yazar, ISBN, kopya) | `Models/Book.cs` | **5–30** |
| `Loan` | Ödünç kaydı + `IsOverdue` | `Models/Loan.cs` | **5–32** |
| `LoanRecord` | Ödünç + kullanıcı + kitap birleşik | `Models/LoanRecord.cs` | **3–8** |
| `Reservation` | Rezervasyon | `Models/Reservation.cs` | **5–23** |
| `ReservationStatus` | Bekliyor/Hazır/Tamamlandı/İptal | `Models/Reservation.cs` | **26–33** |
| `LibrarySettings` | Ödünç süresi, günlük ceza | `Models/LibrarySettings.cs` | **3–6** |
| `LibraryStatistics` | İstatistik özeti | `Models/LibraryStatistics.cs` | **3–29** |
| `OverdueFineRecord` | Gecikme cezası kaydı | `Models/OverdueFineRecord.cs` | **3–8** |

---

## SERVİSLER VE YARDIMCILAR

| Sınıf | Ne işe yarar? | Dosya | Önemli satır |
|-------|---------------|-------|--------------|
| `LibraryService` | Tüm iş kuralları | `Services/LibraryService.cs` | **6–651** |
| `LibraryService.Sync` | DB senkronizasyonu | `Services/LibraryService.Sync.cs` | **11–88** |
| `LibraryAppBootstrap` | Başlatma + rapor export | `Services/LibraryAppBootstrap.cs` | Init **29–42**, Export **44–52** |
| `PhoneNumberValidator` | Telefon 05/11 hane | `Services/PhoneNumberValidator.cs` | **5–45** |
| `OverdueFineCalculator` | Ceza hesaplama | `Services/OverdueFineCalculator.cs` | **7–32** |
| `ReportExporter` | Excel, HTML, PDF | `Services/ReportExporter.cs` | Excel **16**, PDF **115** |
| `DataViewer` | TXT + JSON rapor | `Services/DataViewer.cs` | **9–81** |
| `BackupService` | DB yedekleme | `Services/BackupService.cs` | **5** |
| `LibraryPaths` | Veri klasörü bulma | `Services/LibraryPaths.cs` | **5–32** |
| `SampleDataSeeder` | İlk örnek veri | `Services/SampleDataSeeder.cs` | **5–41** |
| `ClassicBooksSeeder` | Dünya klasiği kitaplar | `Services/ClassicBooksSeeder.cs` | **19–37** |

---

## VERİTABANI VE KALICILIK

| Ne? | Dosya | Satır |
|-----|-------|-------|
| Repository arayüzü | `Persistence/ILibraryRepository.cs` | **3–8** |
| SQLite okuma | `Persistence/SqliteLibraryRepository.cs` | Load **21–41** |
| SQLite yazma (tüm tablo) | `Persistence/SqliteLibraryRepository.cs` | Save **43–76** |
| Tablo oluşturma (CREATE TABLE) | `Persistence/SqliteLibraryRepository.cs` | **~95–132** |
| Kullanıcı INSERT | `Persistence/SqliteLibraryRepository.cs` | InsertUser **304–315** |
| Kitap INSERT | `Persistence/SqliteLibraryRepository.cs` | InsertBook **317–333** |
| Ödünç INSERT | `Persistence/SqliteLibraryRepository.cs` | InsertLoan **335–351** |
| Ayarlar okuma/yazma | `Persistence/SqliteLibraryRepository.cs` | ReadSettings **282–296**, InsertSettings **370–382** |
| JSON export/import | `Persistence/JsonLibraryRepository.cs` | Load **22–31**, Save **33–43** |
| Tüm veri paketi | `Persistence/LibraryData.cs` | **6–21** |

---

## VERİ DOSYALARI (Kod değil — disk)

| Dosya | Konum |
|-------|-------|
| SQLite veritabanı | `src/LibraryManagement.Console/data/kutuphane.db` |
| JSON export | `.../data/kutuphane-verileri.json` |
| Metin rapor | `.../data/kutuphane-raporu.txt` |
| Excel rapor | `.../data/kutuphane-raporu.xlsx` |
| HTML rapor | `.../data/kutuphane-raporu.html` |
| PDF rapor | `.../data/kutuphane-raporu.pdf` |
| Yedekler | `.../data/yedekler/` |

---

## SUNUMDA SIK SORULAN 5 SORU → NEREYE GİT?

| Hoca sorusu | Git |
|-------------|-----|
| "Ödünç ver butonuna basınca ne oluyor?" | Web: `Loans/Index.cshtml.cs` **45** → `LibraryService.cs` **223** |
| "Veri nereye kaydediliyor?" | `LibraryService.cs` **629** → `SqliteLibraryRepository.cs` **43** |
| "Gecikme nasıl anlaşılıyor?" | `Models/Loan.cs` **32** (`IsOverdue`) |
| "Telefon nasıl kontrol ediliyor?" | `PhoneNumberValidator.cs` **5–31** |
| "Web ile WinForms nasıl senkron?" | `LibraryService.Sync.cs` **18–32** + `Program.cs` **22–27** + `MainForm.cs` **199** |

---

## DİĞER NOT DOSYALARIN

| Dosya | İçerik |
|-------|--------|
| **NE-NEREDE.md** | Bu dosya — satır numaraları |
| **SUNUM-HAZIRLIK.md** | Teknolojiler, ezber notları |
| **SUNUM.md** | Kod akışları, adım adım anlatım |
| **AGENTS.md** | Teknik proje rehberi |

---

## İPUCU

IDE'de hızlı arama:
- `Ctrl+P` → dosya adı yaz (ör. `LibraryService.cs`)
- `Ctrl+G` → satır numarası (ör. `223`)
- `Ctrl+Shift+F` → metot adı ara (ör. `BorrowBook`)

*Satır numaraları kod değiştikçe birkaç satır kayabilir; metot adı her zaman doğru hedefi bulur.*
