using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Demo;
using YamuiFramework;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;

namespace YamuiDemoApp {
    public partial class Form1 : YamuiForm {
        public Form1() {
            InitializeComponent();
        }

        private void yamuiComboBox1_SelectedIndexChanged(object sender, EventArgs e) {

        }
        //ThemeManager.Theme = ThemeManager.Theme == MetroThemeStyle.Light ? MetroThemeStyle.Dark : MetroThemeStyle.Light;
        private void yamuiButton5_Click(object sender, EventArgs e) {
            if (!Transition.IsTransitionRunning())
                Transition.run(yamuiButton4, "IsPressed", true, new TransitionType_Flash(3, 300));
        }

        private void yamuiButton3_Click(object sender, EventArgs e) {
            yamuiContextMenu1.Show(yamuiButton3, new Point(0, yamuiButton3.Height));
        }

        private void yamuiTabControl1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void yamuiButton6_Click(object sender, EventArgs e) {
            YamuiTaskWindow.ShowTaskWindow(this, "SubControl in TaskWindow", new TaskWindowControl(), 5);
        }

        private void yamuiToggle3_CheckedChanged(object sender, EventArgs e) {
            ThemeManager.Theme = (yamuiToggle3.Checked) ? Themes.Dark : Themes.Light;
            yamuiContextMenu1.UpdateTheme();
            Refresh();
        }
    }
}
