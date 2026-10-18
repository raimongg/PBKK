using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CampusApp
{
    /// <summary>
    /// ViewModel utama yang mengelola logika bisnis, koleksi ObservableCollection,
    /// perintah CRUD (Tambah, Edit/Update, Hapus, Cari, Reset), serta ringkasan statistik.
    /// </summary>
    public class StudentViewModel : INotifyPropertyChanged
    {
        private readonly StudentRepository _repository;

        private Student _currentStudent;
        private Student? _selectedStudent;
        private bool _isEditMode;
        private string _formTitle = "Tambah Mahasiswa Baru";
        private string _saveButtonText = "Simpan Mahasiswa";
        private string _searchKeyword = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isStatusError;
        private bool _hasStatusMessage;

        private int _totalMahasiswa;
        private int _totalJurusan;
        private string _jurusanTerbanyak = "-";

        public ObservableCollection<Student> Students { get; }
        public ObservableCollection<string> JurusanList { get; }

        public Student CurrentStudent
        {
            get => _currentStudent;
            set => SetProperty(ref _currentStudent, value);
        }

        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value) && value != null && !_isEditMode)
                {
                    // Opsional: saat memilih baris, bisa melihat data
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    FormTitle = value ? "Edit Data Mahasiswa" : "Tambah Mahasiswa Baru";
                    SaveButtonText = value ? "Perbarui Data (Update)" : "Simpan Mahasiswa";
                    OnPropertyChanged(nameof(IsAddMode));
                }
            }
        }

        public bool IsAddMode => !IsEditMode;

        public string FormTitle
        {
            get => _formTitle;
            set => SetProperty(ref _formTitle, value);
        }

        public string SaveButtonText
        {
            get => _saveButtonText;
            set => SetProperty(ref _saveButtonText, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    // Pencarian otomatis saat teks berubah jika diinginkan
                    PerformSearch();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsStatusError
        {
            get => _isStatusError;
            set => SetProperty(ref _isStatusError, value);
        }

        public bool HasStatusMessage
        {
            get => _hasStatusMessage;
            set => SetProperty(ref _hasStatusMessage, value);
        }

        // ================= STATISTIK =================
        public int TotalMahasiswa
        {
            get => _totalMahasiswa;
            set => SetProperty(ref _totalMahasiswa, value);
        }

        public int TotalJurusan
        {
            get => _totalJurusan;
            set => SetProperty(ref _totalJurusan, value);
        }

        public string JurusanTerbanyak
        {
            get => _jurusanTerbanyak;
            set => SetProperty(ref _jurusanTerbanyak, value);
        }

        // ================= COMMANDS =================
        public ICommand SimpanCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand HapusCommand { get; }
        public ICommand BatalEditCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetSearchCommand { get; }

        public StudentViewModel()
        {
            _repository = new StudentRepository();

            Students = new ObservableCollection<Student>();
            JurusanList = new ObservableCollection<string>
            {
                "Teknik Informatika",
                "Sistem Informasi",
                "Teknik Komputer",
                "Manajemen Informatika",
                "Sains Data"
            };

            _currentStudent = new Student { Jurusan = JurusanList[0] };

            SimpanCommand = new RelayCommand(Simpan);
            EditCommand = new RelayCommand<Student>(MulaiEdit);
            HapusCommand = new RelayCommand<Student>(Hapus);
            BatalEditCommand = new RelayCommand(ResetForm);
            SearchCommand = new RelayCommand(PerformSearch);
            ResetSearchCommand = new RelayCommand(ResetSearch);

            LoadData();
        }

        /// <summary>
        /// Memuat data mahasiswa dari database ke ObservableCollection dan menghitung statistik.
        /// </summary>
        public void LoadData()
        {
            var data = _repository.GetAll();
            Students.Clear();
            foreach (var item in data)
            {
                Students.Add(item);
            }

            UpdateStatistics();
        }

        /// <summary>
        /// Logika Simpan: jika IsEditMode = false maka INSERT (Tambah), jika true maka UPDATE (Perbarui).
        /// </summary>
        private void Simpan()
        {
            // 1. Validasi Input
            if (string.IsNullOrWhiteSpace(CurrentStudent.Nim))
            {
                SetStatus("NIM wajib diisi!", isError: true);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentStudent.Nama))
            {
                SetStatus("Nama lengkap mahasiswa wajib diisi!", isError: true);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentStudent.Jurusan))
            {
                SetStatus("Program Studi/Jurusan wajib dipilih!", isError: true);
                return;
            }

            // 2. Eksekusi Tambah atau Update
            if (!IsEditMode)
            {
                // MODE TAMBAH (INSERT)
                if (_repository.IsNimExists(CurrentStudent.Nim))
                {
                    SetStatus($"Gagal: Mahasiswa dengan NIM '{CurrentStudent.Nim}' sudah terdaftar!", isError: true);
                    return;
                }

                try
                {
                    _repository.Insert(CurrentStudent);
                    SetStatus($"Berhasil: Mahasiswa '{CurrentStudent.Nama}' berhasil ditambahkan ke database.", isError: false);
                    ResetForm();
                    LoadData();
                }
                catch (Exception ex)
                {
                    SetStatus($"Terjadi kesalahan saat menyimpan: {ex.Message}", isError: true);
                }
            }
            else
            {
                // MODE EDIT (UPDATE)
                if (_repository.IsNimExists(CurrentStudent.Nim, CurrentStudent.Id))
                {
                    SetStatus($"Gagal: NIM '{CurrentStudent.Nim}' sudah digunakan oleh mahasiswa lain!", isError: true);
                    return;
                }

                try
                {
                    bool updated = _repository.Update(CurrentStudent);
                    if (updated)
                    {
                        SetStatus($"Berhasil: Data mahasiswa '{CurrentStudent.Nama}' berhasil diperbarui!", isError: false);
                        ResetForm();
                        LoadData();
                    }
                    else
                    {
                        SetStatus("Gagal memperbarui data mahasiswa.", isError: true);
                    }
                }
                catch (Exception ex)
                {
                    SetStatus($"Terjadi kesalahan saat memperbarui: {ex.Message}", isError: true);
                }
            }
        }

        /// <summary>
        /// Memulai mode edit untuk mahasiswa yang dipilih (mengisi form dengan data clone).
        /// </summary>
        private void MulaiEdit(Student? student)
        {
            var target = student ?? SelectedStudent;
            if (target == null) return;

            CurrentStudent = target.Clone();
            IsEditMode = true;
            SetStatus($"Mode Edit Aktif: Mengubah data '{target.Nama}' (NIM: {target.Nim}).", isError: false);
        }

        /// <summary>
        /// Menghapus data mahasiswa dari database setelah konfirmasi pengguna.
        /// </summary>
        private void Hapus(Student? student)
        {
            var target = student ?? SelectedStudent;
            if (target == null) return;

            var result = MessageBox.Show(
                $"Apakah Anda yakin ingin menghapus data mahasiswa berikut?\n\nNama : {target.Nama}\nNIM  : {target.Nim}\nJurusan: {target.Jurusan}",
                "Konfirmasi Hapus Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool deleted = _repository.Delete(target.Id);
                    if (deleted)
                    {
                        if (IsEditMode && CurrentStudent.Id == target.Id)
                        {
                            ResetForm();
                        }

                        SetStatus($"Data mahasiswa '{target.Nama}' berhasil dihapus dari database.", isError: false);
                        LoadData();
                    }
                    else
                    {
                        SetStatus("Gagal menghapus data mahasiswa.", isError: true);
                    }
                }
                catch (Exception ex)
                {
                    SetStatus($"Terjadi kesalahan saat menghapus: {ex.Message}", isError: true);
                }
            }
        }

        /// <summary>
        /// Mengembalikan form ke keadaan kosong (mode Tambah).
        /// </summary>
        private void ResetForm()
        {
            CurrentStudent = new Student
            {
                Jurusan = JurusanList.FirstOrDefault() ?? "Teknik Informatika"
            };
            IsEditMode = false;
        }

        /// <summary>
        /// Menjalankan pencarian mahasiswa berdasarkan SearchKeyword.
        /// </summary>
        private void PerformSearch()
        {
            var results = _repository.Search(SearchKeyword);
            Students.Clear();
            foreach (var item in results)
            {
                Students.Add(item);
            }

            UpdateStatistics();
        }

        /// <summary>
        /// Mengatur ulang kolom pencarian dan memuat semua data kembali.
        /// </summary>
        private void ResetSearch()
        {
            _searchKeyword = string.Empty;
            OnPropertyChanged(nameof(SearchKeyword));
            LoadData();
            SetStatus("Pencarian direset, menampilkan semua data.", isError: false);
        }

        /// <summary>
        /// Memperbarui informasi panel statistik (Total Mahasiswa, Jurusan, Jurusan Terbanyak).
        /// </summary>
        private void UpdateStatistics()
        {
            TotalMahasiswa = Students.Count;

            var distinctJurusan = Students
                .Where(s => !string.IsNullOrWhiteSpace(s.Jurusan))
                .Select(s => s.Jurusan)
                .Distinct()
                .ToList();

            TotalJurusan = distinctJurusan.Count;

            if (Students.Count > 0)
            {
                var topGroup = Students
                    .GroupBy(s => s.Jurusan)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                JurusanTerbanyak = topGroup != null ? $"{topGroup.Key} ({topGroup.Count()})" : "-";
            }
            else
            {
                JurusanTerbanyak = "-";
            }
        }

        /// <summary>
        /// Menampilkan pesan status (Sukses atau Error) pada banner UI.
        /// </summary>
        private void SetStatus(string message, bool isError)
        {
            StatusMessage = message;
            IsStatusError = isError;
            HasStatusMessage = !string.IsNullOrWhiteSpace(message);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
