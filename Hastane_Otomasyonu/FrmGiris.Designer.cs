namespace Hastane_Otomasyonu
{
    partial class FrmGiris
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmGiris));
            pnlOverlay = new DevExpress.XtraEditors.PanelControl();
            btnClose = new DevExpress.XtraEditors.SimpleButton();
            pnlMainCard = new DevExpress.XtraEditors.PanelControl();
            stackPanel1 = new DevExpress.Utils.Layout.StackPanel();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubTitle = new DevExpress.XtraEditors.LabelControl();
            btnHastaGiris = new DevExpress.XtraEditors.SimpleButton();
            btnDoktorGiris = new DevExpress.XtraEditors.SimpleButton();
            btnSekreterGiris = new DevExpress.XtraEditors.SimpleButton();
            lblFooter = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)pnlOverlay).BeginInit();
            pnlOverlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlMainCard).BeginInit();
            pnlMainCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)stackPanel1).BeginInit();
            stackPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlOverlay
            // 
            pnlOverlay.Appearance.BackColor = System.Drawing.Color.FromArgb(160, 31, 58, 86);
            pnlOverlay.Appearance.Options.UseBackColor = true;
            pnlOverlay.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlOverlay.Controls.Add(btnClose);
            pnlOverlay.Controls.Add(pnlMainCard);
            pnlOverlay.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlOverlay.Location = new System.Drawing.Point(0, 0);
            pnlOverlay.Name = "pnlOverlay";
            pnlOverlay.Size = new System.Drawing.Size(1200, 800);
            pnlOverlay.TabIndex = 6;
            // 
            // btnClose
            // 
            btnClose.AllowFocus = false;
            btnClose.Appearance.BackColor = System.Drawing.Color.Transparent;
            btnClose.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            btnClose.Appearance.ForeColor = System.Drawing.Color.FromArgb(189, 195, 199);
            btnClose.Appearance.Options.UseBackColor = true;
            btnClose.Appearance.Options.UseFont = true;
            btnClose.Appearance.Options.UseForeColor = true;
            btnClose.AppearanceHovered.ForeColor = System.Drawing.Color.White;
            btnClose.AppearanceHovered.Options.UseForeColor = true;
            btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.Location = new System.Drawing.Point(1150, 15);
            btnClose.Name = "btnClose";
            btnClose.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            btnClose.Size = new System.Drawing.Size(35, 35);
            btnClose.TabIndex = 7;
            btnClose.Text = "✕";
            btnClose.Click += btnClose_Click;
            // 
            // pnlMainCard
            // 
            pnlMainCard.Appearance.BackColor = System.Drawing.Color.FromArgb(235, 241, 247);
            pnlMainCard.Appearance.Options.UseBackColor = true;
            pnlMainCard.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMainCard.Controls.Add(stackPanel1);
            pnlMainCard.Location = new System.Drawing.Point(375, 100);
            pnlMainCard.Name = "pnlMainCard";
            pnlMainCard.Size = new System.Drawing.Size(450, 600);
            pnlMainCard.TabIndex = 0;
            // 
            // stackPanel1
            // 
            stackPanel1.Appearance.BackColor = System.Drawing.Color.Azure;
            stackPanel1.Appearance.Options.UseBackColor = true;
            stackPanel1.Controls.Add(lblTitle);
            stackPanel1.Controls.Add(lblSubTitle);
            stackPanel1.Controls.Add(btnHastaGiris);
            stackPanel1.Controls.Add(btnDoktorGiris);
            stackPanel1.Controls.Add(btnSekreterGiris);
            stackPanel1.Controls.Add(lblFooter);
            stackPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            stackPanel1.LayoutDirection = DevExpress.Utils.Layout.StackPanelLayoutDirection.TopDown;
            stackPanel1.Location = new System.Drawing.Point(0, 0);
            stackPanel1.Name = "stackPanel1";
            stackPanel1.Padding = new System.Windows.Forms.Padding(40);
            stackPanel1.Size = new System.Drawing.Size(450, 600);
            stackPanel1.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AllowHtmlString = true;
            lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 42F, System.Drawing.FontStyle.Bold);
            lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(28, 40, 51);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Appearance.Options.UseTextOptions = true;
            lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblTitle.Location = new System.Drawing.Point(40, 43);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(370, 80);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "<color=#2E86C1>H</color>ASTANE";
            // 
            // lblSubTitle
            // 
            lblSubTitle.Appearance.Font = new System.Drawing.Font("Segoe UI Light", 14F);
            lblSubTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(86, 101, 115);
            lblSubTitle.Appearance.Options.UseFont = true;
            lblSubTitle.Appearance.Options.UseForeColor = true;
            lblSubTitle.Appearance.Options.UseTextOptions = true;
            lblSubTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblSubTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSubTitle.Location = new System.Drawing.Point(40, 129);
            lblSubTitle.Margin = new System.Windows.Forms.Padding(3, 3, 3, 60);
            lblSubTitle.Name = "lblSubTitle";
            lblSubTitle.Size = new System.Drawing.Size(370, 30);
            lblSubTitle.TabIndex = 1;
            lblSubTitle.Text = "Dijital Sağlık Yönetim Platformu";
            // 
            // btnHastaGiris
            // 
            btnHastaGiris.AllowFocus = false;
            btnHastaGiris.Appearance.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            btnHastaGiris.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            btnHastaGiris.Appearance.ForeColor = System.Drawing.Color.White;
            btnHastaGiris.Appearance.Options.UseBackColor = true;
            btnHastaGiris.Appearance.Options.UseFont = true;
            btnHastaGiris.Appearance.Options.UseForeColor = true;
            btnHastaGiris.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            btnHastaGiris.AppearanceHovered.Options.UseBackColor = true;
            btnHastaGiris.AppearancePressed.BackColor = System.Drawing.Color.FromArgb(33, 97, 140);
            btnHastaGiris.AppearancePressed.Options.UseBackColor = true;
            btnHastaGiris.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnHastaGiris.Cursor = System.Windows.Forms.Cursors.Hand;
            btnHastaGiris.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnHastaGiris.Location = new System.Drawing.Point(40, 219);
            btnHastaGiris.LookAndFeel.UseDefaultLookAndFeel = false;
            btnHastaGiris.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            btnHastaGiris.Name = "btnHastaGiris";
            btnHastaGiris.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            btnHastaGiris.Size = new System.Drawing.Size(370, 65);
            btnHastaGiris.TabIndex = 2;
            btnHastaGiris.Text = "  HASTA SİSTEMİ";
            btnHastaGiris.Click += btnHastaGiris_Click;
            // 
            // btnDoktorGiris
            // 
            btnDoktorGiris.AllowFocus = false;
            btnDoktorGiris.Appearance.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            btnDoktorGiris.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            btnDoktorGiris.Appearance.ForeColor = System.Drawing.Color.White;
            btnDoktorGiris.Appearance.Options.UseBackColor = true;
            btnDoktorGiris.Appearance.Options.UseFont = true;
            btnDoktorGiris.Appearance.Options.UseForeColor = true;
            btnDoktorGiris.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(46, 204, 113);
            btnDoktorGiris.AppearanceHovered.Options.UseBackColor = true;
            btnDoktorGiris.AppearancePressed.BackColor = System.Drawing.Color.FromArgb(30, 132, 73);
            btnDoktorGiris.AppearancePressed.Options.UseBackColor = true;
            btnDoktorGiris.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnDoktorGiris.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDoktorGiris.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnDoktorGiris.Location = new System.Drawing.Point(40, 299);
            btnDoktorGiris.LookAndFeel.UseDefaultLookAndFeel = false;
            btnDoktorGiris.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            btnDoktorGiris.Name = "btnDoktorGiris";
            btnDoktorGiris.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            btnDoktorGiris.Size = new System.Drawing.Size(370, 65);
            btnDoktorGiris.TabIndex = 3;
            btnDoktorGiris.Text = "  DOKTOR PANELİ";
            btnDoktorGiris.Click += btnDoktorGiris_Click;
            // 
            // btnSekreterGiris
            // 
            btnSekreterGiris.AllowFocus = false;
            btnSekreterGiris.Appearance.BackColor = System.Drawing.Color.FromArgb(211, 84, 0);
            btnSekreterGiris.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            btnSekreterGiris.Appearance.ForeColor = System.Drawing.Color.White;
            btnSekreterGiris.Appearance.Options.UseBackColor = true;
            btnSekreterGiris.Appearance.Options.UseFont = true;
            btnSekreterGiris.Appearance.Options.UseForeColor = true;
            btnSekreterGiris.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(230, 126, 34);
            btnSekreterGiris.AppearanceHovered.Options.UseBackColor = true;
            btnSekreterGiris.AppearancePressed.BackColor = System.Drawing.Color.FromArgb(175, 62, 7);
            btnSekreterGiris.AppearancePressed.Options.UseBackColor = true;
            btnSekreterGiris.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnSekreterGiris.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSekreterGiris.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnSekreterGiris.Location = new System.Drawing.Point(40, 379);
            btnSekreterGiris.LookAndFeel.UseDefaultLookAndFeel = false;
            btnSekreterGiris.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            btnSekreterGiris.Name = "btnSekreterGiris";
            btnSekreterGiris.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            btnSekreterGiris.Size = new System.Drawing.Size(370, 65);
            btnSekreterGiris.TabIndex = 4;
            btnSekreterGiris.Text = "  SEKRETER GİRİŞİ";
            btnSekreterGiris.Click += btnSekreterGiris_Click;
            // 
            // lblFooter
            // 
            lblFooter.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblFooter.Appearance.ForeColor = System.Drawing.Color.FromArgb(189, 195, 199);
            lblFooter.Appearance.Options.UseFont = true;
            lblFooter.Appearance.Options.UseForeColor = true;
            lblFooter.Location = new System.Drawing.Point(147, 529);
            lblFooter.Margin = new System.Windows.Forms.Padding(3, 70, 3, 3);
            lblFooter.Name = "lblFooter";
            lblFooter.Size = new System.Drawing.Size(156, 15);
            lblFooter.TabIndex = 5;
            lblFooter.Text = "© 2026 Hastane Otomasyonu";
            // 
            // FrmGiris
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImageLayoutStore = System.Windows.Forms.ImageLayout.Stretch;
            BackgroundImageStore = (System.Drawing.Image)resources.GetObject("$this.BackgroundImageStore");
            ClientSize = new System.Drawing.Size(1200, 800);
            Controls.Add(pnlOverlay);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "FrmGiris";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Hospital Management Login";
            Load += FrmGiris_Load;
            ((System.ComponentModel.ISupportInitialize)pnlOverlay).EndInit();
            pnlOverlay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlMainCard).EndInit();
            pnlMainCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)stackPanel1).EndInit();
            stackPanel1.ResumeLayout(false);
            stackPanel1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl pnlOverlay;
        private DevExpress.XtraEditors.PanelControl pnlMainCard;
        private DevExpress.Utils.Layout.StackPanel stackPanel1;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubTitle;
        private DevExpress.XtraEditors.SimpleButton btnHastaGiris;
        private DevExpress.XtraEditors.SimpleButton btnDoktorGiris;
        private DevExpress.XtraEditors.SimpleButton btnSekreterGiris;
        private DevExpress.XtraEditors.LabelControl lblFooter;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}
