# SUNUM HAZIRLIK NOTLARI

Bu dosya, sunuma hazırlanırken çalışacağın kişisel notların.  
Kod akışları ve buton örnekleri için: **`SUNUM.md`**  
Teknik geliştirici rehberi için: **`AGENTS.md`**

---

## BÖLÜM 1 — Projeyi 30 Saniyede Anlat

**Ne?** Kütüphane yönetim sistemi  
**Neden?** Kullanıcı, kitap, ödünç, iade, rezervasyon ve raporları dijital ortamda takip etmek  
**Nasıl?** C# ile yazıldı; veriler SQLite'ta; 3 arayüz (konsol, masaüstü, web) aynı veritabanını kullanıyor  

**Ezber cümle:**
> "Tek bir iş mantığı katmanı var; üç farklı arayüz bu katmana bağlanıyor. Veriler `kutuphane.db` dosyasında kalıcı olarak saklanıyor."

---

## BÖLÜM 2 — Kullandığımız Yazılım Dilleri ve Teknolojiler

### 2.1 C# (C Sharp)

| | |
|---|---|
| **Ne?** Microsoft'un geliştirdiği nesne yönelimli programlama dili |
| **Projede amacı** | Tüm iş mantığı, modeller, veritabanı işlemleri ve arayüz kodları C# ile yazıldı |
| **Nerede kullanıldı?** | Her yerde — projenin ana dili |
| **Ne işe yarar?** | Kullanıcı ekleme, kitap ödünç verme, doğrulama, rapor üretme gibi kuralları kodlarız |
| **Sürüm** | .NET 10 (`net10.0`) |

**Sunumda söyle:**
> "Proje tamamen C# ile geliştirildi. Nesne yönelimli prensipler — sınıf, encapsulation, interface — bu dil üzerinde uygulandı."

**Hoca sorarsa — C# neden?**
> "Tip güvenliği var, hatalar derleme aşamasında yakalanabilir, .NET ekosistemi masaüstü ve web uygulamasını aynı dilde yazmamıza izin verdi."

---

### 2.2 .NET (Dot Net)

| | |
|---|---|
| **Ne?** C# kodunun çalıştığı platform / çerçeve |
| **Projede amacı** | Uygulamanın derlenmesi ve çalışması |
| **Ne işe yarar?** | Konsol, WinForms ve web projelerini aynı çözüm altında birleştirir |
| **Komut** | `dotnet run --project ...` |

**Sunumda söyle:**
> ".NET 10 platformu üzerinde dört proje var: çekirdek kütüphane + konsol + WinForms + web."

---

### 2.3 SQLite

| | |
|---|---|
| **Ne?** Hafif, dosya tabanlı ilişkisel veritabanı |
| **Projede amacı** | Tüm kalıcı veriyi saklamak |
| **Dosya** | `data/kutuphane.db` |
| **Ne işe yarar?** | Uygulama kapansa bile kullanıcılar, kitaplar, ödünçler kaybolmaz |
| **Kütüphane** | `Microsoft.Data.Sqlite` (NuGet paketi) |

**Tablolar (Türkçe isimler):**
- `Kullanicilar` — üyeler
- `Kitaplar` — kitap envanteri
- `OduncKayitlari` — kim ne aldı, ne zaman iade edecek
- `Rezervasyonlar` — sıra bekleyenler
- `Ayarlar` — ödünç süresi, günlük ceza tutarı

**Sunumda söyle:**
> "Ayrı bir veritabanı sunucusu kurmaya gerek yok; tek bir `.db` dosyası tüm veriyi tutuyor. Bu da projeyi taşınabilir ve kurulumu kolay yapıyor."

**Hoca sorarsa — neden SQLite?**
> "Küçük-orta ölçekli proje için yeterli, kurulum gerektirmiyor, dosya olarak kopyalanabilir."

---

### 2.4 LINQ

