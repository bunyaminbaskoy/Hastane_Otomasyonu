using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.Utils.Layout;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;

namespace Hastane_Otomasyonu
{
    public class FrmRandevuModern : XtraForm
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private readonly PanelControl _card;
        private readonly SimpleButton _btnClose;
        private readonly SimpleButton _btnCreate;

        private readonly ComboBoxEdit _cmbBranch;
        private readonly ComboBoxEdit _cmbDoctor;
        private readonly DateEdit _dtDate;
        private readonly MemoEdit _txtNote;
        private readonly TablePanel _tblTimes;

        private string _selectedTime = string.Empty;

        public string HastaTC { get; set; } = "";

        public FrmRandevuModern()
        {
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(980, 620);
            Text = "Randevu İşlemleri";

            // Rounded corners
            Load += (_, _) =>
            {
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 36, 36));
                _card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _card.Width, _card.Height, 28, 28));
            };

            // Root background (dark gradient) is painted in OnPaint.
            BackColor = Hex("#0F172A");

            _card = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Size = new Size(860, 520),
                Location = new Point((ClientSize.Width - 860) / 2, (ClientSize.Height - 520) / 2),
            };
            _card.Appearance.BackColor = Hex("#1E2A38");
            _card.Appearance.Options.UseBackColor = true;
            _card.Paint += Card_Paint;
            Controls.Add(_card);

            // Close
            _btnClose = new SimpleButton
            {
                Text = "✕",
                Cursor = Cursors.Hand,
                Size = new Size(36, 36),
                Location = new Point(_card.Width - 50, 14),
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
            };
            _btnClose.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnClose.Appearance.BackColor = Color.Transparent;
            _btnClose.Appearance.ForeColor = Hex("#B0BEC5");
            _btnClose.Appearance.Options.UseBackColor = true;
            _btnClose.Appearance.Options.UseForeColor = true;
            _btnClose.AppearanceHovered.ForeColor = Color.White;
            _btnClose.AppearanceHovered.Options.UseForeColor = true;
            _btnClose.Click += (_, _) => Close();
            _card.Controls.Add(_btnClose);

            // Title / subtitle
            var lblTitle = new LabelControl
            {
                AllowHtmlString = true,
                Location = new Point(36, 26),
                Text = "<color=#5D9CEC>Randevu</color> <color=#ECEFF1>Al</color>",
            };
            lblTitle.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Hex("#ECEFF1");
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            _card.Controls.Add(lblTitle);

            var lblSub = new LabelControl
            {
                Location = new Point(38, 82),
                Text = "Poliklinik, doktor, tarih ve saat seçerek randevu oluşturun.",
            };
            lblSub.Appearance.Font = new Font("Segoe UI", 11F);
            lblSub.Appearance.ForeColor = Hex("#B0BEC5");
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Appearance.Options.UseForeColor = true;
            _card.Controls.Add(lblSub);

            var sep = new SeparatorControl
            {
                Location = new Point(36, 112),
                Size = new Size(_card.Width - 72, 18)
            };
            sep.LineColor = Color.FromArgb(70, 255, 255, 255);
            _card.Controls.Add(sep);

            // Layout area
            var layout = new LayoutControl
            {
                Location = new Point(36, 140),
                Size = new Size(_card.Width - 72, _card.Height - 200),
                AllowCustomization = false,
                BackColor = Color.Transparent
            };
            layout.Root = new LayoutControlGroup { EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True, GroupBordersVisible = false };
            _card.Controls.Add(layout);

            _cmbBranch = CreateCombo("🏥 Poliklinik Seçiniz");
            _cmbDoctor = CreateCombo("🩺 Doktor Seçiniz");
            _dtDate = new DateEdit
            {
                Properties =
                {
                    Appearance = { BackColor = Hex("#223244"), ForeColor = Hex("#ECEFF1") },
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                    NullValuePrompt = "📅 Tarih Seçiniz",
                    NullValuePromptShowForEmptyValue = true,
                    AutoHeight = false
                }
            };
            _dtDate.Properties.Appearance.Options.UseBackColor = true;
            _dtDate.Properties.Appearance.Options.UseForeColor = true;
            _dtDate.Properties.Buttons.Clear();
            _dtDate.Properties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            _dtDate.Properties.CalendarTimeProperties.Buttons.Clear();
            _dtDate.Properties.CalendarTimeProperties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            _dtDate.Height = 46;

            _tblTimes = CreateTimeTable();

            _txtNote = new MemoEdit
            {
                Properties =
                {
                    NullValuePrompt = "Not (opsiyonel) — şikayet / ek bilgi",
                    NullValuePromptShowForEmptyValue = true,
                    Appearance = { BackColor = Hex("#223244"), ForeColor = Hex("#ECEFF1") },
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple
                }
            };
            _txtNote.Properties.Appearance.Options.UseBackColor = true;
            _txtNote.Properties.Appearance.Options.UseForeColor = true;

            _btnCreate = new SimpleButton
            {
                Text = "RANDEVU OLUŞTUR",
                Cursor = Cursors.Hand,
                Height = 54,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
            };
            _btnCreate.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnCreate.Appearance.BackColor = Hex("#5D9CEC");
            _btnCreate.Appearance.ForeColor = Color.White;
            _btnCreate.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _btnCreate.Appearance.Options.UseBackColor = true;
            _btnCreate.Appearance.Options.UseForeColor = true;
            _btnCreate.Appearance.Options.UseFont = true;
            _btnCreate.AppearanceHovered.BackColor = Hex("#6EA8F1");
            _btnCreate.AppearanceHovered.Options.UseBackColor = true;
            _btnCreate.AppearancePressed.BackColor = Hex("#4B88DA");
            _btnCreate.AppearancePressed.Options.UseBackColor = true;
            _btnCreate.Click += (_, _) =>
            {
                // Şimdilik sadece UI — içerik sonra doldurulacak
                XtraMessageBox.Show("Randevu ekranı tasarlandı. İş mantığını birlikte ekleyeceğiz.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Layout groups
            layout.Controls.Add(_cmbBranch);
            layout.Controls.Add(_cmbDoctor);
            layout.Controls.Add(_dtDate);
            layout.Controls.Add(_tblTimes);
            layout.Controls.Add(_txtNote);
            layout.Controls.Add(_btnCreate);

            var root = (LayoutControlGroup)layout.Root;
            root.AddItem("Poliklinik", _cmbBranch).TextVisible = false;
            root.AddItem("Doktor", _cmbDoctor).TextVisible = false;
            root.AddItem("Tarih", _dtDate).TextVisible = false;
            root.AddItem("Saat", _tblTimes).TextVisible = false;
            root.AddItem("Not", _txtNote).TextVisible = false;
            root.AddItem("Create", _btnCreate).TextVisible = false;

            // Make it draggable from card top area
            _card.MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            using var bg = new LinearGradientBrush(ClientRectangle, Hex("#0F172A"), Hex("#1E2A38"), LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(bg, ClientRectangle);

            // subtle glow
            var glowRect = new Rectangle(-200, -200, Width + 400, Height / 2);
            using var glow = new LinearGradientBrush(glowRect, Color.FromArgb(90, Hex("#5D9CEC")), Color.Transparent, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(glow, glowRect);
            base.OnPaint(e);
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            // Rounded white border around card (premium)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = _card.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            using var path = RoundedRect(rect, 28);
            using var pen = new Pen(Color.FromArgb(180, Color.White), 2f);
            e.Graphics.DrawPath(pen, path);
        }

        private ComboBoxEdit CreateCombo(string prompt)
        {
            var c = new ComboBoxEdit
            {
                Properties =
                {
                    Appearance = { BackColor = Hex("#223244"), ForeColor = Hex("#ECEFF1") },
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                    NullValuePrompt = prompt,
                    NullValuePromptShowForEmptyValue = true,
                    AutoHeight = false
                },
                Height = 46
            };
            c.Properties.Appearance.Options.UseBackColor = true;
            c.Properties.Appearance.Options.UseForeColor = true;
            return c;
        }

        private TablePanel CreateTimeTable()
        {
            var tbl = new TablePanel
            {
                Height = 170,
                Dock = DockStyle.None,
                BackColor = Color.Transparent
            };
            tbl.Appearance.BackColor = Color.Transparent;
            tbl.Appearance.Options.UseBackColor = true;

            // 5 columns x 3 rows
            tbl.Columns.AddRange(new TablePanelColumn[]
            {
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
                new TablePanelColumn(TablePanelEntityStyle.Relative, 1),
            });
            tbl.Rows.AddRange(new TablePanelRow[]
            {
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
                new TablePanelRow(TablePanelEntityStyle.Absolute, 54),
            });

            string[] times = { "09:00", "09:30", "10:00", "10:30", "11:00", "11:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00" };
            int idx = 0;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    if (idx >= times.Length) break;
                    var t = times[idx++];
                    var btn = new SimpleButton
                    {
                        Text = t,
                        Dock = DockStyle.Fill,
                        Cursor = Cursors.Hand,
                        ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                        Margin = new Padding(6),
                    };
                    btn.LookAndFeel.UseDefaultLookAndFeel = false;
                    btn.Appearance.BackColor = Hex("#223244");
                    btn.Appearance.ForeColor = Hex("#ECEFF1");
                    btn.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    btn.Appearance.Options.UseBackColor = true;
                    btn.Appearance.Options.UseForeColor = true;
                    btn.Appearance.Options.UseFont = true;
                    btn.AppearanceHovered.BackColor = Hex("#2D4760");
                    btn.AppearanceHovered.Options.UseBackColor = true;
                    btn.AppearancePressed.BackColor = Hex("#29B6F6");
                    btn.AppearancePressed.Options.UseBackColor = true;

                    btn.Click += (_, _) =>
                    {
                        _selectedTime = t;
                        HighlightTimeButtons(tbl, btn);
                    };

                    // Bu DevExpress sürümünde SetRow/SetColumn static değil, instance metod
                    tbl.SetRow(btn, r);
                    tbl.SetColumn(btn, c);
                    tbl.Controls.Add(btn);
                }
            }
            return tbl;
        }

        private void HighlightTimeButtons(TablePanel tbl, SimpleButton selected)
        {
            foreach (Control c in tbl.Controls)
            {
                if (c is SimpleButton b)
                {
                    bool isSel = ReferenceEquals(b, selected);
                    b.Appearance.BackColor = isSel ? Hex("#29B6F6") : Hex("#223244");
                    b.Appearance.ForeColor = isSel ? Color.White : Hex("#ECEFF1");
                    b.Appearance.Options.UseBackColor = true;
                    b.Appearance.Options.UseForeColor = true;
                }
            }
        }

        private static Color Hex(string hex) => ColorTranslator.FromHtml(hex);

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
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
    }
}


