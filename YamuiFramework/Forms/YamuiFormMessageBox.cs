using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Native;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {
    public partial class YamuiFormMessageBox : YamuiForm {

        #region Fields
        public new Double Opacity {
            get { return base.Opacity; }
            set {
                if (value < 0) {
                    try { Close(); } catch (Exception) {
                        // ignored
                    }
                    return;
                }
                base.Opacity = value;
            }
        }

        private static int _dialogResult = -1;
        private const int ButtonWidth = 110;
        public static YamuiSmokeScreen OwnerSmokeScreen = null;
        #endregion

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
            MinimumSize = Size;
            panelContent.Height = (int)predictedSize.Height;

            // add outro animation
            Tag = false;
            Closing += (sender, args) => {
                // cancel initialise close to run an animation, after that allow it
                if ((bool)Tag) return;
                Tag = true;
                args.Cancel = true;
                FadeOut(this);
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

            FadeIn(msgbox, ownerForm);
            if (waitResponse) {
                msgbox.ShowDialog(new WindowWrapper(ownerHandle));
                if (OwnerSmokeScreen != null) {
                    OwnerSmokeScreen.Close();
                    OwnerSmokeScreen = null;
                }
            } else {
                msgbox.Show(new WindowWrapper(ownerHandle));
            }

            return _dialogResult;
        }

        private static void FadeIn(YamuiFormMessageBox msgBox, YamuiForm ownerForm) {
            // transition on msgbox
            Transition t = new Transition(new TransitionType_CriticalDamping(400));
            t.add(msgBox, "Opacity", 1d);
            msgBox.Opacity = 0d;

            // if owner isn't a yamuiform then run anim on msgbox only
            if (OwnerSmokeScreen != null) { t.run(); return; }
            if (ownerForm == null) { t.run(); return; }

            // otherwise had fadein of smokescreen for yamuiform
            OwnerSmokeScreen = new YamuiSmokeScreen(ownerForm);
            t.add(OwnerSmokeScreen, "Opacity", OwnerSmokeScreen.Opacity);
            OwnerSmokeScreen.Opacity = 0d;
            t.run();
        }

        private void FadeOut(YamuiFormMessageBox msgBox) {
            // transition on msgbox
            Transition t = new Transition(new TransitionType_CriticalDamping(400));
            t.add(msgBox, "Opacity", -0.01d);
            if (OwnerSmokeScreen == null) { t.run(); return; }
            t.add(OwnerSmokeScreen, "Opacity", 0d);
            t.run();
        }

        public static void ClseSmokeScreen() {
            OwnerSmokeScreen.Close();
            OwnerSmokeScreen = null;
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