| | |
|---|---|
| **Ne?** C# içinde veri sorgulama dili (Language Integrated Query) |
| **Projede amacı** | Listeler üzerinde filtreleme, sıralama, gruplama |
| **Nerede?** | `LibraryService` — arama, istatistik, geciken listeler |
| **Ne işe yarar?** | Uzun foreach döngüleri yerine okunabilir sorgular |

**Örnek kullanım (projede):**
- Kullanıcı arama → `Where` + `OrderBy`
- En çok okunan kitaplar → `GroupBy` + `OrderByDescending` + `Take(5)`
- Geciken ödünçler → `Where(l => l.IsOverdue)`

**Sunumda söyle:**
> "LINQ ile koleksiyonlar üzerinde SQL benzeri sorgular yazıyoruz; kod daha kısa ve anlaşılır oluyor."

---

### 2.5 ASP.NET Core + Razor Pages (Web)

| | |
|---|---|
| **Ne?** Microsoft'un web uygulama çerçevesi |
| **Razor Pages** | HTML + C# karışık sayfa modeli (`.cshtml` + `.cshtml.cs`) |
| **Projede amacı** | Tarayıcıdan kütüphane yönetimi |
| **Nerede?** | `LibraryManagement.Web` projesi |
| **Adres** | `http://localhost:5180` (veya terminalde yazan port) |

**Sayfalar:**
| Sayfa | Ne yapar? |
|-------|-----------|
| `/Users` | Kullanıcı ekle / düzenle |
| `/Books` | Kitap ekle / düzenle / sil |
| `/Loans` | Ödünç ver / iade al |
| `/Search` | Kullanıcı ve kitap ara |
| `/Reservations` | Rezervasyon |
| `/Settings` | Ödünç süresi, yedek |

**Sunumda söyle:**
> "Web arayüzü Razor Pages ile yapıldı. Her sayfanın bir HTML görünümü ve arkada C# kodu var. Form gönderilince `OnPost...` metotları çalışıyor."

---

### 2.6 HTML + CSS + Bootstrap (Web arayüzü)

| | |
|---|---|
| **Ne?** Web sayfasının görünüm dilleri |
| **HTML** | Sayfa yapısı (tablolar, formlar, butonlar) |
| **CSS** | Renk, boşluk, yazı tipi |
| **Bootstrap** | Hazır tasarım bileşenleri (buton, tablo, navbar) |
| **Projede amacı** | Web sayfalarını düzenli ve mobil uyumlu göstermek |
| **Nerede?** | `Pages/**/*.cshtml`, `wwwroot/` |

**Sunumda söyle:**
> "Web tarafında Bootstrap kullandık; hazır CSS sınıflarıyla tablo ve form tasarımını hızlı yaptık."

---

### 2.7 Windows Forms (WinForms)

| | |
|---|---|
| **Ne?** Masaüstü grafik arayüz teknolojisi |
| **Projede amacı** | Windows'ta pencere açarak kütüphaneyi yönetmek |
| **Nerede?** | `LibraryManagement.WinForms` — `MainForm.cs` |
| **Ne işe yarar?** | Sekmeli arayüz: kullanıcılar, kitaplar, ödünç, arama, istatistik... |
| **Bileşenler** | `TabControl`, `DataGridView` (tablo), `ComboBox` (açılır liste), `Button` |

**Sunumda söyle:**
> "WinForms ile masaüstü uygulaması yaptık. Web ile aynı `LibraryService`'i kullanıyor; sadece görünüm farklı."

---

### 2.8 Konsol Uygulaması

| | |
|---|---|
| **Ne?** Siyah/beyaz metin tabanlı arayüz |
| **Projede amacı** | Menüden numara seçerek tüm işlemleri yapmak |
| **Nerede?** | `LibraryManagement.Console/Program.cs` |
| **Ne işe yarar?** | GUI olmadan test; tüm özelliklerin CLI karşılığı |

**Sunumda söyle:**
> "Konsol uygulaması da aynı çekirdek kütüphaneyi kullanıyor. Üç arayüz = üç kapı, aynı oda."

---

