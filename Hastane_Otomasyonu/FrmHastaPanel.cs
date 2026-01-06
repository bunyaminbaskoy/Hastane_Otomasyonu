using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DevExpress.XtraEditors;
using DevExpress.Utils.Layout;
using Microsoft.Data.SqlClient;
using Hastane_Otomasyonu.Database;
using Hastane_Otomasyonu.Services;

namespace Hastane_Otomasyonu
{
    public partial class FrmHastaPanel : DevExpress.XtraBars.FluentDesignSystem.FluentDesignForm
    {
        public string HastaTC;
        private string _secilenSaat = "";
        private DevExpress.XtraBars.Navigation.NavigationPage _pageAnaSayfa;
        private DevExpress.XtraBars.Navigation.NavigationPage _pageTibbiBilgiler;
        private DevExpress.XtraBars.Navigation.NavigationPage _pageProfil;
        private DevExpress.XtraBars.Navigation.NavigationPage _pageIletisim;
        private DevExpress.XtraBars.Navigation.NavigationPage _pageRandevuModern;
        private DevExpress.XtraEditors.LabelControl _lblHomeWelcome;
        private DevExpress.XtraEditors.LabelControl _lblHomeNextAppt;
        private FlowLayoutPanel _homeApptList;
        private DevExpress.XtraBars.Navigation.AccordionControlElement _navAnaSayfa;
        private ComboBoxEdit _modernCmbBrans;
        private ComboBoxEdit _modernCmbDoktor;
        private DateEdit _modernDtTarih;
        private SimpleButton _medAllergySave;
        private TextEdit _medAllergyInput;
        private SimpleButton _medAllergyAdd;
        private FlowLayoutPanel _medAllergyList;
        private System.Collections.Generic.List<string> _medAllergyItems = new System.Collections.Generic.List<string>();
        private SimpleButton _medBtnTahliller;
        private SimpleButton _medBtnReceteler;
        private PanelControl _medPanelTahliller;
        private PanelControl _medPanelReceteler;
        private DevExpress.XtraGrid.GridControl _medGridTahliller;
        private DevExpress.XtraGrid.Views.Grid.GridView _medViewTahliller;
        private DevExpress.XtraGrid.GridControl _medGridReceteler;
        private DevExpress.XtraGrid.Views.Grid.GridView _medViewReceteler;
        private PictureEdit _profilePhoto;
        private SimpleButton _profilePhotoUpload;
        private LabelControl _profileTcMasked;
        private TextEdit _profileAdSoyad;
        private DateEdit _profileDogumTarih;
        private LabelControl _profileAgeLabel;
        private ComboBoxEdit _profileCinsiyet;
        private ComboBoxEdit _profileKanGrubu;
        private TextEdit _profileBoy;
        private TextEdit _profileKilo;
        private DevExpress.XtraEditors.ProgressBarControl _profileBmiProgress;
        private LabelControl _profileBmiLabel;
        private TextEdit _profileTelefon;
        private TextEdit _profileEmail;
        private TextEdit _profileIl;
        private TextEdit _profileIlce;
        private MemoEdit _profileAdres;
        private SimpleButton _profileSave;

        // AI (Gemini) - İletişim & Etkileşim sayfası yerine
        private MemoEdit _aiChat;
        private MemoEdit _aiInput;
        private SimpleButton _aiBtnAnalyze;
        private SimpleButton _aiBtnSend;
        private SimpleButton _aiBtnPdfSelect;
        private SimpleButton _aiBtnPdfClear;
        private LabelControl _aiStatus;
        private CancellationTokenSource _aiCts;
        private readonly HttpClient _aiHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
        private string _aiChatBuffer = "";
        private byte[] _aiPdfBytes;
        private string _aiPdfName;

        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int CS_DROPSHADOW = 0x00020000;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public FrmHastaPanel()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            // Sağ üst kapatma butonu resize'da kaymasın
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.Resize += (_, _) =>
            {
                btnClose.Location = new Point(ClientSize.Width - btnClose.Width - 8, 5);
            };

            // Sürükleme olayı
            this.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FrmHastaPanel_Load(object sender, EventArgs e)
        {
            ApplyModernStyles();
            EnsureSidebarPages();
            navigationFrame1.SelectedPage = _pageAnaSayfa ?? pageDashboard;
            LoadHastaBilgileri();
            LoadBranslar();
            LoadNextAppointment(); // Dashboard verisini yükle
        }

        private void EnsureSidebarPages()
        {
            // Bu menü başlıklarının içeriklerini sonra dolduracağız; şimdilik placeholder sayfalar.
            _pageAnaSayfa ??= CreateModernHomePage();
            _pageRandevuModern ??= CreateModernRandevuPage();
            _pageTibbiBilgiler ??= CreateModernMedicalPage();
            _pageProfil ??= CreateModernProfilePage();
            _pageIletisim ??= CreateAiAssistantPage();

            if (!navigationFrame1.Pages.Contains(_pageAnaSayfa))
            {
                navigationFrame1.Controls.Add(_pageAnaSayfa);
                navigationFrame1.Pages.Add(_pageAnaSayfa);
            }
            if (!navigationFrame1.Pages.Contains(_pageRandevuModern))
            {
                navigationFrame1.Controls.Add(_pageRandevuModern);
                navigationFrame1.Pages.Add(_pageRandevuModern);
            }
            if (!navigationFrame1.Pages.Contains(_pageTibbiBilgiler))
            {
                navigationFrame1.Controls.Add(_pageTibbiBilgiler);
                navigationFrame1.Pages.Add(_pageTibbiBilgiler);
            }
            if (!navigationFrame1.Pages.Contains(_pageProfil))
            {
                navigationFrame1.Controls.Add(_pageProfil);
                navigationFrame1.Pages.Add(_pageProfil);
            }
            if (!navigationFrame1.Pages.Contains(_pageIletisim))
            {
                navigationFrame1.Controls.Add(_pageIletisim);
                navigationFrame1.Pages.Add(_pageIletisim);
            }
        }

        private DevExpress.XtraBars.Navigation.NavigationPage CreatePlaceholderPage(string title, string subtitle)
        {
            var page = new DevExpress.XtraBars.Navigation.NavigationPage();
            page.Appearance.BackColor = Color.Transparent;
            page.Appearance.Options.UseBackColor = true;

            var card = new PanelControl();
            card.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            card.Appearance.BackColor = Color.FromArgb(30, 42, 56); // #1E2A38
            card.Appearance.Options.UseBackColor = true;
            card.Size = new Size(780, 220);
            card.Location = new Point(80, 60);

            var lblTitle = new LabelControl();
            lblTitle.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(236, 239, 241); // #ECEFF1
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Location = new Point(40, 40);
            lblTitle.Text = title;

            var lblSub = new LabelControl();
            lblSub.Appearance.Font = new Font("Segoe UI", 12F);
            lblSub.Appearance.ForeColor = Color.FromArgb(176, 190, 197); // #B0BEC5
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Appearance.Options.UseForeColor = true;
            lblSub.Location = new Point(42, 95);
            lblSub.Text = subtitle;

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblSub);
            page.Controls.Add(card);
            return page;
        }

