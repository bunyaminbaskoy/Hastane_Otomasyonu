using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Hastane_Otomasyonu.Database;
using DevExpress.XtraEditors;

namespace Hastane_Otomasyonu
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // DevExpress Modern Skin Ayarları
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("The Bezier");
            DevExpress.Utils.AppearanceObject.DefaultFont = new System.Drawing.Font("Segoe UI", 9F);

            // Veritabanını otomatik oluştur ve başlat
            try
            {
                bool dbInitialized = DatabaseInitializer.InitializeDatabase();
                
                if (dbInitialized)
                {
                    // Veritabanı başarıyla oluşturuldu veya zaten mevcut
                    Application.Run(new FrmGiris());
                }
                else
                {
                    XtraMessageBox.Show(
                        "Veritabanı başlatılamadı. Lütfen SQL Server'ın çalıştığından emin olun.\n\n" +
                        "SQL Server Management Studio'dan manuel olarak CreateDatabase.sql scriptini çalıştırabilirsiniz.",
                        "Veritabanı Hatası",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    $"Uygulama başlatılırken hata oluştu:\n\n{ex.Message}",
                    "Kritik Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
