using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        public static YamuiSmokeScreen OwnerSmokeScreen;
        #endregion

        public YamuiFormMessageBox(MsgType type, string htmlContent, List<string> buttonsList) {
            InitializeComponent();

            // Set buttons
            int i = 0;
            foreach (var buttonText in buttonsList) {
                var yamuiButton1 = new YamuiButton {
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Size = new Size(ButtonWidth, 25),
                    Name = "yamuiButton" + i,
                    TabIndex = buttonsList.Count - i,
                    Tag = i,
                    Text = buttonText
                };
                yamuiButton1.Location = new Point(Width - 12 - ButtonWidth - (ButtonWidth + 5) * i, Height - 12 - yamuiButton1.Height);
                yamuiButton1.ButtonPressed += (sender, args) => {
                    _dialogResult = (int) args._buttonTag;
                    Close();
                };
                Controls.Add(yamuiButton1);
                i++;
            }

            pictureBox.Image = GetImg(type, pictureBox.Size);
            var minButtonsWidth = (ButtonWidth + 5) * buttonsList.Count + 12 + 10;
            
            // resize form and panel
            int j = 0;
            int compHeight;
            do {
                Width = minButtonsWidth;
                contentLabel.Text = htmlContent;
                compHeight = contentPanel.Location.Y + contentLabel.Location.Y + contentLabel.Height + 45;
                compHeight = Math.Min(compHeight, Screen.PrimaryScreen.WorkingArea.Height);
                Size = new Size(minButtonsWidth, compHeight);
                MinimumSize = Size;
                minButtonsWidth = minButtonsWidth*(compHeight/minButtonsWidth);
                j++;
            } while (j < 2 && Height > Width);

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

        public static int ShwDlg(IntPtr ownerHandle, MsgType type, string heading, string text, List<string> buttonsList, bool waitResponse) {
            YamuiForm ownerForm = null;
            try {
                ownerForm = FromHandle(ownerHandle) as YamuiForm;
            } catch (Exception) {
                // ignored
            }

            // new message box
            var msgbox = new YamuiFormMessageBox(type, text, buttonsList);
            msgbox.ShowInTaskbar = !waitResponse;
            if (ownerForm != null && ownerForm.Width > msgbox.Width && ownerForm.Height > msgbox.Height) {
                // center parent
                msgbox.Location = new Point((ownerForm.Width - msgbox.Width) / 2 + ownerForm.Location.X, (ownerForm.Height - msgbox.Height) / 2 + ownerForm.Location.Y);
            } else {
                // center screen
                msgbox.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - msgbox.Width) / 2 + Screen.PrimaryScreen.WorkingArea.Location.X, (Screen.PrimaryScreen.WorkingArea.Height - msgbox.Height) / 2 + Screen.PrimaryScreen.WorkingArea.Location.Y);
            }

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

        private Image GetImg(MsgType type, Size size) {
            var resname = Enum.GetName(typeof (MsgType), type);
            if (resname == null) resname = "ant";
            var imgToResize = Resources.ImageGetter.GetInstance().Get(resname.ToLower());

            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }

    }

    public enum MsgType {
        Ant,
        Error,
        HighImportance,
        Info,
        Ok,
        Pin,
        Poison,
        Question,
        QuestionShield,
        RadioActive,
        Services,
        Skull,
        Warning,
        WarningShield
    }
}
