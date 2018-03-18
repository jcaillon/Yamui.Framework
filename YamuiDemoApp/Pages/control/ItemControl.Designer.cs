using System.ComponentModel;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages.control {
    partial class ItemControl {
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
            this.yamuiComboBox2 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel11 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiComboBox1 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel7 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTrackBar1 = new YamuiFramework.Controls.YamuiSlider();
            this.yamuiTrackBar2 = new YamuiFramework.Controls.YamuiSlider();
            this.SuspendLayout();
            // 
            // yamuiComboBox2
            // 
            this.yamuiComboBox2.BackGrndImage = null;
            this.yamuiComboBox2.Enabled = false;
            this.yamuiComboBox2.GreyScaleBackGrndImage = null;
            this.yamuiComboBox2.IsFocused = false;
            this.yamuiComboBox2.IsHovered = false;
            this.yamuiComboBox2.IsPressed = false;
            this.yamuiComboBox2.Location = new System.Drawing.Point(0, 140);
            this.yamuiComboBox2.Name = "yamuiComboBox2";
            this.yamuiComboBox2.SetImgSize = new System.Drawing.Size(0, 0);
            this.yamuiComboBox2.Size = new System.Drawing.Size(121, 21);
            this.yamuiComboBox2.TabIndex = 27;
            // 
            // yamuiLabel11
            // 
            this.yamuiLabel11.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel11.Location = new System.Drawing.Point(0, 87);
            this.yamuiLabel11.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel11.Name = "yamuiLabel11";
            this.yamuiLabel11.Size = new System.Drawing.Size(95, 19);
            this.yamuiLabel11.TabIndex = 25;
            this.yamuiLabel11.Text = "COMBO BOX";
            // 
            // yamuiComboBox1
            // 
            this.yamuiComboBox1.BackGrndImage = null;
            this.yamuiComboBox1.GreyScaleBackGrndImage = null;
            this.yamuiComboBox1.IsFocused = false;
            this.yamuiComboBox1.IsHovered = false;
            this.yamuiComboBox1.IsPressed = false;
            this.yamuiComboBox1.Location = new System.Drawing.Point(0, 109);
            this.yamuiComboBox1.Name = "yamuiComboBox1";
            this.yamuiComboBox1.SetImgSize = new System.Drawing.Size(0, 0);
            this.yamuiComboBox1.Size = new System.Drawing.Size(121, 21);
            this.yamuiComboBox1.TabIndex = 26;
            this.yamuiComboBox1.Text = "test1";
            this.yamuiComboBox1.WaterMark = "Water mark !";
            // 
            // yamuiLabel7
            // 
            this.yamuiLabel7.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel7.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel7.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel7.Name = "yamuiLabel7";
            this.yamuiLabel7.Size = new System.Drawing.Size(54, 19);
            this.yamuiLabel7.TabIndex = 22;
            this.yamuiLabel7.Text = "SLIDER";
            // 
            // yamuiTrackBar1
            // 
            this.yamuiTrackBar1.Location = new System.Drawing.Point(0, 22);
            this.yamuiTrackBar1.Name = "yamuiTrackBar1";
            this.yamuiTrackBar1.Size = new System.Drawing.Size(212, 23);
            this.yamuiTrackBar1.TabIndex = 23;
            this.yamuiTrackBar1.Text = "yamuiTrackBar1";
            // 
            // yamuiTrackBar2
            // 
            this.yamuiTrackBar2.Enabled = false;
            this.yamuiTrackBar2.Location = new System.Drawing.Point(0, 51);
            this.yamuiTrackBar2.Name = "yamuiTrackBar2";
            this.yamuiTrackBar2.Size = new System.Drawing.Size(191, 23);
            this.yamuiTrackBar2.TabIndex = 24;
            this.yamuiTrackBar2.Text = "yamuiTrackBar2";
            // 
            // ItemControl
            // 
            this.AutoScrollMinSize = new System.Drawing.Size(-503, -154);
            this.Controls.Add(this.yamuiComboBox2);
            this.Controls.Add(this.yamuiLabel11);
            this.Controls.Add(this.yamuiComboBox1);
            this.Controls.Add(this.yamuiLabel7);
            this.Controls.Add(this.yamuiTrackBar1);
            this.Controls.Add(this.yamuiTrackBar2);
            this.Name = "ItemControl";
            this.Size = new System.Drawing.Size(715, 315);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private YamuiComboBox yamuiComboBox2;
        private YamuiLabel yamuiLabel11;
        private YamuiComboBox yamuiComboBox1;
        private YamuiLabel yamuiLabel7;
        private YamuiSlider yamuiTrackBar1;
        private YamuiSlider yamuiTrackBar2;

    }
}
