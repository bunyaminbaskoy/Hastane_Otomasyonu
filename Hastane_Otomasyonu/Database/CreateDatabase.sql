-- =============================================
-- Hastane Otomasyonu Veritabanı Oluşturma Scripti
-- SQL Server Management Studio 2022 için hazırlanmıştır
-- =============================================

-- 1. Veritabanını Oluştur
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Hastane')
BEGIN
    CREATE DATABASE Hastane;
    PRINT 'Hastane veritabanı oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'Hastane veritabanı zaten mevcut.';
END
GO

USE Hastane;
GO

-- =============================================
-- 2. Tabloları Oluştur
-- =============================================

-- Branşlar Tablosu (Doktorlar için)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Branslar]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tbl_Branslar (
        Bransid INT PRIMARY KEY IDENTITY(1,1),
        BransAd VARCHAR(50) NOT NULL UNIQUE
    );
    PRINT 'Tbl_Branslar tablosu oluşturuldu.';
END
GO

-- Doktorlar Tablosu
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
    PRINT 'Tbl_Doktorlar tablosu oluşturuldu.';
END
GO

-- Hastalar Tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tbl_Hastalar (
        Hastaid INT PRIMARY KEY IDENTITY(1,1),
        HastaAd VARCHAR(30) NOT NULL,
        HastaSoyad VARCHAR(30) NOT NULL,
        HastaTC CHAR(11) NOT NULL UNIQUE,
        HastaTelefon VARCHAR(15),
        HastaSifre VARCHAR(50) NOT NULL,
        HastaCinsiyet VARCHAR(5)
    );
    PRINT 'Tbl_Hastalar tablosu oluşturuldu.';
END
GO

-- Sekreterler Tablosu
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
    PRINT 'Tbl_Sekreterler tablosu oluşturuldu.';
END
GO

-- Randevular Tablosu (Gelecek aşamalar için)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Randevular]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tbl_Randevular (
        Randevuid INT PRIMARY KEY IDENTITY(1,1),
        RandevuTarih DATE,
        RandevuSaat TIME,
        RandevuBrans VARCHAR(50),
        RandevuDoktor VARCHAR(60),
        RandevuDurum BIT DEFAULT 0, -- 0: Boş, 1: Dolu
        HastaTC CHAR(11),
        RandevuSikayet VARCHAR(250)
    );
    PRINT 'Tbl_Randevular tablosu oluşturuldu.';
END
GO

-- Duyurular Tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Duyurular]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tbl_Duyurular (
        Duyuruid INT PRIMARY KEY IDENTITY(1,1),
        DuyuruTarih DATETIME DEFAULT GETDATE(),
        DuyuruIcerik VARCHAR(MAX)
    );
        PRINT 'Tbl_Duyurular tablosu oluşturuldu.';
    END
    GO

    -- Tahliller Tablosu
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND type in (N'U'))
    BEGIN
        CREATE TABLE Tbl_Tahliller (
            Tahlilid INT PRIMARY KEY IDENTITY(1,1),
            TahlilAd VARCHAR(100) NOT NULL,
            TahlilTarih DATETIME DEFAULT GETDATE(),
            DoktorAd VARCHAR(60),
            DoktorTC CHAR(11),
            TahlilSonuc VARCHAR(100),
            TahlilDurum VARCHAR(50),
            HastaTC CHAR(11),
            SekreterTC CHAR(11),
            KayitTarihi DATETIME DEFAULT GETDATE()
        );
        PRINT 'Tbl_Tahliller tablosu oluşturuldu.';
    END
    GO

    -- Tablolar varsa kolonları tamamla (kırmadan)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'DoktorTC')
            ALTER TABLE Tbl_Tahliller ADD DoktorTC CHAR(11);
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'SekreterTC')
            ALTER TABLE Tbl_Tahliller ADD SekreterTC CHAR(11);
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Tahliller]') AND name = 'KayitTarihi')
            ALTER TABLE Tbl_Tahliller ADD KayitTarihi DATETIME DEFAULT GETDATE();
    END
    GO

    -- Reçeteler Tablosu
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND type in (N'U'))
    BEGIN
        CREATE TABLE Tbl_Receteler (
            Receteid INT PRIMARY KEY IDENTITY(1,1),
            ReceteKod VARCHAR(20) NOT NULL,
            ReceteTarih DATETIME DEFAULT GETDATE(),
            DoktorAd VARCHAR(60),
            DoktorTC CHAR(11),
            Ilaclar VARCHAR(MAX),
            HastaTC CHAR(11),
            KayitTarihi DATETIME DEFAULT GETDATE()
        );
        PRINT 'Tbl_Receteler tablosu oluşturuldu.';
    END
    GO

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND name = 'DoktorTC')
            ALTER TABLE Tbl_Receteler ADD DoktorTC CHAR(11);
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Receteler]') AND name = 'KayitTarihi')
            ALTER TABLE Tbl_Receteler ADD KayitTarihi DATETIME DEFAULT GETDATE();
    END
    GO

    -- =============================================
    -- 3. Örnek Veri Ekleme (Test için)
    -- =============================================

    -- Örnek Tahliller (Hasta TC: 22222222222 için)
    IF NOT EXISTS (SELECT * FROM Tbl_Tahliller WHERE HastaTC = '22222222222')
    BEGIN
        INSERT INTO Tbl_Tahliller (TahlilAd, TahlilTarih, DoktorAd, TahlilSonuc, TahlilDurum, HastaTC)
        VALUES ('Tam Kan Sayımı', '2023-12-15', 'Dr. Ayşe Yılmaz', 'Normal', '✅ Tamamlandı', '22222222222'),
               ('Biyokimya (Glikoz)', '2023-12-10', 'Dr. Mehmet Kaya', '95 mg/dL', '✅ Tamamlandı', '22222222222'),
               ('Hormon Paneli (TSH)', '2023-12-05', 'Dr. Zeynep Demir', '2.5 mIU/L', '✅ Tamamlandı', '22222222222'),
               ('Vitamin D3', '2023-11-20', 'Dr. Ayşe Yılmaz', '18 ng/mL', '⚠️ Düşük', '22222222222'),
               ('İdrar Tahlili', GETDATE(), 'Dr. Can Öztürk', 'İnceleniyor', '⏳ Beklemede', '22222222222');
    END
    GO

