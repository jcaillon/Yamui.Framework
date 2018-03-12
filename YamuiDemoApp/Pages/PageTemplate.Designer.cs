using System.ComponentModel;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages {
    partial class PageTemplate {
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
            this.SuspendLayout();
            // 
            // yamuiScrollPage1
            // 

            // 
            // PageTemplate
            // 
            this.Name = "PageTemplate";
            this.Size = new System.Drawing.Size(715, 315);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
