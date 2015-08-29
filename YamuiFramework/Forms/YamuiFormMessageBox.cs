using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Native;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {
    public partial class YamuiFormMessageBox : YamuiForm {

        private static int _dialogResult = -1;
        private const int ButtonWidth = 110;

        public YamuiFormMessageBox(string htmlContent, List<string> buttonsList) {
            InitializeComponent();

            // Set background 
            panelMain.BackColor = ThemeManager.Current.FormColorBackColor;
            panelMain.UseCustomBackColor = true;

            int i = 0;
            foreach (var buttonText in buttonsList) {
                var yamuiButton1 = new YamuiButton {
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Size = new Size(ButtonWidth, 30),
                    Name = "yamuiButton" + i,
                    TabIndex = buttonsList.Count - i,
                    Tag = i,
                    Text = buttonText
                };
                yamuiButton1.Location = new Point(panelMain.Width - ButtonWidth - 5 - (ButtonWidth + 5) * i, panelMain.Height - yamuiButton1.Height - 5);
                yamuiButton1.MouseClick += (sender, args) => {
                    var x = (YamuiButton) sender;
                    _dialogResult = (int)x.Tag;
                    Close();
                };
                yamuiButton1.KeyDown += (sender, args) => {
                    if (args.KeyCode == Keys.Enter || args.KeyCode == Keys.Space || args.KeyCode == Keys.Return) {
                        var x = (YamuiButton) sender;
                        _dialogResult = (int) x.Tag;
                        Close();
                    }
                };
                panelMain.Controls.Add(yamuiButton1);
                i++;
            }

            var buttonsWidth = 10 + (ButtonWidth + 5)*buttonsList.Count + 50;
            var predictedSize = MeasureHtmlContent(htmlContent, buttonsWidth);
            int fWidth = (int)predictedSize.Width;
            int fHeight = (int)predictedSize.Height;
            if (fHeight > 2 * fWidth) {
                predictedSize = MeasureHtmlContent(htmlContent, buttonsWidth * (fHeight / fWidth));
            }

            fWidth = Math.Min(22 + fWidth, Screen.PrimaryScreen.WorkingArea.Width);
            fHeight = Math.Min(41 + 40 + fHeight, Screen.PrimaryScreen.WorkingArea.Height);
            
            // resize form and panel
            Size = new Size(fWidth, fHeight);
            panelContent.Height = (int)predictedSize.Height;

            // add outro animation..
            Tag = false;
            Closing += (sender, args) => {
                if ((bool)Tag) return;
                Tag = true;
                args.Cancel = true;
                var t = new Transition(new TransitionType_Acceleration(200));
                t.add(this, "Opacity", 0d);
                t.TransitionCompletedEvent += (o, args1) => {
                    Close();
                };
                t.run();
            };
            
            Shown += (sender, args) => {
                Focus();
                WinApi.SetForegroundWindow(Handle);
            };
        }

        private SizeF MeasureHtmlContent(string htmlContent, int prefWidth = 0) {
            using (var g = panelContent.CreateGraphics()) {
                return HtmlRender.Measure(g, htmlContent, prefWidth, HtmlHandler.GetBaseCssData(), null, (sender, args) => HtmlHandler.OnImageLoad(args));
            }
        }

        public static int ShwDlg(IntPtr ownerHandle, MsgType type, string heading, string text, List<string> buttonsList, bool waitResponse) {
            string imgSrc;
            switch (type) {
                case MsgType.Warning:
                    imgSrc = "warning";
                    break;
                case MsgType.Error:
                    imgSrc = "error";
                    break;
                case MsgType.Information:
                    imgSrc = "info";
                    break;
                case MsgType.Question:
                    imgSrc = "question";
                    break;
                case MsgType.ShieldedQuestion:
                    imgSrc = "question_shield";
                    break;
                default:
                    imgSrc = "services";
                    break;
            }
            if (!text.StartsWith(@"<html"))
            text = @"
<h1>
    <img src=""" + imgSrc + @""" />
    " + heading + @"
</h1>
<div>
	<p>
	    " + text + @"
	</p>
</div>";
            YamuiForm ownerForm = null;
            try {
                ownerForm = FromHandle(ownerHandle) as YamuiForm;
            } catch (Exception) {
                // ignored
            }

            // new message box
            var msgbox = new YamuiFormMessageBox(text, buttonsList);
            msgbox.ShowInTaskbar = !waitResponse;
            if (ownerForm != null && ownerForm.Width > msgbox.Width && ownerForm.Height > msgbox.Height) {
                // center parent
                msgbox.Location = new Point((ownerForm.Width - msgbox.Width) / 2 + ownerForm.Location.X, (ownerForm.Height - msgbox.Height) / 2 + ownerForm.Location.Y);
            } else {
                // center screen
                msgbox.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - msgbox.Width) / 2 + Screen.PrimaryScreen.WorkingArea.Location.X, (Screen.PrimaryScreen.WorkingArea.Height - msgbox.Height) / 2 + Screen.PrimaryScreen.WorkingArea.Location.Y);
            }

            msgbox.panelContent.Text = text;

            Transition t = new Transition(new TransitionType_Acceleration(200));
            t.add(msgbox, "Opacity", 1d);
            msgbox.Opacity = 0d;

            if (waitResponse) {
                SmokeScreen ownerSmokeScreen = null;

                // smokescreen intro anim
                if (ownerForm != null) {
                    ownerSmokeScreen = new SmokeScreen(ownerForm);
                    Transition t2 = new Transition(new TransitionType_Acceleration(300));
                    t2.add(ownerSmokeScreen, "Opacity", ownerSmokeScreen.Opacity);
                    ownerSmokeScreen.Opacity = 0;
                    Transition.runChain(t2, t);
                } else {
                    t.run();
                }
                msgbox.ShowDialog(new WindowWrapper(ownerHandle));

                // smokescreen outro anim
                if (ownerSmokeScreen != null) {
                    t = new Transition(new TransitionType_Acceleration(300));
                    t.add(ownerSmokeScreen, "Opacity", 0d);
                    t.TransitionCompletedEvent += (sender, args) => {
                        ownerSmokeScreen.Close();
                    };
                    t.run();
                }
            } else {
                t.run();
                msgbox.Show(new WindowWrapper(ownerHandle));
            }

            return _dialogResult;
        }
    }

    public enum MsgType {
        Information,
        Question,
        Warning,
        Error,
        ShieldedQuestion,
        Default
    }
}
