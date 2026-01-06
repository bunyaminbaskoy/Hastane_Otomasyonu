using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using DevExpress.XtraBars.FluentDesignSystem;
using DevExpress.XtraBars.Navigation;
using DevExpress.Utils.Layout;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using Microsoft.Data.SqlClient;
using Hastane_Otomasyonu.Database;

namespace Hastane_Otomasyonu
{
    public class FrmDoktorPanel : FluentDesignForm
    {
        public string DoktorTC { get; set; } = "";

        private readonly FluentDesignFormContainer _container;
        private readonly AccordionControl _menu;
        private readonly NavigationFrame _frame;
        private readonly SimpleButton _btnClose;

        private NavigationPage _pageHome;
        private NavigationPage _pageRandevular;
        private NavigationPage _pageRecete;

        private LabelControl _lblWelcome;
        private GridControl _gridRandevu;
        private DevExpress.XtraGrid.Views.Grid.GridView _viewRandevu;
        private FlowLayoutPanel _todayList;

        private ComboBoxEdit _cmbHasta;
        private MemoEdit _txtIlaclar;
        private SimpleButton _btnPdfSelect;
        private LabelControl _lblPdfInfo;
        private SimpleButton _btnPdfClear;
        private byte[] _selectedPdfBytes;
        private string _selectedPdfName;
        private SimpleButton _btnReceteKaydet;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public FrmDoktorPanel()
        {
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1280;
            Height = 820;

            _btnClose = new SimpleButton
            {
                Text = "✕",
                Cursor = Cursors.Hand,
                Size = new Size(36, 36),
                Location = new Point(Width - 50, 6),
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _btnClose.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnClose.Appearance.BackColor = Color.Transparent;
            _btnClose.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
            _btnClose.Appearance.Options.UseBackColor = true;
            _btnClose.Appearance.Options.UseForeColor = true;
            _btnClose.AppearanceHovered.ForeColor = Color.White;
            _btnClose.AppearanceHovered.Options.UseForeColor = true;
            _container = new FluentDesignFormContainer
            {
                Dock = DockStyle.Fill
            };
            _container.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            _container.Appearance.Options.UseBackColor = true;
            Controls.Add(_container);

            // Dock sırası önemli: önce Fill container eklensin, sonra Left menu eklensin (aksi halde sağ taraf menünün altına girer)
            _menu = new AccordionControl
            {
                Dock = DockStyle.Left,
                Width = 290,
                ScrollBarMode = ScrollBarMode.Touch
            };
            ApplyModernMenuStyles(accentHover: ColorTranslator.FromHtml("#22C55E"), accentPressed: ColorTranslator.FromHtml("#16A34A"));
            Controls.Add(_menu);

            _btnClose.Click += (_, _) => Close();
            Controls.Add(_btnClose);
            _btnClose.BringToFront();
            Resize += (_, _) => _btnClose.Location = new Point(ClientSize.Width - _btnClose.Width - 10, 6);

            _frame = new NavigationFrame { Dock = DockStyle.Fill, AllowTransitionAnimation = DevExpress.Utils.DefaultBoolean.False };
            _container.Controls.Add(_frame);

            BuildMenu();
            BuildPages();
            // İlk açılışta her zaman Ana Sayfa gelsin
            _frame.SelectedPage = _pageHome;
            Shown += (_, _) =>
            {
                try
                {
                    _frame.SelectedPage = _pageHome;
                    LoadHome();
                }
                catch { }
            };
        }

        private void ApplyModernMenuStyles(Color accentHover, Color accentPressed)
        {
            _menu.LookAndFeel.UseDefaultLookAndFeel = false;
            _menu.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;

            Color bg = ColorTranslator.FromHtml("#111827");
            Color fg = Color.White;
            Color itemBg = ColorTranslator.FromHtml("#0F172A");

            _menu.Appearance.AccordionControl.BackColor = bg;
            _menu.Appearance.AccordionControl.Options.UseBackColor = true;

            TrySetAccordionViewType(_menu, "HamburgerMenu");
            TrySetAccordionViewType(_menu, "Standard");

            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Normal", "BackColor" }, itemBg);
            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Normal", "ForeColor" }, fg);
            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Hovered", "BackColor" }, accentHover);
            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Hovered", "ForeColor" }, Color.White);
            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Pressed", "BackColor" }, accentPressed);
            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Pressed", "ForeColor" }, Color.White);

            var menuFont = new Font("Segoe UI Semibold", 12F);
            _menu.Font = menuFont;
            _menu.ForeColor = fg;

            var elementHeightProp = _menu.GetType().GetProperty("ElementHeight");
            if (elementHeightProp != null && elementHeightProp.CanWrite)
                elementHeightProp.SetValue(_menu, 70, null);
        }

        private void BuildMenu()
        {
            AccordionControlElement Item(string text)
            {
                var el = new AccordionControlElement { Style = ElementStyle.Item, Text = text };
                StyleMenuElement(el,
                    normalBg: ColorTranslator.FromHtml("#0F172A"),
                    normalFg: Color.White,
                    hoverBg: ColorTranslator.FromHtml("#22C55E"),
                    hoverFg: Color.White,
                    pressedBg: ColorTranslator.FromHtml("#16A34A"));
                // ikonlar
                try
                {
                    var asm = typeof(DevExpress.Images.ImageResourceCache).Assembly;
                    if (text == "Ana Sayfa")
                        el.ImageOptions.SvgImage = TryLoadSvg(asm, "DevExpress.Images.SvgImages.Dashboards.Dashboard.svg", "DevExpress.Images.SvgImages.Navigation.Home.svg");
                    else if (text == "Randevularım")
                        el.ImageOptions.SvgImage = TryLoadSvg(asm, "DevExpress.Images.SvgImages.Scheduling.NewAppointment.svg");
                    else if (text == "Reçete Oluştur")
                        el.ImageOptions.SvgImage = TryLoadSvg(asm, "DevExpress.Images.SvgImages.Medical.Prescription.svg", "DevExpress.Images.SvgImages.Medical.MedicalPrescription.svg");
                    else if (text == "Çıkış Yap")
                        el.ImageOptions.SvgImage = TryLoadSvg(asm, "DevExpress.Images.SvgImages.Actions.Exit.svg", "DevExpress.Images.SvgImages.Actions.Close.svg");
                }
                catch { }
                return el;
            }

            var navHome = Item("Ana Sayfa");
            var navRandevu = Item("Randevularım");
            var navRecete = Item("Reçete Oluştur");
            var navExit = Item("Çıkış Yap");
            StyleMenuElement(navExit,
                normalBg: ColorTranslator.FromHtml("#0F172A"),
                normalFg: Color.White,
                hoverBg: ColorTranslator.FromHtml("#EF4444"),
                hoverFg: Color.White,
                pressedBg: ColorTranslator.FromHtml("#DC2626"));

            navHome.Click += (_, _) => { _frame.SelectedPage = _pageHome; LoadHome(); };
            navRandevu.Click += (_, _) => { _frame.SelectedPage = _pageRandevular; LoadRandevular(); };
            navRecete.Click += (_, _) => { _frame.SelectedPage = _pageRecete; LoadRecetePatients(); };
            navExit.Click += (_, _) => Close();

            _menu.Elements.AddRange(new[] { navHome, navRandevu, navRecete, navExit });
        }

        private void BuildPages()
        {
            _pageHome = new NavigationPage();
            _pageRandevular = new NavigationPage();
            _pageRecete = new NavigationPage();
            _frame.Pages.AddRange(new[] { _pageHome, _pageRandevular, _pageRecete });

            BuildHomePage();
            BuildRandevuPage();
            BuildRecetePage();
        }

        private PanelControl BuildCard(Control content)
        {
            var card = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };
            card.Appearance.BackColor = Color.Transparent;
            card.Appearance.Options.UseBackColor = true;

            var fill = Color.FromArgb(30, 42, 56);
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
                using var b = new SolidBrush(fill);
                e.Graphics.FillPath(b, path);
                using var p = new Pen(Color.FromArgb(120, Color.White), 1.4f);
                e.Graphics.DrawPath(p, path);
            };
            card.Controls.Add(content);
            return card;
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
            catch { }
        }

