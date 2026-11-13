using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace CampusApp.Models
{
    /// <summary>
    /// Model data mahasiswa yang mengimplementasikan INotifyPropertyChanged dan IDataErrorInfo untuk validasi input.
    /// Mendukung pemetaan ke database MySQL.
    /// </summary>
    public class Mahasiswa : INotifyPropertyChanged, IDataErrorInfo
    {
        private int _id;
        private string _nama = string.Empty;
        private string _nim = string.Empty;
        private string _programStudi = "Teknik Informatika";
        private DateTime? _tanggalLahir = new DateTime(2003, 1, 1);
        private string _jenisKelamin = "Laki-laki";
        private string _email = string.Empty;
        private string _alamat = string.Empty;
        private string _noTelepon = string.Empty;
        private DateTime _waktuDaftar = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Nama
        {
            get => _nama;
            set
            {
                if (_nama != value)
                {
                    _nama = value;
                    OnPropertyChanged(nameof(Nama));
                    OnPropertyChanged(nameof(Inisial));
                }
            }
        }

        public string Nim
        {
            get => _nim;
            set
            {
                if (_nim != value)
                {
                    _nim = value;
                    OnPropertyChanged(nameof(Nim));
                }
            }
        }

        public string ProgramStudi
        {
            get => _programStudi;
            set
            {
                if (_programStudi != value)
                {
                    _programStudi = value;
                    OnPropertyChanged(nameof(ProgramStudi));
                }
            }
        }

        public DateTime? TanggalLahir
        {
            get => _tanggalLahir;
            set
            {
                if (_tanggalLahir != value)
                {
                    _tanggalLahir = value;
                    OnPropertyChanged(nameof(TanggalLahir));
                    OnPropertyChanged(nameof(Umur));
                }
            }
        }

        public string JenisKelamin
        {
            get => _jenisKelamin;
            set
            {
                if (_jenisKelamin != value)
                {
                    _jenisKelamin = value;
                    OnPropertyChanged(nameof(JenisKelamin));
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        public string Alamat
        {
            get => _alamat;
            set
            {
                if (_alamat != value)
                {
                    _alamat = value;
                    OnPropertyChanged(nameof(Alamat));
                }
            }
        }

        public string NoTelepon
        {
            get => _noTelepon;
            set
            {
                if (_noTelepon != value)
                {
                    _noTelepon = value;
                    OnPropertyChanged(nameof(NoTelepon));
                }
            }
        }

        public DateTime WaktuDaftar
        {
            get => _waktuDaftar;
            set
            {
                if (_waktuDaftar != value)
                {
                    _waktuDaftar = value;
                    OnPropertyChanged(nameof(WaktuDaftar));
                }
            }
        }

        public string Inisial
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nama)) return "M";
                var parts = Nama.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
                return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
            }
        }

        public int Umur
        {
            get
            {
                if (!TanggalLahir.HasValue) return 0;
                var today = DateTime.Today;
                var age = today.Year - TanggalLahir.Value.Year;
                if (TanggalLahir.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        #region IDataErrorInfo Members

        public string Error => null!;

        public string this[string columnName]
        {
            get
            {
                string? result = null;

                switch (columnName)
                {
                    case nameof(Nama):
                        if (string.IsNullOrWhiteSpace(Nama))
                            result = "Nama lengkap wajib diisi.";
                        else if (Nama.Trim().Length < 3)
                            result = "Nama minimal 3 karakter.";
                        break;

                    case nameof(Nim):
                        if (string.IsNullOrWhiteSpace(Nim))
                            result = "NIM wajib diisi.";
                        else if (!Regex.IsMatch(Nim.Trim(), @"^\d+$"))
                            result = "NIM hanya boleh berisi angka.";
                        else if (Nim.Trim().Length < 5 || Nim.Trim().Length > 15)
                            result = "Panjang NIM antara 5 sampai 15 digit angka.";
                        break;

                    case nameof(ProgramStudi):
                        if (string.IsNullOrWhiteSpace(ProgramStudi))
                            result = "Program Studi wajib dipilih.";
                        break;

                    case nameof(NoTelepon):
                        if (string.IsNullOrWhiteSpace(NoTelepon))
                            result = "Nomor telepon wajib diisi.";
                        else if (!Regex.IsMatch(NoTelepon.Trim(), @"^(\+62|62|0)[0-9]{8,13}$"))
                            result = "Format nomor telepon tidak valid (contoh: 08123456789 atau +628123456789).";
                        break;

                    case nameof(TanggalLahir):
                        if (!TanggalLahir.HasValue)
                            result = "Tanggal lahir wajib dipilih.";
                        else if (TanggalLahir.Value > DateTime.Today.AddYears(-15))
                            result = "Calon mahasiswa minimal berusia 15 tahun.";
                        break;

                    case nameof(Email):
                        if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                            result = "Format email tidak valid (contoh: nama@domain.com).";
                        break;
                }

                return result ?? string.Empty;
            }
        }

        public bool IsValid =>
            string.IsNullOrEmpty(this[nameof(Nama)]) &&
            string.IsNullOrEmpty(this[nameof(Nim)]) &&
            string.IsNullOrEmpty(this[nameof(ProgramStudi)]) &&
            string.IsNullOrEmpty(this[nameof(NoTelepon)]) &&
            string.IsNullOrEmpty(this[nameof(TanggalLahir)]) &&
            string.IsNullOrEmpty(this[nameof(Email)]);

        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Mahasiswa Clone()
        {
            return new Mahasiswa
            {
                Id = this.Id,
                Nama = this.Nama,
                Nim = this.Nim,
                ProgramStudi = this.ProgramStudi,
                TanggalLahir = this.TanggalLahir,
                JenisKelamin = this.JenisKelamin,
                Email = this.Email,
                Alamat = this.Alamat,
                NoTelepon = this.NoTelepon,
                WaktuDaftar = this.WaktuDaftar
            };
        }
    }
}