### 2.9 JSON

| | |
|---|---|
| **Ne?** İnsan ve makine tarafından okunabilir veri formatı |
| **Projede amacı** | Verilerin dışa aktarımı + eski veri taşıma |
| **Dosya** | `kutuphane-verileri.json` |
| **Ne işe yarar?** | Veritabanındaki tüm veriyi okunabilir dosyada görmek; yedek / inceleme |

**Sunumda söyle:**
> "Her değişiklikten sonra JSON dosyası da güncelleniyor; veriyi metin editöründe açıp okuyabilirsiniz."

---

### 2.10 ClosedXML (Excel)

| | |
|---|---|
| **Ne?** Excel dosyası oluşturan NuGet paketi |
| **Projede amacı** | `.xlsx` rapor üretmek |
| **Dosya** | `kutuphane-raporu.xlsx` |
| **Ne işe yarar?** | Kullanıcı, kitap, ödünç listesini Excel'de paylaşmak / analiz etmek |

---

### 2.11 QuestPDF (PDF)

| | |
|---|---|
| **Ne?** PDF belgesi oluşturan NuGet paketi |
| **Projede amacı** | `.pdf` rapor üretmek |
| **Dosya** | `kutuphane-raporu.pdf` |
| **Ne işe yarar?** | Yazdırılabir / resmi görünümlü rapor |

---

## BÖLÜM 3 — Teknoloji → Proje Parçası Eşleştirmesi

```
┌─────────────────────────────────────────────────────────┐
│  C# + .NET 10          →  Tüm proje                     │
├─────────────────────────────────────────────────────────┤
│  WinForms              →  Masaüstü pencere              │
│  ASP.NET Razor Pages   →  Web sitesi                    │
│  Console               →  Metin menüsü                  │
├─────────────────────────────────────────────────────────┤
│  LibraryService (C#)   →  İş kuralları (ortak)          │
│  SQLite                →  kalıcı veri                   │
│  LINQ                  →  arama & istatistik            │
├─────────────────────────────────────────────────────────┤
│  JSON / TXT / XLSX /   →  Rapor dışa aktarım            │
│  HTML / PDF                                             │
└─────────────────────────────────────────────────────────┘
```

---

## BÖLÜM 4 — Kavramlar (Hoca Sorarsa Kısa Cevap)

### Interface (Arayüz) — `ILibraryRepository`
> "Veriyi nereden okuduğumuzu soyutluyoruz. SQLite veya JSON fark etmez; aynı `Load` ve `Save` metotları kullanılır."

### Encapsulation (Kapsülleme)
> "Listeler private; dışarıdan sadece `AddBook`, `BorrowBook` gibi metotlarla erişiliyor."

### Model (Model sınıfı)
> "Verinin şekli: User, Book, Loan. Veritabanı satırının C# karşılığı."

### Service (Servis)
> "İş kuralları: 'Müsait kopya yoksa ödünç verme', 'Telefon 05 ile başlamalı' gibi."

### Repository (Depo)
> "Veritabanına okuma/yazma. UI doğrudan SQLite'a dokunmaz."

### Dependency Injection (Bağımlılık enjeksiyonu) — Web'de
> "Web sayfası `LibraryService`'i constructor'dan alır; kendisi oluşturmaz. `Program.cs`'te kayıtlıdır."

---

## BÖLÜM 5 — Her Teknolojinin Tek Cümlelik Özeti (Ezber Kartları)

