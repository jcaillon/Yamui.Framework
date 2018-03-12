﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiDemoApp.Pages.control {
    public partial class Classic : YamuiPage {
        public Classic() {
            InitializeComponent();
        }

        private void yamuiButton5_Click(object sender, EventArgs e) {
            yamuiButton4.UseCustomBackColor = true;
            Transition.run(yamuiButton4, "BackColor", YamuiThemeManager.Current.ButtonNormalBack, YamuiThemeManager.Current.AccentColor, new TransitionType_Flash(3, 300), (o, args) => { yamuiButton4.UseCustomBackColor = false;  });
        }

        private void yamuiButton1_Click(object sender, EventArgs e) {
            YamuiThemeManager.TabAnimationAllowed = false;
        }

        private void yamuiButton4_Click(object sender, EventArgs e) {
            // We create a transition to animate all four properties at the same time...
            Transition t = new Transition(new TransitionType_Linear(1000));
            //t.add(yamuiButton5, "Text", "What the hell???");
            t.add(yamuiButton5, "Text", (yamuiButton5.Text == @"What the hell???") ? "Holy molly" : "What the hell???");
            t.run();
        }
        private void yamuiCharButton4_Click(object sender, EventArgs e) {
            // take a screenshot of the form and darken it:
            Bitmap bmp = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            using (Graphics G = Graphics.FromImage(bmp)) {
                G.CompositingMode = CompositingMode.SourceOver;
                G.CopyFromScreen(PointToScreen(new Point(0, 0)), new Point(0, 0), ClientRectangle.Size);
                double percent = 0.60;
                Color darken = Color.FromArgb((int)(255 * percent), Color.Black);
                using (Brush brsh = new SolidBrush(darken)) {
                    G.FillRectangle(brsh, ClientRectangle);
                }
            }

            // put the darkened screenshot into a Panel and bring it to the front:
            using (Panel p = new Panel()) {
                p.Location = new Point(0, 0);
                p.Size = ClientRectangle.Size;
                p.BackgroundImage = bmp;
                Controls.Add(p);
                p.BringToFront();

                // display your dialog somehow:
                Form frm = new Form();
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            } // panel will be disposed and the form will "lighten" again...
        }

        private void yamuiCharButton5_Click(object sender, EventArgs e) {

        }

        private void yamuiButton5_ButtonPressed(object sender, EventArgs e) {
            yamuiButton4.UseCustomBackColor = true;
            Transition.run(yamuiButton4, "BackColor", YamuiThemeManager.Current.ButtonNormalBack, YamuiThemeManager.Current.AccentColor, new TransitionType_Flash(3, 300), (o, args) => { yamuiButton4.UseCustomBackColor = false; });
        }

        private void yamuiCharButton1_Click(object sender, EventArgs e) {
            var menu = new YamuiMenu();
            menu.SpawnLocation = Cursor.Position;
            menu.MenuList = new List<YamuiMenuItem> {
                new YamuiMenuItem {
                    DisplayText = "zefefzefzef zedf item 1",
                    Children = new List<FilteredTypeTreeListItem> {
                        new YamuiMenuItem {DisplayText = "child 1"}
                    }
                },
                new YamuiMenuItem {
                    DisplayText = "item 2",
                },
                new YamuiMenuItem {
                    DisplayText = "item 3",
                    Children = new List<FilteredTypeTreeListItem> {
                        new YamuiMenuItem {DisplayText = "child 1"},
                        new YamuiMenuItem {DisplayText = "child 2"}
                    }
                }
            };
            menu.Show();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public Int32 x;
            public Int32 y;
            public POINT(Int32 x, Int32 y) { this.x = x; this.y = y; }
        }


        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition() {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return new Point(lpPoint.x, lpPoint.y);
        }
    }
}
