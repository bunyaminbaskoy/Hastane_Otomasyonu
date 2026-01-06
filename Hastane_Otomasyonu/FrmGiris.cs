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
using System.Drawing.Drawing2D;

namespace Hastane_Otomasyonu
{
    public partial class FrmGiris : DevExpress.XtraEditors.XtraForm
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public FrmGiris()
        {
            InitializeComponent();

            // Formu sürüklenebilir yap
            SetupDraggableForm();

            // Kartın rounded border'ını custom çiz (BorderStyle+Region kesilme yapabiliyor)
            pnlMainCard.Paint += pnlMainCard_Paint;
        }

        private void SetupDraggableForm()
        {
            pnlOverlay.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void FrmGiris_Load(object sender, EventArgs e)
        {
            // ===== MODERN THEME (HEX / Material-Flat Palet) =====
            // Not: Designer içinde atanmış renkleri burada DevExpress Appearance ile override ediyoruz.
            // Böylece "Windows default" havası gider ve tek noktadan tema yönetilir.
            ApplyModernTheme();

            // ===== ROUNDED CORNERS (Oval Köşeler) =====
            // Ana formun köşelerini ovalleştir (40px)
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 40, 40));

            // Ortadaki beyaz panelin köşelerini ovalleştir (40px - daha modern ve yumuşak)
            pnlMainCard.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMainCard.Width, pnlMainCard.Height, 40, 40));

            // Butonların köşelerini ovalleştir (15px - modern flat style)
            // Not: DevExpress SimpleButton'ın kendi köşelerini ovalleştirmek için LookAndFeel de kullanılabilir 
            // ama Region kesin çözüm sağlar.
            int btnCorner = 15;
            btnHastaGiris.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnHastaGiris.Width, btnHastaGiris.Height, btnCorner, btnCorner));
            btnDoktorGiris.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnDoktorGiris.Width, btnDoktorGiris.Height, btnCorner, btnCorner));
            btnSekreterGiris.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnSekreterGiris.Width, btnSekreterGiris.Height, btnCorner, btnCorner));

            // ===== BUTON YAZI RENKLERİNİ ZORLA BEYAZ YAP =====
            // Designer'da set edildi ama Load'da tekrar garanti altına alalım
            SetButtonTextColor(btnHastaGiris, Color.White);
            SetButtonTextColor(btnDoktorGiris, Color.White);
            SetButtonTextColor(btnSekreterGiris, Color.White);

            // ===== SVG İKONLARI YÜKLEME =====
            LoadSvgIcons();
        }

        private static Color Hex(string hex)
        {
            // hex format: "#RRGGBB"
            return ColorTranslator.FromHtml(hex);
        }

        private static Color HexA(int alpha, string hex)
        {
            var c = ColorTranslator.FromHtml(hex);
            return Color.FromArgb(alpha, c);
        }

        private void ApplyModernTheme()
        {
            // ---- Skin override (Program.cs içindeki global skin'i bu form için ez)
            // Global: The Bezier. Bu formda arka planı sabitlemek için default look&feel'i kapatıyoruz.
            this.LookAndFeel.UseDefaultLookAndFeel = false;

            // ---- En arka plan (büyük form): hastane_bg görseli görünsün
            // BackgroundImage zaten Designer/Resx tarafında bağlı. Biz burada formu tek renk ile ezmiyoruz.
            // Görselin görünmesi için overlay'i yarı saydam bırakıyoruz.
            pnlOverlay.Appearance.BackColor = Color.FromArgb(160, 31, 58, 86);
            pnlOverlay.Appearance.Options.UseBackColor = true;

            // ---- Küçük form (kart): #1E2A38 (istenen)
            var cardBg = Hex("#1E2A38");
            // DevExpress border + Region birlikte köşelerde "kesilmiş" görünüm yapabiliyor.
            // Bu yüzden kendi border'ımızı Paint ile yuvarlak çiziyoruz.
            pnlMainCard.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMainCard.Padding = new Padding(2);
            pnlMainCard.Appearance.BackColor = cardBg;
            pnlMainCard.Appearance.BackColor2 = cardBg;
            pnlMainCard.Appearance.Options.UseBackColor = true;

            // stackPanel1 kartın içi (aynı renk)
            stackPanel1.Appearance.BackColor = cardBg;
            stackPanel1.Appearance.BackColor2 = cardBg;
            stackPanel1.Appearance.Options.UseBackColor = true;

            // ---- Yazılar (koyu kart üstünde okunabilirlik)
            if (lblTitle != null)
            {
                lblTitle.AllowHtmlString = true;
                lblTitle.Appearance.ForeColor = Hex("#ECEFF1");
                lblTitle.Appearance.Options.UseForeColor = true;
                lblTitle.Text = "<color=#64B5F6>H</color><color=#ECEFF1>ASTANE</color>";
            }

            if (lblSubTitle != null)
            {
                lblSubTitle.Appearance.ForeColor = Hex("#B0BEC5");
                lblSubTitle.Appearance.Options.UseForeColor = true;
            }

            if (lblFooter != null)
            {
                lblFooter.Appearance.ForeColor = Hex("#90A4AE");
                lblFooter.Appearance.Options.UseForeColor = true;
            }

            // ---- Butonlar (istenen yeni palet)
            // Hasta: #5D9CEC (Yumuşak Gök Mavisi)
            ApplyModernButtonFromBase(btnHastaGiris, "#5D9CEC", textColor: Color.White);

            // Doktor: #48CFAD (Mint Yeşili - Mat)
            ApplyModernButtonFromBase(btnDoktorGiris, "#48CFAD", textColor: Color.White);

            // Sekreter: #FFCE54 (Ayçiçeği Sarısı - Pastel) -> yazı koyu
            ApplyModernButtonFromBase(btnSekreterGiris, "#FFCE54", textColor: Hex("#1E2A38"));

            // Close butonu (varsa) - daha "premium" görünüm
            if (btnClose != null)
            {
                btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                btnClose.Appearance.ForeColor = Hex("#B0BEC5");
                btnClose.Appearance.Options.UseForeColor = true;
                btnClose.AppearanceHovered.ForeColor = Color.White;
                btnClose.AppearanceHovered.Options.UseForeColor = true;
            }
        }

        private void ApplyModernButtonFromBase(SimpleButton btn, string baseHex, Color textColor)
        {
            var c = Hex(baseHex);
            // Hover biraz aç, pressed biraz koyu (Material hissi)
            var hover = AdjustColor(c, 1.10f);
            var pressed = AdjustColor(c, 0.90f);

            ApplyModernButton(btn,
                base1: baseHex, base2: baseHex,
                hover1: ToHex(hover), hover2: ToHex(hover),
                pressed1: ToHex(pressed), pressed2: ToHex(pressed));

            SetButtonTextColor(btn, textColor);
        }

        private static Color AdjustColor(Color color, float factor)
        {
            // factor > 1: lighten, factor < 1: darken
            int r = Math.Clamp((int)(color.R * factor), 0, 255);
            int g = Math.Clamp((int)(color.G * factor), 0, 255);
            int b = Math.Clamp((int)(color.B * factor), 0, 255);
            return Color.FromArgb(color.A, r, g, b);
        }

        private static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private void ApplyModernButton(SimpleButton btn,
            string base1, string base2,
            string hover1, string hover2,
            string pressed1, string pressed2)
        {
            btn.LookAndFeel.UseDefaultLookAndFeel = false;
            btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            // Normal
            btn.Appearance.BackColor = Hex(base1);
            btn.Appearance.BackColor2 = Hex(base2);
            btn.Appearance.GradientMode = LinearGradientMode.Vertical;
            btn.Appearance.Options.UseBackColor = true;

            // Hover
            btn.AppearanceHovered.BackColor = Hex(hover1);
            btn.AppearanceHovered.BackColor2 = Hex(hover2);
            btn.AppearanceHovered.GradientMode = LinearGradientMode.Vertical;
            btn.AppearanceHovered.Options.UseBackColor = true;

            // Pressed
            btn.AppearancePressed.BackColor = Hex(pressed1);
            btn.AppearancePressed.BackColor2 = Hex(pressed2);
            btn.AppearancePressed.GradientMode = LinearGradientMode.Vertical;
            btn.AppearancePressed.Options.UseBackColor = true;

            // Subtle highlight border (cam efekti gibi çok ince)
            btn.Appearance.BorderColor = HexA(120, "#FFFFFF");
            btn.Appearance.Options.UseBorderColor = true;
        }

        private void SetButtonTextColor(SimpleButton btn, Color color)
        {
            btn.Appearance.ForeColor = color;
            btn.Appearance.Options.UseForeColor = true;
            btn.AppearanceHovered.ForeColor = color;
            btn.AppearanceHovered.Options.UseForeColor = true;
            btn.AppearancePressed.ForeColor = color;
            btn.AppearancePressed.Options.UseForeColor = true;
        }

        private void pnlMainCard_Paint(object sender, PaintEventArgs e)
        {
            // Yuvarlatılmış köşeli beyaz border çiz (köşe kesilmesini giderir)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlMainCard.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            int radius = 40; // FrmGiris_Load'da Region ile verdiğimiz ile uyumlu
            using var path = GetRoundedRectPath(rect, radius);
            using var pen = new Pen(Color.FromArgb(200, Color.White), 2f);
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

        private void LoadSvgIcons()
        {
            try
            {
                // Hasta Girişi - User Icon
                btnHastaGiris.ImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources(
                    "DevExpress.Images.SvgImages.Business.Business_User.svg",
                    typeof(DevExpress.Images.ImageResourceCache).Assembly);
                btnHastaGiris.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);

                // Doktor Girişi - Stethoscope/Medical Icon
                btnDoktorGiris.ImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources(
                    "DevExpress.Images.SvgImages.Icon Builder.Travel_Medical.svg",
                    typeof(DevExpress.Images.ImageResourceCache).Assembly);
                btnDoktorGiris.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);

                // Sekreter Girişi - Clipboard/Guide Icon
                btnSekreterGiris.ImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources(
                    "DevExpress.Images.SvgImages.Business.Business_BusinessGuide.svg",
                    typeof(DevExpress.Images.ImageResourceCache).Assembly);
                btnSekreterGiris.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Icon loading error: " + ex.Message);
            }
        }

        private void btnHastaGiris_Click(object sender, EventArgs e)
        {
            FrmHastaGiris frm = new FrmHastaGiris();
            frm.Show();
        }

        private void btnDoktorGiris_Click(object sender, EventArgs e)
        {
            FrmDoktorGiris frm = new FrmDoktorGiris();
            frm.Show();
        }

        private void btnSekreterGiris_Click(object sender, EventArgs e)
        {
            FrmSekreterGiris frm = new FrmSekreterGiris();
            frm.Show();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pnlOverlay_Paint(object sender, PaintEventArgs e)
        {

        }

        // Buton tıklama olaylarını ileride ekleyeceğiz
    }
}

