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
using DevExpress.Utils.Animation;
using Microsoft.Data.SqlClient;
using Hastane_Otomasyonu.Database;
using System.Drawing.Drawing2D;

namespace Hastane_Otomasyonu
{
    public partial class FrmDoktorGiris : DevExpress.XtraEditors.XtraForm
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public FrmDoktorGiris()
        {
            InitializeComponent();
            // DirectX Hızlandırma ve Geçiş Efekti
            this.DoubleBuffered = true;
            pnlMainCard.Paint += pnlMainCard_Paint;
        }

        private void FrmDoktorGiris_Load(object sender, EventArgs e)
        {
            ApplyDarkTheme();

            // Form ve Panel Modernizasyonu
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 40, 40));
            pnlMainCard.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMainCard.Width, pnlMainCard.Height, 30, 30));

            // İkonları Yükle
            var assembly = typeof(DevExpress.Images.ImageResourceCache).Assembly;
            txtTC.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Business.Business_User.svg", assembly);
            txtSifre.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Security.Key.svg", assembly);
        }

        private static Color Hex(string hex) => ColorTranslator.FromHtml(hex);
        private static Color HexA(int alpha, string hex)
        {
            var c = ColorTranslator.FromHtml(hex);
            return Color.FromArgb(alpha, c);
        }

        private void ApplyDarkTheme()
        {
            this.LookAndFeel.UseDefaultLookAndFeel = false;

            var cardBg = Hex("#1E2A38");
            pnlMainCard.Appearance.BackColor = cardBg;
            pnlMainCard.Appearance.Options.UseBackColor = true;
            pnlMainCard.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMainCard.Padding = new Padding(24, 24, 24, 18);

            lblTitle.AllowHtmlString = true;
            lblTitle.Appearance.ForeColor = Hex("#ECEFF1");
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Text = "<color=#48CFAD>DOKTOR</color> <color=#ECEFF1>GİRİŞİ</color>";

            lblSubTitle.Appearance.ForeColor = Hex("#B0BEC5");
            lblSubTitle.Appearance.Options.UseForeColor = true;

            btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnClose.Appearance.BackColor = Color.Transparent;
            btnClose.Appearance.Options.UseBackColor = true;
            btnClose.Appearance.ForeColor = Hex("#B0BEC5");
            btnClose.Appearance.Options.UseForeColor = true;
            btnClose.AppearanceHovered.ForeColor = Color.White;
            btnClose.AppearanceHovered.Options.UseForeColor = true;
            btnClose.Text = "✕";

            StyleTextEdit(txtTC, "TC Kimlik No");
            StyleTcEdit(txtTC);
            StyleTextEdit(txtSifre, "Şifre");
            txtSifre.Properties.PasswordChar = '•';

            // Doktor butonu rengi (mint)
            StylePrimaryButton(btnLogin, "#48CFAD");

            hypKayitOl.AllowHtmlString = true;
            hypKayitOl.Appearance.ForeColor = Hex("#B0BEC5");
            hypKayitOl.Appearance.Options.UseForeColor = true;
            hypKayitOl.Text = "Hesabınız yok mu? <color=#48CFAD><href>Kayıt Ol</href></color>";
        }

        private void StyleTextEdit(TextEdit edit, string prompt)
        {
            edit.Properties.NullValuePrompt = prompt;
            edit.Properties.NullValuePromptShowForEmptyValue = true;
            edit.Properties.Appearance.BackColor = Hex("#223244");
            edit.Properties.Appearance.ForeColor = Hex("#ECEFF1");
            edit.Properties.Appearance.Font = new Font("Segoe UI", 12F);
            edit.Properties.Appearance.Options.UseBackColor = true;
            edit.Properties.Appearance.Options.UseForeColor = true;
            edit.Properties.Appearance.Options.UseFont = true;

            edit.Properties.AppearanceFocused.BackColor = Hex("#25384C");
            edit.Properties.AppearanceFocused.ForeColor = Hex("#FFFFFF");
            edit.Properties.AppearanceFocused.Options.UseBackColor = true;
            edit.Properties.AppearanceFocused.Options.UseForeColor = true;

            edit.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            edit.Properties.AutoHeight = false;
        }

        private void StyleTcEdit(TextEdit edit)
        {
            edit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Simple;
            edit.Properties.Mask.EditMask = "00000000000";
            edit.Properties.Mask.UseMaskAsDisplayFormat = true;
            edit.Properties.Mask.ShowPlaceHolders = false;
            edit.Properties.Mask.PlaceHolder = ' ';
            edit.Properties.MaxLength = 11;
            edit.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
        }

        private void StylePrimaryButton(SimpleButton btn, string baseHex)
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
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.Font = new Font("Segoe UI Semibold", 14F);
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Appearance.Options.UseFont = true;

            btn.AppearanceHovered.BackColor = hoverC;
            btn.AppearanceHovered.ForeColor = Color.White;
            btn.AppearanceHovered.Options.UseBackColor = true;
            btn.AppearanceHovered.Options.UseForeColor = true;

            btn.AppearancePressed.BackColor = pressedC;
            btn.AppearancePressed.ForeColor = Color.White;
            btn.AppearancePressed.Options.UseBackColor = true;
            btn.AppearancePressed.Options.UseForeColor = true;
        }

        private void FrmDoktorGiris_Paint(object sender, PaintEventArgs e)
        {
            // Arka plan: panel konseptiyle uyumlu koyu degrade
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            using var bg = new LinearGradientBrush(this.ClientRectangle, Hex("#0F172A"), Hex("#1E2A38"), LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(bg, this.ClientRectangle);

            var glowRect = new Rectangle(-100, -100, this.Width + 200, this.Height / 2);
            using var glow = new LinearGradientBrush(glowRect, HexA(90, "#48CFAD"), Color.Transparent, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(glow, glowRect);
        }

        private void pnlMainCard_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlMainCard.ClientRectangle;
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

        private void hypKayitOl_Click(object sender, EventArgs e)
        {
            FrmDoktorKayit frm = new FrmDoktorKayit();
            frm.ShowDialog();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Validasyon
            if (string.IsNullOrWhiteSpace(txtTC.Text) || string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                XtraMessageBox.Show("Lütfen TC Kimlik No ve Şifrenizi girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    string query = "SELECT COUNT(*) FROM Tbl_Doktorlar WHERE DoktorTC = @tc AND DoktorSifre = @sifre";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@tc", txtTC.Text.Trim());
                        cmd.Parameters.AddWithValue("@sifre", txtSifre.Text);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        
                        if (count > 0)
                        {
                            var panel = new FrmDoktorPanel { DoktorTC = txtTC.Text.Trim() };
                            panel.FormClosed += (_, _) => this.Close();
                            panel.Show();
                            this.Hide();
                        }
                        else
                        {
                            XtraMessageBox.Show("TC Kimlik No veya Şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

