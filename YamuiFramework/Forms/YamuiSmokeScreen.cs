using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using YamuiFramework.Controls;

namespace YamuiFramework.Forms {
    public class YamuiSmokeScreen : Form {

        #region Constructor
        public YamuiSmokeScreen(Form tocover) {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            BackColor = Color.Black;
            Opacity = 0.5;
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            AutoScaleMode = AutoScaleMode.None;
            Location = tocover.PointToScreen(Point.Empty);
            ClientSize = tocover.ClientSize;
            tocover.LocationChanged += Cover_LocationChanged;
            tocover.ClientSizeChanged += Cover_ClientSizeChanged;
            Show(tocover);
            tocover.Focus();
            // Disable Aero transitions, the plexiglass gets too visible
            if (Environment.OSVersion.Version.Major >= 6) {
                int value = 1;
                DwmSetWindowAttribute(tocover.Handle, DWMWA_TRANSITIONS_FORCEDISABLED, ref value, 4);
            }
        }
        #endregion


        #region Events
        private void Cover_LocationChanged(object sender, EventArgs e) {
            // Ensure the plexiglass follows the owner
            Location = Owner.PointToScreen(Point.Empty);
        }

        private void Cover_ClientSizeChanged(object sender, EventArgs e) {
            // Ensure the plexiglass keeps the owner covered
            ClientSize = Owner.ClientSize;
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            // Restore owner
            Owner.LocationChanged -= Cover_LocationChanged;
            Owner.ClientSizeChanged -= Cover_ClientSizeChanged;
            if (!Owner.IsDisposed && Environment.OSVersion.Version.Major >= 6) {
                int value = 0;
                DwmSetWindowAttribute(Owner.Handle, DWMWA_TRANSITIONS_FORCEDISABLED, ref value, 4);
            }
            base.OnFormClosing(e);
        }

        protected override void OnActivated(EventArgs e) {
            // Always keep the owner activated instead
            BeginInvoke(new Action(() => Owner.Activate()));
        }
        #endregion


        #region API call
        private const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int attrLen);
        #endregion
    }
}
