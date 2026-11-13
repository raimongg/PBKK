-- ====================================================================
-- SISTEM REGISTRASI MAHASISWA (SISMA)
-- Mata Kuliah : Pemrograman Berbasis Kerangka Kerja (PBKK)
-- Arsitektur  : MVVM (Model-View-ViewModel) + MySQL Database
-- Penulis     : Raihan Rasyid Ramadhan (5025241224)
-- ====================================================================

-- 1. PEMBUATAN DATABASE
CREATE DATABASE IF NOT EXISTS registrasi_mahasiswa_db
CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE registrasi_mahasiswa_db;

-- 2. HAPUS TABEL LAMA JIKA ADA (OPSIONAL)
-- DROP TABLE IF EXISTS mahasiswa;

-- 3. PEMBUATAN TABEL MAHASISWA
CREATE TABLE IF NOT EXISTS mahasiswa (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nim VARCHAR(20) NOT NULL UNIQUE,
    nama VARCHAR(150) NOT NULL,
    program_studi VARCHAR(100) NOT NULL,
    tanggal_lahir DATE NULL,
    jenis_kelamin ENUM('Laki-laki', 'Perempuan') NOT NULL DEFAULT 'Laki-laki',
    email VARCHAR(100) NULL,
    no_hp VARCHAR(20) NULL,
    alamat TEXT NULL,
    waktu_daftar DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_nim (nim),
    INDEX idx_nama (nama),
    INDEX idx_prodi (program_studi)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. INSERT DATA AWAL (SEEDING SAMPLE DATA)
INSERT INTO mahasiswa (nim, nama, program_studi, tanggal_lahir, jenis_kelamin, email, no_hp, alamat, waktu_daftar)
VALUES
('24010101', 'Aditya Pratama Putra', 'Teknik Informatika', '2003-05-14', 'Laki-laki', 'aditya.putra@campus.ac.id', '081234567890', 'Jl. Merdeka Barat No. 45, Bandung', NOW() - INTERVAL 3 DAY),
('24020102', 'Clarissa Anindya Putri', 'Sistem Informasi', '2003-09-21', 'Perempuan', 'clarissa.anindya@campus.ac.id', '081987654321', 'Jl. Diponegoro No. 12, Surabaya', NOW() - INTERVAL 2 DAY),
('24030103', 'Fajar Hidayatullah', 'Manajemen Informatika', '2004-02-08', 'Laki-laki', 'fajar.hidayat@campus.ac.id', '082155667788', 'Jl. Sudirman No. 88, Jakarta Selatan', NOW() - INTERVAL 1 DAY),
('24040104', 'Nabila Zahra Khairunnisa', 'Sains Data', '2003-11-19', 'Perempuan', 'nabila.zahra@campus.ac.id', '085712349876', 'Jl. Malioboro No. 20, Yogyakarta', NOW())
ON DUPLICATE KEY UPDATE nama = VALUES(nama);

-- 5. VIEW UNTUK RINGKASAN STATISTIK AKADEMIK
CREATE OR REPLACE VIEW v_statistik_prodi AS
SELECT 
    program_studi,
    COUNT(*) AS total_mahasiswa,
    SUM(CASE WHEN jenis_kelamin = 'Laki-laki' THEN 1 ELSE 0 END) AS total_laki_laki,
    SUM(CASE WHEN jenis_kelamin = 'Perempuan' THEN 1 ELSE 0 END) AS total_perempuan
FROM mahasiswa
GROUP BY program_studi
ORDER BY total_mahasiswa DESC;

-- 6. CONTOH QUERY OPERASI CRUD (SESUAI ADO.NET)
-- A. Read / Select All
-- SELECT id, nim, nama, program_studi, tanggal_lahir, jenis_kelamin, email, no_hp, alamat, waktu_daftar FROM mahasiswa ORDER BY id DESC;

-- B. Search by Keyword
-- SELECT * FROM mahasiswa WHERE nim LIKE '%2401%' OR nama LIKE '%Aditya%' OR program_studi LIKE '%Informatika%';

-- C. Create / Insert New Mahasiswa
-- INSERT INTO mahasiswa (nim, nama, program_studi, tanggal_lahir, jenis_kelamin, email, no_hp, alamat)
-- VALUES ('24050105', 'Reza Rahardian', 'Teknik Informatika', '2003-07-12', 'Laki-laki', 'reza.rahardian@campus.ac.id', '081299887766', 'Jl. Kebon Jeruk No. 10, Surabaya');

-- D. Update Mahasiswa
-- UPDATE mahasiswa SET nama = 'Aditya Pratama, S.Kom', program_studi = 'Teknik Informatika' WHERE nim = '24010101';

-- E. Delete Mahasiswa
-- DELETE FROM mahasiswa WHERE nim = '24050105';
