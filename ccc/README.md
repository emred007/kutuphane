# Kütüphane Yönetim Sistemi

C# ile geliştirilmiş kütüphane yönetim sistemi. Kullanıcıları ve kitapları yönetir; ödünç alma, iade, rezervasyon ve raporlama işlemlerini kaydeder.

## Özellikler

- Kullanıcı oluşturma
- Kitap ekleme, düzenleme, silme
- Çoklu kopya desteği
- Kitap ödünç verme ve iade alma
- Ödünç süresi ayarlama (varsayılan 14 gün)
- Geciken iadeleri listeleme
- Kullanıcı ve kitap arama
- Rezervasyon sistemi
- İstatistik paneli (en çok okunan kitaplar, en aktif kullanıcılar)
- Excel ve HTML rapor dışa aktarma
- Veritabanı yedekleme
- Web arayüzü (ASP.NET Razor Pages)

## Proje Yapısı

```
src/
  LibraryManagement/          # Ana kütüphane (modeller + servis + SQLite)
  LibraryManagement.Console/  # Konsol arayüzü
  LibraryManagement.WinForms/ # Windows Forms grafik arayüzü
  LibraryManagement.Web/      # Web arayüzü
```

## Kalıcı Veri (SQLite)

Tüm veriler **SQLite veritabanında** saklanır: `data/kutuphane.db`

Veritabanı tabloları:
- `Kullanicilar`
- `Kitaplar`
- `OduncKayitlari`
- `Rezervasyonlar`
- `Ayarlar`

### Okunabilir dışa aktarımlar

Her veri değişikliğinde veya rapor menüsünden şu dosyalar güncellenir:

| Dosya | Açıklama |
|-------|----------|
| `kutuphane-verileri.json` | Tüm veriler JSON formatında |
| `kutuphane-raporu.txt` | Okunabilir metin raporu |
| `kutuphane-raporu.xlsx` | Excel raporu |
| `kutuphane-raporu.html` | HTML raporu |
| `kutuphane-raporu.pdf` | PDF raporu |
| `yedekler/` | Veritabanı yedekleri |

## Çalıştırma

### Grafik arayüz (önerilen)

```bash
dotnet run --project src/LibraryManagement.WinForms
```

Sekmeler: Kullanıcılar, Kitaplar, Ödünç İşlemleri, Geçmiş, Rapor, Geciken İadeler, Arama, İstatistikler, Rezervasyon, Ayarlar.

### Web arayüzü

```bash
dotnet run --project src/LibraryManagement.Web
```

Tarayıcıda `http://localhost:5xxx` adresini açın (port terminalde gösterilir).

### Konsol uygulaması

```bash
dotnet run --project src/LibraryManagement.Console
```

Menü seçenekleri 0–18 arası tüm özellikleri kapsar.

## Modeller

| Model | Açıklama |
|-------|----------|
| `User` | Kütüphane üyesi |
| `Book` | Kütüphanedeki kitap (çoklu kopya) |
| `Loan` | Ödünç alma kaydı (son iade tarihi ile) |
| `LoanRecord` | Kullanıcı + kitap + ödünç bilgisi |
| `Reservation` | Kitap rezervasyonu |
| `LibrarySettings` | Ödünç süresi vb. ayarlar |
| `LibraryStatistics` | İstatistik özeti |
