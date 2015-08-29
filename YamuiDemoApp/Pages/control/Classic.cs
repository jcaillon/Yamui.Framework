using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
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
            ThemeManager.TabAnimationAllowed = false;
        }

        private void yamuiButton4_Click(object sender, EventArgs e) {
            // We create a transition to animate all four properties at the same time...
            Transition t = new Transition(new TransitionType_Linear(1000));
            //t.add(yamuiButton5, "Text", "What the hell???");
            t.add(yamuiButton5, "Text", (yamuiButton5.Text == @"What the hell???") ? "Holy molly" : "What the hell???");
            t.run();
        }

        private void yamuiCharButton3_Click(object sender, EventArgs e) {
            //var smk = new SmokeScreen(FindForm());
            YamuiFormMessageBox.ShwDlg(FindForm().Handle, MsgType.Error, "ERrooor", "Wtf did you do you fool!", new List<string> {"fu", "ok"}, true);
        }

        private void yamuiCharButton4_Click(object sender, EventArgs e) {
            // take a screenshot of the form and darken it:
            Bitmap bmp = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            using (Graphics G = Graphics.FromImage(bmp)) {
                G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                G.CopyFromScreen(this.PointToScreen(new Point(0, 0)), new Point(0, 0), this.ClientRectangle.Size);
                double percent = 0.60;
                Color darken = Color.FromArgb((int)(255 * percent), Color.Black);
                using (Brush brsh = new SolidBrush(darken)) {
                    G.FillRectangle(brsh, this.ClientRectangle);
                }
            }

            // put the darkened screenshot into a Panel and bring it to the front:
            using (Panel p = new Panel()) {
                p.Location = new Point(0, 0);
                p.Size = this.ClientRectangle.Size;
                p.BackgroundImage = bmp;
                this.Controls.Add(p);
                p.BringToFront();

                // display your dialog somehow:
                Form frm = new Form();
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            } // panel will be disposed and the form will "lighten" again...
        }
    }
}
