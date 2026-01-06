namespace Hastane_Otomasyonu
{
    partial class FrmHastaGiris
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.Utils.Animation.Transition transition1 = new DevExpress.Utils.Animation.Transition();
            DevExpress.Utils.Animation.SlideFadeTransition slideFadeTransition1 = new DevExpress.Utils.Animation.SlideFadeTransition();
            this.pnlMainCard = new DevExpress.XtraEditors.PanelControl();
            this.hypKayitOl = new DevExpress.XtraEditors.HyperlinkLabelControl();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            this.btnLogin = new DevExpress.XtraEditors.SimpleButton();
            this.txtSifre = new DevExpress.XtraEditors.TextEdit();
            this.txtTC = new DevExpress.XtraEditors.TextEdit();
            this.lblSubTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.transitionManager1 = new DevExpress.Utils.Animation.TransitionManager(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pnlMainCard)).BeginInit();
            this.pnlMainCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSifre.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTC.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMainCard
            // 
            this.pnlMainCard.Appearance.BackColor = System.Drawing.Color.White;
            this.pnlMainCard.Appearance.Options.UseBackColor = true;
            this.pnlMainCard.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlMainCard.Controls.Add(this.hypKayitOl);
            this.pnlMainCard.Controls.Add(this.btnClose);
            this.pnlMainCard.Controls.Add(this.btnLogin);
            this.pnlMainCard.Controls.Add(this.txtSifre);
            this.pnlMainCard.Controls.Add(this.txtTC);
            this.pnlMainCard.Controls.Add(this.lblSubTitle);
            this.pnlMainCard.Controls.Add(this.lblTitle);
            this.pnlMainCard.Location = new System.Drawing.Point(100, 100);
            this.pnlMainCard.Name = "pnlMainCard";
            this.pnlMainCard.Size = new System.Drawing.Size(400, 500);
            this.pnlMainCard.TabIndex = 0;
            // 
            // hypKayitOl
            // 
            this.hypKayitOl.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.hypKayitOl.Appearance.Options.UseFont = true;
            this.hypKayitOl.Location = new System.Drawing.Point(145, 410);
            this.hypKayitOl.Name = "hypKayitOl";
            this.hypKayitOl.Size = new System.Drawing.Size(110, 17);
            this.hypKayitOl.TabIndex = 6;
            this.hypKayitOl.Text = "Hesabınız yok mu? <href>Kayıt Ol</href>";
            this.hypKayitOl.Click += new System.EventHandler(this.hypKayitOl_Click);
            // 
            // btnClose
            // 
            this.btnClose.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.btnClose.Appearance.Options.UseForeColor = true;
            this.btnClose.Location = new System.Drawing.Point(365, 10);
            this.btnClose.LookAndFeel.UseDefaultLookAndFeel = false;
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(25, 25);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "X";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(165)))), ((int)(((byte)(211)))));
            this.btnLogin.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.btnLogin.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Appearance.Options.UseBackColor = true;
            this.btnLogin.Appearance.Options.UseFont = true;
            this.btnLogin.Appearance.Options.UseForeColor = true;
            this.btnLogin.Location = new System.Drawing.Point(50, 330);
            this.btnLogin.LookAndFeel.UseDefaultLookAndFeel = false;
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(300, 60);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "GİRİŞ YAP";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // txtSifre
            // 
            this.txtSifre.Location = new System.Drawing.Point(50, 245);
            this.txtSifre.Name = "txtSifre";
            this.txtSifre.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtSifre.Properties.Appearance.Options.UseFont = true;
            this.txtSifre.Properties.AutoHeight = false;
            this.txtSifre.Properties.NullValuePrompt = "Şifre";
            this.txtSifre.Properties.PasswordChar = '*';
            this.txtSifre.Size = new System.Drawing.Size(300, 45);
            this.txtSifre.TabIndex = 3;
            // 
            // txtTC
            // 
            this.txtTC.Location = new System.Drawing.Point(50, 180);
            this.txtTC.Name = "txtTC";
            this.txtTC.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtTC.Properties.Appearance.Options.UseFont = true;
            this.txtTC.Properties.AutoHeight = false;
            this.txtTC.Properties.Mask.EditMask = "00000000000";
            this.txtTC.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Simple;
            this.txtTC.Properties.NullValuePrompt = "TC Kimlik No";
            this.txtTC.Size = new System.Drawing.Size(300, 45);
            this.txtTC.TabIndex = 2;
            // 
            // lblSubTitle
            // 
            this.lblSubTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblSubTitle.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.lblSubTitle.Appearance.Options.UseFont = true;
            this.lblSubTitle.Appearance.Options.UseForeColor = true;
            this.lblSubTitle.Location = new System.Drawing.Point(85, 105);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Size = new System.Drawing.Size(230, 21);
            this.lblSubTitle.TabIndex = 1;
            this.lblSubTitle.Text = "Lütfen bilgilerinizle giriş yapın.";
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(165)))), ((int)(((byte)(211)))));
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Location = new System.Drawing.Point(75, 50);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(250, 47);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "HASTA GİRİŞİ";
            // 
            // transitionManager1
            // 
            transition1.Control = this;
            transition1.TransitionType = slideFadeTransition1;
            this.transitionManager1.Transitions.Add(transition1);
            // 
            // FrmHastaGiris
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 700);
            this.Controls.Add(this.pnlMainCard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmHastaGiris";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hasta Giriş";
            this.Load += new System.EventHandler(this.FrmHastaGiris_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FrmHastaGiris_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.pnlMainCard)).EndInit();
            this.pnlMainCard.ResumeLayout(false);
            this.pnlMainCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSifre.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTC.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        private DevExpress.XtraEditors.PanelControl pnlMainCard;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubTitle;
        private DevExpress.XtraEditors.TextEdit txtTC;
        private DevExpress.XtraEditors.TextEdit txtSifre;
        private DevExpress.XtraEditors.SimpleButton btnLogin;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.HyperlinkLabelControl hypKayitOl;
        private DevExpress.Utils.Animation.TransitionManager transitionManager1;
    }
}
