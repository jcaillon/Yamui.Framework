using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Demo;
using YamuiFramework.Controls;
using YamuiFramework.Forms;

namespace YamuiDemoApp.Pages {
    public partial class Other : YamuiUserControl {
        public Other() {
            InitializeComponent();
        }

        private void yamuiButton6_Click(object sender, EventArgs e) {
            YamuiTaskWindow.ShowTaskWindow(this, "SubControl in TaskWindow", new TaskWindowControl(), 5);
        }
    }
}