| Teknoloji | Tek cümle |
|-----------|-----------|
| **C#** | Projenin yazıldığı ana dil |
| **.NET 10** | Uygulamanın çalıştığı platform |
| **SQLite** | Verilerin saklandığı dosya veritabanı |
| **LINQ** | Listeleri sorgulama aracı |
| **Razor Pages** | Web sayfaları (HTML + C#) |
| **Bootstrap** | Web sayfası tasarımı |
| **WinForms** | Masaüstü pencere arayüzü |
| **Konsol** | Metin menülü arayüz |
| **JSON** | Veri dışa aktarım formatı |
| **ClosedXML** | Excel raporu |
| **QuestPDF** | PDF raporu |
| **Microsoft.Data.Sqlite** | C#'tan SQLite'a bağlanma paketi |

---

## BÖLÜM 6 — Sunumda Gösterebileceğin Demo Sırası

1. WinForms veya Web'i aç  
2. Yeni kullanıcı ekle → telefon doğrulamasını anlat (05, 11 hane)  
3. Kitap ödünç ver → `BorrowBook` akışını anlat  
4. Diğer arayüzde listeyi göster → senkronizasyonu anlat (~2 sn)  
5. `data/kutuphane.db` ve `kutuphane-verileri.json` dosyalarını göster  
6. İsteğe bağlı: Excel veya PDF raporu aç  

---

## BÖLÜM 7 — Muhtemel Hoca Soruları (Bu Dosyadan Çalış)

**S: Hangi dilleri kullandınız?**  
C: Ana dil C#. Web tarafında HTML/CSS (Bootstrap). Veri formatı JSON. Veritabanı SQLite.

**S: Neden 3 arayüz?**  
C: Aynı iş mantığını farklı kullanıcı senaryolarına sunmak — terminal, masaüstü, tarayıcı.

**S: Veriler nerede?**  
C: `kutuphane.db` SQLite dosyası. Her işlemden sonra JSON/Excel/PDF raporları da güncellenir.

**S: LINQ ne işe yaradı?**  
C: Arama, istatistik ve geciken listelerde filtreleme/sıralama/gruplama.

**S: Web ile WinForms nasıl senkron?**  
C: Aynı DB dosyası; dosya değişince `ReloadIfChanged()` belleği yeniler. WinForms'ta 2 sn timer, web'de her istek.

**S: Hangi NuGet paketleri?**  
C: Microsoft.Data.Sqlite, ClosedXML, QuestPDF.

**S: OOP nerede?**  
C: Sınıflar (User, Book), encapsulation (private listeler), interface (ILibraryRepository), polimorfizm (SQLite/JSON repo).

---

## BÖLÜM 8 — Çalışma Kontrol Listesi

Sunumdan önce işaretle:

- [ ] Projeyi tek cümleyle anlatabiliyorum  
- [ ] 4 katmanı sayabiliyorum (arayüz, servis, model, veri)  
- [ ] C#, SQLite, LINQ, Razor, WinForms ne işe yarar biliyorum  
- [ ] Ödünç ver akışını baştan sona anlatabiliyorum (`SUNUM.md` Bölüm 5)  
- [ ] Telefon doğrulamasını anlatabiliyorum  
- [ ] Senkronizasyonu anlatabiliyorum  
- [ ] Demo'yu en az bir kez denedim  
- [ ] `data/` klasöründeki dosyaları bulabiliyorum  

---

## BÖLÜM 9 — Hızlı Komutlar (Demo İçin)

```powershell
# WinForms
dotnet run --project src/LibraryManagement.WinForms

# Web
dotnet run --project src/LibraryManagement.Web --urls "http://localhost:5180"

# Konsol
dotnet run --project src/LibraryManagement.Console
```

**Veri klasörü:**  
`src/LibraryManagement.Console/data/`

---

## BÖLÜM 10 — Diğer Not Dosyaların

| Dosya | Ne için? |
|-------|----------|
| **BASLAT.md** | Terminale yazılacak komutlar (projeyi başlat) |
| **NE-NEREDE.md** | Özellik → dosya + satır numarası (hızlı bul) |
| **SUNUM-HAZIRLIK.md** | Bu dosya — teknolojiler, kavramlar, ezber kartları |
| **SUNUM.md** | Kod akışları, buton örnekleri, adım adım anlatım |
| **AGENTS.md** | Teknik proje rehberi |
| **README.md** | Genel proje özeti |

---

*Son güncelleme: Proje sürümü .NET 10 — Kütüphane Yönetim Sistemi*
