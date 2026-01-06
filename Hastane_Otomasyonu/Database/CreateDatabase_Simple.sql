-- =============================================
-- Hastane Veritabanı Oluşturma Scripti (Basitleştirilmiş)
-- SQL Server Management Studio 2022
-- =============================================

-- 1. Veritabanını Oluştur (Eğer yoksa)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Hastane')
BEGIN
    CREATE DATABASE Hastane;
END
GO

-- 2. Hastane veritabanını kullan
USE Hastane;
GO

-- 3. Eğer tablolar varsa sil (isteğe bağlı - dikkatli kullanın)
/*
DROP TABLE IF EXISTS Tbl_Randevular;
DROP TABLE IF EXISTS Tbl_Duyurular;
DROP TABLE IF EXISTS Tbl_Hastalar;
DROP TABLE IF EXISTS Tbl_Doktorlar;
DROP TABLE IF EXISTS Tbl_Sekreterler;
DROP TABLE IF EXISTS Tbl_Branslar;
GO
*/

-- 4. Branşlar Tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Branslar]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tbl_Branslar (
        Bransid INT PRIMARY KEY IDENTITY(1,1),
        BransAd VARCHAR(50) NOT NULL UNIQUE
    );
    PRINT 'Tbl_Branslar tablosu oluşturuldu.';
END
GO

-- 5. Doktorlar Tablosu
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

-- 6. Hastalar Tablosu
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

-- 7. Sekreterler Tablosu
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

-- 8. Randevular Tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Randevular]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tbl_Randevular (
        Randevuid INT PRIMARY KEY IDENTITY(1,1),
        RandevuTarih DATE,
        RandevuSaat TIME,
        RandevuBrans VARCHAR(50),
        RandevuDoktor VARCHAR(60),
        RandevuDurum BIT DEFAULT 0,
        HastaTC CHAR(11),
        RandevuSikayet VARCHAR(250)
    );
    PRINT 'Tbl_Randevular tablosu oluşturuldu.';
END
GO

-- 9. Duyurular Tablosu
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

-- 10. Branşlar Ekle
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

-- 11. Test Doktor Ekle
IF NOT EXISTS (SELECT * FROM Tbl_Doktorlar WHERE DoktorTC = '11111111111')
    INSERT INTO Tbl_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre)
    VALUES ('Test', 'Doktor', 'Dahiliye', '11111111111', '1234');
GO

-- 12. Test Hasta Ekle
IF NOT EXISTS (SELECT * FROM Tbl_Hastalar WHERE HastaTC = '22222222222')
    INSERT INTO Tbl_Hastalar (HastaAd, HastaSoyad, HastaTC, HastaTelefon, HastaSifre, HastaCinsiyet)
    VALUES ('Test', 'Hasta', '22222222222', '(555) 123-4567', '1234', 'Erkek');
GO

-- 13. Test Sekreter Ekle
IF NOT EXISTS (SELECT * FROM Tbl_Sekreterler WHERE SekreterTC = '33333333333')
    INSERT INTO Tbl_Sekreterler (SekreterAd, SekreterSoyad, SekreterTC, SekreterTelefon, SekreterSifre)
    VALUES ('Test', 'Sekreter', '33333333333', '(555) 987-6543', '1234');
GO

PRINT '';
PRINT '========================================';
PRINT 'HASTANE VERİTABANI BAŞARIYLA OLUŞTURULDU!';
PRINT '========================================';
PRINT '';
PRINT 'Test Kullanıcıları:';
PRINT 'Doktor   - TC: 11111111111, Şifre: 1234';
PRINT 'Hasta    - TC: 22222222222, Şifre: 1234';
PRINT 'Sekreter - TC: 33333333333, Şifre: 1234';
PRINT '';

