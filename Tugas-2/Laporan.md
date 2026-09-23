# Week 03 - Membuat Aplikasi Kalkulator Sederhana

**Nama**: Raihan Rasyid  
**NRP**: 5025241224  
**Mata Kuliah**: PBKK D
**Platform**: .NET 8.0 (`net8.0`)  

---

## 1. Tujuan
Membangun aplikasi kalkulator berbasis C# pada platform .NET dengan fokus pada:
- Pengelolaan alur program dan interaksi pengguna melalui sistem kendali *event-driven*.
- Penerapan konsep Object-Oriented Programming (OOP) dengan memisahkan logika komputasi matematika dan pengendali antarmuka (*Separation of Concerns*).
- Implementasi logika aritmatika standar serta fitur lanjutan: operasi **Pangkat ($x^y$)** dan **Akar Kuadrat ($\sqrt{x}$)**.
- Penerapan mekanisme penanganan kesalahan (*exception handling*) untuk mencegah terjadinya penghentian paksa (*crash*) pada program.

---

## 2. Struktur Berkas yang Dikumpulkan

Berikut adalah berkas kode sumber utama proyek yang dikumpulkan:

```text
KalkulatorApp/
├── Program.cs             # Titik masuk utama eksekusi program (Entry Point)
├── CalculatorEngine.cs    # Class logika perhitungan matematika murni (OOP Engine)
├── MainWindow.axaml.cs    # Pengendali logika tampilan, event handling, & input keyboard
└── KalkulatorApp.csproj   # Berkas konfigurasi proyek .NET 8 dan dependensi pustaka
```

### Penjelasan Peran Setiap Berkas:

