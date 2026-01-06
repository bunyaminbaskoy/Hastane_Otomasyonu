using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;
using DevExpress.XtraEditors;

namespace Hastane_Otomasyonu.Database
{
    /// <summary>
    /// Veritabanını otomatik oluşturan ve başlatan sınıf
    /// </summary>
    public static class DatabaseInitializer
    {
        private static string GetMasterConnectionString()
        {
            // Master veritabanına bağlanmak için connection string
            string baseConnectionString = ConfigurationManager.ConnectionStrings["HastaneOtomasyonuConnection"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(baseConnectionString))
            {
                // Eğer App.config'de bulunamazsa varsayılan kullan
                return "Server=(local);Database=master;Integrated Security=True;TrustServerCertificate=True;";
            }

            // Connection string'den Database kısmını çıkar ve master ile değiştir
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnectionString);
            builder.InitialCatalog = "master";
            return builder.ConnectionString;
        }

        private static string GetServerName()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["HastaneOtomasyonuConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                return "(local)";
            }

            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }

        /// <summary>
        /// Veritabanının varlığını kontrol eder
        /// </summary>
        public static bool DatabaseExists()
        {
            try
            {
                using (var connection = new SqlConnection(GetMasterConnectionString()))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM sys.databases WHERE name = 'Hastane'";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Veritabanını oluşturur
        /// </summary>
        public static bool CreateDatabase()
        {
            try
            {
                using (var connection = new SqlConnection(GetMasterConnectionString()))
                {
                    connection.Open();
                    string createDbQuery = @"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Hastane')
                        BEGIN
                            CREATE DATABASE Hastane;
                        END";
                    
                    using (var cmd = new SqlCommand(createDbQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Veritabanı oluşturulurken hata oluştu:\n{ex.Message}", "Hata", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Tüm tabloları oluşturur
        /// </summary>
        public static bool CreateTables()
        {
            try
            {
                using (var connection = SqlBaglantisi.Instance.GetConnection())
                {
                    // Branşlar Tablosu
                    string createBranslar = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Branslar]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE Tbl_Branslar (
                                Bransid INT PRIMARY KEY IDENTITY(1,1),
                                BransAd VARCHAR(50) NOT NULL UNIQUE
                            );
                        END";

                    // Doktorlar Tablosu
                    string createDoktorlar = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Doktorlar]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE Tbl_Doktorlar (
                                Doktorid INT PRIMARY KEY IDENTITY(1,1),
                                DoktorAd VARCHAR(30) NOT NULL,
                                DoktorSoyad VARCHAR(30) NOT NULL,
                                DoktorBrans VARCHAR(50),
                                DoktorTC CHAR(11) NOT NULL UNIQUE,
                                DoktorSifre VARCHAR(50) NOT NULL
                            );
                        END";

                        // Hastalar Tablosu
                        string createHastalar = @"
                            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND type in (N'U'))
                            BEGIN
                                CREATE TABLE Tbl_Hastalar (
                                    Hastaid INT PRIMARY KEY IDENTITY(1,1),
                                    HastaAd VARCHAR(30) NOT NULL,
                                    HastaSoyad VARCHAR(30) NOT NULL,
                                    HastaTC CHAR(11) NOT NULL UNIQUE,
                                    HastaTelefon VARCHAR(15),
                                    HastaSifre VARCHAR(50) NOT NULL,
                                    HastaCinsiyet VARCHAR(5),
                                    HastaKanGrubu VARCHAR(10),
                                    HastaAlerjiler VARCHAR(MAX),
                                    HastaHastaliklar VARCHAR(MAX),
                                    HastaDogumTarihi DATE,
                                    HastaEmail VARCHAR(120),
                                    HastaIl VARCHAR(50),
                                    HastaIlce VARCHAR(50),
                                    HastaAdres VARCHAR(MAX),
                                    HastaBoyCm INT,
                                    HastaKiloKg INT,
                                    HastaFoto VARBINARY(MAX)
                                );
                            END
                            ELSE
                            BEGIN
                                -- Eğer tablo varsa ama yeni kolonlar yoksa ekle
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaKanGrubu')
                                    ALTER TABLE Tbl_Hastalar ADD HastaKanGrubu VARCHAR(10);
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaAlerjiler')
                                    ALTER TABLE Tbl_Hastalar ADD HastaAlerjiler VARCHAR(MAX);
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaHastaliklar')
                                    ALTER TABLE Tbl_Hastalar ADD HastaHastaliklar VARCHAR(MAX);
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaDogumTarihi')
                                    ALTER TABLE Tbl_Hastalar ADD HastaDogumTarihi DATE;
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaEmail')
                                    ALTER TABLE Tbl_Hastalar ADD HastaEmail VARCHAR(120);
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaIl')
                                    ALTER TABLE Tbl_Hastalar ADD HastaIl VARCHAR(50);
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaIlce')
                                    ALTER TABLE Tbl_Hastalar ADD HastaIlce VARCHAR(50);
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaAdres')
                                    ALTER TABLE Tbl_Hastalar ADD HastaAdres VARCHAR(MAX);
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaBoyCm')
                                    ALTER TABLE Tbl_Hastalar ADD HastaBoyCm INT;
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaKiloKg')
                                    ALTER TABLE Tbl_Hastalar ADD HastaKiloKg INT;
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND name = 'HastaFoto')
                                    ALTER TABLE Tbl_Hastalar ADD HastaFoto VARBINARY(MAX);
                            END";

                    // Sekreterler Tablosu
                    string createSekreterler = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Sekreterler]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE Tbl_Sekreterler (
                                Sekreterid INT PRIMARY KEY IDENTITY(1,1),
                                SekreterAd VARCHAR(30) NOT NULL,
                                SekreterSoyad VARCHAR(30) NOT NULL,
                                SekreterTC CHAR(11) NOT NULL UNIQUE,
                                SekreterTelefon VARCHAR(15),
                                SekreterSifre VARCHAR(50) NOT NULL
                            );
                        END";

                    // Randevular Tablosu
                    string createRandevular = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Randevular]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE Tbl_Randevular (
                                Randevuid INT PRIMARY KEY IDENTITY(1,1),
                                RandevuTarih DATE,
                                RandevuSaat TIME,
                                RandevuBrans VARCHAR(50),
                                RandevuDoktor VARCHAR(60),
                                RandevuDoktorTC CHAR(11),
                                RandevuDurum BIT DEFAULT 0,
                                HastaTC CHAR(11),
                                RandevuSikayet VARCHAR(250),
                                KayitTarihi DATETIME DEFAULT GETDATE()
                            );
                        END";

                    // Duyurular Tablosu
                    string createDuyurular = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Duyurular]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE Tbl_Duyurular (
                                Duyuruid INT PRIMARY KEY IDENTITY(1,1),
                                DuyuruTarih DATETIME DEFAULT GETDATE(),
                                DuyuruIcerik VARCHAR(MAX)
                            );
                        END";

                    // Tahliller Tablosu
                    string createTahliller = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE Tbl_Tahliller (
                                Tahlilid INT PRIMARY KEY IDENTITY(1,1),
                                TahlilAd VARCHAR(100) NOT NULL,
                                TahlilTur VARCHAR(50),
                                TahlilTarih DATETIME DEFAULT GETDATE(),
                                DoktorAd VARCHAR(60),
                                DoktorTC CHAR(11),
                                TahlilSonuc VARCHAR(100),
                                TahlilDurum VARCHAR(50),
                                TahlilPdf VARBINARY(MAX),
                                TahlilPdfFileName VARCHAR(255),
                                TahlilPdfMime VARCHAR(50),
                                HastaTC CHAR(11),
                                SekreterTC CHAR(11),
                                KayitTarihi DATETIME DEFAULT GETDATE()
                            );
                        END";

                    // Reçeteler Tablosu
                    string createReceteler = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE Tbl_Receteler (
                                Receteid INT PRIMARY KEY IDENTITY(1,1),
                                ReceteKod VARCHAR(20) NOT NULL,
                                ReceteTarih DATETIME DEFAULT GETDATE(),
                                DoktorAd VARCHAR(60),
                                DoktorTC CHAR(11),
                                Ilaclar VARCHAR(MAX),
                                RecetePdf VARBINARY(MAX),
                                RecetePdfFileName VARCHAR(255),
                                RecetePdfMime VARCHAR(50),
                                HastaTC CHAR(11),
                                KayitTarihi DATETIME DEFAULT GETDATE()
                            );
                        END";

                    // Tüm tabloları oluştur
                    string[] createQueries = { 
                        createBranslar, 
                        createDoktorlar, 
                        createHastalar, 
                        createSekreterler, 
                        createRandevular, 
                        createDuyurular,
                        createTahliller,
                        createReceteler
                    };

                    foreach (string query in createQueries)
                    {
                        using (var cmd = new SqlCommand(query, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // --- Var olan tablolara eksik kolonları ekle (migration-like, kırmadan) ---
                    // Tbl_Randevular
                    using (var cmd = new SqlCommand(@"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Randevular]') AND type in (N'U'))
                        BEGIN
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Randevular]') AND name = 'RandevuDoktorTC')
                                ALTER TABLE Tbl_Randevular ADD RandevuDoktorTC CHAR(11);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Randevular]') AND name = 'KayitTarihi')
                                ALTER TABLE Tbl_Randevular ADD KayitTarihi DATETIME DEFAULT GETDATE();
                        END", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Tbl_Tahliller
                    using (var cmd = new SqlCommand(@"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND type in (N'U'))
                        BEGIN
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'DoktorTC')
                                ALTER TABLE Tbl_Tahliller ADD DoktorTC CHAR(11);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'SekreterTC')
                                ALTER TABLE Tbl_Tahliller ADD SekreterTC CHAR(11);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'KayitTarihi')
                                ALTER TABLE Tbl_Tahliller ADD KayitTarihi DATETIME DEFAULT GETDATE();
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'TahlilTur')
                                ALTER TABLE Tbl_Tahliller ADD TahlilTur VARCHAR(50);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'TahlilPdf')
                                ALTER TABLE Tbl_Tahliller ADD TahlilPdf VARBINARY(MAX);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'TahlilPdfFileName')
                                ALTER TABLE Tbl_Tahliller ADD TahlilPdfFileName VARCHAR(255);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'TahlilPdfMime')
                                ALTER TABLE Tbl_Tahliller ADD TahlilPdfMime VARCHAR(50);
                        END", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Tbl_Receteler
                    using (var cmd = new SqlCommand(@"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND type in (N'U'))
                        BEGIN
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND name = 'DoktorTC')
                                ALTER TABLE Tbl_Receteler ADD DoktorTC CHAR(11);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND name = 'KayitTarihi')
                                ALTER TABLE Tbl_Receteler ADD KayitTarihi DATETIME DEFAULT GETDATE();
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND name = 'RecetePdf')
                                ALTER TABLE Tbl_Receteler ADD RecetePdf VARBINARY(MAX);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND name = 'RecetePdfFileName')
                                ALTER TABLE Tbl_Receteler ADD RecetePdfFileName VARCHAR(255);
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND name = 'RecetePdfMime')
                                ALTER TABLE Tbl_Receteler ADD RecetePdfMime VARCHAR(50);
                        END", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Tablolar oluşturulurken hata oluştu:\n{ex.Message}", "Hata", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Varsayılan verileri ekler (Branşlar ve Test Kullanıcıları)
        /// </summary>
        public static bool InsertDefaultData()
        {
            try
            {
                using (var connection = SqlBaglantisi.Instance.GetConnection())
                {
                    // Branşlar Ekle
                    string insertBranslar = @"
                        IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'Dahiliye')
                            INSERT INTO Tbl_Branslar (BransAd) VALUES ('Dahiliye');
                        IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'Göz')
                            INSERT INTO Tbl_Branslar (BransAd) VALUES ('Göz');
                        IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'KBB')
                            INSERT INTO Tbl_Branslar (BransAd) VALUES ('KBB');
                        IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'Kardiyoloji')
                            INSERT INTO Tbl_Branslar (BransAd) VALUES ('Kardiyoloji');
                        IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'Ortopedi')
                            INSERT INTO Tbl_Branslar (BransAd) VALUES ('Ortopedi');";

                    // Test Doktor Ekle
                    // Randevu ekranını test etmek için birden fazla doktor ekleyelim (her branşa 2 doktor)
                    string insertDoktor = @"
                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111111')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Test', 'Doktor', 'Dahiliye', '11111111111', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111112')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Ahmet', 'Yıldız', 'Dahiliye', '11111111112', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111113')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Elif', 'Kara', 'Göz', '11111111113', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111114')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Mert', 'Aydın', 'Göz', '11111111114', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111115')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Zeynep', 'Demir', 'KBB', '11111111115', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111116')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Can', 'Öztürk', 'KBB', '11111111116', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111117')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Ayşe', 'Yılmaz', 'Kardiyoloji', '11111111117', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111118')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Mehmet', 'Kaya', 'Kardiyoloji', '11111111118', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111119')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Deniz', 'Şahin', 'Ortopedi', '11111111119', '1234');

                        IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111120')
                            INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
                            VALUES ('Burak', 'Koç', 'Ortopedi', '11111111120', '1234');";

                        // Test Hasta Ekle
                        string insertHasta = @"
                            IF NOT EXISTS (SELECT * FROM Tbl_Hastalar WHERE HastaTC = '22222222222')
                                INSERT INTO Tbl_Hastalar (HastaAd, HastaSoyad, HastaTC, HastaTelefon, HastaSifre, HastaCinsiyet, HastaKanGrubu, HastaAlerjiler, HastaHastaliklar)
                                VALUES ('Test', 'Hasta', '22222222222', '(555) 123-4567', '1234', 'Erkek', 'A Rh+', 'Penisilin, Polen', 'Tip 1 Diyabet, Hipertansiyon');
                            ELSE
                                UPDATE Tbl_Hastalar SET 
                                    HastaKanGrubu = 'A Rh+', 
                                    HastaAlerjiler = 'Penisilin, Polen', 
                                    HastaHastaliklar = 'Tip 1 Diyabet, Hipertansiyon' 
                                WHERE HastaTC = '22222222222' AND HastaKanGrubu IS NULL;";

                    // Test Sekreter Ekle
                    string insertSekreter = @"
                        IF NOT EXISTS (SELECT * FROM Tbl_Sekreterler WHERE SekreterTC = '33333333333')
                        BEGIN
                            INSERT INTO Tbl_Sekreterler (SekreterAd, SekreterSoyad, SekreterTC, SekreterTelefon, SekreterSifre)
                            VALUES ('Test', 'Sekreter', '33333333333', '(555) 987-6543', '1234');
                        END";

                    // Örnek Tahliller Ekle
                    string insertTahliller = @"
                        IF NOT EXISTS (SELECT * FROM Tbl_Tahliller WHERE HastaTC = '22222222222' AND TahlilAd = 'Kardiyoloji Paneli')
                        BEGIN
                            INSERT INTO Tbl_Tahliller (TahlilAd, TahlilTarih, DoktorAd, TahlilSonuc, TahlilDurum, HastaTC)
                            VALUES ('Tam Kan Sayımı', '2023-12-15', 'Dr. Ayşe Yılmaz', 'Normal', '✅ Tamamlandı', '22222222222'),
                                   ('Biyokimya (Glikoz)', '2023-12-10', 'Dr. Mehmet Kaya', '95 mg/dL', '✅ Tamamlandı', '22222222222'),
                                   ('Hormon Paneli (TSH)', '2023-12-05', 'Dr. Zeynep Demir', '2.5 mIU/L', '✅ Tamamlandı', '22222222222'),
                                   ('Vitamin D3', '2023-11-20', 'Dr. Ayşe Yılmaz', '18 ng/mL', '⚠️ Düşük', '22222222222'),
                                   ('Kardiyoloji Paneli', GETDATE(), 'Dr. Can Öztürk', 'İnceleniyor', '⏳ Beklemede', '22222222222');
                        END";

                    // Örnek Reçeteler Ekle
                    string insertReceteler = @"
                        IF NOT EXISTS (SELECT * FROM Tbl_Receteler WHERE HastaTC = '22222222222')
                        BEGIN
                            INSERT INTO Tbl_Receteler (ReceteKod, ReceteTarih, DoktorAd, Ilaclar, HastaTC)
                            VALUES ('RX-10293', '2023-12-15', 'Dr. Ayşe Yılmaz', 'Parol 500mg, Augmentin 1g', '22222222222'),
                                   ('RX-10455', '2023-12-10', 'Dr. Mehmet Kaya', 'Glifor 1000mg, Coraspin 100mg', '22222222222'),
                                   ('RX-10566', '2023-11-20', 'Dr. Ayşe Yılmaz', 'Devit-3 Damla', '22222222222'),
                                   ('RX-10899', GETDATE(), 'Dr. Zeynep Demir', 'Euthyrox 50mcg', '22222222222');
                        END";

                    string[] insertQueries = { insertBranslar, insertDoktor, insertHasta, insertSekreter, insertTahliller, insertReceteler };

                    foreach (string query in insertQueries)
                    {
                        using (var cmd = new SqlCommand(query, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Varsayılan veriler eklenirken hata oluştu:\n{ex.Message}", "Uyarı", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return false;
            }
        }

        /// <summary>
        /// Veritabanını tamamen başlatır (oluşturur, tabloları oluşturur, verileri ekler)
        /// </summary>
        public static bool InitializeDatabase()
        {
            try
            {
                // 1. Veritabanı var mı kontrol et
                if (!DatabaseExists())
                {
                    // Veritabanını oluştur
                    if (!CreateDatabase())
                    {
                        return false;
                    }

                    // Bağlantıyı kapat ve yeniden aç (yeni veritabanına bağlanmak için)
                    SqlBaglantisi.Instance.CloseConnection();
                    System.Threading.Thread.Sleep(500); // Kısa bir bekleme
                }

                // 2. Tabloları oluştur
                if (!CreateTables())
                {
                    return false;
                }

                // 3. Varsayılan verileri ekle
                InsertDefaultData();

                return true;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Veritabanı başlatılırken hata oluştu:\n{ex.Message}", "Hata", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
    }
}

