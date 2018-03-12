using System.ComponentModel;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages {
    partial class SettingAppearance {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.comboTheme = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiComboBox3 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel22 = new YamuiFramework.Controls.YamuiLabel();
            this._simplePanelAccentColor = new YamuiFramework.Controls.YamuiSimplePanel();
            this.yamuiLabel21 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel20 = new YamuiFramework.Controls.YamuiLabel();
            this.SuspendLayout();
            // 
            // yamuiScrollPage1
            // 
            // 
            // yamuiScrollPage1.ContentPanel
            // 
            this.Controls.Add(this.comboTheme);
            this.Controls.Add(this.yamuiComboBox3);
            this.Controls.Add(this.yamuiLabel22);
            this.Controls.Add(this._simplePanelAccentColor);
            this.Controls.Add(this.yamuiLabel21);
            this.Controls.Add(this.yamuiLabel20);
            // 
            // comboTheme
            // 
            this.comboTheme.Location = new System.Drawing.Point(0, 29);
            this.comboTheme.Name = "comboTheme";
            this.comboTheme.Size = new System.Drawing.Size(180, 21);
            this.comboTheme.TabIndex = 19;
            // 
            // yamuiComboBox3
            // 
            this.yamuiComboBox3.Location = new System.Drawing.Point(0, 256);
            this.yamuiComboBox3.Name = "yamuiComboBox3";
            this.yamuiComboBox3.Size = new System.Drawing.Size(121, 21);
            this.yamuiComboBox3.TabIndex = 18;
            // 
            // yamuiLabel22
            // 
            this.yamuiLabel22.AutoSize = true;
            this.yamuiLabel22.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel22.Location = new System.Drawing.Point(0, 227);
            this.yamuiLabel22.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel22.Name = "yamuiLabel22";
            this.yamuiLabel22.Size = new System.Drawing.Size(78, 19);
            this.yamuiLabel22.TabIndex = 17;
            this.yamuiLabel22.Text = "FONT SIZE";
            // 
            // PanelAccentColor
            // 
            this._simplePanelAccentColor.Location = new System.Drawing.Point(0, 101);
            this._simplePanelAccentColor.Margin = new System.Windows.Forms.Padding(0);
            this._simplePanelAccentColor.Name = "_simplePanelAccentColor";
            this._simplePanelAccentColor.Size = new System.Drawing.Size(715, 108);
            this._simplePanelAccentColor.TabIndex = 16;
            // 
            // yamuiLabel21
            // 
            this.yamuiLabel21.AutoSize = true;
            this.yamuiLabel21.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel21.Location = new System.Drawing.Point(0, 75);
            this.yamuiLabel21.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel21.Name = "yamuiLabel21";
            this.yamuiLabel21.Size = new System.Drawing.Size(114, 19);
            this.yamuiLabel21.TabIndex = 15;
            this.yamuiLabel21.Text = "ACCENT COLOR";
            // 
            // yamuiLabel20
            // 
            this.yamuiLabel20.AutoSize = true;
            this.yamuiLabel20.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel20.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel20.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel20.Name = "yamuiLabel20";
            this.yamuiLabel20.Size = new System.Drawing.Size(55, 19);
            this.yamuiLabel20.TabIndex = 14;
            this.yamuiLabel20.Text = "THEME";
            // 
            // SettingAppearance
            // 
            this.Name = "SettingAppearance";
            this.Size = new System.Drawing.Size(715, 315);
            this.ResumeLayout(false);

        }

        #endregion
        
        private YamuiComboBox comboTheme;
        private YamuiComboBox yamuiComboBox3;
        private YamuiLabel yamuiLabel22;
        private YamuiSimplePanel _simplePanelAccentColor;
        private YamuiLabel yamuiLabel21;
        private YamuiLabel yamuiLabel20;


    }
}