        private static void StyleMenuElement(object element,
            Color normalBg, Color normalFg,
            Color hoverBg, Color hoverFg,
            Color pressedBg)
        {
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

            var heightProp = element.GetType().GetProperty("Height");
            if (heightProp != null && heightProp.CanWrite)
            {
                try { heightProp.SetValue(element, 70, null); } catch { }
            }
        }

        private void BuildHomePage()
        {
            var host = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(22) };
            host.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            host.Appearance.Options.UseBackColor = true;

            _lblWelcome = new LabelControl
            {
                AllowHtmlString = true,
                Dock = DockStyle.Top,
                Height = 64,
                Text = "<color=#22C55E>Doktor Paneli</color> <color=#ECEFF1>— Hoş geldiniz</color>"
            };
            _lblWelcome.Appearance.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            _lblWelcome.Appearance.ForeColor = ColorTranslator.FromHtml("#ECEFF1");
            _lblWelcome.Appearance.Options.UseFont = true;
            _lblWelcome.Appearance.Options.UseForeColor = true;

            var content = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill };
            content.Appearance.BackColor = Color.Transparent;
            content.Appearance.Options.UseBackColor = true;

            // Hasta panelindeki "yaklaşan randevular" konsepti gibi lacivert kart + chip listesi
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _lblWelcome.Dock = DockStyle.Fill;
            outer.Controls.Add(_lblWelcome, 0, 0);

            var sep = new SeparatorControl { Dock = DockStyle.Fill };
            sep.LineColor = Color.FromArgb(70, 255, 255, 255);
            outer.Controls.Add(sep, 0, 1);

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

            var title = new LabelControl
            {
                Text = "Bugünün Randevuları",
                Dock = DockStyle.Top,
                Height = 26
            };
            title.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            title.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            title.Appearance.Options.UseFont = true;
            title.Appearance.Options.UseForeColor = true;
            listCard.Controls.Add(title);

            _todayList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 12, 0, 0)
            };
            listCard.Controls.Add(_todayList);
            _todayList.BringToFront();

            outer.Controls.Add(listCard, 0, 2);
            content.Controls.Add(outer);

            host.Controls.Add(BuildCard(content));
            _pageHome.Controls.Add(host);
        }

        private void BuildRandevuPage()
        {
            var host = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(22) };
            host.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            host.Appearance.Options.UseBackColor = true;

            // Konsepte uygun: tek bir lacivert kart içinde başlık + grid (bindirme yok)
            var content = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill };
            content.Appearance.BackColor = Color.Transparent;
            content.Appearance.Options.UseBackColor = true;

            var cardInner = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };
            cardInner.Appearance.BackColor = ColorTranslator.FromHtml("#0F172A");
            cardInner.Appearance.Options.UseBackColor = true;
            cardInner.SizeChanged += (s, e) =>
            {
                if (cardInner.Width <= 0 || cardInner.Height <= 0) return;
                cardInner.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, cardInner.Width, cardInner.Height, 18, 18));
            };

            // Kullanıcı isteği: üstteki "Randevularım" yazısı olmasın
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cardInner.Controls.Add(layout);

            var grid = new GridControl { Dock = DockStyle.Fill };
            // Skin override: beyaz header/gri yazı sorununu engelle
            grid.LookAndFeel.UseDefaultLookAndFeel = false;
            grid.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            grid.BackColor = ColorTranslator.FromHtml("#0B1220");

            _viewRandevu = new DevExpress.XtraGrid.Views.Grid.GridView();
            grid.MainView = _viewRandevu;
            grid.ViewCollection.Add(_viewRandevu);
            _gridRandevu = grid;

            _viewRandevu.OptionsBehavior.Editable = false;
            _viewRandevu.OptionsBehavior.ReadOnly = true;
            _viewRandevu.OptionsView.ShowGroupPanel = false;
            _viewRandevu.OptionsView.EnableAppearanceEvenRow = true;
            _viewRandevu.OptionsView.EnableAppearanceOddRow = true;
            _viewRandevu.OptionsView.ShowIndicator = false;
            _viewRandevu.RowHeight = 44;

            // Header: beyaz yerine koyu zemin + yeşil vurgu
            _viewRandevu.Appearance.HeaderPanel.BackColor = ColorTranslator.FromHtml("#0B1220");
            _viewRandevu.Appearance.HeaderPanel.BackColor2 = ColorTranslator.FromHtml("#0F172A");
            _viewRandevu.Appearance.HeaderPanel.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            _viewRandevu.Appearance.HeaderPanel.ForeColor = Color.White;
            _viewRandevu.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 11F);
            _viewRandevu.Appearance.HeaderPanel.Options.UseBackColor = true;
            _viewRandevu.Appearance.HeaderPanel.Options.UseForeColor = true;
            _viewRandevu.Appearance.HeaderPanel.Options.UseFont = true;
            _viewRandevu.Appearance.HeaderPanel.Options.UseTextOptions = true;
            _viewRandevu.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            _viewRandevu.Appearance.HeaderPanel.BorderColor = ColorTranslator.FromHtml("#22C55E");
            _viewRandevu.Appearance.HeaderPanel.Options.UseBorderColor = true;

            _viewRandevu.Appearance.Row.BackColor = ColorTranslator.FromHtml("#0B1220");
            _viewRandevu.Appearance.Row.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _viewRandevu.Appearance.Row.Font = new Font("Segoe UI", 10F);
            _viewRandevu.Appearance.Row.Options.UseBackColor = true;
            _viewRandevu.Appearance.Row.Options.UseForeColor = true;
            _viewRandevu.Appearance.Row.Options.UseFont = true;

            _viewRandevu.Appearance.EvenRow.BackColor = ColorTranslator.FromHtml("#0B1220");
            _viewRandevu.Appearance.OddRow.BackColor = ColorTranslator.FromHtml("#0F172A");
            _viewRandevu.Appearance.EvenRow.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _viewRandevu.Appearance.OddRow.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _viewRandevu.Appearance.EvenRow.Options.UseBackColor = true;
            _viewRandevu.Appearance.OddRow.Options.UseBackColor = true;
            _viewRandevu.Appearance.EvenRow.Options.UseForeColor = true;
            _viewRandevu.Appearance.OddRow.Options.UseForeColor = true;

            layout.Controls.Add(grid, 0, 0);

            content.Controls.Add(cardInner);
            host.Controls.Add(BuildCard(content));
            _pageRandevular.Controls.Add(host);
        }

        private void BuildRecetePage()
        {
            var host = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(22) };
            host.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            host.Appearance.Options.UseBackColor = true;

            var content = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(18) };
            content.Appearance.BackColor = Color.Transparent;
            content.Appearance.Options.UseBackColor = true;

            var title = new LabelControl
            {
                AllowHtmlString = true,
                Dock = DockStyle.Top,
                Height = 52,
                Text = "<color=#22C55E>Reçete</color> <color=#ECEFF1>Oluştur</color>"
            };
            title.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            title.Appearance.Options.UseFont = true;
            content.Controls.Add(title);

            var subtitle = new LabelControl
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Bugünkü randevulardan hastayı seçin ve PDF reçeteyi yükleyin."
            };
            subtitle.Appearance.Font = new Font("Segoe UI", 11F);
            subtitle.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
            subtitle.Appearance.Options.UseFont = true;
            subtitle.Appearance.Options.UseForeColor = true;
            content.Controls.Add(subtitle);
            subtitle.BringToFront();
            title.BringToFront();

            PanelControl WrapField(Control editor)
            {
                var wrap = new PanelControl
                {
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(12, 10, 12, 10)
                };
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
                editor.Dock = DockStyle.Fill;
                wrap.Controls.Add(editor);
                editor.BringToFront();
                return wrap;
            }

            var form = new TableLayoutPanel { Dock = DockStyle.Top, Height = 420, ColumnCount = 1, RowCount = 4, Padding = new Padding(0, 14, 0, 0) };
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));  // hasta
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));  // pdf
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 210)); // not
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));  // kaydet

            _cmbHasta = new ComboBoxEdit { Dock = DockStyle.Fill };
            _cmbHasta.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _cmbHasta.Properties.NullValuePrompt = "Hasta seç (bugünkü randevular)";
            _cmbHasta.Properties.NullValuePromptShowForEmptyValue = true;
            _cmbHasta.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _cmbHasta.Properties.Appearance.BackColor = Color.Transparent;
            _cmbHasta.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _cmbHasta.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _cmbHasta.Properties.Appearance.Options.UseBackColor = true;
            _cmbHasta.Properties.Appearance.Options.UseForeColor = true;
            _cmbHasta.Properties.Appearance.Options.UseFont = true;

            _txtIlaclar = new MemoEdit { Dock = DockStyle.Fill };
            _txtIlaclar.Properties.NullValuePrompt = "Not / ilaçlar (opsiyonel)";
            _txtIlaclar.Properties.NullValuePromptShowForEmptyValue = true;
            _txtIlaclar.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _txtIlaclar.Properties.Appearance.BackColor = Color.Transparent;
            _txtIlaclar.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _txtIlaclar.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            _txtIlaclar.Properties.Appearance.Options.UseBackColor = true;
            _txtIlaclar.Properties.Appearance.Options.UseForeColor = true;
            _txtIlaclar.Properties.Appearance.Options.UseFont = true;

            _btnPdfSelect = new SimpleButton
            {
                Text = "PDF Seç",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _btnPdfSelect.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnPdfSelect.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _btnPdfSelect.Appearance.ForeColor = Color.White;
            _btnPdfSelect.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _btnPdfSelect.Appearance.Options.UseBackColor = true;
            _btnPdfSelect.Appearance.Options.UseForeColor = true;
            _btnPdfSelect.Appearance.Options.UseFont = true;
            _btnPdfSelect.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#1D4ED8");
            _btnPdfSelect.AppearanceHovered.Options.UseBackColor = true;
            _btnPdfSelect.AppearancePressed.BackColor = ColorTranslator.FromHtml("#2563EB");
            _btnPdfSelect.AppearancePressed.Options.UseBackColor = true;
            _btnPdfSelect.SizeChanged += (s, e) => _btnPdfSelect.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _btnPdfSelect.Width, _btnPdfSelect.Height, 28, 28));
            _btnPdfSelect.Click += (_, _) => PickPdf();

            _lblPdfInfo = new LabelControl
            {
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Text = "PDF: seçilmedi"
            };
            _lblPdfInfo.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
            _lblPdfInfo.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblPdfInfo.Appearance.Options.UseForeColor = true;
            _lblPdfInfo.Appearance.Options.UseFont = true;

            _btnPdfClear = new SimpleButton
            {
                Text = "✕",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _btnPdfClear.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnPdfClear.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _btnPdfClear.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _btnPdfClear.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _btnPdfClear.Appearance.Options.UseBackColor = true;
            _btnPdfClear.Appearance.Options.UseForeColor = true;
            _btnPdfClear.Appearance.Options.UseFont = true;
            _btnPdfClear.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#DC2626");
            _btnPdfClear.AppearanceHovered.ForeColor = Color.White;
            _btnPdfClear.AppearanceHovered.Options.UseBackColor = true;
            _btnPdfClear.AppearanceHovered.Options.UseForeColor = true;
            _btnPdfClear.SizeChanged += (s, e) => _btnPdfClear.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _btnPdfClear.Width, _btnPdfClear.Height, 28, 28));
            _btnPdfClear.Click += (_, _) =>
            {
                _selectedPdfBytes = null;
                _selectedPdfName = null;
                if (_lblPdfInfo != null) _lblPdfInfo.Text = "PDF: seçilmedi";
            };

            _btnReceteKaydet = new SimpleButton
            {
                Text = "REÇETEYİ KAYDET",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _btnReceteKaydet.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnReceteKaydet.Appearance.BackColor = ColorTranslator.FromHtml("#22C55E");
            _btnReceteKaydet.Appearance.ForeColor = ColorTranslator.FromHtml("#0B1220");
            _btnReceteKaydet.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _btnReceteKaydet.Appearance.Options.UseBackColor = true;
            _btnReceteKaydet.Appearance.Options.UseForeColor = true;
            _btnReceteKaydet.Appearance.Options.UseFont = true;
            _btnReceteKaydet.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#16A34A");
            _btnReceteKaydet.AppearanceHovered.Options.UseBackColor = true;
            _btnReceteKaydet.AppearancePressed.BackColor = ColorTranslator.FromHtml("#15803D");
            _btnReceteKaydet.AppearancePressed.Options.UseBackColor = true;
            _btnReceteKaydet.SizeChanged += (s, e) => _btnReceteKaydet.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _btnReceteKaydet.Width, _btnReceteKaydet.Height, 28, 28));
            _btnReceteKaydet.Click += (_, _) => SaveRecete();

            // PDF satırı: buton + info + temizle
            var pdfRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            pdfRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            pdfRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pdfRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
            pdfRow.Controls.Add(_btnPdfSelect, 0, 0);
            pdfRow.Controls.Add(_lblPdfInfo, 1, 0);
            pdfRow.Controls.Add(_btnPdfClear, 2, 0);

            var noteWrap = WrapField(_txtIlaclar);
            var hastaWrap = WrapField(_cmbHasta);

            form.Controls.Add(hastaWrap, 0, 0);
            form.Controls.Add(WrapField(pdfRow), 0, 1);
            form.Controls.Add(noteWrap, 0, 2);
            form.Controls.Add(_btnReceteKaydet, 0, 3);

            content.Controls.Add(form);
            form.BringToFront();
            host.Controls.Add(BuildCard(content));
            _pageRecete.Controls.Add(host);
        }

        private void LoadHome()
        {
            try
            {
                using var conn = SqlBaglantisi.Instance.GetConnection();
                using var cmd = new SqlCommand("SELECT DoktorAd, DoktorSoyad FROM Tbl_Doktorlar WHERE DoktorTC=@tc", conn);
                cmd.Parameters.AddWithValue("@tc", DoktorTC);
                using var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    var name = $"{dr["DoktorAd"]} {dr["DoktorSoyad"]}";
                    _lblWelcome.Text = $"<color=#22C55E>Hoş Geldiniz,</color> <color=#ECEFF1>Dr. {name}</color>";
                }
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
            LoadTodayAppointments();
        }

        private void LoadTodayAppointments()
        {
            try
            {
                if (_todayList == null) return;
                using var conn = SqlBaglantisi.Instance.GetConnection();
                string query = @"SELECT r.RandevuSaat, r.RandevuBrans, r.HastaTC,
                                        ISNULL(h.HastaAd,'') AS HastaAd, ISNULL(h.HastaSoyad,'') AS HastaSoyad
                                 FROM Tbl_Randevular r
                                 LEFT JOIN Tbl_Hastalar h ON h.HastaTC = r.HastaTC
                                 WHERE r.RandevuTarih = CAST(GETDATE() AS DATE) AND r.RandevuDoktorTC = @dtc AND r.RandevuDurum = 1
                                 ORDER BY r.RandevuSaat ASC";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@dtc", DoktorTC);
                using var dr = cmd.ExecuteReader();

                _todayList.SuspendLayout();
                _todayList.Controls.Clear();
                while (dr.Read())
                {
                    var saat = (TimeSpan)dr["RandevuSaat"];
                    string hastaTc = dr["HastaTC"]?.ToString() ?? "";
                    string ad = dr["HastaAd"]?.ToString() ?? "";
                    string soyad = dr["HastaSoyad"]?.ToString() ?? "";
                    string brans = dr["RandevuBrans"]?.ToString() ?? "";

                    var chip = new PanelControl
                    {
                        BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                        Size = new Size(430, 78),
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
                        using var accent = new Pen(ColorTranslator.FromHtml("#22C55E"), 3f);
                        e.Graphics.DrawLine(accent, 8, 10, 8, rect.Bottom - 10);
                    };

                    var lblTime = new LabelControl
                    {
                        Text = saat.ToString(@"hh\:mm"),
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
                        Text = $"{MaskTc(hastaTc)}   •   {ad} {soyad}".Trim(),
                        AutoSizeMode = LabelAutoSizeMode.None,
                        Location = new Point(100, 12),
                        Size = new Size(310, 22)
                    };
                    lblMain.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                    lblMain.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
                    lblMain.Appearance.Options.UseFont = true;
                    lblMain.Appearance.Options.UseForeColor = true;

                    var lblSub = new LabelControl
                    {
                        Text = brans,
                        AutoSizeMode = LabelAutoSizeMode.None,
                        Location = new Point(100, 40),
                        Size = new Size(310, 20)
                    };
                    lblSub.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                    lblSub.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
                    lblSub.Appearance.Options.UseFont = true;
                    lblSub.Appearance.Options.UseForeColor = true;

                    chip.Controls.Add(lblTime);
                    chip.Controls.Add(lblMain);
                    chip.Controls.Add(lblSub);
                    _todayList.Controls.Add(chip);
                }
                _todayList.ResumeLayout();
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private static string MaskTc(string tc)
        {
            if (string.IsNullOrWhiteSpace(tc) || tc.Length < 11) return "***********";
            return $"{tc.Substring(0, 3)}******{tc.Substring(tc.Length - 2, 2)}";
        }

        private void LoadRandevular()
        {
            try
            {
                using var conn = SqlBaglantisi.Instance.GetConnection();
                // yeni kolon varsa onu kullanır, yoksa isim ile filtreler (geri uyum)
                string query = @"SELECT RandevuTarih, RandevuSaat, RandevuBrans, RandevuDoktor, HastaTC
                                 FROM Tbl_Randevular
                                 WHERE (RandevuDoktorTC = @dtc OR @dtc IS NULL) AND RandevuTarih >= CAST(GETDATE() AS DATE) AND RandevuDurum = 1
                                 ORDER BY RandevuTarih ASC, RandevuSaat ASC";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@dtc", (object)(string.IsNullOrWhiteSpace(DoktorTC) ? DBNull.Value : DoktorTC));
                var dt = new DataTable();
                using var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                _gridRandevu.DataSource = dt;
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void SaveRecete()
        {
            if (string.IsNullOrWhiteSpace(_cmbHasta?.Text))
            {
                XtraMessageBox.Show("Lütfen hasta seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_selectedPdfBytes == null || _selectedPdfBytes.Length == 0)
            {
                XtraMessageBox.Show("Lütfen reçete PDF dosyası seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = SqlBaglantisi.Instance.GetConnection();
                // Doktor adını çek
                string doktorAdSoyad = "Doktor";
                using (var cmdD = new SqlCommand("SELECT DoktorAd, DoktorSoyad FROM Tbl_Doktorlar WHERE DoktorTC=@tc", conn))
                {
                    cmdD.Parameters.AddWithValue("@tc", DoktorTC);
                    using var dr = cmdD.ExecuteReader();
                    if (dr.Read()) doktorAdSoyad = $"Dr. {dr["DoktorAd"]} {dr["DoktorSoyad"]}";
                }

                string hastaTc = ExtractTc(_cmbHasta.Text);
                if (string.IsNullOrWhiteSpace(hastaTc) || hastaTc.Length != 11)
                {
                    XtraMessageBox.Show("Hasta TC okunamadı. Lütfen tekrar seçim yapın.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string kod = GenerateReceteKod();
                string q = @"INSERT INTO Tbl_Receteler (ReceteKod, ReceteTarih, DoktorAd, DoktorTC, Ilaclar, RecetePdf, RecetePdfFileName, RecetePdfMime, HastaTC)
                             VALUES (@kod, GETDATE(), @dad, @dtc, @ilaclar, @pdf, @pdfName, @mime, @htc)";
                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@kod", kod);
                cmd.Parameters.AddWithValue("@dad", doktorAdSoyad);
                cmd.Parameters.AddWithValue("@dtc", DoktorTC);
                cmd.Parameters.AddWithValue("@ilaclar", _txtIlaclar.Text.Trim());
                cmd.Parameters.AddWithValue("@pdf", _selectedPdfBytes);
                cmd.Parameters.AddWithValue("@pdfName", string.IsNullOrWhiteSpace(_selectedPdfName) ? (object)DBNull.Value : _selectedPdfName);
                cmd.Parameters.AddWithValue("@mime", "application/pdf");
                cmd.Parameters.AddWithValue("@htc", hastaTc);
                cmd.ExecuteNonQuery();

                XtraMessageBox.Show($"Reçete kaydedildi.\nKod: {kod}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _txtIlaclar.Text = "";
                _selectedPdfBytes = null;
                _selectedPdfName = null;
                if (_lblPdfInfo != null) _lblPdfInfo.Text = "PDF: seçilmedi";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Reçete kaydı başarısız: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void PickPdf()
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "PDF Dosyası|*.pdf",
                Title = "Reçete PDF Seç"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            _selectedPdfBytes = System.IO.File.ReadAllBytes(dlg.FileName);
            _selectedPdfName = System.IO.Path.GetFileName(dlg.FileName);
            if (_lblPdfInfo != null) _lblPdfInfo.Text = "PDF: " + _selectedPdfName;
        }

        private void LoadRecetePatients()
        {
            try
            {
                if (_cmbHasta == null) return;
                using var conn = SqlBaglantisi.Instance.GetConnection();
                string q = @"SELECT DISTINCT r.HastaTC,
                                    ISNULL(h.HastaAd,'') AS HastaAd,
                                    ISNULL(h.HastaSoyad,'') AS HastaSoyad
                             FROM Tbl_Randevular r
                             LEFT JOIN Tbl_Hastalar h ON h.HastaTC = r.HastaTC
                             WHERE r.RandevuTarih = CAST(GETDATE() AS DATE) AND r.RandevuDoktorTC = @dtc AND r.HastaTC IS NOT NULL
                             ORDER BY r.HastaTC";
                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@dtc", DoktorTC);
                using var dr = cmd.ExecuteReader();
                _cmbHasta.Properties.Items.Clear();
                while (dr.Read())
                {
                    var tc = dr["HastaTC"]?.ToString() ?? "";
                    var ad = dr["HastaAd"]?.ToString() ?? "";
                    var soyad = dr["HastaSoyad"]?.ToString() ?? "";
                    var item = string.IsNullOrWhiteSpace(ad + soyad) ? tc : $"{tc} - {ad} {soyad}".Trim();
                    _cmbHasta.Properties.Items.Add(item);
                }
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private static string ExtractTc(string hastaItemText)
        {
            if (string.IsNullOrWhiteSpace(hastaItemText)) return null;
            // "TC - Ad Soyad" formatı
            var t = hastaItemText.Trim();
            if (t.Length >= 11)
            {
                var first = t.Substring(0, 11);
                bool allDigits = true;
                for (int i = 0; i < first.Length; i++)
                {
                    if (!char.IsDigit(first[i])) { allDigits = false; break; }
                }
                if (allDigits) return first;
            }
            return t.Length == 11 ? t : null;
        }

        private static string GenerateReceteKod()
        {
            // RX-XXXXXXXX (8 karakter) rastgele
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Span<byte> bytes = stackalloc byte[8];
            RandomNumberGenerator.Fill(bytes);
            char[] chars = new char[8];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = alphabet[bytes[i] % alphabet.Length];
            return "RX-" + new string(chars);
        }
    }
}


