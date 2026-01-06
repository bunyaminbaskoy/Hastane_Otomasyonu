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
    public partial class FrmHastaKayit : DevExpress.XtraEditors.XtraForm
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public FrmHastaKayit()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Paint += FrmHastaKayit_Paint;
            pnlMain.Paint += pnlMain_Paint;
        }

        // Extra profile fields (programmatic UI; designer bozulmasın)
        private DateEdit _dtDogumTarih;
        private ComboBoxEdit _cmbCinsiyet;
        private ComboBoxEdit _cmbKanGrubu;
        private TextEdit _txtEmail;
        private TextEdit _txtBoy;
        private TextEdit _txtKilo;
        private TextEdit _txtIl;
        private TextEdit _txtIlce;
        private MemoEdit _txtAdres;

        private static Color Hex(string hex) => ColorTranslator.FromHtml(hex);
        private static Color HexA(int alpha, string hex)
        {
            var c = ColorTranslator.FromHtml(hex);
            return Color.FromArgb(alpha, c);
        }

        private void FrmHastaKayit_Paint(object sender, PaintEventArgs e)
        {
            // Arka plan: giriş ekranlarıyla uyumlu koyu degrade + hafif accent glow
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            using var bg = new LinearGradientBrush(this.ClientRectangle, Hex("#0F172A"), Hex("#1E2A38"), LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(bg, this.ClientRectangle);

            var glowRect = new Rectangle(-120, -120, this.Width + 240, this.Height / 2);
            using var glow = new LinearGradientBrush(glowRect, HexA(90, "#5D9CEC"), Color.Transparent, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(glow, glowRect);
        }

        private void FrmHastaKayit_Load(object sender, EventArgs e)
        {
            ApplyDarkTheme();

            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 40, 40));
            pnlMain.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMain.Width, pnlMain.Height, 30, 30));
            BuildExtraFields();
            
            // İkonları Yükle
            var assembly = typeof(DevExpress.Images.ImageResourceCache).Assembly;
            txtAd.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Business.Business_Badge.svg", assembly);
            txtTC.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Business.Business_User.svg", assembly);
            txtTel.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Mobile.MobilePhone.svg", assembly);
            txtSifre.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Security.Key.svg", assembly);
        }

        private void ApplyDarkTheme()
        {
            // Global skin (The Bezier) etkisini bu formda arka plan için ez
            this.LookAndFeel.UseDefaultLookAndFeel = false;

            var cardBg = Hex("#1E2A38");
            var accent = Hex("#5D9CEC");

            // Ana kart
            pnlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMain.Appearance.BackColor = cardBg;
            pnlMain.Appearance.Options.UseBackColor = true;
            pnlMain.Padding = new Padding(2);

            // Sol şerit
            pnlLeft.Appearance.BackColor = Hex("#223244");
            pnlLeft.Appearance.BackColor2 = Hex("#1E2A38");
            pnlLeft.Appearance.GradientMode = LinearGradientMode.Vertical;
            pnlLeft.Appearance.Options.UseBackColor = true;

            // "No image data" yazısını kaldır
            picLeft.Properties.NullText = "";
            picLeft.Properties.ShowMenu = false;

            lblLeftTitle.Appearance.ForeColor = Hex("#ECEFF1");
            lblLeftTitle.Appearance.Options.UseForeColor = true;

            // Başlık
            lblTitle.AllowHtmlString = true;
            lblTitle.Appearance.ForeColor = Hex("#ECEFF1");
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Text = $"<color=#{accent.R:X2}{accent.G:X2}{accent.B:X2}>Hasta</color> <color=#ECEFF1>Kaydı</color>";

            // Close
            btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnClose.Appearance.BackColor = Color.Transparent;
            btnClose.Appearance.Options.UseBackColor = true;
            btnClose.Appearance.ForeColor = Hex("#B0BEC5");
            btnClose.Appearance.Options.UseForeColor = true;
            btnClose.AppearanceHovered.ForeColor = Color.White;
            btnClose.AppearanceHovered.Options.UseForeColor = true;
            btnClose.Text = "✕";

            // Inputlar
            StyleTextEdit(txtAd, "Adınız");
            StyleTextEdit(txtSoyad, "Soyadınız");
            StyleTextEdit(txtTC, "TC Kimlik No");
            StyleTextEdit(txtTel, "Telefon Numarası");
            StyleTextEdit(txtSifre, "Şifre", isPassword: true);

            // Buton (hasta: mavi)
            StylePrimaryButton(btnKaydet, "#5D9CEC", textColor: Color.White);
        }

        private void BuildExtraFields()
        {
            // Bu metodu birden fazla çağırmayalım
            if (_dtDogumTarih != null) return;

            // Formu uzat (kayıt ekranı yeni alanlar için yer açsın)
            this.ClientSize = new Size(800, 920);
            pnlMain.Size = new Size(700, 820);
            pnlMain.Location = new Point(50, 50);
            pnlMain.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMain.Width, pnlMain.Height, 30, 30));

            // Var olan kaydet butonunu aşağı al
            btnKaydet.Location = new Point(btnKaydet.Location.X, 735);
            btnKaydet.Size = new Size(360, 54);

            int x = 300;
            int w = 360;
            int y = 340;
            int h = 40;

            _dtDogumTarih = new DateEdit
            {
                Location = new Point(x, y),
                Size = new Size(w, h)
            };
            _dtDogumTarih.Properties.AutoHeight = false;
            _dtDogumTarih.Properties.NullValuePrompt = "Doğum Tarihi";
            _dtDogumTarih.Properties.NullValuePromptShowForEmptyValue = true;
            _dtDogumTarih.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            _dtDogumTarih.Properties.Buttons.Clear();
            _dtDogumTarih.Properties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            _dtDogumTarih.Properties.CalendarTimeProperties.Buttons.Clear();
            _dtDogumTarih.Properties.CalendarTimeProperties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            pnlMain.Controls.Add(_dtDogumTarih);

            y += 60;
            _cmbCinsiyet = new ComboBoxEdit
            {
                Location = new Point(x, y),
                Size = new Size(175, h)
            };
            _cmbCinsiyet.Properties.AutoHeight = false;
            _cmbCinsiyet.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _cmbCinsiyet.Properties.NullValuePrompt = "Cinsiyet";
            _cmbCinsiyet.Properties.NullValuePromptShowForEmptyValue = true;
            _cmbCinsiyet.Properties.Items.AddRange(new object[] { "Erkek", "Kadın" });
            pnlMain.Controls.Add(_cmbCinsiyet);

            _cmbKanGrubu = new ComboBoxEdit
            {
                Location = new Point(485, y),
                Size = new Size(175, h)
            };
            _cmbKanGrubu.Properties.AutoHeight = false;
            _cmbKanGrubu.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _cmbKanGrubu.Properties.NullValuePrompt = "Kan Grubu";
            _cmbKanGrubu.Properties.NullValuePromptShowForEmptyValue = true;
            _cmbKanGrubu.Properties.Items.AddRange(new object[] { "A Rh+", "A Rh-", "B Rh+", "B Rh-", "AB Rh+", "AB Rh-", "0 Rh+", "0 Rh-" });
            pnlMain.Controls.Add(_cmbKanGrubu);

            y += 60;
            _txtEmail = new TextEdit
            {
                Location = new Point(x, y),
                Size = new Size(w, h)
            };
            pnlMain.Controls.Add(_txtEmail);

            y += 60;
            _txtBoy = new TextEdit { Location = new Point(x, y), Size = new Size(175, h) };
            _txtBoy.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            _txtBoy.Properties.Mask.EditMask = "d";
            pnlMain.Controls.Add(_txtBoy);

            _txtKilo = new TextEdit { Location = new Point(485, y), Size = new Size(175, h) };
            _txtKilo.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            _txtKilo.Properties.Mask.EditMask = "d";
            pnlMain.Controls.Add(_txtKilo);

            y += 60;
            _txtIl = new TextEdit { Location = new Point(x, y), Size = new Size(175, h) };
            _txtIlce = new TextEdit { Location = new Point(485, y), Size = new Size(175, h) };
            pnlMain.Controls.Add(_txtIl);
            pnlMain.Controls.Add(_txtIlce);

            y += 60;
            _txtAdres = new MemoEdit { Location = new Point(x, y), Size = new Size(w, 90) };
            pnlMain.Controls.Add(_txtAdres);

            // Style’ları konseptle uyumla (mevcut StyleTextEdit’i kullan)
            StyleTextEdit(_dtDogumTarih, "Doğum Tarihi");
            StyleTextEdit(_cmbCinsiyet, "Cinsiyet");
            StyleTextEdit(_cmbKanGrubu, "Kan Grubu");
            StyleTextEdit(_txtEmail, "E-Posta Adresi");
            StyleTextEdit(_txtBoy, "Boy (cm)");
            StyleTextEdit(_txtKilo, "Kilo (kg)");
            StyleTextEdit(_txtIl, "İl");
            StyleTextEdit(_txtIlce, "İlçe");

            _txtAdres.Properties.NullValuePrompt = "Açık Adres";
            _txtAdres.Properties.NullValuePromptShowForEmptyValue = true;
            _txtAdres.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            _txtAdres.Properties.Appearance.BackColor = Hex("#223244");
            _txtAdres.Properties.Appearance.ForeColor = Hex("#ECEFF1");
            _txtAdres.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            _txtAdres.Properties.Appearance.Options.UseBackColor = true;
            _txtAdres.Properties.Appearance.Options.UseForeColor = true;
            _txtAdres.Properties.Appearance.Options.UseFont = true;
        }

        private void StyleTextEdit(BaseEdit edit, string prompt)
        {
            // BaseEdit overload: DateEdit / ComboBoxEdit / TextEdit ortak stil
            if (edit == null) return;

            if (edit is TextEdit te)
            {
                StyleTextEdit(te, prompt);
                return;
            }
            if (edit is DateEdit de)
            {
                de.Properties.NullValuePrompt = prompt;
                de.Properties.NullValuePromptShowForEmptyValue = true;
                de.Properties.Appearance.BackColor = Hex("#223244");
                de.Properties.Appearance.ForeColor = Hex("#ECEFF1");
                de.Properties.Appearance.Font = new Font("Segoe UI", 11F);
                de.Properties.Appearance.Options.UseBackColor = true;
                de.Properties.Appearance.Options.UseForeColor = true;
                de.Properties.Appearance.Options.UseFont = true;
                de.Properties.AppearanceFocused.BackColor = Hex("#25384C");
                de.Properties.AppearanceFocused.ForeColor = Hex("#FFFFFF");
                de.Properties.AppearanceFocused.Options.UseBackColor = true;
                de.Properties.AppearanceFocused.Options.UseForeColor = true;
                de.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
                de.Properties.AutoHeight = false;
                return;
            }
            if (edit is ComboBoxEdit ce)
            {
                ce.Properties.NullValuePrompt = prompt;
                ce.Properties.NullValuePromptShowForEmptyValue = true;
                ce.Properties.Appearance.BackColor = Hex("#223244");
                ce.Properties.Appearance.ForeColor = Hex("#ECEFF1");
                ce.Properties.Appearance.Font = new Font("Segoe UI", 11F);
                ce.Properties.Appearance.Options.UseBackColor = true;
                ce.Properties.Appearance.Options.UseForeColor = true;
                ce.Properties.Appearance.Options.UseFont = true;
                ce.Properties.AppearanceFocused.BackColor = Hex("#25384C");
                ce.Properties.AppearanceFocused.ForeColor = Hex("#FFFFFF");
                ce.Properties.AppearanceFocused.Options.UseBackColor = true;
                ce.Properties.AppearanceFocused.Options.UseForeColor = true;
                ce.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
                ce.Properties.AutoHeight = false;
            }
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
            // Kart çevresine beyaz rounded border
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
                string.IsNullOrWhiteSpace(txtTC.Text) || string.IsNullOrWhiteSpace(txtSifre.Text))
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
                    // TC kontrolü - Aynı TC'ye sahip hasta var mı?
                    string checkQuery = "SELECT COUNT(*) FROM Tbl_Hastalar WHERE HastaTC = @tc";
                    using (var checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@tc", txtTC.Text.Trim());
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        
                        if (count > 0)
                        {
                            XtraMessageBox.Show("Bu TC Kimlik No ile kayıtlı bir hasta zaten mevcut!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Hasta kaydı ekle
                    string insertQuery = @"INSERT INTO Tbl_Hastalar 
                                          (HastaAd, HastaSoyad, HastaTC, HastaTelefon, HastaSifre, HastaCinsiyet,
                                           HastaDogumTarihi, HastaKanGrubu, HastaEmail, HastaBoyCm, HastaKiloKg, HastaIl, HastaIlce, HastaAdres) 
                                          VALUES (@ad, @soyad, @tc, @telefon, @sifre, @cinsiyet,
                                           @dogum, @kangrubu, @email, @boy, @kilo, @il, @ilce, @adres)";
                    
                    using (var insertCmd = new SqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@ad", txtAd.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@soyad", txtSoyad.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@tc", txtTC.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@telefon", string.IsNullOrWhiteSpace(txtTel.Text) ? (object)DBNull.Value : txtTel.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@sifre", txtSifre.Text);
                        insertCmd.Parameters.AddWithValue("@cinsiyet", _cmbCinsiyet?.EditValue == null ? (object)DBNull.Value : _cmbCinsiyet.Text);
                        insertCmd.Parameters.AddWithValue("@dogum", _dtDogumTarih?.EditValue == null ? (object)DBNull.Value : Convert.ToDateTime(_dtDogumTarih.EditValue).Date);
                        insertCmd.Parameters.AddWithValue("@kangrubu", _cmbKanGrubu?.EditValue == null ? (object)DBNull.Value : _cmbKanGrubu.Text);
                        insertCmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(_txtEmail?.Text) ? (object)DBNull.Value : _txtEmail.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@boy", int.TryParse(_txtBoy?.Text, out var boy) ? boy : (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@kilo", int.TryParse(_txtKilo?.Text, out var kilo) ? kilo : (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@il", string.IsNullOrWhiteSpace(_txtIl?.Text) ? (object)DBNull.Value : _txtIl.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@ilce", string.IsNullOrWhiteSpace(_txtIlce?.Text) ? (object)DBNull.Value : _txtIlce.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@adres", string.IsNullOrWhiteSpace(_txtAdres?.Text) ? (object)DBNull.Value : _txtAdres.Text.Trim());

                        int result = insertCmd.ExecuteNonQuery();
                        
                        if (result > 0)
                        {
                            XtraMessageBox.Show("Hasta kaydı başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

