namespace YamuiDemoApp.Pages.Navigation
{
    partial class UserControl1
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.scrollPanelTest1 = new YamuiDemoApp.Pages.Navigation.ScrollPanelTest();
            this.htmlLabel1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.SuspendLayout();
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.Location = new System.Drawing.Point(394, 24);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(75, 23);
            this.yamuiLabel1.TabIndex = 2;
            this.yamuiLabel1.Text = "yamuiLabel1";
            // 
            // scrollPanelTest1
            // 
            this.scrollPanelTest1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.scrollPanelTest1.Location = new System.Drawing.Point(4, 4);
            this.scrollPanelTest1.Name = "scrollPanelTest1";
            this.scrollPanelTest1.Size = new System.Drawing.Size(349, 42);
            this.scrollPanelTest1.TabIndex = 3;
            this.scrollPanelTest1.Text = "scrollPanelTest1";
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.BackColor = System.Drawing.SystemColors.Window;
            this.htmlLabel1.BaseStylesheet = null;
            this.htmlLabel1.Location = new System.Drawing.Point(14, 299);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(59, 15);
            this.htmlLabel1.TabIndex = 4;
            this.htmlLabel1.TabStop = false;
            this.htmlLabel1.Text = "htmlLabel1";
            // 
            // UserControl1
            // 
            this.AutoScrollMinSize = new System.Drawing.Size(0, -18);
            this.Controls.Add(this.htmlLabel1);
            this.Controls.Add(this.scrollPanelTest1);
            this.Controls.Add(this.yamuiLabel1);
            this.Name = "UserControl1";
            this.Size = new System.Drawing.Size(449, 272);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private YamuiFramework.Controls.YamuiLabel yamuiLabel1;
        private ScrollPanelTest scrollPanelTest1;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlLabel htmlLabel1;
    }
}
