using System;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Themes;

namespace YamuiDemoApp.Pages.control {
    public partial class Classic : YamuiPage {
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

        private void yamuiButton4_Click(object sender, EventArgs e) {
            // We create a transition to animate all four properties at the same time...
            Transition t = new Transition(new TransitionType_Linear(1000));
            //t.add(yamuiButton5, "Text", "What the hell???");
            t.add(yamuiButton5, "Text", (yamuiButton5.Text == @"What the hell???") ? "Holy molly" : "What the hell???");
            t.run();
        }
    }
}
