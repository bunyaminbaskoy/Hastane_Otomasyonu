using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.XtraBars.FluentDesignSystem;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Hastane_Otomasyonu.Database;

namespace Hastane_Otomasyonu
{
    public class FrmSekreterPanel : FluentDesignForm
    {
        public string SekreterTC { get; set; } = "";

        private readonly FluentDesignFormContainer _container;
        private readonly AccordionControl _menu;
        private readonly NavigationFrame _frame;
        private readonly SimpleButton _btnClose;

        private NavigationPage _pageRandevular;
        private NavigationPage _pageTahlil;
        private NavigationPage _pageHastaList;
        private NavigationPage _pageDoktorList;

        private DevExpress.XtraGrid.GridControl _gridRandevu;
        private DevExpress.XtraGrid.Views.Grid.GridView _viewRandevu;
        private DevExpress.XtraGrid.GridControl _gridHasta;
        private DevExpress.XtraGrid.Views.Grid.GridView _viewHasta;
        private DevExpress.XtraGrid.GridControl _gridDoktor;
        private DevExpress.XtraGrid.Views.Grid.GridView _viewDoktor;

        private SimpleButton _btnOnayla;
        private SimpleButton _btnYenile;

        private TextEdit _txtHastaTc;
        private TextEdit _txtTahlilAd;
        private TextEdit _txtSonuc;
        private ComboBoxEdit _cmbDurum;
        private SimpleButton _btnTahlilKaydet;

        // Modern tahlil upload
        private ComboBoxEdit _tahlilHastaSelect;
        private ComboBoxEdit _tahlilTurSelect;
        private MemoEdit _tahlilNot;
        private SimpleButton _tahlilPdfSelect;
        private SimpleButton _tahlilPdfClear;
        private LabelControl _tahlilPdfInfo;
        private byte[] _tahlilPdfBytes;
        private string _tahlilPdfName;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public FrmSekreterPanel()
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

            // Dock sırası önemli: önce Fill container, sonra Left menu (aksi halde sağ taraf menünün altına girer)
            _menu = new AccordionControl
            {
                Dock = DockStyle.Left,
                Width = 290,
                ScrollBarMode = ScrollBarMode.Touch
            };
            ApplyModernMenuStyles(accentHover: ColorTranslator.FromHtml("#F59E0B"), accentPressed: ColorTranslator.FromHtml("#D97706"));
            Controls.Add(_menu);

            _btnClose.Click += (_, _) => Close();
            Controls.Add(_btnClose);
            _btnClose.BringToFront();
            Resize += (_, _) => _btnClose.Location = new Point(ClientSize.Width - _btnClose.Width - 10, 6);

            _frame = new NavigationFrame { Dock = DockStyle.Fill, AllowTransitionAnimation = DevExpress.Utils.DefaultBoolean.False };
            _container.Controls.Add(_frame);

            BuildMenu();
            BuildPages();
            // Sekreterde Ana Sayfa kaldırıldı: direkt Randevular açılsın
            _frame.SelectedPage = _pageRandevular;
            Shown += (_, _) =>
            {
                try { _frame.SelectedPage = _pageRandevular; LoadRandevular(); } catch { }
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
            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Hovered", "ForeColor" }, ColorTranslator.FromHtml("#0B1220"));
            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Pressed", "BackColor" }, accentPressed);
            TrySetNestedProperty(_menu, new[] { "Appearance", "Item", "Pressed", "ForeColor" }, ColorTranslator.FromHtml("#0B1220"));

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
                    hoverBg: ColorTranslator.FromHtml("#F59E0B"),
                    hoverFg: ColorTranslator.FromHtml("#0B1220"),
                    pressedBg: ColorTranslator.FromHtml("#D97706"));
                try
                {
                    var asm = typeof(DevExpress.Images.ImageResourceCache).Assembly;
                    if (text == "Ana Sayfa")
                        el.ImageOptions.SvgImage = TryLoadSvg(asm, "DevExpress.Images.SvgImages.Dashboards.Dashboard.svg", "DevExpress.Images.SvgImages.Navigation.Home.svg");
                    else if (text == "Randevular")
                        el.ImageOptions.SvgImage = TryLoadSvg(asm, "DevExpress.Images.SvgImages.Scheduling.NewAppointment.svg");
                    else if (text == "Tahlil Yükle")
                        el.ImageOptions.SvgImage = TryLoadSvg(asm, "DevExpress.Images.SvgImages.Medical.Lab.svg", "DevExpress.Images.SvgImages.Medical.MedicalRecord.svg");
                    else if (text == "Çıkış Yap")
                        el.ImageOptions.SvgImage = TryLoadSvg(asm, "DevExpress.Images.SvgImages.Actions.Exit.svg", "DevExpress.Images.SvgImages.Actions.Close.svg");
                }
                catch { }
                return el;
            }

