using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Hastane_Otomasyonu.Controls
{
    public class ModernComboBox : UserControl
    {
        private string _placeholder = "Seçiniz...";
        private object _selectedItem = null;
        private List<object> _items = new List<object>();
        private bool _isHovered = false;
        private bool _isFocused = false;
        private bool _isDroppedDown = false;
        private Form _dropdownForm;
        private ListBox _listBox;

        public event EventHandler SelectedIndexChanged;

        public List<object> Items => _items;

        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value; Invalidate(); }
        }

        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public object EditValue
        {
            get => SelectedItem;
            set => SelectedItem = value;
        }

        public override string Text => _selectedItem?.ToString() ?? "";

        public ModernComboBox()
        {
            this.Size = new Size(250, 45);
            this.BackColor = Color.FromArgb(245, 245, 245); // WhiteSmoke
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;

            InitializeDropdown();
        }

        private void InitializeDropdown()
        {
            _listBox = new ListBox
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                ItemHeight = 35,
                DrawMode = DrawMode.OwnerDrawFixed
            };

            _listBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                
                bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                e.Graphics.FillRectangle(new SolidBrush(isSelected ? Color.FromArgb(37, 99, 235) : Color.White), e.Bounds);
                
                string text = _listBox.Items[e.Index].ToString();
                TextRenderer.DrawText(e.Graphics, text, _listBox.Font, e.Bounds, 
                    isSelected ? Color.White : Color.FromArgb(30, 41, 59), 
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };

            _listBox.Click += (s, e) =>
            {
                if (_listBox.SelectedItem != null)
                {
                    SelectedItem = _listBox.SelectedItem;
                    CloseDropdown();
                }
            };

            _dropdownForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                BackColor = Color.White,
                TopMost = true
            };
            
            Panel pnl = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(1) };
            pnl.Controls.Add(_listBox);
            _dropdownForm.Controls.Add(pnl);

            _dropdownForm.Deactivate += (s, e) => CloseDropdown();
        }

        private void OpenDropdown()
        {
            if (_items.Count == 0) return;

            _listBox.Items.Clear();
            foreach (var item in _items) _listBox.Items.Add(item);

            Point screenPoint = this.PointToScreen(new Point(0, this.Height));
            _dropdownForm.Location = screenPoint;
            _dropdownForm.Width = this.Width;
            _dropdownForm.Height = Math.Min(200, _items.Count * 35 + 5);
            
            _isDroppedDown = true;
            _isFocused = true;
            _dropdownForm.Show();
            Invalidate();
        }

        private void CloseDropdown()
        {
            _dropdownForm.Hide();
            _isDroppedDown = false;
            _isFocused = false;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_isDroppedDown) CloseDropdown();
            else OpenDropdown();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            this.BackColor = Color.White;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            this.BackColor = Color.FromArgb(245, 245, 245);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background & Border
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            GraphicsPath path = GetRoundedRect(rect, 15);

            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillPath(brush, path);
            }

            Color borderColor = _isFocused ? Color.FromArgb(37, 99, 235) : 
                               (_isHovered ? Color.FromArgb(203, 213, 225) : Color.FromArgb(226, 232, 240));
            using (Pen pen = new Pen(borderColor, _isFocused ? 2 : 1))
            {
                g.DrawPath(pen, path);
            }

            // Text / Placeholder
            string displayLink = _selectedItem != null ? _selectedItem.ToString() : _placeholder;
            Color textColor = _selectedItem != null ? Color.FromArgb(30, 41, 59) : Color.FromArgb(148, 163, 184);
            
            TextRenderer.DrawText(g, displayLink, new Font("Segoe UI Semibold", 10.5F), 
                new Rectangle(15, 0, Width - 50, Height), textColor, 
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            // Chevron Down Icon
            int iconX = Width - 30;
            int iconY = Height / 2;
            using (Pen iconPen = new Pen(Color.FromArgb(100, 116, 139), 2))
            {
                g.DrawLine(iconPen, iconX - 5, iconY - 2, iconX, iconY + 3);
                g.DrawLine(iconPen, iconX, iconY + 3, iconX + 5, iconY - 2);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