        private DevExpress.XtraBars.Navigation.NavigationPage CreateAiAssistantPage()
        {
            var page = new DevExpress.XtraBars.Navigation.NavigationPage();
            page.Name = "pageAiAssistant";
            page.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            page.Appearance.Options.UseBackColor = true;
            page.Padding = new Padding(22);

            var card = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 24, 28, 22)
            };
            card.Appearance.BackColor = Color.Transparent;
            card.Appearance.Options.UseBackColor = true;

            var fill = Color.FromArgb(30, 42, 56); // #1E2A38
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = card.ClientRectangle;
                rect.Inflate(-1, -1);
                int radius = 22;
                int d = radius * 2;
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                using var brush = new SolidBrush(fill);
                e.Graphics.FillPath(brush, path);
                using var pen = new Pen(Color.FromArgb(120, Color.White), 1.4f);
                e.Graphics.DrawPath(pen, path);
            };

            var lblTitle = new LabelControl
            {
                AllowHtmlString = true,
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 58,
                Text = "<color=#5D9CEC>Yapay Zeka</color> <color=#ECEFF1>Asistanı</color>"
            };
            lblTitle.Appearance.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = ColorTranslator.FromHtml("#ECEFF1");
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;

            var lblInfo = new LabelControl
            {
                Dock = DockStyle.Top,
                Height = 44,
                Text = "Kan tahlili PDF dosyanızı yükleyip (varsa referans aralıklarıyla) normal/yüksek/düşük olarak özetler. Tıbbi tanı yerine geçmez."
            };
            lblInfo.Appearance.Font = new Font("Segoe UI", 11F);
            lblInfo.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
            lblInfo.Appearance.Options.UseFont = true;
            lblInfo.Appearance.Options.UseForeColor = true;

            _aiStatus = new LabelControl
            {
                Dock = DockStyle.Top,
                Height = 22,
                Text = ""
            };
            _aiStatus.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _aiStatus.Appearance.ForeColor = ColorTranslator.FromHtml("#94A3B8");
            _aiStatus.Appearance.Options.UseFont = true;
            _aiStatus.Appearance.Options.UseForeColor = true;

            var sep = new SeparatorControl { Dock = DockStyle.Top, Height = 18 };
            sep.LineColor = Color.FromArgb(70, 255, 255, 255);

            // PDF seçimi (hasta yükler)
            var pdfRow = new TablePanel { Dock = DockStyle.Top, Height = 54 };
            pdfRow.Appearance.BackColor = Color.Transparent;
            pdfRow.Appearance.Options.UseBackColor = true;
            pdfRow.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Absolute, 160),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Absolute, 54),
            });
            pdfRow.Rows.Add(new TablePanelRow(TablePanelEntityStyle.Absolute, 54));

            _aiBtnPdfSelect = new SimpleButton
            {
                Text = "PDF Seç",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _aiBtnPdfSelect.LookAndFeel.UseDefaultLookAndFeel = false;
            _aiBtnPdfSelect.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _aiBtnPdfSelect.Appearance.ForeColor = Color.White;
            _aiBtnPdfSelect.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            _aiBtnPdfSelect.Appearance.Options.UseBackColor = true;
            _aiBtnPdfSelect.Appearance.Options.UseForeColor = true;
            _aiBtnPdfSelect.Appearance.Options.UseFont = true;
            _aiBtnPdfSelect.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#1F2937");
            _aiBtnPdfSelect.AppearanceHovered.Options.UseBackColor = true;

            var pdfInfo = new LabelControl
            {
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Text = "PDF: seçilmedi"
            };
            pdfInfo.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
            pdfInfo.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            pdfInfo.Appearance.Options.UseForeColor = true;
            pdfInfo.Appearance.Options.UseFont = true;

            _aiBtnPdfClear = new SimpleButton
            {
                Text = "✕",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _aiBtnPdfClear.LookAndFeel.UseDefaultLookAndFeel = false;
            _aiBtnPdfClear.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _aiBtnPdfClear.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _aiBtnPdfClear.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _aiBtnPdfClear.Appearance.Options.UseBackColor = true;
            _aiBtnPdfClear.Appearance.Options.UseForeColor = true;
            _aiBtnPdfClear.Appearance.Options.UseFont = true;
            _aiBtnPdfClear.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#DC2626");
            _aiBtnPdfClear.AppearanceHovered.ForeColor = Color.White;
            _aiBtnPdfClear.AppearanceHovered.Options.UseBackColor = true;
            _aiBtnPdfClear.AppearanceHovered.Options.UseForeColor = true;

            pdfRow.Controls.Add(_aiBtnPdfSelect);
            pdfRow.SetColumn(_aiBtnPdfSelect, 0);
            pdfRow.SetRow(_aiBtnPdfSelect, 0);
            pdfRow.Controls.Add(pdfInfo);
            pdfRow.SetColumn(pdfInfo, 1);
            pdfRow.SetRow(pdfInfo, 0);
            pdfRow.Controls.Add(_aiBtnPdfClear);
            pdfRow.SetColumn(_aiBtnPdfClear, 2);
            pdfRow.SetRow(_aiBtnPdfClear, 0);

            _aiChat = new MemoEdit
            {
                Dock = DockStyle.Fill
            };
            _aiChat.Properties.ReadOnly = true;
            _aiChat.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _aiChat.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            _aiChat.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _aiChat.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            _aiChat.Properties.Appearance.Options.UseBackColor = true;
            _aiChat.Properties.Appearance.Options.UseForeColor = true;
            _aiChat.Properties.Appearance.Options.UseFont = true;
            _aiChat.Properties.ScrollBars = ScrollBars.Vertical;

            var bottom = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 140,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Padding = new Padding(0, 14, 0, 0)
            };
            bottom.Appearance.BackColor = Color.Transparent;
            bottom.Appearance.Options.UseBackColor = true;

            _aiInput = new MemoEdit
            {
                Dock = DockStyle.Fill
            };
            _aiInput.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _aiInput.Properties.NullValuePrompt = "Sorunuzu yazın (örn. 'Bu tahlillerde dikkat çeken bir şey var mı?')";
            _aiInput.Properties.NullValuePromptShowForEmptyValue = true;
            _aiInput.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _aiInput.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _aiInput.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            _aiInput.Properties.Appearance.Options.UseBackColor = true;
            _aiInput.Properties.Appearance.Options.UseForeColor = true;
            _aiInput.Properties.Appearance.Options.UseFont = true;

            _aiBtnAnalyze = new SimpleButton
            {
                Text = "TAHLİLLERİ DEĞERLENDİR",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _aiBtnAnalyze.LookAndFeel.UseDefaultLookAndFeel = false;
            _aiBtnAnalyze.Appearance.BackColor = ColorTranslator.FromHtml("#5D9CEC");
            _aiBtnAnalyze.Appearance.ForeColor = Color.White;
            _aiBtnAnalyze.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            _aiBtnAnalyze.Appearance.Options.UseBackColor = true;
            _aiBtnAnalyze.Appearance.Options.UseForeColor = true;
            _aiBtnAnalyze.Appearance.Options.UseFont = true;
            _aiBtnAnalyze.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#6EA8F1");
            _aiBtnAnalyze.AppearanceHovered.Options.UseBackColor = true;
            _aiBtnAnalyze.AppearancePressed.BackColor = ColorTranslator.FromHtml("#4B88DA");
            _aiBtnAnalyze.AppearancePressed.Options.UseBackColor = true;

            _aiBtnSend = new SimpleButton
            {
                Text = "GÖNDER",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _aiBtnSend.LookAndFeel.UseDefaultLookAndFeel = false;
            _aiBtnSend.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _aiBtnSend.Appearance.ForeColor = Color.White;
            _aiBtnSend.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            _aiBtnSend.Appearance.Options.UseBackColor = true;
            _aiBtnSend.Appearance.Options.UseForeColor = true;
            _aiBtnSend.Appearance.Options.UseFont = true;
            _aiBtnSend.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#1F2937");
            _aiBtnSend.AppearanceHovered.Options.UseBackColor = true;
            _aiBtnSend.AppearancePressed.BackColor = ColorTranslator.FromHtml("#0B1220");
            _aiBtnSend.AppearancePressed.Options.UseBackColor = true;

            var layout = new TablePanel { Dock = DockStyle.Fill };
            layout.Appearance.BackColor = Color.Transparent;
            layout.Appearance.Options.UseBackColor = true;
            layout.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Absolute, 220),
                new TablePanelColumn(TablePanelEntityStyle.Absolute, 120),
            });
            layout.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
            });

            layout.Controls.Add(_aiInput);
            layout.SetColumn(_aiInput, 0);
            layout.SetRow(_aiInput, 0);
            layout.SetColumnSpan(_aiInput, 3);

            layout.Controls.Add(_aiBtnAnalyze);
            layout.SetColumn(_aiBtnAnalyze, 1);
            layout.SetRow(_aiBtnAnalyze, 1);

            layout.Controls.Add(_aiBtnSend);
            layout.SetColumn(_aiBtnSend, 2);
            layout.SetRow(_aiBtnSend, 1);

            bottom.Controls.Add(layout);

            card.Controls.Add(_aiChat);
            card.Controls.Add(bottom);
            card.Controls.Add(sep);
            card.Controls.Add(pdfRow);
            card.Controls.Add(_aiStatus);
            card.Controls.Add(lblInfo);
            card.Controls.Add(lblTitle);

            page.Controls.Add(card);

            // Wire-up
            _aiBtnAnalyze.Click += async (_, _) => await RunAiAnalysisAsync(userQuestion: null);
            _aiBtnSend.Click += async (_, _) => await RunAiAnalysisAsync(userQuestion: _aiInput?.Text);
            _aiBtnPdfSelect.Click += (_, _) =>
            {
                using var dlg = new OpenFileDialog
                {
                    Filter = "PDF Dosyası (*.pdf)|*.pdf",
                    Title = "Kan Tahlili PDF Seç"
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var bytes = System.IO.File.ReadAllBytes(dlg.FileName);
                        const int maxBytes = 8 * 1024 * 1024;
                        if (bytes.Length > maxBytes)
                        {
                            XtraMessageBox.Show("PDF çok büyük (8MB üstü). Lütfen daha küçük bir PDF yükleyin.", "PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        _aiPdfBytes = bytes;
                        _aiPdfName = System.IO.Path.GetFileName(dlg.FileName);
                        pdfInfo.Text = $"PDF: {_aiPdfName}";
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show(ex.Message, "PDF Okuma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
            _aiBtnPdfClear.Click += (_, _) =>
            {
                _aiPdfBytes = null;
                _aiPdfName = null;
                pdfInfo.Text = "PDF: seçilmedi";
            };

            // First render: show key status
            UpdateAiAvailabilityUI();

            return page;
        }

        private void UpdateAiAvailabilityUI()
        {
            try
            {
                var key = GeminiClient.GetApiKeyFromEnvironment();
                bool ok = !string.IsNullOrWhiteSpace(key);
                if (_aiBtnAnalyze != null) _aiBtnAnalyze.Enabled = ok;
                if (_aiBtnSend != null) _aiBtnSend.Enabled = ok;
                if (_aiStatus != null)
                {
                    _aiStatus.Text = ok
                        ? "Hazır: Tahlillerinizden otomatik özet çıkarabilirsiniz."
                        : "Gemini API anahtarı yok. Windows ortam değişkeni olarak GEMINI_API_KEY tanımlayın.";
                }
            }
            catch { }
        }

        private string BuildTahlilPrompt(string labsText, string userQuestion)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Rolün: Sağlık bilgisi veren bir asistan. TIBBİ TANI KOYMA.");
            sb.AppendLine("Kısıt: Yanıt Türkçe olsun. Kısa, anlaşılır, maddeler halinde yaz.");
            sb.AppendLine("Uyarı: Bu yorum bir doktora danışmanın yerine geçmez. Acil belirti varsa acile yönlendir.");
            sb.AppendLine();
            sb.AppendLine("Eğer kullanıcı PDF yüklediyse PDF içindeki kan tahlilini oku ve değerlendir.");
            sb.AppendLine("PDF'de referans aralığı varsa: her parametre için Düşük/Normal/Yüksek sınıflandır.");
            sb.AppendLine("Referans aralığı yoksa: kesin sınıflandırma yapma, 'Bilinmiyor' de ve gerekli referans aralığını iste.");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(labsText))
            {
                sb.AppendLine("Aşağıdaki tahlil kayıtları hastane otomasyonundan alınmıştır (ek bilgi):");
                sb.AppendLine(labsText);
                sb.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(userQuestion))
            {
                sb.AppendLine("Hastanın sorusu:");
                sb.AppendLine(userQuestion.Trim());
                sb.AppendLine();
            }
            sb.AppendLine("İstek:");
            sb.AppendLine("- Eğer PDF varsa şu formatta tablo üret: Parametre | Değer | Birim | Referans | Yorum (Düşük/Normal/Yüksek/Bilinmiyor).");
            sb.AppendLine("- Sonra kısa özet ve dikkat edilmesi gereken olası noktaları belirt (kesin teşhis iddiası olmadan).");
            sb.AppendLine("- Doktora sorulabilecek 5 soru öner.");
            return sb.ToString();
        }

        private string GetLatestTahlillerAsText(int maxRows = 20)
        {
            try
            {
                using var conn = SqlBaglantisi.Instance.GetConnection();
                string q = @"SELECT TOP (@n)
                                    ISNULL(CONVERT(varchar(10), TahlilTarih, 120), '') AS Tarih,
                                    ISNULL(TahlilTur,'') AS Tur,
                                    ISNULL(TahlilAd,'') AS Ad,
                                    ISNULL(TahlilSonuc,'') AS Sonuc,
                                    ISNULL(TahlilDurum,'') AS Durum,
                                    ISNULL(DoktorAd,'') AS Doktor
                             FROM Tbl_Tahliller
                             WHERE HastaTC = @tc
                             ORDER BY TahlilTarih DESC";
                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@n", maxRows);
                cmd.Parameters.AddWithValue("@tc", HastaTC);
                using var r = cmd.ExecuteReader();
                var sb = new StringBuilder();
                sb.AppendLine("Tarih | Tür | Tahlil | Sonuç | Durum | Doktor");
                sb.AppendLine("--- | --- | --- | --- | --- | ---");
                int count = 0;
                while (r.Read())
                {
                    count++;
                    string tarih = r["Tarih"]?.ToString();
                    string tur = r["Tur"]?.ToString();
                    string ad = r["Ad"]?.ToString();
                    string sonuc = r["Sonuc"]?.ToString();
                    string durum = r["Durum"]?.ToString();
                    string doktor = r["Doktor"]?.ToString();
                    sb.AppendLine($"{tarih} | {tur} | {ad} | {sonuc} | {durum} | {doktor}");
                }
                if (count == 0)
                {
                    sb.AppendLine("(Bu hastaya ait tahlil kaydı bulunamadı.)");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"(Tahliller okunamadı: {ex.Message})";
            }
            finally
            {
                SqlBaglantisi.Instance.CloseConnection();
            }
        }

        private void AppendAiChat(string speaker, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var ts = DateTime.Now.ToString("HH:mm");
            _aiChatBuffer += $"[{ts}] {speaker}:\r\n{text.Trim()}\r\n\r\n";
            if (_aiChat != null)
            {
                _aiChat.Text = _aiChatBuffer;
                _aiChat.SelectionStart = _aiChat.Text.Length;
                _aiChat.ScrollToCaret();
            }
        }

        private async Task RunAiAnalysisAsync(string userQuestion)
        {
            UpdateAiAvailabilityUI();
            var key = GeminiClient.GetApiKeyFromEnvironment();
            if (string.IsNullOrWhiteSpace(key))
            {
                XtraMessageBox.Show("Gemini API anahtarı bulunamadı. Ortam değişkeni olarak GEMINI_API_KEY tanımlayın.", "Yapay Zeka", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // boş soru varsa sadece analiz
            if (!string.IsNullOrWhiteSpace(userQuestion))
            {
                AppendAiChat("Siz", userQuestion);
            }

            string labs = _aiPdfBytes != null ? "" : GetLatestTahlillerAsText(20);
            string prompt = BuildTahlilPrompt(labs, userQuestion);

            _aiBtnAnalyze.Enabled = false;
            _aiBtnSend.Enabled = false;
            if (_aiBtnPdfSelect != null) _aiBtnPdfSelect.Enabled = false;
            if (_aiBtnPdfClear != null) _aiBtnPdfClear.Enabled = false;
            if (_aiStatus != null) _aiStatus.Text = "Yapay zeka yanıt üretiyor...";

            _aiCts?.Cancel();
            _aiCts?.Dispose();
            _aiCts = new CancellationTokenSource();

            try
            {
                var client = new GeminiClient(_aiHttp, key);
                string answer;
                if (_aiPdfBytes != null)
                {
                    AppendAiChat("Sistem", $"PDF analiz ediliyor: {_aiPdfName ?? "dosya"}");
                    answer = await client.GenerateWithPdfAsync(prompt, _aiPdfBytes, "application/pdf", _aiCts.Token);
                }
                else
                {
                    answer = await client.GenerateAsync(prompt, _aiCts.Token);
                }
                AppendAiChat("Asistan", answer);
                if (_aiInput != null) _aiInput.Text = "";
                if (_aiStatus != null) _aiStatus.Text = "Hazır.";
            }
            catch (OperationCanceledException)
            {
                if (_aiStatus != null) _aiStatus.Text = "İptal edildi.";
            }
            catch (Exception ex)
            {
                if (_aiStatus != null) _aiStatus.Text = "Hata oluştu.";
                XtraMessageBox.Show(ex.Message, "Gemini Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _aiBtnAnalyze.Enabled = true;
                _aiBtnSend.Enabled = true;
                if (_aiBtnPdfSelect != null) _aiBtnPdfSelect.Enabled = true;
                if (_aiBtnPdfClear != null) _aiBtnPdfClear.Enabled = true;
            }
        }

        private DevExpress.XtraBars.Navigation.NavigationPage CreateModernHomePage()
        {
            var page = new DevExpress.XtraBars.Navigation.NavigationPage();
            page.Name = "pageAnaSayfaModern";
            page.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            page.Appearance.Options.UseBackColor = true;
            page.Padding = new Padding(22);

            var card = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 24, 28, 22)
            };
            card.Appearance.BackColor = Color.Transparent;
            card.Appearance.Options.UseBackColor = true;

            var cardFill = Color.FromArgb(30, 42, 56); // #1E2A38
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = card.ClientRectangle;
                rect.Inflate(-1, -1);

                int radius = 22;
                int d = radius * 2;
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();

                using var brush = new SolidBrush(cardFill);
                e.Graphics.FillPath(brush, path);

                using var pen = new Pen(Color.FromArgb(120, Color.White), 1.4f);
                e.Graphics.DrawPath(pen, path);
            };

            var lblWelcome = new LabelControl
            {
                AllowHtmlString = true,
                AutoSizeMode = LabelAutoSizeMode.None,
                Height = 64,
                Text = "<color=#5D9CEC>Hoş Geldiniz,</color> <color=#ECEFF1>...</color>"
            };
            lblWelcome.Appearance.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblWelcome.Appearance.ForeColor = Color.FromArgb(236, 239, 241);
            lblWelcome.Appearance.Options.UseFont = true;
            lblWelcome.Appearance.Options.UseForeColor = true;
            _lblHomeWelcome = lblWelcome;

            var lblSub = new LabelControl
            {
                Height = 28,
                Text = "Yaklaşan randevularınız listeleniyor."
            };
            lblSub.Appearance.Font = new Font("Segoe UI", 11F);
            lblSub.Appearance.ForeColor = Color.FromArgb(176, 190, 197);
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Appearance.Options.UseForeColor = true;

            var sep = new SeparatorControl { Height = 18 };
            sep.LineColor = Color.FromArgb(70, 255, 255, 255);

            var listCard = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };
            listCard.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            listCard.Appearance.Options.UseBackColor = true;
            listCard.SizeChanged += (s, e) =>
            {
                if (listCard.Width <= 0 || listCard.Height <= 0) return;
                listCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, listCard.Width, listCard.Height, 18, 18));
            };

            // DevExpress Dock bazı kombinasyonlarda yine üst üste binebiliyor.
            // Garanti çözüm: içeride 2 satırlı TablePanel (başlık + içerik)
            var listLayout = new TablePanel { Dock = DockStyle.Fill };
            listLayout.Appearance.BackColor = Color.Transparent;
            listLayout.Appearance.Options.UseBackColor = true;
            listLayout.Columns.Add(new TablePanelColumn(TablePanelEntityStyle.Relative, 1));
            listLayout.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 28),
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),
            });
            listCard.Controls.Add(listLayout);

            var lblListTitle = new LabelControl
            {
                Text = "Yaklaşan Randevular",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Fill
            };
            lblListTitle.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblListTitle.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            lblListTitle.Appearance.Options.UseFont = true;
            lblListTitle.Appearance.Options.UseForeColor = true;
            lblListTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            listLayout.Controls.Add(lblListTitle);
            listLayout.SetRow(lblListTitle, 0);
            listLayout.SetColumn(lblListTitle, 0);

            var lblList = new LabelControl
            {
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Fill,
                Text = "Yükleniyor..."
            };
            lblList.Appearance.Font = new Font("Segoe UI", 11F);
            lblList.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
            lblList.Appearance.Options.UseFont = true;
            lblList.Appearance.Options.UseForeColor = true;
            lblList.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            lblList.Padding = new Padding(0, 10, 0, 0);
            listLayout.Controls.Add(lblList);
            listLayout.SetRow(lblList, 1);
            listLayout.SetColumn(lblList, 0);
            _lblHomeNextAppt = lblList;

            // Doktor panelindeki gibi kart/chip listesi
            _homeApptList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };
            listLayout.Controls.Add(_homeApptList);
            listLayout.SetRow(_homeApptList, 1);
            listLayout.SetColumn(_homeApptList, 0);
            _homeApptList.Visible = false; // veri gelince açacağız; yoksa label kullan

            // Stabil layout: TablePanel ile üst üste binmeyi tamamen engelle
            var layout = new TablePanel { Dock = DockStyle.Fill };
            layout.Appearance.BackColor = Color.Transparent;
            layout.Appearance.Options.UseBackColor = true;
            layout.Columns.Add(new TablePanelColumn(TablePanelEntityStyle.Relative, 1));
            layout.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 64),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 28),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 18),
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),
            });

            layout.Controls.Add(lblWelcome);
            layout.SetRow(lblWelcome, 0);
            layout.SetColumn(lblWelcome, 0);
            lblWelcome.Dock = DockStyle.Fill;

            layout.Controls.Add(lblSub);
            layout.SetRow(lblSub, 1);
            layout.SetColumn(lblSub, 0);
            lblSub.Dock = DockStyle.Fill;

            layout.Controls.Add(sep);
            layout.SetRow(sep, 2);
            layout.SetColumn(sep, 0);
            sep.Dock = DockStyle.Fill;

            layout.Controls.Add(listCard);
            layout.SetRow(listCard, 3);
            layout.SetColumn(listCard, 0);

            card.Controls.Add(layout);

            page.Controls.Add(card);
            return page;
        }

        private DevExpress.XtraBars.Navigation.NavigationPage CreateModernMedicalPage()
        {
            var page = new DevExpress.XtraBars.Navigation.NavigationPage();
            page.Name = "pageTibbiBilgilerModern";
            page.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            page.Appearance.Options.UseBackColor = true;
            page.Padding = new Padding(22);

            var card = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };
            card.Appearance.BackColor = Color.Transparent;
            card.Appearance.Options.UseBackColor = true;

            var cardFill = Color.FromArgb(30, 42, 56); // #1E2A38
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = card.ClientRectangle;
                rect.Inflate(-1, -1);

                int radius = 22;
                int d = radius * 2;
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();

                using var brush = new SolidBrush(cardFill);
                e.Graphics.FillPath(brush, path);

                using var pen = new Pen(Color.FromArgb(120, Color.White), 1.4f);
                e.Graphics.DrawPath(pen, path);
            };

            var lblTitle = new LabelControl
            {
                AllowHtmlString = true,
                Location = new Point(36, 30),
                Text = "<color=#5D9CEC>Tıbbi</color> <color=#ECEFF1>Bilgiler ve Sonuçlar</color>"
            };
            lblTitle.Appearance.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(236, 239, 241);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            card.Controls.Add(lblTitle);

            var lblSub = new LabelControl
            {
                Location = new Point(38, 82),
                Text = "Alerjilerinizi güncelleyin, tahlil sonuçlarınızı ve reçetelerinizi görüntüleyin."
            };
            lblSub.Appearance.Font = new Font("Segoe UI", 11F);
            lblSub.Appearance.ForeColor = Color.FromArgb(176, 190, 197);
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Appearance.Options.UseForeColor = true;
            card.Controls.Add(lblSub);

            var sep = new SeparatorControl { Location = new Point(36, 110), Size = new Size(Math.Max(0, card.Width - 72), 18) };
            sep.LineColor = Color.FromArgb(70, 255, 255, 255);
            card.Controls.Add(sep);
            card.SizeChanged += (s, e) =>
            {
                sep.Size = new Size(Math.Max(0, card.Width - 72), 18);
            };

            var layout = new TablePanel { Dock = DockStyle.None };
            layout.Appearance.BackColor = Color.Transparent;
            layout.Appearance.Options.UseBackColor = true;
            layout.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1.0f),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1.6f),
            });
            layout.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),
            });
            card.Controls.Add(layout);

            void LayoutContent()
            {
                const int left = 36;
                const int top = 140;
                const int right = 36;
                const int bottom = 28;
                layout.Location = new Point(left, top);
                layout.Size = new Size(
                    Math.Max(0, card.Width - (left + right)),
                    Math.Max(0, card.Height - (top + bottom))
                );
            }
            LayoutContent();
            card.SizeChanged += (s, e) => LayoutContent();

            // Sol: Alerji kartı
            var allergyCard = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };
            allergyCard.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            allergyCard.Appearance.Options.UseBackColor = true;
            allergyCard.SizeChanged += (s, e) =>
            {
                if (allergyCard.Width <= 0 || allergyCard.Height <= 0) return;
                allergyCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, allergyCard.Width, allergyCard.Height, 18, 18));
            };

            var lblAllergyTitle = new LabelControl
            {
                Dock = DockStyle.Top,
                Height = 26,
                Text = "Alerjilerim"
            };
            lblAllergyTitle.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblAllergyTitle.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            lblAllergyTitle.Appearance.Options.UseFont = true;
            lblAllergyTitle.Appearance.Options.UseForeColor = true;
            allergyCard.Controls.Add(lblAllergyTitle);

            // Alerji kartı iç layout (başlık / ekleme satırı / liste / kaydet)
            var allergyLayout = new TablePanel { Dock = DockStyle.Fill };
            allergyLayout.Appearance.BackColor = Color.Transparent;
            allergyLayout.Appearance.Options.UseBackColor = true;
            allergyLayout.Columns.Add(new TablePanelColumn(TablePanelEntityStyle.Relative, 1));
            allergyLayout.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 26),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 62),
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 56),
            });
            allergyCard.Controls.Add(allergyLayout);

            allergyLayout.Controls.Add(lblAllergyTitle);
            allergyLayout.SetRow(lblAllergyTitle, 0);
            allergyLayout.SetColumn(lblAllergyTitle, 0);
            lblAllergyTitle.Dock = DockStyle.Fill;

            // Ekleme satırı: TextEdit + "+" butonu
            var addRow = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };
            addRow.Appearance.BackColor = Color.Transparent;
            addRow.Appearance.Options.UseBackColor = true;

            var addTbl = new TablePanel { Dock = DockStyle.Fill };
            addTbl.Appearance.BackColor = Color.Transparent;
            addTbl.Appearance.Options.UseBackColor = true;
            addTbl.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Absolute, 54),
            });
            addTbl.Rows.Add(new TablePanelRow(TablePanelEntityStyle.Relative, 1));
            addRow.Controls.Add(addTbl);

            _medAllergyInput = new TextEdit { Dock = DockStyle.Fill };
            _medAllergyInput.Properties.NullValuePrompt = "Alerji ekle (örn: penisilin)";
            _medAllergyInput.Properties.NullValuePromptShowForEmptyValue = true;
            _medAllergyInput.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _medAllergyInput.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _medAllergyInput.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _medAllergyInput.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            _medAllergyInput.Properties.Appearance.Options.UseBackColor = true;
            _medAllergyInput.Properties.Appearance.Options.UseForeColor = true;
            _medAllergyInput.Properties.Appearance.Options.UseFont = true;
            _medAllergyInput.Padding = new Padding(12, 10, 12, 10);

            // Modern field border
            var inputWrap = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 6, 10, 6),
            };
            inputWrap.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            inputWrap.Appearance.Options.UseBackColor = true;
            inputWrap.SizeChanged += (s, e) =>
            {
                if (inputWrap.Width <= 0 || inputWrap.Height <= 0) return;
                inputWrap.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, inputWrap.Width, inputWrap.Height, 16, 16));
                inputWrap.Invalidate();
            };
            bool focused = false;
            _medAllergyInput.Enter += (s, e) => { focused = true; inputWrap.Invalidate(); };
            _medAllergyInput.Leave += (s, e) => { focused = false; inputWrap.Invalidate(); };
            inputWrap.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = inputWrap.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                int radius = 16;
                int d = radius * 2;
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                var border = focused ? ColorTranslator.FromHtml("#2563EB") : ColorTranslator.FromHtml("#334155");
                using var pen = new Pen(border, focused ? 2f : 1.4f);
                e.Graphics.DrawPath(pen, path);
            };
            inputWrap.Controls.Add(_medAllergyInput);

            _medAllergyAdd = new SimpleButton
            {
                Text = "+",
                Cursor = Cursors.Hand,
                Dock = DockStyle.Fill,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _medAllergyAdd.LookAndFeel.UseDefaultLookAndFeel = false;
            _medAllergyAdd.Appearance.BackColor = ColorTranslator.FromHtml("#2563EB");
            _medAllergyAdd.Appearance.ForeColor = Color.White;
            _medAllergyAdd.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            _medAllergyAdd.Appearance.Options.UseBackColor = true;
            _medAllergyAdd.Appearance.Options.UseForeColor = true;
            _medAllergyAdd.Appearance.Options.UseFont = true;
            _medAllergyAdd.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#2F74FF");
            _medAllergyAdd.AppearanceHovered.Options.UseBackColor = true;
            _medAllergyAdd.AppearancePressed.BackColor = ColorTranslator.FromHtml("#1D4ED8");
            _medAllergyAdd.AppearancePressed.Options.UseBackColor = true;
            _medAllergyAdd.SizeChanged += (s, e) => ApplyPillRegion(_medAllergyAdd);

            addTbl.Controls.Add(inputWrap);
            addTbl.SetColumn(inputWrap, 0);
            addTbl.SetRow(inputWrap, 0);
            addTbl.Controls.Add(_medAllergyAdd);
            addTbl.SetColumn(_medAllergyAdd, 1);
            addTbl.SetRow(_medAllergyAdd, 0);

            allergyLayout.Controls.Add(addRow);
            allergyLayout.SetRow(addRow, 1);
            allergyLayout.SetColumn(addRow, 0);

            // Liste alanı (maddeli)
            _medAllergyList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0)
            };
            allergyLayout.Controls.Add(_medAllergyList);
            allergyLayout.SetRow(_medAllergyList, 2);
            allergyLayout.SetColumn(_medAllergyList, 0);

            _medAllergySave = new SimpleButton
            {
                Text = "Kaydet",
                Cursor = Cursors.Hand,
                Height = 48,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _medAllergySave.LookAndFeel.UseDefaultLookAndFeel = false;
            _medAllergySave.Appearance.BackColor = ColorTranslator.FromHtml("#2563EB");
            _medAllergySave.Appearance.ForeColor = Color.White;
            _medAllergySave.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _medAllergySave.Appearance.Options.UseBackColor = true;
            _medAllergySave.Appearance.Options.UseForeColor = true;
            _medAllergySave.Appearance.Options.UseFont = true;
            _medAllergySave.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#2F74FF");
            _medAllergySave.AppearanceHovered.Options.UseBackColor = true;
            _medAllergySave.AppearancePressed.BackColor = ColorTranslator.FromHtml("#1D4ED8");
            _medAllergySave.AppearancePressed.Options.UseBackColor = true;
            _medAllergySave.SizeChanged += (s, e) => ApplyPillRegion(_medAllergySave);
            allergyLayout.Controls.Add(_medAllergySave);
            allergyLayout.SetRow(_medAllergySave, 3);
            allergyLayout.SetColumn(_medAllergySave, 0);
            _medAllergySave.Dock = DockStyle.Fill;

            // Sağ: Alerji konseptiyle aynı koyu kart + üstte büyük buton sekmeler (Tahlillerim / Reçetelerim)
            var rightCard = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };
            rightCard.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            rightCard.Appearance.Options.UseBackColor = true;
            rightCard.SizeChanged += (s, e) =>
            {
                if (rightCard.Width <= 0 || rightCard.Height <= 0) return;
                rightCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, rightCard.Width, rightCard.Height, 18, 18));
            };

            var rightLayout = new TablePanel { Dock = DockStyle.Fill };
            rightLayout.Appearance.BackColor = Color.Transparent;
            rightLayout.Appearance.Options.UseBackColor = true;
            rightLayout.Columns.Add(new TablePanelColumn(TablePanelEntityStyle.Relative, 1));
            rightLayout.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 56),
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),
            });
            rightCard.Controls.Add(rightLayout);

            var btnBar = new TablePanel { Dock = DockStyle.Fill };
            btnBar.Appearance.BackColor = Color.Transparent;
            btnBar.Appearance.Options.UseBackColor = true;
            btnBar.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
            });
            btnBar.Rows.Add(new TablePanelRow(TablePanelEntityStyle.Relative, 1));
            rightLayout.Controls.Add(btnBar);
            rightLayout.SetRow(btnBar, 0);
            rightLayout.SetColumn(btnBar, 0);

            SimpleButton MakeTabBtn(string text)
            {
                var b = new SimpleButton
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    Cursor = Cursors.Hand,
                    ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
                };
                b.LookAndFeel.UseDefaultLookAndFeel = false;
                b.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                b.Appearance.ForeColor = Color.White;
                b.Appearance.Options.UseFont = true;
                b.Appearance.Options.UseForeColor = true;
                b.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
                b.Appearance.Options.UseBackColor = true;
                b.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#1D4ED8");
                b.AppearanceHovered.Options.UseBackColor = true;
                b.AppearancePressed.BackColor = ColorTranslator.FromHtml("#2563EB");
                b.AppearancePressed.Options.UseBackColor = true;
                b.SizeChanged += (s, e) => ApplyPillRegion(b);
                return b;
            }

            _medBtnTahliller = MakeTabBtn("Tahlillerim");
            _medBtnReceteler = MakeTabBtn("Reçetelerim");
            btnBar.Controls.Add(_medBtnTahliller);
            btnBar.SetColumn(_medBtnTahliller, 0);
            btnBar.SetRow(_medBtnTahliller, 0);
            btnBar.Controls.Add(_medBtnReceteler);
            btnBar.SetColumn(_medBtnReceteler, 1);
            btnBar.SetRow(_medBtnReceteler, 0);

            var contentHost = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 12, 0, 0)
            };
            contentHost.Appearance.BackColor = Color.Transparent;
            contentHost.Appearance.Options.UseBackColor = true;
            rightLayout.Controls.Add(contentHost);
            rightLayout.SetRow(contentHost, 1);
            rightLayout.SetColumn(contentHost, 0);

            _medPanelTahliller = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill
            };
            _medPanelTahliller.Appearance.BackColor = Color.Transparent;
            _medPanelTahliller.Appearance.Options.UseBackColor = true;

            _medPanelReceteler = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Visible = false
            };
            _medPanelReceteler.Appearance.BackColor = Color.Transparent;
            _medPanelReceteler.Appearance.Options.UseBackColor = true;

            contentHost.Controls.Add(_medPanelTahliller);
            contentHost.Controls.Add(_medPanelReceteler);

            _medGridTahliller = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            _medGridTahliller.LookAndFeel.UseDefaultLookAndFeel = false;
            _medGridTahliller.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            _medGridTahliller.BackColor = ColorTranslator.FromHtml("#0F172A");
            _medViewTahliller = new DevExpress.XtraGrid.Views.Grid.GridView();
            _medGridTahliller.MainView = _medViewTahliller;
            _medGridTahliller.ViewCollection.Add(_medViewTahliller);
            _medPanelTahliller.Controls.Add(_medGridTahliller);

            _medGridReceteler = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            _medGridReceteler.LookAndFeel.UseDefaultLookAndFeel = false;
            _medGridReceteler.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            _medGridReceteler.BackColor = ColorTranslator.FromHtml("#0F172A");
            _medViewReceteler = new DevExpress.XtraGrid.Views.Grid.GridView();
            _medGridReceteler.MainView = _medViewReceteler;
            _medGridReceteler.ViewCollection.Add(_medViewReceteler);
            _medPanelReceteler.Controls.Add(_medGridReceteler);

            // Grid görünümü: modern başlık
            void StyleGrid(DevExpress.XtraGrid.Views.Grid.GridView view)
            {
                view.OptionsBehavior.Editable = false;
                view.OptionsBehavior.ReadOnly = true;
                view.OptionsView.ShowGroupPanel = false;
                view.RowHeight = 42;
                view.Appearance.HeaderPanel.BackColor = ColorTranslator.FromHtml("#1D4ED8");
                view.Appearance.HeaderPanel.ForeColor = Color.White;
                view.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 11F);
                view.Appearance.HeaderPanel.Options.UseBackColor = true;
                view.Appearance.HeaderPanel.Options.UseForeColor = true;
                view.Appearance.HeaderPanel.Options.UseFont = true;
                view.Appearance.HeaderPanel.Options.UseTextOptions = true;
                view.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                view.Appearance.Row.BackColor = ColorTranslator.FromHtml("#0F172A");
                view.Appearance.Row.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
                view.Appearance.Row.Font = new Font("Segoe UI", 10F);
                view.Appearance.Row.Options.UseBackColor = true;
                view.Appearance.Row.Options.UseForeColor = true;
                view.Appearance.Row.Options.UseFont = true;
            }
            StyleGrid(_medViewTahliller);
            StyleGrid(_medViewReceteler);

            layout.Controls.Add(allergyCard);
            layout.SetColumn(allergyCard, 0);
            layout.SetRow(allergyCard, 0);

            layout.Controls.Add(rightCard);
            layout.SetColumn(rightCard, 1);
            layout.SetRow(rightCard, 0);

            _medAllergyAdd.Click += (s, e) => AddMedicalAllergyFromInput();
            _medAllergyInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    AddMedicalAllergyFromInput();
                }
            };
            _medAllergySave.Click += (s, e) => SaveMedicalAllergies();

            void SetMedicalTab(bool showTahlil)
            {
                if (_medPanelTahliller != null) _medPanelTahliller.Visible = showTahlil;
                if (_medPanelReceteler != null) _medPanelReceteler.Visible = !showTahlil;
                if (_medBtnTahliller != null)
                {
                    _medBtnTahliller.Appearance.BackColor = showTahlil ? ColorTranslator.FromHtml("#2563EB") : ColorTranslator.FromHtml("#111827");
                    _medBtnTahliller.Appearance.Options.UseBackColor = true;
                }
                if (_medBtnReceteler != null)
                {
                    _medBtnReceteler.Appearance.BackColor = showTahlil ? ColorTranslator.FromHtml("#111827") : ColorTranslator.FromHtml("#2563EB");
                    _medBtnReceteler.Appearance.Options.UseBackColor = true;
                }
            }
            _medBtnTahliller.Click += (s, e) => SetMedicalTab(true);
            _medBtnReceteler.Click += (s, e) => SetMedicalTab(false);
            SetMedicalTab(true);

            // Tahlil PDF indir: satıra çift tıkla
            try
            {
                _medViewTahliller.DoubleClick += (s, e) =>
                {
                    try
                    {
                        var view = _medViewTahliller;
                        if (view == null || view.FocusedRowHandle < 0) return;
                        var idObj = view.GetRowCellValue(view.FocusedRowHandle, "Tahlilid");
                        if (idObj == null || idObj == DBNull.Value) return;
                        DownloadTahlilPdf(Convert.ToInt32(idObj));
                    }
                    catch { }
                };
            }
            catch { }

            // Reçete PDF aç: reçete satırına çift tıkla
            try
            {
                _medViewReceteler.DoubleClick += (s, e) =>
                {
                    try
                    {
                        var view = _medViewReceteler;
                        if (view == null || view.FocusedRowHandle < 0) return;
                        var idObj = view.GetRowCellValue(view.FocusedRowHandle, "Receteid");
                        if (idObj == null || idObj == DBNull.Value) return;
                        OpenRecetePdf(Convert.ToInt32(idObj));
                    }
                    catch { }
                };
            }
            catch { }

            page.Controls.Add(card);
            return page;
        }

        private void OpenRecetePdf(int receteId)
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string q = @"SELECT RecetePdf, RecetePdfFileName
                                 FROM Tbl_Receteler
                                 WHERE Receteid = @id AND HastaTC = @tc";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", receteId);
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (!dr.Read()) return;
                            if (dr["RecetePdf"] == DBNull.Value) return;
                            var bytes = (byte[])dr["RecetePdf"];
                            if (bytes == null || bytes.Length == 0) return;

                            string fileName = dr["RecetePdfFileName"] == DBNull.Value ? $"Recete_{receteId}.pdf" : dr["RecetePdfFileName"].ToString();
                            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) fileName += ".pdf";
                            using var sfd = new SaveFileDialog
                            {
                                Filter = "PDF Dosyası|*.pdf",
                                FileName = fileName,
                                Title = "Reçeteyi İndir"
                            };
                            if (sfd.ShowDialog() != DialogResult.OK) return;
                            System.IO.File.WriteAllBytes(sfd.FileName, bytes);
                            XtraMessageBox.Show("Reçete indirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("PDF açılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void DownloadTahlilPdf(int tahlilId)
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string q = @"SELECT TahlilPdf, TahlilPdfFileName
                                 FROM Tbl_Tahliller
                                 WHERE Tahlilid = @id AND HastaTC = @tc";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", tahlilId);
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (!dr.Read()) return;
                            if (dr["TahlilPdf"] == DBNull.Value) return;
                            var bytes = (byte[])dr["TahlilPdf"];
                            if (bytes == null || bytes.Length == 0) return;

                            string fileName = dr["TahlilPdfFileName"] == DBNull.Value ? $"Tahlil_{tahlilId}.pdf" : dr["TahlilPdfFileName"].ToString();
                            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) fileName += ".pdf";
                            using var sfd = new SaveFileDialog
                            {
                                Filter = "PDF Dosyası|*.pdf",
                                FileName = fileName,
                                Title = "Tahlili İndir"
                            };
                            if (sfd.ShowDialog() != DialogResult.OK) return;
                            System.IO.File.WriteAllBytes(sfd.FileName, bytes);
                            XtraMessageBox.Show("Tahlil indirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Tahlil indirirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private DevExpress.XtraBars.Navigation.NavigationPage CreateModernProfilePage()
        {
            var page = new DevExpress.XtraBars.Navigation.NavigationPage();
            page.Name = "pageProfilModern";
            page.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            page.Appearance.Options.UseBackColor = true;
            page.Padding = new Padding(22);

            var card = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };
            card.Appearance.BackColor = Color.Transparent;
            card.Appearance.Options.UseBackColor = true;

            var cardFill = Color.FromArgb(30, 42, 56); // #1E2A38
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = card.ClientRectangle;
                rect.Inflate(-1, -1);

                int radius = 22;
                int d = radius * 2;
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                using var brush = new SolidBrush(cardFill);
                e.Graphics.FillPath(brush, path);
                using var pen = new Pen(Color.FromArgb(120, Color.White), 1.4f);
                e.Graphics.DrawPath(pen, path);
            };

            var lblTitle = new LabelControl
            {
                AllowHtmlString = true,
                Location = new Point(36, 30),
                Text = "<color=#5D9CEC>Profil</color> <color=#ECEFF1>ve Kişisel Bilgiler</color>"
            };
            lblTitle.Appearance.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(236, 239, 241);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            card.Controls.Add(lblTitle);

            var lblSub = new LabelControl
            {
                Location = new Point(38, 82),
                Text = "Profilinizi güncelleyin ve VKİ değerini görsel olarak takip edin."
            };
            lblSub.Appearance.Font = new Font("Segoe UI", 11F);
            lblSub.Appearance.ForeColor = Color.FromArgb(176, 190, 197);
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Appearance.Options.UseForeColor = true;
            card.Controls.Add(lblSub);

            var sep = new SeparatorControl { Location = new Point(36, 110), Size = new Size(Math.Max(0, card.Width - 72), 18) };
            sep.LineColor = Color.FromArgb(70, 255, 255, 255);
            card.Controls.Add(sep);
            card.SizeChanged += (s, e) => sep.Size = new Size(Math.Max(0, card.Width - 72), 18);

            var layout = new TablePanel { Dock = DockStyle.None };
            layout.Appearance.BackColor = Color.Transparent;
            layout.Appearance.Options.UseBackColor = true;
            layout.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1.0f),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1.6f),
            });
            layout.Rows.Add(new TablePanelRow(TablePanelEntityStyle.Relative, 1));
            card.Controls.Add(layout);

            void LayoutContent()
            {
                const int left = 36;
                const int top = 140;
                const int right = 36;
                const int bottom = 28;
                layout.Location = new Point(left, top);
                layout.Size = new Size(
                    Math.Max(0, card.Width - (left + right)),
                    Math.Max(0, card.Height - (top + bottom))
                );
            }
            LayoutContent();
            card.SizeChanged += (s, e) => LayoutContent();

            PanelControl MakeSectionCard()
            {
                var p = new PanelControl
                {
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(16)
                };
                p.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
                p.Appearance.Options.UseBackColor = true;
                p.SizeChanged += (s, e) =>
                {
                    if (p.Width <= 0 || p.Height <= 0) return;
                    p.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, p.Width, p.Height, 18, 18));
                };
                return p;
            }

            // Left card: photo + basic info
            var leftCard = MakeSectionCard();
            var leftLayout = new TablePanel { Dock = DockStyle.Fill };
            leftLayout.Appearance.BackColor = Color.Transparent;
            leftLayout.Appearance.Options.UseBackColor = true;
            leftLayout.Columns.Add(new TablePanelColumn(TablePanelEntityStyle.Relative, 1));
            leftLayout.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 160), // photo smaller
                new TablePanelRow(TablePanelEntityStyle.Absolute, 44),  // upload button
                new TablePanelRow(TablePanelEntityStyle.Absolute, 200), // boy/kilo + VKI block
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),
            });
            leftCard.Controls.Add(leftLayout);

            var photoWrap = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill };
            photoWrap.Appearance.BackColor = Color.Transparent;
            photoWrap.Appearance.Options.UseBackColor = true;
            leftLayout.Controls.Add(photoWrap);
            leftLayout.SetRow(photoWrap, 0);
            leftLayout.SetColumn(photoWrap, 0);

            _profilePhoto = new PictureEdit
            {
                Properties =
                {
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                    ShowMenu = false,
                    SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze,
                    NullText = ""
                },
                Size = new Size(112, 112)
            };
            _profilePhoto.BackColor = Color.Transparent;
            _profilePhoto.Properties.Appearance.BackColor = Color.Transparent;
            _profilePhoto.Properties.Appearance.Options.UseBackColor = true;

            var photoBg = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Size = new Size(132, 132)
            };
            photoBg.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            photoBg.Appearance.Options.UseBackColor = true;
            photoBg.SizeChanged += (s, e) =>
            {
                if (photoBg.Width <= 0 || photoBg.Height <= 0) return;
                photoBg.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, photoBg.Width, photoBg.Height, 80, 80));
            };
            photoBg.Controls.Add(_profilePhoto);
            photoBg.Padding = new Padding(10);
            _profilePhoto.Dock = DockStyle.Fill;

            photoWrap.Controls.Add(photoBg);
            photoWrap.SizeChanged += (s, e) =>
            {
                photoBg.Location = new Point(
                    Math.Max(0, (photoWrap.Width - photoBg.Width) / 2),
                    10
                );
            };

            // Upload button centered & smaller
            var uploadHost = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
            };
            uploadHost.Appearance.BackColor = Color.Transparent;
            uploadHost.Appearance.Options.UseBackColor = true;

            _profilePhotoUpload = new SimpleButton
            {
                Text = "Fotoğraf Yükle",
                Cursor = Cursors.Hand,
                Size = new Size(200, 40),
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _profilePhotoUpload.LookAndFeel.UseDefaultLookAndFeel = false;
            _profilePhotoUpload.Appearance.BackColor = ColorTranslator.FromHtml("#2563EB");
            _profilePhotoUpload.Appearance.ForeColor = Color.White;
            _profilePhotoUpload.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _profilePhotoUpload.Appearance.Options.UseBackColor = true;
            _profilePhotoUpload.Appearance.Options.UseForeColor = true;
            _profilePhotoUpload.Appearance.Options.UseFont = true;
            _profilePhotoUpload.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#2F74FF");
            _profilePhotoUpload.AppearanceHovered.Options.UseBackColor = true;
            _profilePhotoUpload.AppearancePressed.BackColor = ColorTranslator.FromHtml("#1D4ED8");
            _profilePhotoUpload.AppearancePressed.Options.UseBackColor = true;
            _profilePhotoUpload.SizeChanged += (s, e) => ApplyPillRegion(_profilePhotoUpload);
            uploadHost.Controls.Add(_profilePhotoUpload);
            uploadHost.SizeChanged += (s, e) =>
            {
                _profilePhotoUpload.Location = new Point(
                    Math.Max(0, (uploadHost.Width - _profilePhotoUpload.Width) / 2),
                    Math.Max(0, (uploadHost.Height - _profilePhotoUpload.Height) / 2)
                );
            };
            leftLayout.Controls.Add(uploadHost);
            leftLayout.SetRow(uploadHost, 1);
            leftLayout.SetColumn(uploadHost, 0);

            PanelControl MakeField(string title, Control editor, bool readOnly = false)
            {
                var wrap = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(12, 10, 12, 10) };
                // Daha belirgin input alanı: daha koyu zemin + ince border
                wrap.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
                wrap.Appearance.Options.UseBackColor = true;
                wrap.SizeChanged += (s, e) =>
                {
                    if (wrap.Width <= 0 || wrap.Height <= 0) return;
                    wrap.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, wrap.Width, wrap.Height, 16, 16));
                };
                wrap.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    var rect = wrap.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    using var path = new System.Drawing.Drawing2D.GraphicsPath();
                    int radius = 16;
                    int d = radius * 2;
                    path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                    path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                    path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();
                    using var pen = new Pen(ColorTranslator.FromHtml("#334155"), 1.4f);
                    e.Graphics.DrawPath(pen, path);
                };

                var lbl = new LabelControl { Text = title, Dock = DockStyle.Top, Height = 18 };
                lbl.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                lbl.Appearance.ForeColor = ColorTranslator.FromHtml("#94A3B8");
                lbl.Appearance.Options.UseFont = true;
                lbl.Appearance.Options.UseForeColor = true;
                wrap.Controls.Add(lbl);

                editor.Dock = DockStyle.Fill;
                wrap.Controls.Add(editor);
                editor.BringToFront();
                return wrap;
            }

            // Boy/Kilo + VKI bloğu (foto altı)
            var bmiBlock = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 8, 0, 0)
            };
            bmiBlock.Appearance.BackColor = Color.Transparent;
            bmiBlock.Appearance.Options.UseBackColor = true;

            var bmiTbl = new TablePanel { Dock = DockStyle.Fill };
            bmiTbl.Appearance.BackColor = Color.Transparent;
            bmiTbl.Appearance.Options.UseBackColor = true;
            bmiTbl.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
            });
            bmiTbl.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 70),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 120),
            });
            bmiBlock.Controls.Add(bmiTbl);

            _profileBoy = new TextEdit();
            _profileBoy.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _profileBoy.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            _profileBoy.Properties.Mask.EditMask = "d";
            _profileBoy.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            _profileBoy.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileBoy.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _profileBoy.Properties.Appearance.Options.UseBackColor = true;
            _profileBoy.Properties.Appearance.Options.UseForeColor = true;
            _profileBoy.Properties.Appearance.Options.UseFont = true;

            _profileKilo = new TextEdit();
            _profileKilo.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _profileKilo.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            _profileKilo.Properties.Mask.EditMask = "d";
            _profileKilo.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            _profileKilo.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileKilo.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _profileKilo.Properties.Appearance.Options.UseBackColor = true;
            _profileKilo.Properties.Appearance.Options.UseForeColor = true;
            _profileKilo.Properties.Appearance.Options.UseFont = true;

            var boyWrap = MakeField("Boy (cm)", _profileBoy);
            var kiloWrap = MakeField("Kilo (kg)", _profileKilo);
            bmiTbl.Controls.Add(boyWrap); bmiTbl.SetRow(boyWrap, 0); bmiTbl.SetColumn(boyWrap, 0);
            bmiTbl.Controls.Add(kiloWrap); bmiTbl.SetRow(kiloWrap, 0); bmiTbl.SetColumn(kiloWrap, 1);

            var bmiWrap = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(12, 10, 12, 10) };
            bmiWrap.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            bmiWrap.Appearance.Options.UseBackColor = true;
            bmiWrap.SizeChanged += (s, e) =>
            {
                if (bmiWrap.Width <= 0 || bmiWrap.Height <= 0) return;
                bmiWrap.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, bmiWrap.Width, bmiWrap.Height, 16, 16));
            };
            var bmiTitle = new LabelControl { Text = "Vücut Kitle İndeksi (VKİ)", Dock = DockStyle.Top, Height = 18 };
            bmiTitle.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            bmiTitle.Appearance.ForeColor = ColorTranslator.FromHtml("#94A3B8");
            bmiTitle.Appearance.Options.UseFont = true;
            bmiTitle.Appearance.Options.UseForeColor = true;
            bmiWrap.Controls.Add(bmiTitle);

            _profileBmiLabel = new LabelControl { Text = "VKİ: -", Dock = DockStyle.Top, Height = 20 };
            _profileBmiLabel.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _profileBmiLabel.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileBmiLabel.Appearance.Options.UseFont = true;
            _profileBmiLabel.Appearance.Options.UseForeColor = true;
            bmiWrap.Controls.Add(_profileBmiLabel);
            _profileBmiLabel.BringToFront();

            _profileBmiProgress = new DevExpress.XtraEditors.ProgressBarControl { Dock = DockStyle.Fill };
            _profileBmiProgress.Properties.Minimum = 10;
            _profileBmiProgress.Properties.Maximum = 40;
            _profileBmiProgress.Properties.ShowTitle = true;
            _profileBmiProgress.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _profileBmiProgress.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            _profileBmiProgress.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#2563EB");
            _profileBmiProgress.Properties.Appearance.Options.UseBackColor = true;
            _profileBmiProgress.Properties.Appearance.Options.UseForeColor = true;
            bmiWrap.Controls.Add(_profileBmiProgress);
            _profileBmiProgress.BringToFront();

            bmiTbl.Controls.Add(bmiWrap);
            bmiTbl.SetRow(bmiWrap, 1);
            bmiTbl.SetColumn(bmiWrap, 0);
            bmiTbl.SetColumnSpan(bmiWrap, 2);

            leftLayout.Controls.Add(bmiBlock);
            leftLayout.SetRow(bmiBlock, 2);
            leftLayout.SetColumn(bmiBlock, 0);

            _profileTcMasked = new LabelControl { AutoSizeMode = LabelAutoSizeMode.None, Dock = DockStyle.Fill, Text = "TC: ***" };
            _profileTcMasked.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _profileTcMasked.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileTcMasked.Appearance.Options.UseFont = true;
            _profileTcMasked.Appearance.Options.UseForeColor = true;
            leftLayout.Controls.Add(MakeField("TC Kimlik No", _profileTcMasked, readOnly: true));
            leftLayout.SetRow(leftLayout.Controls[leftLayout.Controls.Count - 1], 3);

            _profileAdSoyad = new TextEdit { Properties = { ReadOnly = true, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder } };
            _profileAdSoyad.Properties.Appearance.BackColor = Color.Transparent;
            _profileAdSoyad.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileAdSoyad.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _profileAdSoyad.Properties.Appearance.Options.UseBackColor = true;
            _profileAdSoyad.Properties.Appearance.Options.UseForeColor = true;
            _profileAdSoyad.Properties.Appearance.Options.UseFont = true;
            leftLayout.Controls.Add(MakeField("Ad - Soyad", _profileAdSoyad, readOnly: true));
            leftLayout.SetRow(leftLayout.Controls[leftLayout.Controls.Count - 1], 4);

            var birthHost = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill };
            birthHost.Appearance.BackColor = Color.Transparent;
            birthHost.Appearance.Options.UseBackColor = true;
            var birthTbl = new TablePanel { Dock = DockStyle.Fill };
            birthTbl.Appearance.BackColor = Color.Transparent;
            birthTbl.Appearance.Options.UseBackColor = true;
            birthTbl.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Absolute, 86),
            });
            birthTbl.Rows.Add(new TablePanelRow(TablePanelEntityStyle.Relative, 1));
            _profileDogumTarih = new DateEdit();
            _profileDogumTarih.Properties.AutoHeight = false;
            _profileDogumTarih.Height = 32;
            _profileDogumTarih.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _profileDogumTarih.Properties.Buttons.Clear();
            _profileDogumTarih.Properties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            _profileDogumTarih.Properties.CalendarTimeProperties.Buttons.Clear();
            _profileDogumTarih.Properties.CalendarTimeProperties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            _profileDogumTarih.Properties.Appearance.BackColor = Color.Transparent;
            _profileDogumTarih.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileDogumTarih.Properties.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _profileDogumTarih.Properties.Appearance.Options.UseBackColor = true;
            _profileDogumTarih.Properties.Appearance.Options.UseForeColor = true;
            _profileDogumTarih.Properties.Appearance.Options.UseFont = true;

            _profileAgeLabel = new LabelControl { Text = "Yaş: -", AutoSizeMode = LabelAutoSizeMode.None, Dock = DockStyle.Fill };
            _profileAgeLabel.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            _profileAgeLabel.Appearance.ForeColor = ColorTranslator.FromHtml("#93C5FD");
            _profileAgeLabel.Appearance.Options.UseFont = true;
            _profileAgeLabel.Appearance.Options.UseForeColor = true;
            _profileAgeLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;

            birthTbl.Controls.Add(_profileDogumTarih);
            birthTbl.SetColumn(_profileDogumTarih, 0);
            birthTbl.Controls.Add(_profileAgeLabel);
            birthTbl.SetColumn(_profileAgeLabel, 1);
            birthHost.Controls.Add(birthTbl);
            leftLayout.Controls.Add(MakeField("Doğum Tarihi ve Yaş", birthHost));
            leftLayout.SetRow(leftLayout.Controls[leftLayout.Controls.Count - 1], 5);

            _profileCinsiyet = new ComboBoxEdit();
            _profileCinsiyet.Properties.AutoHeight = false;
            _profileCinsiyet.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _profileCinsiyet.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _profileCinsiyet.Properties.Items.AddRange(new object[] { "Erkek", "Kadın" });
            _profileCinsiyet.Properties.Appearance.BackColor = Color.Transparent;
            _profileCinsiyet.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileCinsiyet.Properties.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _profileCinsiyet.Properties.Appearance.Options.UseBackColor = true;
            _profileCinsiyet.Properties.Appearance.Options.UseForeColor = true;
            _profileCinsiyet.Properties.Appearance.Options.UseFont = true;
            leftLayout.Controls.Add(MakeField("Cinsiyet", _profileCinsiyet));
            leftLayout.SetRow(leftLayout.Controls[leftLayout.Controls.Count - 1], 6);

            _profileKanGrubu = new ComboBoxEdit();
            _profileKanGrubu.Properties.AutoHeight = false;
            _profileKanGrubu.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _profileKanGrubu.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _profileKanGrubu.Properties.Items.AddRange(new object[] { "A Rh+", "A Rh-", "B Rh+", "B Rh-", "AB Rh+", "AB Rh-", "0 Rh+", "0 Rh-" });
            _profileKanGrubu.Properties.Appearance.BackColor = Color.Transparent;
            _profileKanGrubu.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileKanGrubu.Properties.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _profileKanGrubu.Properties.Appearance.Options.UseBackColor = true;
            _profileKanGrubu.Properties.Appearance.Options.UseForeColor = true;
            _profileKanGrubu.Properties.Appearance.Options.UseFont = true;
            leftLayout.Controls.Add(MakeField("Kan Grubu", _profileKanGrubu));
            leftLayout.SetRow(leftLayout.Controls[leftLayout.Controls.Count - 1], 7);

            // Right card: contact + BMI + address
            var rightCard = MakeSectionCard();
            var rightLayout = new TablePanel { Dock = DockStyle.Fill };
            rightLayout.Appearance.BackColor = Color.Transparent;
            rightLayout.Appearance.Options.UseBackColor = true;
            rightLayout.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
            });
            rightLayout.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 64), // telefon/email (bigger)
                new TablePanelRow(TablePanelEntityStyle.Absolute, 64), // il/ilce (bigger)
                new TablePanelRow(TablePanelEntityStyle.Absolute, 180), // adres (smaller, fixed)
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),   // spacer
                new TablePanelRow(TablePanelEntityStyle.Absolute, 60),  // save
            });
            rightCard.Controls.Add(rightLayout);

            TextEdit MakeTextField()
            {
                var t = new TextEdit();
                t.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                t.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
                t.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
                t.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                t.Properties.Appearance.Options.UseBackColor = true;
                t.Properties.Appearance.Options.UseForeColor = true;
                t.Properties.Appearance.Options.UseFont = true;
                return t;
            }

            _profileTelefon = MakeTextField();
            rightLayout.Controls.Add(MakeField("Cep Telefonu", _profileTelefon));
            rightLayout.SetColumn(rightLayout.Controls[rightLayout.Controls.Count - 1], 0);
            rightLayout.SetRow(rightLayout.Controls[rightLayout.Controls.Count - 1], 0);

            _profileEmail = MakeTextField();
            rightLayout.Controls.Add(MakeField("E-Posta", _profileEmail));
            rightLayout.SetColumn(rightLayout.Controls[rightLayout.Controls.Count - 1], 1);
            rightLayout.SetRow(rightLayout.Controls[rightLayout.Controls.Count - 1], 0);

            _profileIl = MakeTextField();
            rightLayout.Controls.Add(MakeField("İl", _profileIl));
            rightLayout.SetColumn(rightLayout.Controls[rightLayout.Controls.Count - 1], 0);
            rightLayout.SetRow(rightLayout.Controls[rightLayout.Controls.Count - 1], 1);

            _profileIlce = MakeTextField();
            rightLayout.Controls.Add(MakeField("İlçe", _profileIlce));
            rightLayout.SetColumn(rightLayout.Controls[rightLayout.Controls.Count - 1], 1);
            rightLayout.SetRow(rightLayout.Controls[rightLayout.Controls.Count - 1], 1);

            _profileAdres = new MemoEdit { Dock = DockStyle.Fill };
            _profileAdres.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _profileAdres.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            _profileAdres.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _profileAdres.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            _profileAdres.Properties.Appearance.Options.UseBackColor = true;
            _profileAdres.Properties.Appearance.Options.UseForeColor = true;
            _profileAdres.Properties.Appearance.Options.UseFont = true;
            var addrWrap = MakeField("Açık Adres", _profileAdres);
            rightLayout.Controls.Add(addrWrap);
            rightLayout.SetColumn(addrWrap, 0);
            rightLayout.SetRow(addrWrap, 2);
            rightLayout.SetColumnSpan(addrWrap, 2);

            _profileSave = new SimpleButton
            {
                Text = "Profili Kaydet",
                Cursor = Cursors.Hand,
                Dock = DockStyle.Fill,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _profileSave.LookAndFeel.UseDefaultLookAndFeel = false;
            _profileSave.Appearance.BackColor = ColorTranslator.FromHtml("#2563EB");
            _profileSave.Appearance.ForeColor = Color.White;
            _profileSave.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _profileSave.Appearance.Options.UseBackColor = true;
            _profileSave.Appearance.Options.UseForeColor = true;
            _profileSave.Appearance.Options.UseFont = true;
            _profileSave.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#2F74FF");
            _profileSave.AppearanceHovered.Options.UseBackColor = true;
            _profileSave.AppearancePressed.BackColor = ColorTranslator.FromHtml("#1D4ED8");
            _profileSave.AppearancePressed.Options.UseBackColor = true;
            _profileSave.SizeChanged += (s, e) => ApplyPillRegion(_profileSave);
            rightLayout.Controls.Add(_profileSave);
            rightLayout.SetColumn(_profileSave, 0);
            rightLayout.SetRow(_profileSave, 4);
            rightLayout.SetColumnSpan(_profileSave, 2);

            // bind
            _profilePhotoUpload.Click += (s, e) => UploadProfilePhoto();
            _profileSave.Click += (s, e) => SaveProfile();
            _profileBoy.EditValueChanged += (s, e) => UpdateBmiUI();
            _profileKilo.EditValueChanged += (s, e) => UpdateBmiUI();
            _profileDogumTarih.EditValueChanged += (s, e) => UpdateAgeUI();

            layout.Controls.Add(leftCard);
            layout.SetColumn(leftCard, 0);
            layout.SetRow(leftCard, 0);

            layout.Controls.Add(rightCard);
            layout.SetColumn(rightCard, 1);
            layout.SetRow(rightCard, 0);

            page.Controls.Add(card);
            return page;
        }

        private void LoadMedicalPageData()
        {
            // Alerji
            try
            {
                if (_medAllergyList != null)
                {
                    using (var conn = SqlBaglantisi.Instance.GetConnection())
                    {
                        string q = "SELECT HastaAlerjiler FROM Tbl_Hastalar WHERE HastaTC = @tc";
                        using (var cmd = new SqlCommand(q, conn))
                        {
                            cmd.Parameters.AddWithValue("@tc", HastaTC);
                            var v = cmd.ExecuteScalar();
                            _medAllergyItems = ParseAllergyText(v?.ToString());
                            RebuildAllergyListUI();
                        }
                    }
                }
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }

            // Tahliller
            try
            {
                if (_medGridTahliller != null)
                {
                    using (var conn = SqlBaglantisi.Instance.GetConnection())
                    {
                        string query = @"SELECT Tahlilid,
                                                TahlilTur,
                                                TahlilAd,
                                                TahlilTarih,
                                                DoktorAd,
                                                TahlilSonuc,
                                                TahlilDurum,
                                                CASE WHEN TahlilPdf IS NULL THEN 0 ELSE 1 END AS PdfVar,
                                                TahlilPdfFileName
                                         FROM Tbl_Tahliller
                                         WHERE HastaTC = @tc
                                         ORDER BY TahlilTarih DESC";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@tc", HastaTC);
                            var dt = new System.Data.DataTable();
                            using (var da = new SqlDataAdapter(cmd))
                                da.Fill(dt);
                            _medGridTahliller.DataSource = dt;
                            try
                            {
                                if (_medViewTahliller != null)
                                {
                                    if (_medViewTahliller.Columns["Tahlilid"] != null) _medViewTahliller.Columns["Tahlilid"].Visible = false;
                                    if (_medViewTahliller.Columns["TahlilPdfFileName"] != null) _medViewTahliller.Columns["TahlilPdfFileName"].Visible = false;
                                    if (_medViewTahliller.Columns["PdfVar"] != null) _medViewTahliller.Columns["PdfVar"].Caption = "PDF";
                                    if (_medViewTahliller.Columns["TahlilTur"] != null) _medViewTahliller.Columns["TahlilTur"].Caption = "Tür";
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }

            // Reçeteler
            try
            {
                if (_medGridReceteler != null)
                {
                    using (var conn = SqlBaglantisi.Instance.GetConnection())
                    {
                        string query = @"SELECT Receteid,
                                                ReceteKod,
                                                ReceteTarih,
                                                DoktorAd,
                                                Ilaclar,
                                                CASE WHEN RecetePdf IS NULL THEN 0 ELSE 1 END AS PdfVar,
                                                RecetePdfFileName
                                         FROM Tbl_Receteler
                                         WHERE HastaTC = @tc
                                         ORDER BY ReceteTarih DESC";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@tc", HastaTC);
                            var dt = new System.Data.DataTable();
                            using (var da = new SqlDataAdapter(cmd))
                                da.Fill(dt);
                            _medGridReceteler.DataSource = dt;

                            try
                            {
                                if (_medViewReceteler != null)
                                {
                                    if (_medViewReceteler.Columns["Receteid"] != null) _medViewReceteler.Columns["Receteid"].Visible = false;
                                    // Dosya adını panelde göstermeyelim (hasta sadece indir/aç yapsın)
                                    if (_medViewReceteler.Columns["RecetePdfFileName"] != null) _medViewReceteler.Columns["RecetePdfFileName"].Visible = false;
                                    if (_medViewReceteler.Columns["PdfVar"] != null) _medViewReceteler.Columns["PdfVar"].Caption = "PDF";
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void SaveMedicalAllergies()
        {
            try
            {
                if (_medAllergyItems == null) _medAllergyItems = new System.Collections.Generic.List<string>();
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string q = "UPDATE Tbl_Hastalar SET HastaAlerjiler = @alerji WHERE HastaTC = @tc";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        var joined = string.Join(", ", _medAllergyItems);
                        cmd.Parameters.AddWithValue("@alerji", joined);
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        cmd.ExecuteNonQuery();
                    }
                }
                // Kartları yenile
                LoadHastaBilgileri();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Alerjiler kaydedilirken hata oluştu: " + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void LoadProfilePageData()
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string q = @"SELECT HastaAd, HastaSoyad, HastaTC, HastaTelefon, HastaCinsiyet, HastaKanGrubu,
                                        HastaDogumTarihi, HastaEmail, HastaIl, HastaIlce, HastaAdres, HastaBoyCm, HastaKiloKg, HastaFoto
                                 FROM Tbl_Hastalar WHERE HastaTC = @tc";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (!dr.Read()) return;

                            string ad = dr["HastaAd"]?.ToString() ?? "";
                            string soyad = dr["HastaSoyad"]?.ToString() ?? "";
                            string tc = dr["HastaTC"]?.ToString() ?? HastaTC;

                            if (_profileTcMasked != null) _profileTcMasked.Text = MaskTc(tc);
                            if (_profileAdSoyad != null) _profileAdSoyad.Text = (ad + " " + soyad).Trim();

                            if (_profileTelefon != null) _profileTelefon.Text = dr["HastaTelefon"]?.ToString() ?? "";
                            if (_profileEmail != null) _profileEmail.Text = dr["HastaEmail"]?.ToString() ?? "";
                            if (_profileIl != null) _profileIl.Text = dr["HastaIl"]?.ToString() ?? "";
                            if (_profileIlce != null) _profileIlce.Text = dr["HastaIlce"]?.ToString() ?? "";
                            if (_profileAdres != null) _profileAdres.Text = dr["HastaAdres"]?.ToString() ?? "";
                            if (_profileCinsiyet != null) _profileCinsiyet.EditValue = dr["HastaCinsiyet"]?.ToString() ?? null;
                            if (_profileKanGrubu != null) _profileKanGrubu.EditValue = dr["HastaKanGrubu"]?.ToString() ?? null;

                            if (_profileDogumTarih != null)
                            {
                                _profileDogumTarih.EditValue = dr["HastaDogumTarihi"] == DBNull.Value ? null : dr["HastaDogumTarihi"];
                                UpdateAgeUI();
                            }

                            if (_profileBoy != null) _profileBoy.Text = dr["HastaBoyCm"] == DBNull.Value ? "" : dr["HastaBoyCm"].ToString();
                            if (_profileKilo != null) _profileKilo.Text = dr["HastaKiloKg"] == DBNull.Value ? "" : dr["HastaKiloKg"].ToString();
                            UpdateBmiUI();

                            if (_profilePhoto != null)
                            {
                                if (dr["HastaFoto"] != DBNull.Value && dr["HastaFoto"] is byte[] bytes && bytes.Length > 0)
                                {
                                    using var ms = new System.IO.MemoryStream(bytes);
                                    _profilePhoto.Image = Image.FromStream(ms);
                                }
                                else
                                {
                                    _profilePhoto.Image = null;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // sessiz
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void SaveProfile()
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string q = @"UPDATE Tbl_Hastalar SET
                                    HastaDogumTarihi = @dogum,
                                    HastaCinsiyet = @cinsiyet,
                                    HastaKanGrubu = @kangrubu,
                                    HastaBoyCm = @boy,
                                    HastaKiloKg = @kilo,
                                    HastaTelefon = @tel,
                                    HastaEmail = @email,
                                    HastaIl = @il,
                                    HastaIlce = @ilce,
                                    HastaAdres = @adres
                                 WHERE HastaTC = @tc";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        cmd.Parameters.AddWithValue("@dogum", _profileDogumTarih?.EditValue == null ? (object)DBNull.Value : Convert.ToDateTime(_profileDogumTarih.EditValue).Date);
                        cmd.Parameters.AddWithValue("@cinsiyet", string.IsNullOrWhiteSpace(_profileCinsiyet?.Text) ? (object)DBNull.Value : _profileCinsiyet.Text);
                        cmd.Parameters.AddWithValue("@kangrubu", string.IsNullOrWhiteSpace(_profileKanGrubu?.Text) ? (object)DBNull.Value : _profileKanGrubu.Text);
                        cmd.Parameters.AddWithValue("@boy", int.TryParse(_profileBoy?.Text, out var boy) ? boy : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@kilo", int.TryParse(_profileKilo?.Text, out var kilo) ? kilo : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@tel", string.IsNullOrWhiteSpace(_profileTelefon?.Text) ? (object)DBNull.Value : _profileTelefon.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(_profileEmail?.Text) ? (object)DBNull.Value : _profileEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@il", string.IsNullOrWhiteSpace(_profileIl?.Text) ? (object)DBNull.Value : _profileIl.Text.Trim());
                        cmd.Parameters.AddWithValue("@ilce", string.IsNullOrWhiteSpace(_profileIlce?.Text) ? (object)DBNull.Value : _profileIlce.Text.Trim());
                        cmd.Parameters.AddWithValue("@adres", string.IsNullOrWhiteSpace(_profileAdres?.Text) ? (object)DBNull.Value : _profileAdres.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadHastaBilgileri();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Profil kaydedilirken hata oluştu: " + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void UploadProfilePhoto()
        {
            try
            {
                using var dlg = new OpenFileDialog
                {
                    Filter = "Resim Dosyaları|*.png;*.jpg;*.jpeg;*.bmp",
                    Title = "Profil Fotoğrafı Seç"
                };
                if (dlg.ShowDialog() != DialogResult.OK) return;

                byte[] bytes = System.IO.File.ReadAllBytes(dlg.FileName);
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string q = "UPDATE Tbl_Hastalar SET HastaFoto = @foto WHERE HastaTC = @tc";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        cmd.Parameters.Add("@foto", System.Data.SqlDbType.VarBinary, bytes.Length).Value = bytes;
                        cmd.ExecuteNonQuery();
                    }
                }

                if (_profilePhoto != null)
                {
                    using var ms = new System.IO.MemoryStream(bytes);
                    _profilePhoto.Image = Image.FromStream(ms);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Fotoğraf yüklenirken hata oluştu: " + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private static string MaskTc(string tc)
        {
            if (string.IsNullOrWhiteSpace(tc) || tc.Length < 11) return "***********";
            return $"{tc.Substring(0, 3)}******{tc.Substring(tc.Length - 2, 2)}";
        }

        private void UpdateAgeUI()
        {
            if (_profileAgeLabel == null || _profileDogumTarih == null) return;
            if (_profileDogumTarih.EditValue == null)
            {
                _profileAgeLabel.Text = "Yaş: -";
                return;
            }
            var dob = Convert.ToDateTime(_profileDogumTarih.EditValue).Date;
            var today = DateTime.Today;
            int age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;
            _profileAgeLabel.Text = $"Yaş: {Math.Max(0, age)}";
        }

        private void UpdateBmiUI()
        {
            if (_profileBmiProgress == null || _profileBmiLabel == null) return;
            if (!double.TryParse(_profileBoy?.Text, out var boy) || !double.TryParse(_profileKilo?.Text, out var kilo) || boy <= 0 || kilo <= 0)
            {
                _profileBmiLabel.Text = "VKİ: -";
                _profileBmiProgress.EditValue = _profileBmiProgress.Properties.Minimum;
                _profileBmiProgress.Properties.DisplayFormat.FormatString = "";
                return;
            }
            double m = boy / 100.0;
            double bmi = kilo / (m * m);
            double clamped = Math.Max(_profileBmiProgress.Properties.Minimum, Math.Min(_profileBmiProgress.Properties.Maximum, bmi));
            _profileBmiProgress.EditValue = clamped;

            string cat;
            Color color;
            if (bmi < 18.5) { cat = "Zayıf"; color = ColorTranslator.FromHtml("#3B82F6"); }        // mavi
            else if (bmi < 25) { cat = "Normal"; color = ColorTranslator.FromHtml("#22C55E"); }     // yeşil
            else if (bmi < 30) { cat = "Kilolu"; color = ColorTranslator.FromHtml("#F59E0B"); }     // turuncu
            else { cat = "Obez"; color = ColorTranslator.FromHtml("#EF4444"); }                     // kırmızı

            _profileBmiLabel.Text = $"VKİ: {bmi:0.0} ({cat})";
            TrySetNestedProperty(_profileBmiProgress, new[] { "Properties", "Appearance", "ForeColor" }, color);
            TrySetNestedProperty(_profileBmiProgress, new[] { "Properties", "Appearance", "Options", "UseForeColor" }, true);
            _profileBmiProgress.Properties.PercentView = false;
            _profileBmiProgress.Properties.ShowTitle = true;
            _profileBmiProgress.Properties.DisplayFormat.FormatString = _profileBmiLabel.Text;
        }

        private void AddMedicalAllergyFromInput()
        {
            try
            {
                if (_medAllergyInput == null) return;
                var text = (_medAllergyInput.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(text)) return;

                if (_medAllergyItems == null) _medAllergyItems = new System.Collections.Generic.List<string>();
                // duplicate engelle
                foreach (var a in _medAllergyItems)
                {
                    if (string.Equals(a, text, StringComparison.OrdinalIgnoreCase))
                    {
                        _medAllergyInput.Text = "";
                        return;
                    }
                }

                _medAllergyItems.Add(text);
                _medAllergyInput.Text = "";
                RebuildAllergyListUI(highlightLast: true);
            }
            catch { }
        }

        private System.Collections.Generic.List<string> ParseAllergyText(string raw)
        {
            var list = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return list;

            var parts = raw
                .Replace("\r", "\n")
                .Replace(";", ",")
                .Split(new[] { ",", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in parts)
            {
                var t = p.Trim();
                if (string.IsNullOrWhiteSpace(t)) continue;
                bool exists = false;
                foreach (var a in list)
                {
                    if (string.Equals(a, t, StringComparison.OrdinalIgnoreCase)) { exists = true; break; }
                }
                if (!exists) list.Add(t);
            }
            return list;
        }

        private void RebuildAllergyListUI(bool highlightLast = false)
        {
            if (_medAllergyList == null) return;
            _medAllergyList.SuspendLayout();
            _medAllergyList.Controls.Clear();

            var items = _medAllergyItems ?? new System.Collections.Generic.List<string>();
            if (items.Count == 0)
            {
                var empty = new LabelControl
                {
                    Text = "Henüz alerji eklenmedi.",
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Width = Math.Max(0, _medAllergyList.Width - 25),
                    Height = 24
                };
                empty.Appearance.Font = new Font("Segoe UI", 10F);
                empty.Appearance.ForeColor = ColorTranslator.FromHtml("#94A3B8");
                empty.Appearance.Options.UseFont = true;
                empty.Appearance.Options.UseForeColor = true;
                _medAllergyList.Controls.Add(empty);
                _medAllergyList.ResumeLayout();
                return;
            }

            PanelControl lastPanel = null;
            foreach (var a in items)
            {
                var row = new PanelControl
                {
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                    BackColor = Color.Transparent,
                    Padding = new Padding(10, 8, 10, 8),
                    Margin = new Padding(0, 0, 0, 10),
                    Width = Math.Max(0, _medAllergyList.ClientSize.Width - 18),
                    Height = 40
                };
                row.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
                row.Appearance.Options.UseBackColor = true;
                row.SizeChanged += (s, e) =>
                {
                    if (row.Width <= 0 || row.Height <= 0) return;
                    row.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, row.Width, row.Height, 14, 14));
                };

                var bullet = new LabelControl
                {
                    Text = "•",
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Location = new Point(10, 8),
                    Size = new Size(16, 24)
                };
                bullet.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
                bullet.Appearance.ForeColor = ColorTranslator.FromHtml("#93C5FD");
                bullet.Appearance.Options.UseFont = true;
                bullet.Appearance.Options.UseForeColor = true;

                var text = new LabelControl
                {
                    Text = a,
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Location = new Point(28, 10),
                    Size = new Size(Math.Max(0, row.Width - 88), 22)
                };
                text.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                text.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
                text.Appearance.Options.UseFont = true;
                text.Appearance.Options.UseForeColor = true;

                var btnDel = new SimpleButton
                {
                    Text = "✕",
                    Cursor = Cursors.Hand,
                    ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                    Size = new Size(32, 32)
                };
                btnDel.LookAndFeel.UseDefaultLookAndFeel = false;
                btnDel.Appearance.BackColor = Color.Transparent;
                btnDel.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
                btnDel.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
                btnDel.Appearance.Options.UseBackColor = true;
                btnDel.Appearance.Options.UseForeColor = true;
                btnDel.Appearance.Options.UseFont = true;
                btnDel.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#DC2626");
                btnDel.AppearanceHovered.ForeColor = Color.White;
                btnDel.AppearanceHovered.Options.UseBackColor = true;
                btnDel.AppearanceHovered.Options.UseForeColor = true;
                btnDel.AppearancePressed.BackColor = ColorTranslator.FromHtml("#B91C1C");
                btnDel.AppearancePressed.Options.UseBackColor = true;
                btnDel.Location = new Point(Math.Max(0, row.Width - 44), 4);
                btnDel.SizeChanged += (s, e) => ApplyPillRegion(btnDel);
                row.SizeChanged += (s, e) => btnDel.Location = new Point(Math.Max(0, row.Width - 44), 4);
                btnDel.Click += (s, e) =>
                {
                    try
                    {
                        if (_medAllergyItems == null) _medAllergyItems = new System.Collections.Generic.List<string>();
                        for (int i = _medAllergyItems.Count - 1; i >= 0; i--)
                        {
                            if (string.Equals(_medAllergyItems[i], a, StringComparison.OrdinalIgnoreCase))
                                _medAllergyItems.RemoveAt(i);
                        }
                        RebuildAllergyListUI();
                        // kullanıcı "silebilsin" dediği için hemen DB’ye kaydedelim
                        SaveMedicalAllergies();
                    }
                    catch { }
                };

                row.Controls.Add(bullet);
                row.Controls.Add(text);
                row.Controls.Add(btnDel);
                _medAllergyList.Controls.Add(row);
                lastPanel = row;
            }

            _medAllergyList.ResumeLayout();

            // “panoya eklenir gibi” hafif highlight
            if (highlightLast && lastPanel != null)
            {
                var normal = ColorTranslator.FromHtml("#111827");
                var flash = ColorTranslator.FromHtml("#1D4ED8");
                lastPanel.Appearance.BackColor = flash;
                var t = new System.Windows.Forms.Timer { Interval = 140 };
                t.Tick += (s, e) =>
                {
                    t.Stop();
                    t.Dispose();
                    try { lastPanel.Appearance.BackColor = normal; } catch { }
                };
                t.Start();
            }
        }

        private DevExpress.XtraBars.Navigation.NavigationPage CreateModernRandevuPage()
        {
            // Yeni modern randevu alma ekranı: uygulama içinde bir sayfa
            var page = new DevExpress.XtraBars.Navigation.NavigationPage();
            page.Name = "pageRandevuModern";
            // Kart dışı arka plan (buz mavisi yerine koyu, modern ton)
            page.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            page.Appearance.Options.UseBackColor = true;
            // Kartın yuvarlatılmış köşeleri parent tarafından "kesilmesin" diye küçük bir dış boşluk
            page.Padding = new Padding(22);

            // Region ile kırpma bazı makinelerde "çift çizgi/garip köşe" yapabiliyor.
            // Bunun yerine kartı Paint ile rounded olarak çiziyoruz (daha stabil).
            var card = new PanelControl();
            card.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            card.Appearance.BackColor = Color.Transparent;
            card.Appearance.Options.UseBackColor = true;
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(24);

            var cardFill = Color.FromArgb(30, 42, 56); // #1E2A38
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = card.ClientRectangle;
                rect.Inflate(-1, -1);

                int radius = 22;
                int d = radius * 2;
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();

                using var brush = new SolidBrush(cardFill);
                e.Graphics.FillPath(brush, path);

                using var pen = new Pen(Color.FromArgb(120, Color.White), 1.4f);
                e.Graphics.DrawPath(pen, path);
            };

            // Başlık
            var lblTitle = new LabelControl
            {
                AllowHtmlString = true,
                Location = new Point(36, 30),
                Text = "<color=#5D9CEC>Randevu</color> <color=#ECEFF1>İşlemleri</color>"
            };
            lblTitle.Appearance.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(236, 239, 241);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            card.Controls.Add(lblTitle);

            var lblSub = new LabelControl
            {
                Location = new Point(38, 82),
                Text = "Poliklinik, doktor, tarih ve saat seçerek randevunuzu oluşturun."
            };
            lblSub.Appearance.Font = new Font("Segoe UI", 11F);
            lblSub.Appearance.ForeColor = Color.FromArgb(176, 190, 197);
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Appearance.Options.UseForeColor = true;
            card.Controls.Add(lblSub);

            var sep = new SeparatorControl { Location = new Point(36, 110), Size = new Size(Math.Max(0, card.Width - 72), 18) };
            sep.LineColor = Color.FromArgb(70, 255, 255, 255);
            card.Controls.Add(sep);
            card.SizeChanged += (s, e) =>
            {
                // separator genişliği kartla beraber akıcı büyüsün; yazılar/controls kaymasın
                sep.Size = new Size(Math.Max(0, card.Width - 72), 18);
            };

            // Daha uyumlu layout: header üstte kalsın, içerik header'ın ALTINDA başlasın
            var tbl = new TablePanel
            {
                Dock = DockStyle.None
            };
            tbl.Appearance.BackColor = Color.Transparent;
            tbl.Appearance.Options.UseBackColor = true;
            tbl.Columns.AddRange(new[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1.05f),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1.35f),
            });
            tbl.Rows.AddRange(new[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 76),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 76),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 76),
                new TablePanelRow(TablePanelEntityStyle.Relative, 1),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 84),
            });
            card.Controls.Add(tbl);
            void LayoutContent()
            {
                // Başlık + alt yazı + separator alanı: ~140px; altta da nefes alanı
                const int left = 36;
                const int top = 140;
                const int right = 36;
                const int bottom = 28;
                tbl.Location = new Point(left, top);
                tbl.Size = new Size(
                    Math.Max(0, card.Width - (left + right)),
                    Math.Max(0, card.Height - (top + bottom))
                );
            }
            LayoutContent();
            card.SizeChanged += (s, e) => LayoutContent();

            var cmbBrans = CreateModernCombo("🏥 Poliklinik Seçiniz");
            var cmbDoktor = CreateModernCombo("🩺 Doktor Seçiniz");
            var dtTarih = CreateModernDate("📅 Tarih Seçiniz");

            // instance referansları: LoadBranslar / doktor doldurma için
            _modernCmbBrans = cmbBrans;
            _modernCmbDoktor = cmbDoktor;
            _modernDtTarih = dtTarih;

            // Input'ları modern "field" container ile büyüt (sürüm bağımsız)
            var fieldBrans = WrapModernField(cmbBrans);
            var fieldDoktor = WrapModernField(cmbDoktor);
            var fieldTarih = WrapModernField(dtTarih);

            // Input iconları (ContextImage)
            try
            {
                var asm = typeof(DevExpress.Images.ImageResourceCache).Assembly;
                cmbBrans.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Business.Business_Home.svg", asm);
                cmbDoktor.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Icon Builder.Travel_Medical.svg", asm);
                dtTarih.Properties.ContextImageOptions.SvgImage = DevExpress.Utils.Svg.SvgImage.FromResources("DevExpress.Images.SvgImages.Scheduling.NewAppointment.svg", asm);
                cmbBrans.Properties.ContextImageOptions.SvgImageSize = new Size(18, 18);
                cmbDoktor.Properties.ContextImageOptions.SvgImageSize = new Size(18, 18);
                dtTarih.Properties.ContextImageOptions.SvgImageSize = new Size(18, 18);
            }
            catch { }

            var tblTimes = CreateModernTimeGrid((t) => _secilenSaat = t);
            tblTimes.Dock = DockStyle.Fill;

            var btnCreate = new SimpleButton
            {
                Text = "RANDEVU OLUŞTUR",
                Cursor = Cursors.Hand,
                Height = 54,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            btnCreate.LookAndFeel.UseDefaultLookAndFeel = false;
            btnCreate.Appearance.BackColor = Color.FromArgb(93, 156, 236); // #5D9CEC
            btnCreate.Appearance.ForeColor = Color.White;
            btnCreate.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCreate.Appearance.Options.UseBackColor = true;
            btnCreate.Appearance.Options.UseForeColor = true;
            btnCreate.Appearance.Options.UseFont = true;
            btnCreate.AppearanceHovered.BackColor = Color.FromArgb(110, 168, 241);
            btnCreate.AppearanceHovered.Options.UseBackColor = true;
            btnCreate.AppearancePressed.BackColor = Color.FromArgb(75, 136, 218);
            btnCreate.AppearancePressed.Options.UseBackColor = true;
            btnCreate.Margin = new Padding(0, 10, 0, 0);
            btnCreate.Click += (s, e) =>
            {
                // Modern ekrandan randevu oluştur
                if (string.IsNullOrWhiteSpace(cmbBrans.Text) || string.IsNullOrWhiteSpace(cmbDoktor.Text) || dtTarih.EditValue == null || string.IsNullOrWhiteSpace(_secilenSaat))
                {
                    XtraMessageBox.Show("Lütfen Poliklinik, Doktor, Tarih ve Saat seçimini yapın!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using (var conn = SqlBaglantisi.Instance.GetConnection())
                    {
                        // DoktorTC linki (doktor panelinde filtrelemek için)
                        string doktorTc = null;
                        try
                        {
                            using (var cmdTc = new SqlCommand("SELECT TOP 1 DoktorTC FROM Tbl_Doktorlar WHERE (DoktorAd + ' ' + DoktorSoyad) = @adsoyad", conn))
                            {
                                cmdTc.Parameters.AddWithValue("@adsoyad", cmbDoktor.Text);
                                var v = cmdTc.ExecuteScalar();
                                doktorTc = v?.ToString();
                            }
                        }
                        catch { }

                        // Sekreter onayı için: RandevuDurum=0 (beklemede)
                        string q = @"INSERT INTO Tbl_Randevular
                                     (RandevuTarih, RandevuSaat, RandevuBrans, RandevuDoktor, RandevuDoktorTC, RandevuDurum, HastaTC)
                                     VALUES (@tarih, @saat, @brans, @doktor, @doktorTc, 0, @tc)";
                        using (var cmd = new SqlCommand(q, conn))
                        {
                            cmd.Parameters.AddWithValue("@tarih", Convert.ToDateTime(dtTarih.EditValue).Date);
                            cmd.Parameters.AddWithValue("@saat", TimeSpan.Parse(_secilenSaat));
                            cmd.Parameters.AddWithValue("@brans", cmbBrans.Text);
                            cmd.Parameters.AddWithValue("@doktor", cmbDoktor.Text);
                            cmd.Parameters.AddWithValue("@doktorTc", string.IsNullOrWhiteSpace(doktorTc) ? (object)DBNull.Value : doktorTc);
                            cmd.Parameters.AddWithValue("@tc", HastaTC);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    XtraMessageBox.Show(
                        $"✅ Randevu talebiniz alındı (Onay bekliyor).\n\n🏥 {cmbBrans.Text}\n👨‍⚕️ {cmbDoktor.Text}\n📅 {Convert.ToDateTime(dtTarih.EditValue):dd.MM.yyyy}\n🕐 {_secilenSaat}",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // UI temizle + ana sayfadaki yaklaşan randevuları yenile
                    cmbBrans.EditValue = null;
                    cmbDoktor.EditValue = null;
                    dtTarih.EditValue = null;
                    _secilenSaat = "";
                    LoadNextAppointment();
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Randevu kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { SqlBaglantisi.Instance.CloseConnection(); }
            };

            // Sol kolon (field container'lar)
            tbl.Controls.Add(fieldBrans);
            tbl.Controls.Add(fieldDoktor);
            tbl.Controls.Add(fieldTarih);
            tbl.SetColumn(fieldBrans, 0); tbl.SetRow(fieldBrans, 0);
            tbl.SetColumn(fieldDoktor, 0); tbl.SetRow(fieldDoktor, 1);
            tbl.SetColumn(fieldTarih, 0); tbl.SetRow(fieldTarih, 2);

            // Sağ kolon
            tbl.Controls.Add(tblTimes);
            tbl.SetColumn(tblTimes, 1);
            tbl.SetRow(tblTimes, 0);
            // Buton satırı (row 4) boş kalsın; saat grid'i onun üstüne binmesin
            tbl.SetRowSpan(tblTimes, 4);

            // CTA butonu: altta iki kolonu da kapsasın, daha dengeli dursun
            var btnHost = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 12, 0, 0)
            };
            btnHost.Appearance.BackColor = Color.Transparent;
            btnHost.Appearance.Options.UseBackColor = true;
            btnHost.Controls.Add(btnCreate);
            btnCreate.Size = new Size(420, 58);
            btnCreate.Anchor = AnchorStyles.None;
            void CenterBtn()
            {
                btnCreate.Location = new Point(
                    Math.Max(0, (btnHost.Width - btnCreate.Width) / 2),
                    Math.Max(0, (btnHost.Height - btnCreate.Height) / 2)
                );
            }
            btnHost.SizeChanged += (s, e) => CenterBtn();
            CenterBtn();
            tbl.Controls.Add(btnHost);
            tbl.SetColumn(btnHost, 0);
            tbl.SetRow(btnHost, 4);
            tbl.SetColumnSpan(btnHost, 2);

            // Poliklinik yazılmasın -> listeden seçilsin; seçilince doktorlar DB’den gelsin
            cmbBrans.SelectedIndexChanged += (s, e) =>
            {
                TryFillModernDoctors();
            };

            page.Controls.Add(card);
            return page;
        }

        private PanelControl WrapModernField(Control editor)
        {
            var field = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0, 6, 18, 6)
            };
            field.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            field.Appearance.Options.UseBackColor = true;

            // Border + radius (sürüm bağımsız)
            field.SizeChanged += (s, e) =>
            {
                if (field.Width <= 0 || field.Height <= 0) return;
                field.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, field.Width, field.Height, 16, 16));
                field.Invalidate();
            };
            bool focused = false;
            void setFocused(bool v) { focused = v; field.Invalidate(); }

            field.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = field.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                int radius = 16;
                int d = radius * 2;
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                var border = focused ? ColorTranslator.FromHtml("#2563EB") : ColorTranslator.FromHtml("#334155");
                using var pen = new Pen(border, focused ? 2f : 1.4f);
                e.Graphics.DrawPath(pen, path);
            };

            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0);
            editor.Enter += (s, e) => setFocused(true);
            editor.Leave += (s, e) => setFocused(false);
            field.Controls.Add(editor);
            return field;
        }

        private ComboBoxEdit CreateModernCombo(string prompt)
        {
            var c = new ComboBoxEdit
            {
                Height = 60
            };
            // Skin'in beyaza zorlamasını kır
            c.LookAndFeel.UseDefaultLookAndFeel = false;
            c.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            c.Properties.LookAndFeel.UseDefaultLookAndFeel = false;
            c.Properties.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;

            c.Properties.NullValuePrompt = prompt;
            c.Properties.NullValuePromptShowForEmptyValue = true;
            c.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor; // yazma yok, seçim var
            c.Properties.AutoHeight = false;
            // Köşeleri container belirlesin (daha temiz)
            c.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            // Modern dark input (card ile uyumlu)
            c.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            c.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            c.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            c.Properties.Appearance.BorderColor = ColorTranslator.FromHtml("#334155");
            c.Properties.Appearance.Options.UseBackColor = true;
            c.Properties.Appearance.Options.UseForeColor = true;
            c.Properties.Appearance.Options.UseFont = true;
            c.Properties.Appearance.Options.UseBorderColor = true;

            c.Properties.AppearanceFocused.BackColor = ColorTranslator.FromHtml("#111827");
            c.Properties.AppearanceFocused.BorderColor = ColorTranslator.FromHtml("#2563EB");
            c.Properties.AppearanceFocused.Options.UseBackColor = true;
            c.Properties.AppearanceFocused.Options.UseBorderColor = true;

            c.Properties.Padding = new Padding(16, 0, 12, 0);
            c.Properties.DropDownRows = 12;

            // DropDown görünümü (sade, modern)
            c.Properties.AppearanceDropDown.BackColor = ColorTranslator.FromHtml("#0F172A");
            c.Properties.AppearanceDropDown.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            c.Properties.AppearanceDropDown.Options.UseBackColor = true;
            c.Properties.AppearanceDropDown.Options.UseForeColor = true;
            return c;
        }

        private DateEdit CreateModernDate(string prompt)
        {
            var d = new DateEdit { Height = 60 };
            d.LookAndFeel.UseDefaultLookAndFeel = false;
            d.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            d.Properties.LookAndFeel.UseDefaultLookAndFeel = false;
            d.Properties.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;

            d.Properties.NullValuePrompt = prompt;
            d.Properties.NullValuePromptShowForEmptyValue = true;
            d.Properties.AutoHeight = false;
            // Köşeleri container belirlesin (daha temiz)
            d.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            d.Properties.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            d.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            d.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            d.Properties.Appearance.BorderColor = ColorTranslator.FromHtml("#334155");
            d.Properties.Appearance.Options.UseBackColor = true;
            d.Properties.Appearance.Options.UseForeColor = true;
            d.Properties.Appearance.Options.UseFont = true;
            d.Properties.Appearance.Options.UseBorderColor = true;

            d.Properties.AppearanceFocused.BackColor = ColorTranslator.FromHtml("#111827");
            d.Properties.AppearanceFocused.BorderColor = ColorTranslator.FromHtml("#2563EB");
            d.Properties.AppearanceFocused.Options.UseBackColor = true;
            d.Properties.AppearanceFocused.Options.UseBorderColor = true;

            d.Properties.Buttons.Clear();
            d.Properties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            d.Properties.CalendarTimeProperties.Buttons.Clear();
            d.Properties.CalendarTimeProperties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            d.Properties.Padding = new Padding(16, 0, 12, 0);
            return d;
        }

        private Control CreateModernTimeGrid(Action<string> onSelectedTime)
        {
            if (onSelectedTime == null) onSelectedTime = _ => { };
            var host = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(6, 6, 6, 0)
            };
            host.Appearance.BackColor = Color.Transparent;
            host.Appearance.Options.UseBackColor = true;

            var lbl = new LabelControl
            {
                Text = "Saat Seçimi",
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 26
            };
            lbl.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lbl.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
            lbl.Appearance.Options.UseFont = true;
            lbl.Appearance.Options.UseForeColor = true;
            host.Controls.Add(lbl);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0, 14, 0, 0) // "Saat Seçimi" ile butonlar arası boşluk
            };
            host.Controls.Add(flow);

            string[] times = { "09:00", "09:30", "10:00", "10:30", "11:00", "11:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00" };
            SimpleButton selected = null;

            Color baseBg = ColorTranslator.FromHtml("#0F172A");
            Color baseFg = ColorTranslator.FromHtml("#E5E7EB");
            Color baseBorder = ColorTranslator.FromHtml("#334155");
            Color hoverBg = ColorTranslator.FromHtml("#111E35");
            Color accent = ColorTranslator.FromHtml("#2563EB");
            Color accentBorder = ColorTranslator.FromHtml("#93C5FD");

            foreach (var t in times)
            {
                var btn = new SimpleButton
                {
                    Text = t,
                    Size = new Size(132, 52),
                    Margin = new Padding(10, 10, 10, 0),
                    Cursor = Cursors.Hand,
                    ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple
                };
                btn.LookAndFeel.UseDefaultLookAndFeel = false;
                btn.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;

                // hafif gradient + border ile daha "premium"
                btn.Appearance.BackColor = baseBg;
                btn.Appearance.BackColor2 = ColorTranslator.FromHtml("#111827");
                btn.Appearance.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
                btn.Appearance.ForeColor = baseFg;
                btn.Appearance.BorderColor = baseBorder;
                btn.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
                btn.Appearance.Options.UseBackColor = true;
                btn.Appearance.Options.UseForeColor = true;
                btn.Appearance.Options.UseBorderColor = true;
                btn.Appearance.Options.UseFont = true;

                btn.AppearanceHovered.BackColor = hoverBg;
                btn.AppearanceHovered.BorderColor = ColorTranslator.FromHtml("#475569");
                btn.AppearanceHovered.Options.UseBackColor = true;
                btn.AppearanceHovered.Options.UseBorderColor = true;

                btn.AppearancePressed.BackColor = accent;
                btn.AppearancePressed.BorderColor = accentBorder;
                btn.AppearancePressed.Options.UseBackColor = true;
                btn.AppearancePressed.Options.UseBorderColor = true;

                btn.SizeChanged += (s, e) => ApplyPillRegion(btn);
                ApplyPillRegion(btn); // ilk render'da da pill olsun

                btn.Click += (s, e) =>
                {
                    selected = btn;
                    foreach (Control child in flow.Controls)
                    {
                        if (child is SimpleButton b)
                        {
                            bool isSel = ReferenceEquals(b, selected);
                            b.Appearance.BackColor = isSel ? accent : baseBg;
                            b.Appearance.BackColor2 = isSel ? ColorTranslator.FromHtml("#1D4ED8") : ColorTranslator.FromHtml("#111827");
                            b.Appearance.ForeColor = isSel ? Color.White : baseFg;
                            b.Appearance.BorderColor = isSel ? accentBorder : baseBorder;
                        }
                    }
                    onSelectedTime(t);
                };

                flow.Controls.Add(btn);
            }

            return host;
        }

        private void TryFillModernDoctors()
        {
            try
            {
                if (_modernCmbBrans == null || _modernCmbDoktor == null) return;
                var brans = _modernCmbBrans.Text;
                if (string.IsNullOrWhiteSpace(brans)) return;

                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string query = "SELECT DoktorAd, DoktorSoyad FROM Tbl_Doktorlar WHERE DoktorBrans = @brans";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@brans", brans);
                        using (var dr = cmd.ExecuteReader())
                        {
                            _modernCmbDoktor.Properties.Items.Clear();
                            _modernCmbDoktor.EditValue = null;
                            while (dr.Read())
                            {
                                _modernCmbDoktor.Properties.Items.Add(dr["DoktorAd"] + " " + dr["DoktorSoyad"]);
                            }
                        }
                    }
                }
            }
            catch
            {
                // UI crash etmesin
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void ApplyPillRegion(SimpleButton btn)
        {
            if (btn.Width <= 0 || btn.Height <= 0) return;
            int radius = Math.Max(1, btn.Height / 2);
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, radius, radius));
        }

        private void LoadNextAppointment()
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string query = @"SELECT TOP 5 RandevuTarih, RandevuSaat, RandevuBrans, RandevuDoktor 
                                   FROM Tbl_Randevular 
                                   WHERE HastaTC = @tc AND RandevuTarih >= CAST(GETDATE() AS DATE)
                                   ORDER BY RandevuTarih ASC, RandevuSaat ASC";
                    
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        using (var dr = cmd.ExecuteReader())
                        {
                            var lines = new System.Collections.Generic.List<string>();
                            var items = new System.Collections.Generic.List<(DateTime tarih, TimeSpan saat, string brans, string doktor)>();
                            while (dr.Read())
                            {
                                DateTime tarih = Convert.ToDateTime(dr["RandevuTarih"]);
                                TimeSpan saat = (TimeSpan)dr["RandevuSaat"];
                                string brans = dr["RandevuBrans"]?.ToString() ?? "";
                                string doktor = dr["RandevuDoktor"]?.ToString() ?? "";
                                lines.Add($"📅 {tarih:dd.MM.yyyy}  🕐 {saat:hh\\:mm}   🏥 {brans}   👨‍⚕️ {doktor}");
                                items.Add((tarih, saat, brans, doktor));
                            }

                            string text = lines.Count > 0
                                ? string.Join("\n\n", lines)
                                : "Henüz yaklaşan bir randevunuz bulunmuyor.";

                            if (lblNextApptInfo != null)
                            {
                                lblNextApptInfo.Text = text;
                                lblNextApptInfo.ForeColor = lines.Count > 0 ? Color.FromArgb(37, 99, 235) : Color.Gray;
                            }
                            if (_lblHomeNextAppt != null)
                            {
                                _lblHomeNextAppt.Text = text;
                                _lblHomeNextAppt.ForeColor = lines.Count > 0 ? ColorTranslator.FromHtml("#B0BEC5") : ColorTranslator.FromHtml("#94A3B8");
                            }

                            // Modern kart listesi (hasta ana sayfa)
                            try
                            {
                                if (_homeApptList != null)
                                {
                                    _homeApptList.SuspendLayout();
                                    _homeApptList.Controls.Clear();
                                    if (items.Count == 0)
                                    {
                                        _homeApptList.Visible = false;
                                        if (_lblHomeNextAppt != null) _lblHomeNextAppt.Visible = true;
                                    }
                                    else
                                    {
                                        _homeApptList.Visible = true;
                                        if (_lblHomeNextAppt != null) _lblHomeNextAppt.Visible = false;

                                        foreach (var it in items)
                                        {
                                            var chip = new PanelControl
                                            {
                                                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                                                Size = new Size(520, 82),
                                                Margin = new Padding(10, 10, 10, 0),
                                                Padding = new Padding(14, 12, 14, 12)
                                            };
                                            chip.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
                                            chip.Appearance.Options.UseBackColor = true;
                                            chip.SizeChanged += (s, e) =>
                                            {
                                                if (chip.Width <= 0 || chip.Height <= 0) return;
                                                chip.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, chip.Width, chip.Height, 18, 18));
                                            };
                                            chip.Paint += (s, e) =>
                                            {
                                                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                                var rect = chip.ClientRectangle;
                                                rect.Width -= 1;
                                                rect.Height -= 1;
                                                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                                                int radius = 18;
                                                int d = radius * 2;
                                                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                                                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                                                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                                                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                                                path.CloseFigure();
                                                using var pen = new Pen(ColorTranslator.FromHtml("#334155"), 1.4f);
                                                e.Graphics.DrawPath(pen, path);
                                                using var accent = new Pen(ColorTranslator.FromHtml("#2563EB"), 3f);
                                                e.Graphics.DrawLine(accent, 8, 10, 8, rect.Bottom - 10);
                                            };

                                            var lblTime = new LabelControl
                                            {
                                                Text = it.saat.ToString(@"hh\:mm"),
                                                AutoSizeMode = LabelAutoSizeMode.None,
                                                Location = new Point(18, 10),
                                                Size = new Size(70, 24)
                                            };
                                            lblTime.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
                                            lblTime.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
                                            lblTime.Appearance.Options.UseFont = true;
                                            lblTime.Appearance.Options.UseForeColor = true;

                                            var lblMain = new LabelControl
                                            {
                                                Text = $"{it.tarih:dd.MM.yyyy}   •   {it.doktor}",
                                                AutoSizeMode = LabelAutoSizeMode.None,
                                                Location = new Point(100, 12),
                                                Size = new Size(390, 22)
                                            };
                                            lblMain.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                                            lblMain.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
                                            lblMain.Appearance.Options.UseFont = true;
                                            lblMain.Appearance.Options.UseForeColor = true;

                                            var lblSub = new LabelControl
                                            {
                                                Text = it.brans,
                                                AutoSizeMode = LabelAutoSizeMode.None,
                                                Location = new Point(100, 40),
                                                Size = new Size(390, 20)
                                            };
                                            lblSub.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                                            lblSub.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
                                            lblSub.Appearance.Options.UseFont = true;
                                            lblSub.Appearance.Options.UseForeColor = true;

                                            chip.Controls.Add(lblTime);
                                            chip.Controls.Add(lblMain);
                                            chip.Controls.Add(lblSub);
                                            _homeApptList.Controls.Add(chip);
                                        }
                                    }
                                    _homeApptList.ResumeLayout();
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (lblNextApptInfo != null) lblNextApptInfo.Text = "Randevu bilgisi yüklenemedi.";
                if (_lblHomeNextAppt != null) _lblHomeNextAppt.Text = "Randevu bilgisi yüklenemedi.";
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void ApplyModernStyles()
        {
            // ===== TEMİZ SOL MENÜ (TEK AccordionControl) =====
            // Dock / ölçü
            accordionControl1.Dock = DockStyle.Left;
            accordionControl1.Width = 290;
            accordionControl1.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            accordionControl1.LookAndFeel.UseDefaultLookAndFeel = false;

            Color bg = ColorTranslator.FromHtml("#111827");         // koyu modern (slate/ink)
            Color fg = Color.White;
            Color hover = ColorTranslator.FromHtml("#2563EB");      // vurgu mavi
            Color itemBg = ColorTranslator.FromHtml("#0F172A");     // menü item base
            Color itemPressed = ColorTranslator.FromHtml("#1D4ED8");// pressed

            accordionControl1.Appearance.AccordionControl.BackColor = bg;
            accordionControl1.Appearance.AccordionControl.Options.UseBackColor = true;

            // ViewType (HamburgerMenu/Standard) - sürüm farklarına karşı reflection
            TrySetAccordionViewType(accordionControl1, "HamburgerMenu");
            TrySetAccordionViewType(accordionControl1, "Standard");

            // Item stilleri (sürümde varsa)
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Normal", "ForeColor" }, fg);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Hovered", "BackColor" }, hover);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Hovered", "ForeColor" }, Color.White);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Normal", "BackColor" }, itemBg);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Normal", "ForeColor" }, fg);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Pressed", "BackColor" }, itemPressed);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Pressed", "ForeColor" }, fg);

            // Fallback
            accordionControl1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            accordionControl1.ForeColor = fg;
            var elementHeightProp = accordionControl1.GetType().GetProperty("ElementHeight");
            if (elementHeightProp != null && elementHeightProp.CanWrite)
                elementHeightProp.SetValue(accordionControl1, 70, null);

            EnsureAnaSayfaElement(itemBg, fg, hover, itemPressed);

            // Elementleri tek tek stillendir (bazı DevExpress sürümlerinde global Appearance yetmeyebiliyor)
            StyleAccordionElement(navDashboard, itemBg, fg, hover, Color.White, itemPressed);
            StyleAccordionElement(navRandevuAl, itemBg, fg, hover, Color.White, itemPressed);
            StyleAccordionElement(navTahliller, itemBg, fg, hover, Color.White, itemPressed);
            StyleAccordionElement(navReceteler, itemBg, fg, hover, Color.White, itemPressed);
            StyleAccordionElement(navHealthInfo, itemBg, fg, ColorTranslator.FromHtml("#EF4444"), Color.White, ColorTranslator.FromHtml("#DC2626"));

            // Menü fontunu biraz büyüt (metin kaymasın diye AutoSize yerine fixed item height kullanıyoruz)
            var menuFont = new Font("Segoe UI Semibold", 12F);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Normal", "Font" }, menuFont);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Hovered", "Font" }, menuFont);
            TrySetNestedProperty(accordionControl1, new[] { "Appearance", "Item", "Pressed", "Font" }, menuFont);
            accordionControl1.Font = menuFont;

            // Menü metinleri (4 ana kategori)
            navDashboard.Text = "Randevu İşlemleri";
            navRandevuAl.Text = "Tıbbi Bilgiler ve Sonuçlar";
            navTahliller.Text = "Profil ve Kişisel Bilgiler";
            navReceteler.Text = "Yapay Zeka Asistanı";
            navHealthInfo.Text = "Çıkış Yap";

            // Eski click handler'ları söküp yenilerini bağla
            navDashboard.Click -= navDashboard_Click;
            navDashboard.Click += navRandevuIslemleri_Click;
            navRandevuAl.Click -= navRandevuAl_Click;
            navRandevuAl.Click += navTibbiBilgiler_Click;
            navTahliller.Click -= navTahliller_Click;
            navTahliller.Click += navProfil_Click;
            navReceteler.Click -= navReceteler_Click;
            navReceteler.Click += navIletisim_Click;

            // Çıkış click
            navHealthInfo.Click -= btnEditHealth_Click;
            navHealthInfo.Click += navLogout_Click;

            // İkonlar (DevExpress SvgImages)
            var assembly = typeof(DevExpress.Images.ImageResourceCache).Assembly;
            navDashboard.ImageOptions.SvgImage = TryLoadSvg(assembly, "DevExpress.Images.SvgImages.Scheduling.NewAppointment.svg");
            navRandevuAl.ImageOptions.SvgImage = TryLoadSvg(assembly, "DevExpress.Images.SvgImages.Icon Builder.Travel_Medical.svg");
            navTahliller.ImageOptions.SvgImage = TryLoadSvg(assembly, "DevExpress.Images.SvgImages.Business.Business_User.svg");
            navReceteler.ImageOptions.SvgImage = TryLoadSvg(assembly,
                "DevExpress.Images.SvgImages.Medical.Lab.svg",
                "DevExpress.Images.SvgImages.Medical.MedicalRecord.svg",
                "DevExpress.Images.SvgImages.Mail.Mail.svg");
            navHealthInfo.ImageOptions.SvgImage = TryLoadSvg(assembly,
                "DevExpress.Images.SvgImages.Actions.Exit.svg",
                "DevExpress.Images.SvgImages.Actions.Close.svg",
                "DevExpress.Images.SvgImages.Actions.Cancel.svg");

            // Çıkış Yap: kırmızı ikon/hover rengi (destek varsa)
            TrySetNestedProperty(navHealthInfo, new[] { "Appearance", "Hovered", "BackColor" }, ColorTranslator.FromHtml("#EF4444"));
            TrySetNestedProperty(navHealthInfo, new[] { "Appearance", "Hovered", "ForeColor" }, Color.White);

            // Sağ taraf genel arka plan: beyaz yerine yumuşak açık ton
            fluentDesignFormContainer1.Appearance.BackColor = ColorTranslator.FromHtml("#F1F5F9");
            fluentDesignFormContainer1.Appearance.Options.UseBackColor = true;

            // Üst/alt gri-beyaz şeritleri kır: form + fluent bar rengi menüyle aynı olsun
            this.BackColor = bg;
            this.Appearance.BackColor = bg;
            this.Appearance.Options.UseBackColor = true;

            // FluentDesignFormControl bazı sürümlerde style edilemiyor ve üstte gri şerit bırakıyor.
            // En güvenli çözüm: bar'ı kapatıp container'ı yukarı taşımak.
            try
            {
                fluentDesignFormControl1.Dock = DockStyle.None;
                fluentDesignFormControl1.Height = 0;
                fluentDesignFormControl1.Visible = false;
            }
            catch { }

            // Menü genişliği değişince container bazı durumlarda eski X (260) ile kalabiliyor.
            // Kesin çözüm: container'ı menü genişliğine göre manuel konumla (resize'da da güncelle).
            void LayoutMain()
            {
                int leftW = accordionControl1.Width;
                fluentDesignFormContainer1.Dock = DockStyle.None;
                fluentDesignFormContainer1.Location = new Point(leftW, 0);
                fluentDesignFormContainer1.Size = new Size(Math.Max(0, ClientSize.Width - leftW), ClientSize.Height);
                fluentDesignFormContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            }
            LayoutMain();
            this.Resize -= FrmHastaPanel_ResizeLayoutMain;
            this.Resize += FrmHastaPanel_ResizeLayoutMain;

            void FrmHastaPanel_ResizeLayoutMain(object sender, EventArgs e) => LayoutMain();

            // Beyaz kartları kır (tam beyaz yerine modern "off-white")
            var cardBg = ColorTranslator.FromHtml("#F8FAFC");
            pnlHealthCard.Appearance.BackColor = cardBg;
            pnlHealthCard.Appearance.Options.UseBackColor = true;
            pnlNextAppointment.Appearance.BackColor = cardBg;
            pnlNextAppointment.Appearance.Options.UseBackColor = true;
            pnlQuickActions.Appearance.BackColor = cardBg;
            pnlQuickActions.Appearance.Options.UseBackColor = true;
            pnlHealthUpdateCard.Appearance.BackColor = cardBg;
            pnlHealthUpdateCard.Appearance.Options.UseBackColor = true;

            // Ana kartları yuvarlatma
            pnlRandevuCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlRandevuCard.Width, pnlRandevuCard.Height, 30, 30));
            pnlTahlillerCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlTahlillerCard.Width, pnlTahlillerCard.Height, 30, 30));
            pnlRecetelerCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlRecetelerCard.Width, pnlRecetelerCard.Height, 30, 30));
            pnlNextAppointment.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlNextAppointment.Width, pnlNextAppointment.Height, 30, 30));
            pnlQuickActions.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlQuickActions.Width, pnlQuickActions.Height, 30, 30));
            pnlHealthCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlHealthCard.Width, pnlHealthCard.Height, 30, 30));
            pnlHealthUpdateCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlHealthUpdateCard.Width, pnlHealthUpdateCard.Height, 30, 30));
            pnlBloodType.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlBloodType.Width, pnlBloodType.Height, 15, 15));
            lblAllergyValue.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblAllergyValue.Width, lblAllergyValue.Height, 10, 10));
            lblChronicValue.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblChronicValue.Width, lblChronicValue.Height, 10, 10));

            // Hızlı İşlem Butonlarını Yuvarlat
            btnQuickRandevu.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnQuickRandevu.Width, btnQuickRandevu.Height, 20, 20));
            btnQuickTahlil.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnQuickTahlil.Width, btnQuickTahlil.Height, 20, 20));
            btnQuickRecete.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnQuickRecete.Width, btnQuickRecete.Height, 20, 20));
            btnEditHealth.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnEditHealth.Width, btnEditHealth.Height, 10, 10));
            btnHealthSave.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnHealthSave.Width, btnHealthSave.Height, 25, 25));

            // Memo Editleri yuvarlat
            txtHealthAllergies.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtHealthAllergies.Width, txtHealthAllergies.Height, 15, 15));
            txtHealthChronic.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtHealthChronic.Width, txtHealthChronic.Height, 15, 15));
            cmbHealthBloodType.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, cmbHealthBloodType.Width, cmbHealthBloodType.Height, 10, 10));
            
            // ComboBox'ları yuvarlat
            cmbBrans.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, cmbBrans.Width, cmbBrans.Height, 15, 15));
            cmbDoktor.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, cmbDoktor.Width, cmbDoktor.Height, 15, 15));
            
            // Tarih seçici
            dtTarih.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, dtTarih.Width, dtTarih.Height, 15, 15));
            
            // Buton yuvarlatma
            btnRandevuAl.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnRandevuAl.Width, btnRandevuAl.Height, 25, 25));

            // Modern Title Styling
            lblRandevuTitle.Font = new Font("Segoe UI Light", 24F);
            lblRandevuTitle.ForeColor = Color.FromArgb(30, 41, 59);
            lblRandevuTitle.AutoSizeMode = LabelAutoSizeMode.None;
            lblRandevuTitle.Size = new Size(pnlRandevuCard.Width, 50);
            lblRandevuTitle.Location = new Point(0, 40);
            lblRandevuTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblRandevuTitle.Text = "Randevu Alın";

            // Add separator paint logic to panel
            pnlRandevuCard.Paint += (s, e) => {
                using (Pen pen = new Pen(Color.FromArgb(37, 99, 235), 2)) {
                    int centerX = pnlRandevuCard.Width / 2;
                    e.Graphics.DrawLine(pen, centerX - 40, 95, centerX + 40, 95);
                }
            };

            // Saat butonlarını oluştur
            GenerateTimeSlots();
            
            // Tahlil verilerini yükle
            LoadTahliller();
        }

        private void StyleAccordionElement(object element,
            Color normalBg, Color normalFg,
            Color hoverBg, Color hoverFg,
            Color pressedBg)
        {
            // AccordionControlElement'in Appearance zinciri sürüme göre değişebiliyor.
            // Reflection ile güvenli şekilde set ederek beyaz arka plan sorununu kırıyoruz.

            TrySetNestedProperty(element, new[] { "Appearance", "Normal", "BackColor" }, normalBg);
            TrySetNestedProperty(element, new[] { "Appearance", "Normal", "ForeColor" }, normalFg);
            TrySetNestedProperty(element, new[] { "Appearance", "Normal", "Options", "UseBackColor" }, true);
            TrySetNestedProperty(element, new[] { "Appearance", "Normal", "Options", "UseForeColor" }, true);

            TrySetNestedProperty(element, new[] { "Appearance", "Hovered", "BackColor" }, hoverBg);
            TrySetNestedProperty(element, new[] { "Appearance", "Hovered", "ForeColor" }, hoverFg);
            TrySetNestedProperty(element, new[] { "Appearance", "Hovered", "Options", "UseBackColor" }, true);
            TrySetNestedProperty(element, new[] { "Appearance", "Hovered", "Options", "UseForeColor" }, true);

            TrySetNestedProperty(element, new[] { "Appearance", "Pressed", "BackColor" }, pressedBg);
            TrySetNestedProperty(element, new[] { "Appearance", "Pressed", "ForeColor" }, hoverFg);
            TrySetNestedProperty(element, new[] { "Appearance", "Pressed", "Options", "UseBackColor" }, true);
            TrySetNestedProperty(element, new[] { "Appearance", "Pressed", "Options", "UseForeColor" }, true);

            // Element boyutu (varsa)
            var heightProp = element.GetType().GetProperty("Height");
            if (heightProp != null && heightProp.CanWrite)
            {
                try { heightProp.SetValue(element, 70, null); } catch { }
            }
        }

        private void navRandevuIslemleri_Click(object sender, EventArgs e)
        {
            EnsureSidebarPages();
            navigationFrame1.SelectedPage = _pageRandevuModern;
        }

        private void navAnaSayfa_Click(object sender, EventArgs e)
        {
            EnsureSidebarPages();
            navigationFrame1.SelectedPage = _pageAnaSayfa ?? pageDashboard;
        }

        private void EnsureAnaSayfaElement(Color itemBg, Color fg, Color hover, Color itemPressed)
        {
            if (_navAnaSayfa != null) return;

            _navAnaSayfa = new DevExpress.XtraBars.Navigation.AccordionControlElement
            {
                Name = "navAnaSayfa",
                Style = DevExpress.XtraBars.Navigation.ElementStyle.Item,
                Text = "Ana Sayfa"
            };

            // ikon (opsiyonel)
            try
            {
                var assembly = typeof(DevExpress.Images.ImageResourceCache).Assembly;
                _navAnaSayfa.ImageOptions.SvgImage = TryLoadSvg(assembly,
                    "DevExpress.Images.SvgImages.Dashboards.Dashboard.svg",
                    "DevExpress.Images.SvgImages.Dashboards.Dashboard2.svg",
                    "DevExpress.Images.SvgImages.Navigation.Home.svg");
            }
            catch { }

            StyleAccordionElement(_navAnaSayfa, itemBg, fg, hover, Color.White, itemPressed);
            _navAnaSayfa.Click += navAnaSayfa_Click;

            // En üste ekle
            try
            {
                accordionControl1.Elements.Insert(0, _navAnaSayfa);
            }
            catch
            {
                accordionControl1.Elements.Add(_navAnaSayfa);
            }
        }

        private void navTibbiBilgiler_Click(object sender, EventArgs e)
        {
            EnsureSidebarPages();
            navigationFrame1.SelectedPage = _pageTibbiBilgiler;
            LoadMedicalPageData();
        }

        private void navProfil_Click(object sender, EventArgs e)
        {
            EnsureSidebarPages();
            navigationFrame1.SelectedPage = _pageProfil;
            LoadProfilePageData();
        }

        private void navIletisim_Click(object sender, EventArgs e)
        {
            EnsureSidebarPages();
            navigationFrame1.SelectedPage = _pageIletisim;
        }

        private void navLogout_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private static DevExpress.Utils.Svg.SvgImage TryLoadSvg(System.Reflection.Assembly assembly, params string[] resourceNames)
        {
            foreach (var res in resourceNames)
            {
                try
                {
                    var svg = DevExpress.Utils.Svg.SvgImage.FromResources(res, assembly);
                    if (svg != null) return svg;
                }
                catch { }
            }
            return null;
        }

        private static void TrySetAccordionViewType(object accordionControl, string desiredEnumName)
        {
            var prop = accordionControl.GetType().GetProperty("ViewType");
            if (prop == null || !prop.CanWrite) return;
            var enumType = prop.PropertyType;
            if (!enumType.IsEnum) return;
            try
            {
                var value = Enum.Parse(enumType, desiredEnumName, ignoreCase: true);
                prop.SetValue(accordionControl, value, null);
            }
            catch { }
        }

        private static void TrySetNestedProperty(object root, string[] path, object value)
        {
            // DevExpress sürüm/tema farklarında bazı nested Appearance objeleri null dönebiliyor.
            // UI asla bu yüzden crash olmamalı; burada tamamen "fail-silent" davranıyoruz.
            if (root == null || path == null || path.Length == 0) return;
            try
            {
                object current = root;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (current == null) return;
                    var p = current.GetType().GetProperty(path[i]);
                    if (p == null) return;
                    current = p.GetValue(current, null);
                }

                if (current == null) return;
                var leaf = current.GetType().GetProperty(path[^1]);
                if (leaf == null || !leaf.CanWrite) return;
                leaf.SetValue(current, value, null);
            }
            catch
            {
                // ignore
            }
        }

        private void GenerateTimeSlots()
        {
            flowSaatler.Controls.Clear();
            string[] saatler = { "09:00", "09:30", "10:00", "10:30", "11:00", "11:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00" };

            foreach (var saat in saatler)
            {
                SimpleButton btn = new SimpleButton();
                btn.Text = saat;
                btn.Size = new Size(95, 45);
                btn.Margin = new Padding(5);
                btn.Appearance.Font = new Font("Segoe UI Semibold", 11F);
                btn.Appearance.BackColor = Color.FromArgb(147, 197, 253); // Açık mavi
                btn.Appearance.ForeColor = Color.White;
                btn.Appearance.Options.UseBackColor = true;
                btn.Appearance.Options.UseFont = true;
                btn.Appearance.Options.UseForeColor = true;
                btn.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
                btn.LookAndFeel.UseDefaultLookAndFeel = false;
                btn.Cursor = Cursors.Hand;
                
                // Yuvarlatma
                btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 12, 12));

                btn.Click += (s, e) => {
                    // Önceki seçimi temizle
                    foreach (SimpleButton b in flowSaatler.Controls)
                    {
                        b.Appearance.BackColor = Color.FromArgb(147, 197, 253); // Açık mavi
                        b.Appearance.ForeColor = Color.White;
                    }
                    // Yeni seçimi vurgula
                    btn.Appearance.BackColor = Color.FromArgb(59, 130, 246); // Koyu mavi
                    btn.Appearance.ForeColor = Color.White;
                    _secilenSaat = btn.Text;
                };

                flowSaatler.Controls.Add(btn);
            }
        }

        private void LoadHastaBilgileri()
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string query = "SELECT HastaAd, HastaSoyad, HastaKanGrubu, HastaAlerjiler, HastaHastaliklar FROM Tbl_Hastalar WHERE HastaTC = @tc";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                var fullName = $"{dr["HastaAd"]} {dr["HastaSoyad"]}";
                                lblWelcome.Text = $"Hoş Geldiniz, {fullName}";
                                this.Text = $"Hasta Paneli - {fullName}";
                                if (_lblHomeWelcome != null)
                                {
                                    _lblHomeWelcome.Text = $"<color=#5D9CEC>Hoş Geldiniz,</color> <color=#ECEFF1>{fullName}</color>";
                                }

                                // Sağlık Bilgileri
                                lblBloodType.Text = dr["HastaKanGrubu"]?.ToString() ?? "N/A";
                                lblAllergyValue.Text = dr["HastaAlerjiler"]?.ToString() ?? "Yok";
                                lblChronicValue.Text = dr["HastaHastaliklar"]?.ToString() ?? "Yok";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void LoadBranslar()
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string query = "SELECT BransAd FROM Tbl_Branslar";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            cmbBrans.Items.Clear();
                            if (_modernCmbBrans != null)
                            {
                                _modernCmbBrans.Properties.Items.Clear();
                                _modernCmbBrans.EditValue = null;
                            }
                            while (dr.Read())
                            {
                                var b = dr["BransAd"].ToString();
                                cmbBrans.Items.Add(b);
                                if (_modernCmbBrans != null) _modernCmbBrans.Properties.Items.Add(b);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void cmbBrans_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string query = "SELECT DoktorAd, DoktorSoyad FROM Tbl_Doktorlar WHERE DoktorBrans = @brans";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@brans", cmbBrans.Text);
                        using (var dr = cmd.ExecuteReader())
                        {
                            cmbDoktor.Items.Clear();
                            cmbDoktor.SelectedItem = null;
                            while (dr.Read())
                            {
                                cmbDoktor.Items.Add(dr["DoktorAd"] + " " + dr["DoktorSoyad"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void navRandevuAl_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageRandevuAl;
        }

        private void navDashboard_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageDashboard;
        }

        private void navTahliller_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageTahliller;
            LoadTahliller(); // Sayfa açıldığında verileri yenile
        }

        private void navReceteler_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageReceteler;
            LoadReceteler();
        }

        private void btnEditHealth_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageHealthUpdate;
            // Mevcut verileri kutulara doldur
            cmbHealthBloodType.Text = lblBloodType.Text != "N/A" ? lblBloodType.Text : "";
            txtHealthAllergies.Text = lblAllergyValue.Text != "Yok" ? lblAllergyValue.Text : "";
            txtHealthChronic.Text = lblChronicValue.Text != "Yok" ? lblChronicValue.Text : "";
        }

        private void btnHealthSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string query = @"UPDATE Tbl_Hastalar SET 
                                   HastaKanGrubu = @kan, 
                                   HastaAlerjiler = @alerji, 
                                   HastaHastaliklar = @hastalik 
                                   WHERE HastaTC = @tc";
                    
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@kan", cmbHealthBloodType.Text);
                        cmd.Parameters.AddWithValue("@alerji", txtHealthAllergies.Text);
                        cmd.Parameters.AddWithValue("@hastalik", txtHealthChronic.Text);
                        cmd.Parameters.AddWithValue("@tc", HastaTC);

                        cmd.ExecuteNonQuery();
                        XtraMessageBox.Show("Sağlık bilgileriniz başarıyla güncellendi! ✅", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Dashboard'u yenile ve oraya dön
                        LoadHastaBilgileri();
                        navigationFrame1.SelectedPage = pageDashboard;
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Güncelleme sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void LoadReceteler()
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string query = "SELECT ReceteKod, ReceteTarih, DoktorAd, Ilaclar FROM Tbl_Receteler WHERE HastaTC = @tc ORDER BY ReceteTarih DESC";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        
                        System.Data.DataTable dt = new System.Data.DataTable();
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                        
                        gridReceteler.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Reçeteler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally 
            { 
                SqlBaglantisi.Instance.CloseConnection(); 
            }
        }

        private void LoadTahliller()
        {
            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string query = "SELECT TahlilAd, TahlilTarih, DoktorAd, TahlilSonuc, TahlilDurum FROM Tbl_Tahliller WHERE HastaTC = @tc ORDER BY TahlilTarih DESC";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", HastaTC);
                        
                        System.Data.DataTable dt = new System.Data.DataTable();
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                        
                        gridTahliller.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Tahliller yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally 
            { 
                SqlBaglantisi.Instance.CloseConnection(); 
            }
        }

        private void btnRandevuAl_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbBrans.Text) || string.IsNullOrEmpty(cmbDoktor.Text) || dtTarih.EditValue == null || string.IsNullOrEmpty(_secilenSaat))
            {
                XtraMessageBox.Show("Lütfen Branş, Doktor, Tarih ve Saat seçimini yapın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    // Randevuyu veritabanına kaydet
                    // DoktorTC linki (doktor panelinde filtrelemek için)
                    string doktorTc = null;
                    try
                    {
                        using (var cmdTc = new SqlCommand("SELECT TOP 1 DoktorTC FROM Tbl_Doktorlar WHERE (DoktorAd + ' ' + DoktorSoyad) = @adsoyad", conn))
                        {
                            cmdTc.Parameters.AddWithValue("@adsoyad", cmbDoktor.Text);
                            var v = cmdTc.ExecuteScalar();
                            doktorTc = v?.ToString();
                        }
                    }
                    catch { }

                    // Sekreter onayı için: RandevuDurum=0 (beklemede)
                    string query = @"INSERT INTO Tbl_Randevular (RandevuTarih, RandevuSaat, RandevuBrans, RandevuDoktor, RandevuDoktorTC, RandevuDurum, HastaTC) 
                                   VALUES (@tarih, @saat, @brans, @doktor, @doktorTc, 0, @tc)";
                    
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tarih", Convert.ToDateTime(dtTarih.EditValue).Date);
                        cmd.Parameters.AddWithValue("@saat", TimeSpan.Parse(_secilenSaat));
                        cmd.Parameters.AddWithValue("@brans", cmbBrans.Text);
                        cmd.Parameters.AddWithValue("@doktor", cmbDoktor.Text);
                        cmd.Parameters.AddWithValue("@doktorTc", string.IsNullOrWhiteSpace(doktorTc) ? (object)DBNull.Value : doktorTc);
                        cmd.Parameters.AddWithValue("@tc", HastaTC);

                        cmd.ExecuteNonQuery();
                        XtraMessageBox.Show($"✅ Randevu talebiniz alındı (Onay bekliyor).\n\n📋 Branş: {cmbBrans.Text}\n👨‍⚕️ Doktor: {cmbDoktor.Text}\n📅 Tarih: {dtTarih.Text}\n🕐 Saat: {_secilenSaat}", 
                                          "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Formu temizle
                        cmbBrans.EditValue = null;
                        cmbDoktor.EditValue = null;
                        dtTarih.EditValue = null;
                        _secilenSaat = "";
                        GenerateTimeSlots(); // Saat butonlarını yenile
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Randevu kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally 
            { 
                SqlBaglantisi.Instance.CloseConnection(); 
            }
        }
    }
}