-- Branşlar
IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'Dahiliye')
    INSERT INTO Tbl_Branslar (BransAd) VALUES ('Dahiliye');
IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'Göz')
    INSERT INTO Tbl_Branslar (BransAd) VALUES ('Göz');
IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'KBB')
    INSERT INTO Tbl_Branslar (BransAd) VALUES ('KBB');
IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'Kardiyoloji')
    INSERT INTO Tbl_Branslar (BransAd) VALUES ('Kardiyoloji');
IF NOT EXISTS (SELECT * FROM Tbl_Branslar WHERE BransAd = 'Ortopedi')
    INSERT INTO Tbl_Branslar (BransAd) VALUES ('Ortopedi');
GO

PRINT 'Branşlar eklendi.';
GO

-- Test Doktor (TC: 11111111111, Şifre: 1234)
IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111111')
    INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
    VALUES ('Test', 'Doktor', 'Dahiliye', '11111111111', '1234');
GO

-- Ek Doktorlar (Randevu ekranı testi için)
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
    VALUES ('Burak', 'Koç', 'Ortopedi', '11111111120', '1234');
GO

-- Test Hasta (TC: 22222222222, Şifre: 1234)
IF NOT EXISTS (SELECT * FROM Tbl_Hastalar WHERE HastaTC = '22222222222')
    INSERT INTO Tbl_Hastalar (HastaAd, HastaSoyad, HastaTC, HastaTelefon, HastaSifre, HastaCinsiyet)
    VALUES ('Test', 'Hasta', '22222222222', '(555) 123-4567', '1234', 'Erkek');
GO

-- Test Sekreter (TC: 33333333333, Şifre: 1234)
IF NOT EXISTS (SELECT * FROM Tbl_Sekreterler WHERE SekreterTC = '33333333333')
    INSERT INTO Tbl_Sekreterler (SekreterAd, SekreterSoyad, SekreterTC, SekreterTelefon, SekreterSifre)
    VALUES ('Test', 'Sekreter', '33333333333', '(555) 987-6543', '1234');
GO

PRINT 'Test kullanıcıları eklendi.';
PRINT '';
PRINT '========================================';
PRINT 'VERİTABANI BAŞARIYLA OLUŞTURULDU!';
PRINT '========================================';
PRINT '';
PRINT 'Test Kullanıcıları:';
PRINT 'Doktor - TC: 11111111111, Şifre: 1234';
PRINT 'Hasta - TC: 22222222222, Şifre: 1234';
PRINT 'Sekreter - TC: 33333333333, Şifre: 1234';
PRINT '';
GO

