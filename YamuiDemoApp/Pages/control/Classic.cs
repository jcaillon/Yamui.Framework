using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages {
    public partial class Classic : YamuiUserControl {
        public Classic() {
            InitializeComponent();
        }

        private void yamuiButton5_Click(object sender, EventArgs e) {
            if (!Transition.IsTransitionRunning())
                Transition.run(yamuiButton4, "DoPressed", true, new TransitionType_Flash(3, 300));
        }

        private void yamuiButton1_Click(object sender, EventArgs e) {
            ThemeManager.ImageTheme = Properties.Resources.hello_kitty;
            ThemeManager.AnimationAllowed = false;
            FindForm().Refresh();
        }
    }
}
