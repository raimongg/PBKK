using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CampusApp
{
    /// <summary>
    /// Model Mahasiswa (Student) yang mengimplementasikan INotifyPropertyChanged
    /// agar perubahan properti secara otomatis diperbarui pada UI WPF.
    /// </summary>
    public class Student : INotifyPropertyChanged
    {
        private int _id;
        private string _nim = string.Empty;
        private string _nama = string.Empty;
        private string _jurusan = "Teknik Informatika";
        private string _email = string.Empty;
        private string _noHp = string.Empty;
        private string _alamat = string.Empty;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Nim
        {
            get => _nim;
            set => SetProperty(ref _nim, value);
        }

        public string Nama
        {
            get => _nama;
            set => SetProperty(ref _nama, value);
        }

        public string Jurusan
        {
            get => _jurusan;
            set => SetProperty(ref _jurusan, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string NoHp
        {
            get => _noHp;
            set => SetProperty(ref _noHp, value);
        }

        public string Alamat
        {
            get => _alamat;
            set => SetProperty(ref _alamat, value);
        }

        /// <summary>
        /// Membuat salinan (deep copy sederhana) dari objek Student
        /// untuk mencegah perubahan data sebelum tombol update ditekan.
        /// </summary>
        public Student Clone()
        {
            return new Student
            {
                Id = this.Id,
                Nim = this.Nim,
                Nama = this.Nama,
                Jurusan = this.Jurusan,
                Email = this.Email,
                NoHp = this.NoHp,
                Alamat = this.Alamat
            };
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
