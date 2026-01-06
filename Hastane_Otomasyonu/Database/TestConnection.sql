-- =============================================
-- Bağlantı Test Scripti
-- SQL Server Management Studio'da çalıştırın
-- =============================================

-- 1. Veritabanlarını Listele
SELECT name AS 'Veritabanı Adı' 
FROM sys.databases 
ORDER BY name;
GO

-- 2. Hastane veritabanı var mı kontrol et
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Hastane')
    PRINT '✓ Hastane veritabanı MEVCUT';
ELSE
    PRINT '✗ Hastane veritabanı BULUNAMADI - CreateDatabase.sql scriptini çalıştırın!';
GO

-- 3. Hastane veritabanındaki tabloları listele
USE Hastane;
GO

SELECT 
    TABLE_SCHEMA AS 'Schema',
    TABLE_NAME AS 'Tablo Adı'
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

-- 4. Tbl_Hastalar tablosundaki kayıt sayısını kontrol et
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Hastalar]') AND type in (N'U'))
BEGIN
    SELECT COUNT(*) AS 'Toplam Hasta Sayısı' FROM Tbl_Hastalar;
    SELECT TOP 5 * FROM Tbl_Hastalar;
END
ELSE
    PRINT '✗ Tbl_Hastalar tablosu BULUNAMADI!';
GO

PRINT '';
PRINT '========================================';
PRINT 'BAĞLANTI TEST TAMAMLANDI';
PRINT '========================================';
GO

