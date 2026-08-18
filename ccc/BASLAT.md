# BAŞLAT — Terminale Yazılacak Komutlar

Proje kök klasörü:
```
C:\Users\Excalibur\OneDrive\Desktop\ccc
```

Sunum / demo öncesi terminali aç, önce proje klasörüne git:

```powershell
cd "C:\Users\Excalibur\OneDrive\Desktop\ccc"
```

---

## 1) Projeyi Derle (İlk Sefer veya Kod Değiştiyse)

```powershell
dotnet build
```

Hata alırsan (DLL kilitli): aşağıdaki **Durdur** bölümünü uygula, sonra tekrar dene.

---

## 2) WinForms (Masaüstü) — Önerilen

```powershell
dotnet run --project src/LibraryManagement.WinForms
```

Pencere açılır. Kapatmak için pencereyi kapat veya terminalde `Ctrl+C`.

---

## 3) Web (Tarayıcı)

**Sabit port ile (sunum için önerilir):**
```powershell
dotnet run --project src/LibraryManagement.Web --urls "http://localhost:5180"
```

Tarayıcıda aç: **http://localhost:5180**

**Varsayılan port ile:**
```powershell
dotnet run --project src/LibraryManagement.Web
```

Terminalde `Now listening on: http://localhost:....` satırındaki adresi kullan (genelde **5044**).

---

## 4) Konsol (Metin Menüsü)

```powershell
dotnet run --project src/LibraryManagement.Console
```

Menüden numara seç. Çıkmak için `0`.

---

## 5) Sunum İçin: Web + WinForms Birlikte

**İki ayrı terminal penceresi aç.**

**Terminal 1 — Web:**
```powershell
cd "C:\Users\Excalibur\OneDrive\Desktop\ccc"
dotnet run --project src/LibraryManagement.Web --urls "http://localhost:5180"
```

**Terminal 2 — WinForms:**
```powershell
cd "C:\Users\Excalibur\OneDrive\Desktop\ccc"
dotnet run --project src/LibraryManagement.WinForms
```

Web'de eklediğin veri WinForms'ta ~2 saniye içinde görünür (senkronizasyon).

---

## 6) Sadece Veri / Klasik Kitap Seed

```powershell
dotnet run --project src/LibraryManagement.Console -- --seed-only
```

---

## 7) Çalışan Uygulamaları Durdur (Derleme Kilitlenirse)

```powershell
Get-Process -Name "LibraryManagement.WinForms","LibraryManagement.Web" -ErrorAction SilentlyContinue | Stop-Process -Force
```

Sonra tekrar:
```powershell
dotnet build
```

---

## 8) Hızlı Kontrol — Web Ayakta mı?

```powershell
Invoke-WebRequest -Uri "http://localhost:5180" -UseBasicParsing
```

`200` dönerse web çalışıyor.

---

## Özet Tablo (Kopyala-Yapıştır)

| Ne? | Komut |
|-----|--------|
| Klasöre git | `cd "C:\Users\Excalibur\OneDrive\Desktop\ccc"` |
| Derle | `dotnet build` |
| WinForms | `dotnet run --project src/LibraryManagement.WinForms` |
| Web | `dotnet run --project src/LibraryManagement.Web --urls "http://localhost:5180"` |
| Konsol | `dotnet run --project src/LibraryManagement.Console` |
| Web adresi | http://localhost:5180 |
| Durdur | Pencereyi kapat veya terminalde `Ctrl+C` |

---

## Veri Dosyası Konumu

```
src\LibraryManagement.Console\data\kutuphane.db
```

---

## Diğer Not Dosyaların

| Dosya | Ne için? |
|-------|----------|
| **BASLAT.md** | Bu dosya — terminal komutları |
| **NE-NEREDE.md** | Kod dosya + satır numarası |
| **SUNUM-HAZIRLIK.md** | Sunum çalışma notları |
| **SUNUM.md** | Kod akışları |