            var navRandevu = Item("Randevular");
            var navTahlil = Item("Tahlil Yükle");
            var navHasta = Item("Hasta Listesi");
            var navDoktor = Item("Doktor Listesi");
            var navExit = Item("Çıkış Yap");
            StyleMenuElement(navExit,
                normalBg: ColorTranslator.FromHtml("#0F172A"),
                normalFg: Color.White,
                hoverBg: ColorTranslator.FromHtml("#EF4444"),
                hoverFg: Color.White,
                pressedBg: ColorTranslator.FromHtml("#DC2626"));

            navRandevu.Click += (_, _) => { _frame.SelectedPage = _pageRandevular; LoadRandevular(); };
            navTahlil.Click += (_, _) => { _frame.SelectedPage = _pageTahlil; LoadTahlilPatients(); };
            navHasta.Click += (_, _) => { _frame.SelectedPage = _pageHastaList; LoadHastaList(); };
            navDoktor.Click += (_, _) => { _frame.SelectedPage = _pageDoktorList; LoadDoktorList(); };
            navExit.Click += (_, _) => Close();

            _menu.Elements.AddRange(new[] { navRandevu, navTahlil, navHasta, navDoktor, navExit });
        }

        private void BuildPages()
        {
            _pageRandevular = new NavigationPage();
            _pageTahlil = new NavigationPage();
            _pageHastaList = new NavigationPage();
            _pageDoktorList = new NavigationPage();
            _frame.Pages.AddRange(new[] { _pageRandevular, _pageTahlil, _pageHastaList, _pageDoktorList });
            _pageRandevular.Controls.Add(BuildRandevuShell());
            _pageTahlil.Controls.Add(BuildTahlilShell());
            _pageHastaList.Controls.Add(BuildHastaListShell());
            _pageDoktorList.Controls.Add(BuildDoktorListShell());
        }

        private static string MaskTc(string tc)
        {
            if (string.IsNullOrWhiteSpace(tc) || tc.Length < 11) return "***********";
            return $"{tc.Substring(0, 3)}******{tc.Substring(tc.Length - 2, 2)}";
        }

        private Control BuildShell(string titleText, Color accent, out LabelControl headerLabel)
        {
            var host = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(22) };
            host.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            host.Appearance.Options.UseBackColor = true;

            var content = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(18) };
            content.Appearance.BackColor = Color.Transparent;
            content.Appearance.Options.UseBackColor = true;

            headerLabel = new LabelControl
            {
                AllowHtmlString = true,
                Dock = DockStyle.Top,
                Height = 64,
                Text = $"<color=#{accent.R:X2}{accent.G:X2}{accent.B:X2}>{titleText}</color> <color=#ECEFF1>— Hoş geldiniz</color>"
            };
            headerLabel.Appearance.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            headerLabel.Appearance.ForeColor = ColorTranslator.FromHtml("#ECEFF1");
            headerLabel.Appearance.Options.UseFont = true;
            headerLabel.Appearance.Options.UseForeColor = true;
            content.Controls.Add(headerLabel);

            host.Controls.Add(BuildCard(content));
            return host;
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

        private Control BuildRandevuShell()
        {
            var host = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(22) };
            host.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            host.Appearance.Options.UseBackColor = true;

            var content = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill };
            content.Appearance.BackColor = Color.Transparent;
            content.Appearance.Options.UseBackColor = true;

            // Üst aksiyon bar: Yenile + Onayla
            var actionBar = new PanelControl
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Top,
                Height = 54,
                Padding = new Padding(0, 8, 0, 8)
            };
            actionBar.Appearance.BackColor = Color.Transparent;
            actionBar.Appearance.Options.UseBackColor = true;

            _btnYenile = new SimpleButton
            {
                Text = "Yenile",
                Cursor = Cursors.Hand,
                Dock = DockStyle.Left,
                Width = 160,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _btnYenile.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnYenile.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _btnYenile.Appearance.ForeColor = Color.White;
            _btnYenile.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _btnYenile.Appearance.Options.UseBackColor = true;
            _btnYenile.Appearance.Options.UseForeColor = true;
            _btnYenile.Appearance.Options.UseFont = true;
            _btnYenile.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#1F2937");
            _btnYenile.AppearanceHovered.Options.UseBackColor = true;
            _btnYenile.SizeChanged += (s, e) => _btnYenile.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _btnYenile.Width, _btnYenile.Height, 24, 24));
            _btnYenile.Click += (_, _) => LoadRandevular();

            _btnOnayla = new SimpleButton
            {
                Text = "Seçili Randevuyu Onayla",
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = 260,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _btnOnayla.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnOnayla.Appearance.BackColor = ColorTranslator.FromHtml("#F59E0B");
            _btnOnayla.Appearance.ForeColor = ColorTranslator.FromHtml("#0B1220");
            _btnOnayla.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _btnOnayla.Appearance.Options.UseBackColor = true;
            _btnOnayla.Appearance.Options.UseForeColor = true;
            _btnOnayla.Appearance.Options.UseFont = true;
            _btnOnayla.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#D97706");
            _btnOnayla.AppearanceHovered.Options.UseBackColor = true;
            _btnOnayla.AppearancePressed.BackColor = ColorTranslator.FromHtml("#B45309");
            _btnOnayla.AppearancePressed.Options.UseBackColor = true;
            _btnOnayla.SizeChanged += (s, e) => _btnOnayla.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _btnOnayla.Width, _btnOnayla.Height, 24, 24));
            _btnOnayla.Click += (_, _) => ApproveSelectedAppointment();

            actionBar.Controls.Add(_btnOnayla);
            actionBar.Controls.Add(_btnYenile);

            _gridRandevu = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            _viewRandevu = new DevExpress.XtraGrid.Views.Grid.GridView();
            _gridRandevu.MainView = _viewRandevu;
            _gridRandevu.ViewCollection.Add(_viewRandevu);

            _viewRandevu.OptionsBehavior.Editable = false;
            _viewRandevu.OptionsView.ShowGroupPanel = false;
            _viewRandevu.RowHeight = 42;
            _viewRandevu.Appearance.HeaderPanel.BackColor = ColorTranslator.FromHtml("#D97706");
            _viewRandevu.Appearance.HeaderPanel.ForeColor = ColorTranslator.FromHtml("#0B1220");
            _viewRandevu.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 11F);
            _viewRandevu.Appearance.HeaderPanel.Options.UseBackColor = true;
            _viewRandevu.Appearance.HeaderPanel.Options.UseForeColor = true;
            _viewRandevu.Appearance.HeaderPanel.Options.UseFont = true;
            _viewRandevu.Appearance.Row.BackColor = ColorTranslator.FromHtml("#0F172A");
            _viewRandevu.Appearance.Row.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _viewRandevu.Appearance.Row.Font = new Font("Segoe UI", 10F);
            _viewRandevu.Appearance.Row.Options.UseBackColor = true;
            _viewRandevu.Appearance.Row.Options.UseForeColor = true;
            _viewRandevu.Appearance.Row.Options.UseFont = true;

            content.Controls.Add(_gridRandevu);
            content.Controls.Add(actionBar);

            host.Controls.Add(BuildCard(content));
            return host;
        }

        private Control BuildHastaListShell()
        {
            var host = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(22) };
            host.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            host.Appearance.Options.UseBackColor = true;

            var content = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill };
            content.Appearance.BackColor = Color.Transparent;
            content.Appearance.Options.UseBackColor = true;

            var title = new LabelControl
            {
                AllowHtmlString = true,
                Dock = DockStyle.Top,
                Height = 52,
                Text = "<color=#F59E0B>Hasta</color> <color=#ECEFF1>Listesi</color>"
            };
            title.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            title.Appearance.Options.UseFont = true;

            _gridHasta = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            _viewHasta = new DevExpress.XtraGrid.Views.Grid.GridView();
            _gridHasta.MainView = _viewHasta;
            _gridHasta.ViewCollection.Add(_viewHasta);
            _gridHasta.LookAndFeel.UseDefaultLookAndFeel = false;
            _gridHasta.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;

            StyleGrid(_viewHasta, headerBg: "#D97706", headerFg: "#0B1220");

            content.Controls.Add(_gridHasta);
            content.Controls.Add(title);
            title.BringToFront();

            host.Controls.Add(BuildCard(content));
            return host;
        }

        private Control BuildDoktorListShell()
        {
            var host = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill, Padding = new Padding(22) };
            host.Appearance.BackColor = ColorTranslator.FromHtml("#0B1220");
            host.Appearance.Options.UseBackColor = true;

            var content = new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, Dock = DockStyle.Fill };
            content.Appearance.BackColor = Color.Transparent;
            content.Appearance.Options.UseBackColor = true;

            var title = new LabelControl
            {
                AllowHtmlString = true,
                Dock = DockStyle.Top,
                Height = 52,
                Text = "<color=#F59E0B>Doktor</color> <color=#ECEFF1>Listesi</color>"
            };
            title.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            title.Appearance.Options.UseFont = true;

            _gridDoktor = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            _viewDoktor = new DevExpress.XtraGrid.Views.Grid.GridView();
            _gridDoktor.MainView = _viewDoktor;
            _gridDoktor.ViewCollection.Add(_viewDoktor);
            _gridDoktor.LookAndFeel.UseDefaultLookAndFeel = false;
            _gridDoktor.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;

            StyleGrid(_viewDoktor, headerBg: "#D97706", headerFg: "#0B1220");

            content.Controls.Add(_gridDoktor);
            content.Controls.Add(title);
            title.BringToFront();

            host.Controls.Add(BuildCard(content));
            return host;
        }

        private static void StyleGrid(DevExpress.XtraGrid.Views.Grid.GridView view, string headerBg, string headerFg)
        {
            view.OptionsBehavior.Editable = false;
            view.OptionsBehavior.ReadOnly = true;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowIndicator = false;
            view.RowHeight = 42;
            view.Appearance.HeaderPanel.BackColor = ColorTranslator.FromHtml(headerBg);
            view.Appearance.HeaderPanel.ForeColor = ColorTranslator.FromHtml(headerFg);
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

            view.OptionsView.EnableAppearanceEvenRow = true;
            view.OptionsView.EnableAppearanceOddRow = true;
            view.Appearance.EvenRow.BackColor = ColorTranslator.FromHtml("#0B1220");
            view.Appearance.OddRow.BackColor = ColorTranslator.FromHtml("#0F172A");
            view.Appearance.EvenRow.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            view.Appearance.OddRow.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            view.Appearance.EvenRow.Options.UseBackColor = true;
            view.Appearance.OddRow.Options.UseBackColor = true;
            view.Appearance.EvenRow.Options.UseForeColor = true;
            view.Appearance.OddRow.Options.UseForeColor = true;
        }

        private Control BuildTahlilShell()
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
                Text = "<color=#F59E0B>Tahlil</color> <color=#ECEFF1>Yükle</color>"
            };
            title.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            title.Appearance.Options.UseFont = true;
            content.Controls.Add(title);

            var subtitle = new LabelControl
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Randevusu olan hastayı seçin, tahlil türünü seçin ve PDF sonucu yükleyin."
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

            _tahlilHastaSelect = new ComboBoxEdit { Dock = DockStyle.Fill };
            _tahlilHastaSelect.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _tahlilHastaSelect.Properties.NullValuePrompt = "Hasta seç (randevusu olanlar)";
            _tahlilHastaSelect.Properties.NullValuePromptShowForEmptyValue = true;
            _tahlilHastaSelect.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _tahlilHastaSelect.Properties.Appearance.BackColor = Color.Transparent;
            _tahlilHastaSelect.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _tahlilHastaSelect.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _tahlilHastaSelect.Properties.Appearance.Options.UseBackColor = true;
            _tahlilHastaSelect.Properties.Appearance.Options.UseForeColor = true;
            _tahlilHastaSelect.Properties.Appearance.Options.UseFont = true;

            _tahlilTurSelect = new ComboBoxEdit { Dock = DockStyle.Fill };
            _tahlilTurSelect.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _tahlilTurSelect.Properties.Items.AddRange(new object[]
            {
                "Kan", "İdrar", "Biyokimya", "Hemogram", "Hormon", "MR", "Röntgen", "Ultrason", "EKG", "Diğer"
            });
            _tahlilTurSelect.Properties.NullValuePrompt = "Tahlil türü seç";
            _tahlilTurSelect.Properties.NullValuePromptShowForEmptyValue = true;
            _tahlilTurSelect.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _tahlilTurSelect.Properties.Appearance.BackColor = Color.Transparent;
            _tahlilTurSelect.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _tahlilTurSelect.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _tahlilTurSelect.Properties.Appearance.Options.UseBackColor = true;
            _tahlilTurSelect.Properties.Appearance.Options.UseForeColor = true;
            _tahlilTurSelect.Properties.Appearance.Options.UseFont = true;

            _tahlilNot = new MemoEdit { Dock = DockStyle.Fill };
            _tahlilNot.Properties.NullValuePrompt = "Not (opsiyonel)";
            _tahlilNot.Properties.NullValuePromptShowForEmptyValue = true;
            _tahlilNot.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _tahlilNot.Properties.Appearance.BackColor = Color.Transparent;
            _tahlilNot.Properties.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _tahlilNot.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            _tahlilNot.Properties.Appearance.Options.UseBackColor = true;
            _tahlilNot.Properties.Appearance.Options.UseForeColor = true;
            _tahlilNot.Properties.Appearance.Options.UseFont = true;

            _tahlilPdfSelect = new SimpleButton
            {
                Text = "PDF Seç",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _tahlilPdfSelect.LookAndFeel.UseDefaultLookAndFeel = false;
            _tahlilPdfSelect.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _tahlilPdfSelect.Appearance.ForeColor = Color.White;
            _tahlilPdfSelect.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _tahlilPdfSelect.Appearance.Options.UseBackColor = true;
            _tahlilPdfSelect.Appearance.Options.UseForeColor = true;
            _tahlilPdfSelect.Appearance.Options.UseFont = true;
            _tahlilPdfSelect.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#1F2937");
            _tahlilPdfSelect.AppearanceHovered.Options.UseBackColor = true;
            _tahlilPdfSelect.SizeChanged += (s, e) => _tahlilPdfSelect.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _tahlilPdfSelect.Width, _tahlilPdfSelect.Height, 28, 28));
            _tahlilPdfSelect.Click += (_, _) => PickTahlilPdf();

            _tahlilPdfInfo = new LabelControl
            {
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Text = "PDF: seçilmedi"
            };
            _tahlilPdfInfo.Appearance.ForeColor = ColorTranslator.FromHtml("#B0BEC5");
            _tahlilPdfInfo.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _tahlilPdfInfo.Appearance.Options.UseForeColor = true;
            _tahlilPdfInfo.Appearance.Options.UseFont = true;

            _tahlilPdfClear = new SimpleButton
            {
                Text = "✕",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _tahlilPdfClear.LookAndFeel.UseDefaultLookAndFeel = false;
            _tahlilPdfClear.Appearance.BackColor = ColorTranslator.FromHtml("#111827");
            _tahlilPdfClear.Appearance.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            _tahlilPdfClear.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _tahlilPdfClear.Appearance.Options.UseBackColor = true;
            _tahlilPdfClear.Appearance.Options.UseForeColor = true;
            _tahlilPdfClear.Appearance.Options.UseFont = true;
            _tahlilPdfClear.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#DC2626");
            _tahlilPdfClear.AppearanceHovered.ForeColor = Color.White;
            _tahlilPdfClear.AppearanceHovered.Options.UseBackColor = true;
            _tahlilPdfClear.AppearanceHovered.Options.UseForeColor = true;
            _tahlilPdfClear.SizeChanged += (s, e) => _tahlilPdfClear.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _tahlilPdfClear.Width, _tahlilPdfClear.Height, 28, 28));
            _tahlilPdfClear.Click += (_, _) =>
            {
                _tahlilPdfBytes = null;
                _tahlilPdfName = null;
                if (_tahlilPdfInfo != null) _tahlilPdfInfo.Text = "PDF: seçilmedi";
            };

            _btnTahlilKaydet = new SimpleButton
            {
                Text = "TAHLİLİ KAYDET",
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _btnTahlilKaydet.LookAndFeel.UseDefaultLookAndFeel = false;
            _btnTahlilKaydet.Appearance.BackColor = ColorTranslator.FromHtml("#F59E0B");
            _btnTahlilKaydet.Appearance.ForeColor = ColorTranslator.FromHtml("#0B1220");
            _btnTahlilKaydet.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _btnTahlilKaydet.Appearance.Options.UseBackColor = true;
            _btnTahlilKaydet.Appearance.Options.UseForeColor = true;
            _btnTahlilKaydet.Appearance.Options.UseFont = true;
            _btnTahlilKaydet.AppearanceHovered.BackColor = ColorTranslator.FromHtml("#D97706");
            _btnTahlilKaydet.AppearanceHovered.Options.UseBackColor = true;
            _btnTahlilKaydet.AppearancePressed.BackColor = ColorTranslator.FromHtml("#B45309");
            _btnTahlilKaydet.AppearancePressed.Options.UseBackColor = true;
            _btnTahlilKaydet.SizeChanged += (s, e) => _btnTahlilKaydet.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _btnTahlilKaydet.Width, _btnTahlilKaydet.Height, 28, 28));
            _btnTahlilKaydet.Click += (_, _) => SaveTahlil();

            var pdfRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            pdfRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            pdfRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pdfRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
            pdfRow.Controls.Add(_tahlilPdfSelect, 0, 0);
            pdfRow.Controls.Add(_tahlilPdfInfo, 1, 0);
            pdfRow.Controls.Add(_tahlilPdfClear, 2, 0);

            var form = new TableLayoutPanel { Dock = DockStyle.Top, Height = 460, ColumnCount = 1, RowCount = 5, Padding = new Padding(0, 14, 0, 0) };
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));   // hasta
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));   // tür
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));   // pdf
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));  // not
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));   // kaydet

            form.Controls.Add(WrapField(_tahlilHastaSelect), 0, 0);
            form.Controls.Add(WrapField(_tahlilTurSelect), 0, 1);
            form.Controls.Add(WrapField(pdfRow), 0, 2);
            form.Controls.Add(WrapField(_tahlilNot), 0, 3);
            form.Controls.Add(_btnTahlilKaydet, 0, 4);

            content.Controls.Add(form);
            form.BringToFront();
            host.Controls.Add(BuildCard(content));
            return host;
        }

        private void LoadRandevular()
        {
            try
            {
                using var conn = SqlBaglantisi.Instance.GetConnection();
                string query = @"SELECT Randevuid,
                                        RandevuTarih,
                                        RandevuSaat,
                                        RandevuBrans,
                                        RandevuDoktor,
                                        CONCAT(LEFT(HastaTC,3),'******',RIGHT(HastaTC,2)) AS HastaTC,
                                        CASE WHEN RandevuDurum = 1 THEN 'Onaylı' ELSE 'Beklemede' END AS Durum
                                 FROM Tbl_Randevular
                                 WHERE RandevuTarih >= CAST(GETDATE() AS DATE)
                                 ORDER BY RandevuTarih ASC, RandevuSaat ASC";
                using var cmd = new SqlCommand(query, conn);
                var dt = new DataTable();
                using var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                _gridRandevu.DataSource = dt;
                try
                {
                    if (_viewRandevu?.Columns["Randevuid"] != null) _viewRandevu.Columns["Randevuid"].Visible = false;
                    if (_viewRandevu?.Columns["Durum"] != null) _viewRandevu.Columns["Durum"].Caption = "Durum";
                }
                catch { }
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void ApproveSelectedAppointment()
        {
            try
            {
                if (_viewRandevu == null || _viewRandevu.FocusedRowHandle < 0)
                {
                    XtraMessageBox.Show("Lütfen onaylamak için bir randevu seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var idObj = _viewRandevu.GetRowCellValue(_viewRandevu.FocusedRowHandle, "Randevuid");
                if (idObj == null || idObj == DBNull.Value)
                {
                    XtraMessageBox.Show("Seçili randevu bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int id = Convert.ToInt32(idObj);

                using (var conn = SqlBaglantisi.Instance.GetConnection())
                {
                    string q = "UPDATE Tbl_Randevular SET RandevuDurum = 1 WHERE Randevuid = @id";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int n = cmd.ExecuteNonQuery();
                        if (n > 0)
                            XtraMessageBox.Show("Randevu onaylandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                LoadRandevular();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Onay işlemi sırasında hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void LoadHastaList()
        {
            try
            {
                if (_gridHasta == null) return;
                using var conn = SqlBaglantisi.Instance.GetConnection();
                string q = @"SELECT CONCAT(LEFT(HastaTC,3),'******',RIGHT(HastaTC,2)) AS HastaTC,
                                    HastaAd,
                                    HastaSoyad,
                                    HastaTelefon
                             FROM Tbl_Hastalar
                             ORDER BY HastaAd, HastaSoyad";
                using var cmd = new SqlCommand(q, conn);
                var dt = new DataTable();
                using var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                _gridHasta.DataSource = dt;
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void LoadDoktorList()
        {
            try
            {
                if (_gridDoktor == null) return;
                using var conn = SqlBaglantisi.Instance.GetConnection();
                string q = @"SELECT CONCAT(LEFT(DoktorTC,3),'******',RIGHT(DoktorTC,2)) AS DoktorTC,
                                    DoktorAd,
                                    DoktorSoyad,
                                    DoktorBrans
                             FROM Tbl_Doktorlar
                             ORDER BY DoktorBrans, DoktorAd, DoktorSoyad";
                using var cmd = new SqlCommand(q, conn);
                var dt = new DataTable();
                using var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                _gridDoktor.DataSource = dt;
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void SaveTahlil()
        {
            try
            {
                string hastaTc = ExtractTc(_tahlilHastaSelect?.Text);
                if (string.IsNullOrWhiteSpace(hastaTc) || hastaTc.Length != 11)
                {
                    XtraMessageBox.Show("Lütfen hastayı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(_tahlilTurSelect?.Text))
                {
                    XtraMessageBox.Show("Lütfen tahlil türünü seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_tahlilPdfBytes == null || _tahlilPdfBytes.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen PDF tahlil sonucunu seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var conn = SqlBaglantisi.Instance.GetConnection();
                string q = @"INSERT INTO Tbl_Tahliller (TahlilAd, TahlilTur, TahlilTarih, DoktorAd, DoktorTC, TahlilSonuc, TahlilDurum,
                                                       TahlilPdf, TahlilPdfFileName, TahlilPdfMime, HastaTC, SekreterTC)
                             VALUES (@ad, @tur, GETDATE(), NULL, NULL, @sonuc, @durum,
                                     @pdf, @pdfName, @mime, @htc, @stc)";
                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@ad", _tahlilTurSelect.Text.Trim());
                cmd.Parameters.AddWithValue("@tur", _tahlilTurSelect.Text.Trim());
                cmd.Parameters.AddWithValue("@sonuc", string.IsNullOrWhiteSpace(_tahlilNot?.Text) ? (object)DBNull.Value : _tahlilNot.Text.Trim());
                cmd.Parameters.AddWithValue("@durum", "Yüklendi");
                cmd.Parameters.AddWithValue("@pdf", _tahlilPdfBytes);
                cmd.Parameters.AddWithValue("@pdfName", string.IsNullOrWhiteSpace(_tahlilPdfName) ? (object)DBNull.Value : _tahlilPdfName);
                cmd.Parameters.AddWithValue("@mime", "application/pdf");
                cmd.Parameters.AddWithValue("@htc", hastaTc);
                cmd.Parameters.AddWithValue("@stc", string.IsNullOrWhiteSpace(SekreterTC) ? (object)DBNull.Value : SekreterTC);
                cmd.ExecuteNonQuery();

                XtraMessageBox.Show("Tahlil kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _tahlilNot.Text = "";
                _tahlilTurSelect.EditValue = null;
                _tahlilPdfBytes = null;
                _tahlilPdfName = null;
                if (_tahlilPdfInfo != null) _tahlilPdfInfo.Text = "PDF: seçilmedi";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Tahlil kaydı başarısız: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private void PickTahlilPdf()
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "PDF Dosyası|*.pdf",
                Title = "Tahlil Sonucu PDF Seç"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            _tahlilPdfBytes = System.IO.File.ReadAllBytes(dlg.FileName);
            _tahlilPdfName = System.IO.Path.GetFileName(dlg.FileName);
            if (_tahlilPdfInfo != null) _tahlilPdfInfo.Text = "PDF: " + _tahlilPdfName;
        }

        private void LoadTahlilPatients()
        {
            try
            {
                if (_tahlilHastaSelect == null) return;
                using var conn = SqlBaglantisi.Instance.GetConnection();
                // Randevusu olan (onaylı) hastalar: bugün ve sonrası
                string q = @"SELECT DISTINCT r.HastaTC,
                                    ISNULL(h.HastaAd,'') AS HastaAd,
                                    ISNULL(h.HastaSoyad,'') AS HastaSoyad
                             FROM Tbl_Randevular r
                             LEFT JOIN Tbl_Hastalar h ON h.HastaTC = r.HastaTC
                             WHERE r.RandevuTarih >= CAST(GETDATE() AS DATE) AND r.RandevuDurum = 1 AND r.HastaTC IS NOT NULL
                             ORDER BY r.HastaTC";
                using var cmd = new SqlCommand(q, conn);
                using var dr = cmd.ExecuteReader();
                _tahlilHastaSelect.Properties.Items.Clear();
                while (dr.Read())
                {
                    var tc = dr["HastaTC"]?.ToString() ?? "";
                    var ad = dr["HastaAd"]?.ToString() ?? "";
                    var soyad = dr["HastaSoyad"]?.ToString() ?? "";
                    var item = string.IsNullOrWhiteSpace(ad + soyad) ? tc : $"{tc} - {ad} {soyad}".Trim();
                    _tahlilHastaSelect.Properties.Items.Add(item);
                }
            }
            catch { }
            finally { SqlBaglantisi.Instance.CloseConnection(); }
        }

        private static string ExtractTc(string itemText)
        {
            if (string.IsNullOrWhiteSpace(itemText)) return null;
            var t = itemText.Trim();
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
    }
}


