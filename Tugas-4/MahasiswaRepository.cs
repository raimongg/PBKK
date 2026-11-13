using System;
using System.Collections.Generic;
using MySqlConnector;
using CampusApp.Models;

namespace CampusApp.Repositories
{
    /// <summary>
    /// Repository untuk operasi database MySQL ADO.NET (CRUD + Pencarian Data Registrasi Mahasiswa).
    /// Menggunakan library MySqlConnector berkinerja tinggi.
    /// </summary>
    public class MahasiswaRepository
    {
        private readonly string _connectionString;

        public static readonly string DefaultConnectionString =
            "Server=localhost;Port=3306;Database=kostsync_db;User ID=kostsync_user;Password=kostsync_pass;AllowPublicKeyRetrieval=True;SslMode=None;CharSet=utf8mb4;";

        public MahasiswaRepository(string? connectionString = null)
        {
            _connectionString = connectionString 
                                ?? Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") 
                                ?? DefaultConnectionString;
            
            InitializeDatabase();
        }

        /// <summary>
        /// Menguji konektivitas ke server MySQL.
        /// </summary>
        public (bool Success, string Message) TestConnection()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                return (true, "Terhubung ke database MySQL dengan sukses.");
            }
            catch (Exception ex)
            {
                return (false, $"Gagal terhubung ke MySQL: {ex.Message}");
            }
        }

        /// <summary>
        /// Membuat tabel 'mahasiswa' di MySQL jika belum ada, serta melakukan seeding data awal jika kosong.
        /// </summary>
        public void InitializeDatabase()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS mahasiswa (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        nim VARCHAR(20) NOT NULL UNIQUE,
                        nama VARCHAR(150) NOT NULL,
                        program_studi VARCHAR(100) NOT NULL,
                        tanggal_lahir DATE NULL,
                        jenis_kelamin VARCHAR(20) NOT NULL DEFAULT 'Laki-laki',
                        email VARCHAR(100) NULL,
                        no_hp VARCHAR(20) NULL,
                        alamat TEXT NULL,
                        waktu_daftar DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                ";

                using (var cmd = new MySqlCommand(createTableSql, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Cek apakah tabel kosong untuk mengisi data awal
                string countSql = "SELECT COUNT(*) FROM mahasiswa;";
                using (var countCmd = new MySqlCommand(countSql, connection))
                {
                    long count = Convert.ToInt64(countCmd.ExecuteScalar() ?? 0);
                    if (count == 0)
                    {
                        string seedSql = @"
                            INSERT INTO mahasiswa (nim, nama, program_studi, tanggal_lahir, jenis_kelamin, email, no_hp, alamat, waktu_daftar) VALUES
                            ('24010101', 'Aditya Pratama Putra', 'Teknik Informatika', '2003-05-14', 'Laki-laki', 'aditya.putra@campus.ac.id', '081234567890', 'Jl. Merdeka Barat No. 45, Bandung', NOW() - INTERVAL 3 DAY),
                            ('24020102', 'Clarissa Anindya Putri', 'Sistem Informasi', '2003-09-21', 'Perempuan', 'clarissa.anindya@campus.ac.id', '081987654321', 'Jl. Diponegoro No. 12, Surabaya', NOW() - INTERVAL 2 DAY),
                            ('24030103', 'Fajar Hidayatullah', 'Manajemen Informatika', '2004-02-08', 'Laki-laki', 'fajar.hidayat@campus.ac.id', '082155667788', 'Jl. Sudirman No. 88, Jakarta Selatan', NOW() - INTERVAL 1 DAY),
                            ('24040104', 'Nabila Zahra Khairunnisa', 'Sains Data', '2003-11-19', 'Perempuan', 'nabila.zahra@campus.ac.id', '085712349876', 'Jl. Malioboro No. 20, Yogyakarta', NOW());
                        ";
                        using var seedCmd = new MySqlCommand(seedSql, connection);
                        seedCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MahasiswaRepository] Error saat inisialisasi database: {ex.Message}");
            }
        }

        /// <summary>
        /// Mengambil seluruh data mahasiswa dari MySQL (opsional dengan pencarian).
        /// </summary>
        public List<Mahasiswa> GetAll(string search = "")
        {
            var list = new List<Mahasiswa>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                string query = @"
                    SELECT id, nim, nama, program_studi, tanggal_lahir, jenis_kelamin, email, no_hp, alamat, waktu_daftar 
                    FROM mahasiswa 
                ";

                bool hasSearch = !string.IsNullOrWhiteSpace(search);
                if (hasSearch)
                {
                    query += " WHERE nim LIKE @search OR nama LIKE @search OR program_studi LIKE @search ";
                }

                query += " ORDER BY id DESC;";

                using var cmd = new MySqlCommand(query, connection);
                if (hasSearch)
                {
                    cmd.Parameters.AddWithValue("@search", $"%{search.Trim()}%");
                }

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapReaderToMahasiswa(reader));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MahasiswaRepository] Error saat GetAll: {ex.Message}");
            }

            return list;
        }

        /// <summary>
        /// Mengambil satu data mahasiswa berdasarkan ID.
        /// </summary>
        public Mahasiswa? GetById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                string query = "SELECT id, nim, nama, program_studi, tanggal_lahir, jenis_kelamin, email, no_hp, alamat, waktu_daftar FROM mahasiswa WHERE id = @id LIMIT 1;";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapReaderToMahasiswa(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MahasiswaRepository] Error saat GetById: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Menambahkan data mahasiswa baru ke MySQL (CREATE / INSERT).
        /// Mengembalikan ID data yang baru ditambahkan.
        /// </summary>
        public int Insert(Mahasiswa m)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO mahasiswa (nim, nama, program_studi, tanggal_lahir, jenis_kelamin, email, no_hp, alamat, waktu_daftar)
                VALUES (@nim, @nama, @program_studi, @tanggal_lahir, @jenis_kelamin, @email, @no_hp, @alamat, @waktu_daftar);
                SELECT LAST_INSERT_ID();
            ";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@nim", m.Nim.Trim());
            cmd.Parameters.AddWithValue("@nama", m.Nama.Trim());
            cmd.Parameters.AddWithValue("@program_studi", m.ProgramStudi.Trim());
            cmd.Parameters.AddWithValue("@tanggal_lahir", m.TanggalLahir.HasValue ? m.TanggalLahir.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@jenis_kelamin", string.IsNullOrWhiteSpace(m.JenisKelamin) ? "Laki-laki" : m.JenisKelamin);
            cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(m.Email) ? (object)DBNull.Value : m.Email.Trim());
            cmd.Parameters.AddWithValue("@no_hp", string.IsNullOrWhiteSpace(m.NoTelepon) ? (object)DBNull.Value : m.NoTelepon.Trim());
            cmd.Parameters.AddWithValue("@alamat", string.IsNullOrWhiteSpace(m.Alamat) ? (object)DBNull.Value : m.Alamat.Trim());
            cmd.Parameters.AddWithValue("@waktu_daftar", m.WaktuDaftar != default ? m.WaktuDaftar : DateTime.Now);

            object? result = cmd.ExecuteScalar();
            int newId = Convert.ToInt32(result ?? 0);
            m.Id = newId;
            return newId;
        }

        /// <summary>
        /// Memperbarui data mahasiswa di MySQL (UPDATE).
        /// </summary>
        public bool Update(Mahasiswa m)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE mahasiswa
                SET nim = @nim,
                    nama = @nama,
                    program_studi = @program_studi,
                    tanggal_lahir = @tanggal_lahir,
                    jenis_kelamin = @jenis_kelamin,
                    email = @email,
                    no_hp = @no_hp,
                    alamat = @alamat
                WHERE id = @id OR nim = @nim;
            ";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", m.Id);
            cmd.Parameters.AddWithValue("@nim", m.Nim.Trim());
            cmd.Parameters.AddWithValue("@nama", m.Nama.Trim());
            cmd.Parameters.AddWithValue("@program_studi", m.ProgramStudi.Trim());
            cmd.Parameters.AddWithValue("@tanggal_lahir", m.TanggalLahir.HasValue ? m.TanggalLahir.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@jenis_kelamin", string.IsNullOrWhiteSpace(m.JenisKelamin) ? "Laki-laki" : m.JenisKelamin);
            cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(m.Email) ? (object)DBNull.Value : m.Email.Trim());
            cmd.Parameters.AddWithValue("@no_hp", string.IsNullOrWhiteSpace(m.NoTelepon) ? (object)DBNull.Value : m.NoTelepon.Trim());
            cmd.Parameters.AddWithValue("@alamat", string.IsNullOrWhiteSpace(m.Alamat) ? (object)DBNull.Value : m.Alamat.Trim());

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus data mahasiswa berdasarkan ID atau NIM.
        /// </summary>
        public bool Delete(int id, string? nim = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM mahasiswa WHERE id = @id OR nim = @nim;";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nim", nim ?? string.Empty);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Mengecek apakah NIM sudah ada di database (untuk validasi duplikasi).
        /// </summary>
        public bool IsNimExists(string nim, int? excludeId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT COUNT(*) FROM mahasiswa WHERE nim = @nim";
            if (excludeId.HasValue && excludeId.Value > 0)
            {
                query += " AND id != @excludeId";
            }
            query += ";";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@nim", nim.Trim());
            if (excludeId.HasValue && excludeId.Value > 0)
            {
                cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);
            }

            long count = Convert.ToInt64(cmd.ExecuteScalar() ?? 0);
            return count > 0;
        }

        /// <summary>
        /// Mengambil ringkasan statistik (Total Mahasiswa, Total Program Studi, Jurusan Terbanyak).
        /// </summary>
        public (int TotalMahasiswa, int TotalProdi, string ProdiTerbanyak) GetStatistics()
        {
            int totalMahasiswa = 0;
            int totalProdi = 0;
            string prodiTerbanyak = "-";

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                // 1. Total Mahasiswa
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM mahasiswa;", connection))
                {
                    totalMahasiswa = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                }

                // 2. Total Program Studi Unik
                using (var cmd = new MySqlCommand("SELECT COUNT(DISTINCT program_studi) FROM mahasiswa;", connection))
                {
                    totalProdi = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                }

                // 3. Jurusan dengan Mahasiswa Terbanyak
                string queryTop = @"
                    SELECT program_studi, COUNT(*) as jml 
                    FROM mahasiswa 
                    GROUP BY program_studi 
                    ORDER BY jml DESC 
                    LIMIT 1;
                ";
                using (var cmd = new MySqlCommand(queryTop, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        prodiTerbanyak = $"{reader.GetString(0)} ({reader.GetInt32(1)})";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MahasiswaRepository] Error saat GetStatistics: {ex.Message}");
            }

            return (totalMahasiswa, totalProdi, prodiTerbanyak);
        }

        private static Mahasiswa MapReaderToMahasiswa(MySqlDataReader reader)
        {
            return new Mahasiswa
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nim = reader.GetString(reader.GetOrdinal("nim")),
                Nama = reader.GetString(reader.GetOrdinal("nama")),
                ProgramStudi = reader.GetString(reader.GetOrdinal("program_studi")),
                TanggalLahir = reader.IsDBNull(reader.GetOrdinal("tanggal_lahir"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("tanggal_lahir")),
                JenisKelamin = reader.IsDBNull(reader.GetOrdinal("jenis_kelamin"))
                    ? "Laki-laki"
                    : reader.GetString(reader.GetOrdinal("jenis_kelamin")),
                Email = reader.IsDBNull(reader.GetOrdinal("email"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("email")),
                NoTelepon = reader.IsDBNull(reader.GetOrdinal("no_hp"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("no_hp")),
                Alamat = reader.IsDBNull(reader.GetOrdinal("alamat"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("alamat")),
                WaktuDaftar = reader.IsDBNull(reader.GetOrdinal("waktu_daftar"))
                    ? DateTime.Now
                    : reader.GetDateTime(reader.GetOrdinal("waktu_daftar"))
            };
        }
    }
}
