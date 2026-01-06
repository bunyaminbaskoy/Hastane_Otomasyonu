using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Microsoft.Data.SqlClient;
using Hastane_Otomasyonu.Database;
using System.Drawing.Drawing2D;

namespace Hastane_Otomasyonu
{
    public partial class FrmDoktorKayit : DevExpress.XtraEditors.XtraForm
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public FrmDoktorKayit()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Paint += FrmDoktorKayit_Paint;
            pnlMain.Paint += pnlMain_Paint;
        }

        private static Color Hex(string hex) => ColorTranslator.FromHtml(hex);
        private static Color HexA(int alpha, string hex)
        {
            var c = ColorTranslator.FromHtml(hex);
            return Color.FromArgb(alpha, c);
        }

        private void FrmDoktorKayit_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            using var bg = new LinearGradientBrush(this.ClientRectangle, Hex("#0F172A"), Hex("#1E2A38"), LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(bg, this.ClientRectangle);

            var glowRect = new Rectangle(-120, -120, this.Width + 240, this.Height / 2);
            using var glow = new LinearGradientBrush(glowRect, HexA(90, "#48CFAD"), Color.Transparent, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(glow, glowRect);
        }

        private void FrmDoktorKayit_Load(object sender, EventArgs e)
        {
            ApplyDarkTheme();

            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 40, 40));
            pnlMain.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMain.Width, pnlMain.Height, 30, 30));
            
            var assembly = typeof(DevExpress.Images.ImageResourceCache).Assembly;
            txtAd.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Business.Business_Badge.svg", assembly);
            txtBrans.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Icon Builder.Travel_Medical.svg", assembly);
            txtTC.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Business.Business_User.svg", assembly);
            txtSifre.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Security.Key.svg", assembly);
        }

        private void ApplyDarkTheme()
        {
            this.LookAndFeel.UseDefaultLookAndFeel = false;

            var cardBg = Hex("#1E2A38");
            var accent = Hex("#48CFAD");

            pnlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMain.Appearance.BackColor = cardBg;
            pnlMain.Appearance.Options.UseBackColor = true;
            pnlMain.Padding = new Padding(2);

            pnlLeft.Appearance.BackColor = Hex("#223244");
            pnlLeft.Appearance.BackColor2 = Hex("#1E2A38");
            pnlLeft.Appearance.GradientMode = LinearGradientMode.Vertical;
            pnlLeft.Appearance.Options.UseBackColor = true;

            picLeft.Properties.NullText = "";
            picLeft.Properties.ShowMenu = false;

            lblLeftTitle.Appearance.ForeColor = Hex("#ECEFF1");
            lblLeftTitle.Appearance.Options.UseForeColor = true;

            lblTitle.AllowHtmlString = true;
            lblTitle.Appearance.ForeColor = Hex("#ECEFF1");
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Text = $"<color=#{accent.R:X2}{accent.G:X2}{accent.B:X2}>Doktor</color> <color=#ECEFF1>Kaydı</color>";

            btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnClose.Appearance.BackColor = Color.Transparent;
            btnClose.Appearance.Options.UseBackColor = true;
            btnClose.Appearance.ForeColor = Hex("#B0BEC5");
            btnClose.Appearance.Options.UseForeColor = true;
            btnClose.AppearanceHovered.ForeColor = Color.White;
            btnClose.AppearanceHovered.Options.UseForeColor = true;
            btnClose.Text = "✕";

            StyleTextEdit(txtAd, "Doktor Adı");
            StyleTextEdit(txtSoyad, "Doktor Soyadı");
            StyleTextEdit(txtTC, "TC Kimlik No");
            StyleTextEdit(txtBrans, "Uzmanlık Branşı");
            StyleTextEdit(txtSifre, "Şifre", isPassword: true);

            StylePrimaryButton(btnKaydet, "#48CFAD", textColor: Color.White);
        }

        private void StyleTextEdit(TextEdit edit, string prompt, bool isPassword = false)
        {
            edit.Properties.NullValuePrompt = prompt;
            edit.Properties.NullValuePromptShowForEmptyValue = true;
            edit.Properties.Appearance.BackColor = Hex("#223244");
            edit.Properties.Appearance.ForeColor = Hex("#ECEFF1");
            edit.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            edit.Properties.Appearance.Options.UseBackColor = true;
            edit.Properties.Appearance.Options.UseForeColor = true;
            edit.Properties.Appearance.Options.UseFont = true;
            edit.Properties.AppearanceFocused.BackColor = Hex("#25384C");
            edit.Properties.AppearanceFocused.ForeColor = Hex("#FFFFFF");
            edit.Properties.AppearanceFocused.Options.UseBackColor = true;
            edit.Properties.AppearanceFocused.Options.UseForeColor = true;
            edit.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            edit.Properties.AutoHeight = false;
            if (isPassword)
            {
                edit.Properties.PasswordChar = '•';
            }
        }

        private void StylePrimaryButton(SimpleButton btn, string baseHex, Color textColor)
        {
            var baseC = Hex(baseHex);
            var hoverC = Color.FromArgb(
                baseC.A,
                Math.Min(255, (int)(baseC.R * 1.08f)),
                Math.Min(255, (int)(baseC.G * 1.08f)),
                Math.Min(255, (int)(baseC.B * 1.08f)));
            var pressedC = Color.FromArgb(
                baseC.A,
                Math.Max(0, (int)(baseC.R * 0.92f)),
                Math.Max(0, (int)(baseC.G * 0.92f)),
                Math.Max(0, (int)(baseC.B * 0.92f)));

            btn.LookAndFeel.UseDefaultLookAndFeel = false;
            btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btn.Appearance.BackColor = baseC;
            btn.Appearance.ForeColor = textColor;
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.AppearanceHovered.BackColor = hoverC;
            btn.AppearanceHovered.ForeColor = textColor;
            btn.AppearanceHovered.Options.UseBackColor = true;
            btn.AppearanceHovered.Options.UseForeColor = true;
            btn.AppearancePressed.BackColor = pressedC;
            btn.AppearancePressed.ForeColor = textColor;
            btn.AppearancePressed.Options.UseBackColor = true;
            btn.AppearancePressed.Options.UseForeColor = true;
        }

        private void pnlMain_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlMain.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            int radius = 30;
            using var path = GetRoundedRectPath(rect, radius);
            using var pen = new Pen(Color.FromArgb(180, Color.White), 2f);
            e.Graphics.DrawPath(pen, path);
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            // Validasyon kontrolleri
            if (string.IsNullOrWhiteSpace(txtAd.Text) || string.IsNullOrWhiteSpace(txtSoyad.Text) ||
                string.IsNullOrWhiteSpace(txtTC.Text) || string.IsNullOrWhiteSpace(txtSifre.Text) ||
                string.IsNullOrWhiteSpace(txtBrans.Text))
            {
                XtraMessageBox.Show("Lütfen tüm zorunlu alanları doldurun!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtTC.Text.Length != 11)
            {
                XtraMessageBox.Show("TC Kimlik No 11 haneli olmalıdır!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = SqlBaglantisi.Instance.GetConnection())
                {
                    // TC kontrolü - Aynı TC'ye sahip doktor var mı?
                    string checkQuery = "SELECT COUNT(*) FROM Tbl_Doktorlar WHERE DoktorTC = @tc";
                    using (var checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@tc", txtTC.Text.Trim());
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        
                        if (count > 0)
                        {
                            XtraMessageBox.Show("Bu TC Kimlik No ile kayıtlı bir doktor zaten mevcut!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Doktor kaydı ekle
                    string insertQuery = @"INSERT INTO Tbl_Doktorlar 
                                          (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTC, DoktorSifre) 
                                          VALUES (@ad, @soyad, @brans, @tc, @sifre)";
                    
                    using (var insertCmd = new SqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@ad", txtAd.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@soyad", txtSoyad.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@brans", txtBrans.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@tc", txtTC.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@sifre", txtSifre.Text);

                        int result = insertCmd.ExecuteNonQuery();
                        
                        if (result > 0)
                        {
                            XtraMessageBox.Show("Doktor kaydı başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                        }
                        else
                        {
                            XtraMessageBox.Show("Kayıt işlemi başarısız!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                XtraMessageBox.Show($"Veritabanı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SqlBaglantisi.Instance.CloseConnection();
            }
        }
    }
}

