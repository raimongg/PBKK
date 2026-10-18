using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace CampusApp
{
    /// <summary>
    /// Repository untuk operasi database ADO.NET (CRUD + Pencarian Data Mahasiswa).
    /// Menggunakan SQLite secara lokal agar portabel, ringan, dan mandiri tanpa konfigurasi rumit.
    /// </summary>
    public class StudentRepository
    {
        private readonly string _connectionString;

        public StudentRepository(string dbName = "students.db")
        {
            _connectionString = $"Data Source={dbName}";
            InitializeDatabase();
        }

        /// <summary>
        /// Inisialisasi tabel SQLite dan seeder data awal jika tabel masih kosong.
        /// </summary>
        public void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nim TEXT NOT NULL UNIQUE,
                    Nama TEXT NOT NULL,
                    Jurusan TEXT NOT NULL,
                    Email TEXT,
                    NoHp TEXT,
                    Alamat TEXT
                );
            ";

            using (var cmd = new SqliteCommand(createTableSql, connection))
            {
                cmd.ExecuteNonQuery();
            }

            // Cek apakah data kosong, jika kosong tambahkan sample data awal
            string countSql = "SELECT COUNT(*) FROM Students;";
            using (var countCmd = new SqliteCommand(countSql, connection))
            {
                long count = (long)(countCmd.ExecuteScalar() ?? 0L);
                if (count == 0)
                {
                    string seedSql = @"
                        INSERT INTO Students (Nim, Nama, Jurusan, Email, NoHp, Alamat) VALUES
                        ('24010101', 'Aditya Pratama Putra', 'Teknik Informatika', 'aditya.putra@campus.ac.id', '081234567890', 'Jl. Merdeka Barat No. 45, Bandung'),
                        ('24020102', 'Clarissa Anindya Putri', 'Sistem Informasi', 'clarissa.anindya@campus.ac.id', '081987654321', 'Jl. Diponegoro No. 12, Surabaya'),
                        ('24030103', 'Fajar Hidayatullah', 'Teknik Komputer', 'fajar.hidayat@campus.ac.id', '082155667788', 'Jl. Sudirman No. 88, Jakarta'),
                        ('24040104', 'Nabila Zahra Khairunnisa', 'Sains Data', 'nabila.zahra@campus.ac.id', '085712349876', 'Jl. Malioboro No. 20, Yogyakarta');
                    ";
                    using var seedCmd = new SqliteCommand(seedSql, connection);
                    seedCmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Mengambil seluruh data mahasiswa dari database (Read/Select).
        /// </summary>
        public List<Student> GetAll()
        {
            var list = new List<Student>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = "SELECT Id, Nim, Nama, Jurusan, Email, NoHp, Alamat FROM Students ORDER BY Id DESC;";
            using var cmd = new SqliteCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Student
                {
                    Id = reader.GetInt32(0),
                    Nim = reader.GetString(1),
                    Nama = reader.GetString(2),
                    Jurusan = reader.GetString(3),
                    Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    NoHp = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Alamat = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                });
            }

            return list;
        }

        /// <summary>
        /// Menambahkan data mahasiswa baru ke database (Create/Insert).
        /// </summary>
        public int Insert(Student student)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO Students (Nim, Nama, Jurusan, Email, NoHp, Alamat)
                VALUES (@Nim, @Nama, @Jurusan, @Email, @NoHp, @Alamat);
                SELECT last_insert_rowid();
            ";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nim", student.Nim?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Nama", student.Nama?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Jurusan", student.Jurusan?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Email", student.Email?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@NoHp", student.NoHp?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Alamat", student.Alamat?.Trim() ?? string.Empty);

            long insertedId = (long)(cmd.ExecuteScalar() ?? 0L);
            student.Id = (int)insertedId;
            return student.Id;
        }

        /// <summary>
        /// Memperbarui data mahasiswa yang sudah ada (Update).
        /// </summary>
        public bool Update(Student student)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE Students
                SET Nim = @Nim,
                    Nama = @Nama,
                    Jurusan = @Jurusan,
                    Email = @Email,
                    NoHp = @NoHp,
                    Alamat = @Alamat
                WHERE Id = @Id;
            ";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", student.Id);
            cmd.Parameters.AddWithValue("@Nim", student.Nim?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Nama", student.Nama?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Jurusan", student.Jurusan?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Email", student.Email?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@NoHp", student.NoHp?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Alamat", student.Alamat?.Trim() ?? string.Empty);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus data mahasiswa berdasarkan Id (Delete).
        /// </summary>
        public bool Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM Students WHERE Id = @Id;";
            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Mencari data mahasiswa berdasarkan kata kunci (NIM, Nama, Jurusan, Email, atau Alamat).
        /// </summary>
        public List<Student> Search(string keyword)
        {
            var list = new List<Student>();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return GetAll();
            }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT Id, Nim, Nama, Jurusan, Email, NoHp, Alamat
                FROM Students
                WHERE Nim LIKE @kw
                   OR Nama LIKE @kw
                   OR Jurusan LIKE @kw
                   OR Email LIKE @kw
                   OR Alamat LIKE @kw
                ORDER BY Id DESC;
            ";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@kw", $"%{keyword.Trim()}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Student
                {
                    Id = reader.GetInt32(0),
                    Nim = reader.GetString(1),
                    Nama = reader.GetString(2),
                    Jurusan = reader.GetString(3),
                    Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    NoHp = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Alamat = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                });
            }

            return list;
        }

        /// <summary>
        /// Memeriksa apakah NIM sudah terdaftar (digunakan untuk validasi unik).
        /// </summary>
        public bool IsNimExists(string nim, int excludeId = 0)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = "SELECT COUNT(*) FROM Students WHERE LOWER(Nim) = LOWER(@Nim) AND Id != @ExcludeId;";
            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nim", nim.Trim());
            cmd.Parameters.AddWithValue("@ExcludeId", excludeId);

            long count = (long)(cmd.ExecuteScalar() ?? 0L);
            return count > 0;
        }
    }
}
