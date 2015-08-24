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
using YamuiFramework.Controls;
using YamuiFramework.Forms;

namespace YamuiDemoApp {
    public partial class Form1 : YamuiForm {
        public Form1() {
            InitializeComponent();
            //yamuiTabControlMain.ApplyHideThisSettings(); //TODO: replace this by a onLoad event in yamuiLoad, foreach yamuitabControl do apply
        }

        private void yamuiTabControl1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void yamuiTabPage9_Click(object sender, EventArgs e) {

        }

        private void Form1_Load(object sender, EventArgs e) {
            //ApplyHideSettingGlobally(this);
        }

        private void ApplyHideSettingGlobally(Control parent) {
            foreach (Control c in parent.Controls) {
                if (c.GetType() == typeof(YamuiTabControl)) {
                    YamuiTabControl fuu = (YamuiTabControl)c;
                    //fuu.ApplyHideThisSettings();
                    MessageBox.Show(fuu.Name);
                }
                ApplyHideSettingGlobally(c); // recurvise
            }
        }

        private void yamuiLink6_Click(object sender, EventArgs e) {
            GoToPage(yamuiTabControlMain, yamuiTabMainSetting, yamuiTabControlSecSetting, yamuiTabSecAppearance);
        }
    }
}