1. **[`CalculatorEngine.cs`](file:///home/raihanrasyid/code/KalkulatorApp/CalculatorEngine.cs)**:
   Class murni (*Pure C# Class*) yang bertindak sebagai mesin komputasi tanpa ketergantungan visual. Berkas ini bertanggung jawab mengolah perhitungan aritmatika (`+`, `-`, `×`, `÷`), operasi pangkat (`^`), operasi akar kuadrat (`√`), serta memvalidasi aturan matematika.

2. **[`MainWindow.axaml.cs`](file:///home/raihanrasyid/code/KalkulatorApp/MainWindow.axaml.cs)**:
   Berkas pengendali (*Code-Behind*) yang menangani seluruh logika interaksi program, mulai dari manajemen *state* kalkulator, penanganan event klik tombol (*event handler*), eksekusi instan operasi akar, pemrosesan tombol keyboard fisik, hingga penanganan error di tingkat antarmuka.

3. **[`Program.cs`](file:///home/raihanrasyid/code/KalkulatorApp/Program.cs)**:
   Titik awal eksekusi (*entry point*) yang menginisialisasi lingkungan runtime aplikasi, mengatur threading antarmuka grafis, dan menjalankan siklus proses (*message loop*) program.

4. **[`KalkulatorApp.csproj`](file:///home/raihanrasyid/code/KalkulatorApp/KalkulatorApp.csproj)**:
   Berkas XML berbasis MSBuild yang mendefinisikan target framework (`net8.0`), tipe output eksekusi (`WinExe`), pengaturan kompilasi C#, serta paket dependensi antarmuka grafis yang digunakan.

---

## 3. Struktur Eksekusi Utama: Program.cs

Berkas [`Program.cs`](file:///home/raihanrasyid/code/KalkulatorApp/Program.cs) bertindak sebagai titik masuk (*entry point*) aplikasi yang menyiapkan lingkungan eksekusi sebelum jendela program dijalankan.

### Penjelasan Teknis:
- **`[STAThread]`**:
  Atribut wajib yang menginstruksikan sistem operasi agar thread utama berjalan dalam model *Single-Threaded Apartment*, memastikan komponen UI dapat berinteraksi secara aman tanpa terjadi konflik memori atau *thread race condition*.
- **`BuildAvaloniaApp()`**:
  Fungsi konfigurasi yang mengaktifkan deteksi platform sistem operasi (`UsePlatformDetect`), font antarmuka sistem (`WithInterFont`), serta pencatatan log diagnostik (`LogToTrace`).
- **`StartWithClassicDesktopLifetime(args)`**:
  Memulai siklus hidup aplikasi desktop dan memasukkannya ke dalam pemrosesan *message loop*, sehingga program tetap aktif berjalan dan merespons interaksi hingga jendela ditutup oleh pengguna.

---

## 4. Pusat Kendali Logika dan Interaksi: CalculatorEngine.cs & MainWindow.axaml.cs

Sistem dirancang dengan memisahkan modul komputasi matematika dengan pengendali antarmuka pengguna:

### 4.1 Logika Perhitungan Matematika (`CalculatorEngine.cs`)
- **Operasi Biner (`PerformOperation`)**:
  Menjalankan operasi dua operand menggunakan struktur percabangan `switch`:
  - Penjumlahan (`+`), Pengurangan (`-`), Perkalian (`×`/`*`), dan Pembagian (`÷`/`/`).
  - **Operasi Pangkat (`^`)**: Dieksekusi menggunakan pustaka bawaan `Math.Pow(operand1, operand2)`. Dilengkapi pengecekan khusus jika basis bernilai 0 dengan pangkat negatif ($0^{-n}$) untuk melempar `DivideByZeroException`.
  - Pengecekan nilai tak terdefinisi (`double.IsNaN`) dan nilai melampaui batas (`double.IsInfinity`).
- **Operasi Unari (`PerformUnaryOperation`)**:
  - **Operasi Akar Kuadrat (`√`)**: Dieksekusi menggunakan pustaka bawaan `Math.Sqrt(operand)`. Dilengkapi validasi kondisi (`operand < 0`) untuk menolak akar bilangan negatif dengan melempar `ArgumentException`.

### 4.2 Pengendali Tampilan & Event Handler (`MainWindow.axaml.cs`)
- **Manajemen Status (*State Management*)**:
  Menyimpan variabel `firstValue`, `currentOperator`, serta penanda status (`isOperatorClicked` dan `isResultShown`) untuk mendukung alur operasi bertahap (*chaining*) tanpa harus menekan tombol sama dengan di setiap tahap.
- **Kalkulasi Akar Cepat (`CalculateSquareRoot`)**:
  Mengeksekusi akar kuadrat secara instan pada nilai yang aktif di layar. Jika ditekan di tengah operasi berjalan (misal: pengguna mengetik `10 + 9` lalu menekan `√`), nilai `9` langsung dikonversi menjadi `3` dan label status menampilkan `10 + √(9)`.
- **Dukungan Keyboard Fisik (`OnKeyDown`)**:
  Menangkap event penekanan keyboard:
  - Angka `0`-`9` dan Numpad `0`-`9`.
  - Operator aritmatika (`+`, `-`, `*`, `/`).
  - Pangkat: Kombinasi tombol <kbd>Shift</kbd> + <kbd>6</kbd> (`^`).
  - Akar: Tombol <kbd>R</kbd> (*Root*).
  - Eksekusi: <kbd>Enter</kbd> (sama dengan).
  - Hapus: <kbd>Backspace</kbd> (hapus digit terakhir) dan <kbd>Escape</kbd> / <kbd>C</kbd> (reset total).

---

## 5. Penanganan Kesalahan (Exception Handling)

Implementasi blok `try-catch` diterapkan pada `MainWindow.axaml.cs` saat memanggil metode dari `CalculatorEngine.cs` untuk menangkal penghentian aplikasi secara paksa (*crash*):

- **`DivideByZeroException`**:
  Ditangkap secara spesifik saat terjadi pembagian dengan angka nol atau pangkat $0^{-n}$. Aplikasi menampilkan pesan peringatan `"Tidak dapat membagi dengan nol."` pada label operasi serta mereset tampilan secara aman.
- **`ArgumentException`**:
  Ditangkap ketika pengguna mencoba menghitung akar dari bilangan negatif, memunculkan notifikasi `"Tidak dapat menghitung akar dari bilangan negatif."`.
- **`InvalidOperationException` & `OverflowException`**:
  Menangani kasus hasil komputasi yang bukan bilangan riil atau hasil yang melampaui batas representasi numerik.
- **`FormatException`**:
  Mencegah kegagalan parsing nilai teks menjadi tipe data numerik `double`.

---

## 6. Dokumentasi & Hasil Pengujian

### 6.1 Status Tampilan Program
- **Tampilan Awal**: Aplikasi berada dalam kondisi bersih menampilkan angka default `0` dengan label operasi kosong, siap menerima input dari klik maupun keyboard.

  
  <img width="433" height="651" alt="Screenshot From 2026-09-22 08-26-04" src="https://github.com/user-attachments/assets/73e2aec7-3969-4b8b-9417-818ee315b583" />

- **Tampilan Operasi Berjalan**: Label operasi menampilkan histori perhitungan secara transparan (contoh: `2 ^ 8 =` atau `10 + √(9)`).

  
  <img width="436" height="648" alt="Screenshot From 2026-09-23 07-28-29" src="https://github.com/user-attachments/assets/32f138cc-85d7-457e-a4a0-644631b08195" />


- **Tampilan Operasi Pembagian**

  <img width="431" height="647" alt="image" src="https://github.com/user-attachments/assets/421c6f9a-776d-47f0-9987-27fbf8214355" />

- **Tampilan Operasi Perkalian**

<img width="436" height="648" alt="Screenshot From 2026-09-23 07-30-06" src="https://github.com/user-attachments/assets/62088281-d539-4a53-800f-ac7e780eab4e" />

- **Tampilan Operasi Pengurangan**

<img width="436" height="648" alt="Screenshot From 2026-09-23 07-30-23" src="https://github.com/user-attachments/assets/fa7f5b3a-4f00-4249-91fb-3614f350e0df" />

- **Tampilan Operasi Penjumlahan**

  <img width="436" height="648" alt="Screenshot From 2026-09-23 07-30-38" src="https://github.com/user-attachments/assets/342b10d3-97ba-4d89-bba9-0ecafb8cd8fc" />

