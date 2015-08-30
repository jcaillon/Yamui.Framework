using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;

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
            this.yamuiPanel1 = new YamuiFramework.Controls.YamuiPanel();
            this.yamuiComboBox2 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiComboBox1 = new YamuiFramework.Controls.YamuiComboBox();
            this.yamuiLabel11 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiDateTime2 = new YamuiFramework.Controls.YamuiDateTime();
            this.yamuiDateTime1 = new YamuiFramework.Controls.YamuiDateTime();
            this.yamuiLabel10 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiTrackBar2 = new YamuiFramework.Controls.YamuiSlider();
            this.yamuiTrackBar1 = new YamuiFramework.Controls.YamuiSlider();
            this.yamuiLabel7 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // yamuiPanel1
            // 
            this.yamuiPanel1.Controls.Add(this.yamuiComboBox2);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel11);
            this.yamuiPanel1.Controls.Add(this.yamuiComboBox1);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel7);
            this.yamuiPanel1.Controls.Add(this.yamuiDateTime2);
            this.yamuiPanel1.Controls.Add(this.yamuiTrackBar1);
            this.yamuiPanel1.Controls.Add(this.yamuiDateTime1);
            this.yamuiPanel1.Controls.Add(this.yamuiTrackBar2);
            this.yamuiPanel1.Controls.Add(this.yamuiLabel10);
            this.yamuiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yamuiPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.HorizontalScrollbarSize = 10;
            this.yamuiPanel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiPanel1.Name = "yamuiPanel1";
            this.yamuiPanel1.Size = new System.Drawing.Size(715, 315);
            this.yamuiPanel1.TabIndex = 0;
            this.yamuiPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.yamuiPanel1.VerticalScrollbarSize = 10;
            // 
            // yamuiComboBox2
            // 
            this.yamuiComboBox2.Enabled = false;
            this.yamuiComboBox2.ItemHeight = 19;
            this.yamuiComboBox2.Location = new System.Drawing.Point(0, 140);
            this.yamuiComboBox2.Name = "yamuiComboBox2";
            this.yamuiComboBox2.Size = new System.Drawing.Size(121, 25);
            this.yamuiComboBox2.TabIndex = 21;
            // 
            // yamuiComboBox1
            // 
            this.yamuiComboBox1.ItemHeight = 19;
            this.yamuiComboBox1.Items.AddRange(new object[] {
            "test1",
            "test2",
            "test1",
            "test2",
            "test1",
            "test2",
            "test1",
            "test20",
            "test21",
            "test22",
            "test1",
            "test2",
            "test1",
            "test2"});
            this.yamuiComboBox1.Location = new System.Drawing.Point(0, 109);
            this.yamuiComboBox1.Name = "yamuiComboBox1";
            this.yamuiComboBox1.Size = new System.Drawing.Size(121, 25);
            this.yamuiComboBox1.TabIndex = 20;
            this.yamuiComboBox1.WaterMark = "Water mark !";
            // 
            // yamuiLabel11
            // 
            this.yamuiLabel11.AutoSize = true;
            this.yamuiLabel11.Function = LabelFunction.Heading;
            this.yamuiLabel11.Location = new System.Drawing.Point(0, 87);
            this.yamuiLabel11.Name = "yamuiLabel11";
            this.yamuiLabel11.Size = new System.Drawing.Size(95, 19);
            this.yamuiLabel11.TabIndex = 19;
            this.yamuiLabel11.Text = "COMBO BOX";
            // 
            // yamuiDateTime2
            // 
            this.yamuiDateTime2.Enabled = false;
            this.yamuiDateTime2.Location = new System.Drawing.Point(0, 233);
            this.yamuiDateTime2.Name = "yamuiDateTime2";
            this.yamuiDateTime2.Size = new System.Drawing.Size(200, 20);
            this.yamuiDateTime2.TabIndex = 18;
            // 
            // yamuiDateTime1
            // 
            this.yamuiDateTime1.Location = new System.Drawing.Point(0, 207);
            this.yamuiDateTime1.Name = "yamuiDateTime1";
            this.yamuiDateTime1.Size = new System.Drawing.Size(200, 20);
            this.yamuiDateTime1.TabIndex = 17;
            // 
            // yamuiLabel10
            // 
            this.yamuiLabel10.AutoSize = true;
            this.yamuiLabel10.Function = LabelFunction.Heading;
            this.yamuiLabel10.Location = new System.Drawing.Point(0, 185);
            this.yamuiLabel10.Name = "yamuiLabel10";
            this.yamuiLabel10.Size = new System.Drawing.Size(80, 19);
            this.yamuiLabel10.TabIndex = 16;
            this.yamuiLabel10.Text = "DATE TIME";
            // 
            // yamuiTrackBar2
            // 
            this.yamuiTrackBar2.Enabled = false;
            this.yamuiTrackBar2.Location = new System.Drawing.Point(0, 51);
            this.yamuiTrackBar2.Name = "yamuiTrackBar2";
            this.yamuiTrackBar2.Size = new System.Drawing.Size(191, 23);
            this.yamuiTrackBar2.TabIndex = 15;
            this.yamuiTrackBar2.Text = "yamuiTrackBar2";
            // 
            // yamuiTrackBar1
            // 
            this.yamuiTrackBar1.Location = new System.Drawing.Point(0, 22);
            this.yamuiTrackBar1.Name = "yamuiTrackBar1";
            this.yamuiTrackBar1.Size = new System.Drawing.Size(191, 23);
            this.yamuiTrackBar1.TabIndex = 14;
            this.yamuiTrackBar1.Text = "yamuiTrackBar1";
            // 
            // yamuiLabel7
            // 
            this.yamuiLabel7.AutoSize = true;
            this.yamuiLabel7.Function = LabelFunction.Heading;
            this.yamuiLabel7.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel7.Name = "yamuiLabel7";
            this.yamuiLabel7.Size = new System.Drawing.Size(54, 19);
            this.yamuiLabel7.TabIndex = 13;
            this.yamuiLabel7.Text = "SLIDER";
            // 
            // ItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.yamuiPanel1);
            this.Name = "ItemControl";
            this.Size = new System.Drawing.Size(715, 315);
            this.yamuiPanel1.ResumeLayout(false);
            this.yamuiPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiPanel yamuiPanel1;
        private YamuiComboBox yamuiComboBox2;
        private YamuiLabel yamuiLabel11;
        private YamuiComboBox yamuiComboBox1;
        private YamuiLabel yamuiLabel7;
        private YamuiDateTime yamuiDateTime2;
        private YamuiSlider yamuiTrackBar1;
        private YamuiDateTime yamuiDateTime1;
        private YamuiSlider yamuiTrackBar2;
        private YamuiLabel yamuiLabel10;
    }
}
