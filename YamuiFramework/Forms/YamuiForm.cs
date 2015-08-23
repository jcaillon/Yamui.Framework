using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Web.UI.Design.WebControls;
using System.Windows.Forms;
using YamuiFramework.Native;

namespace YamuiFramework.Forms {

    #region Enums

    public enum BackLocation {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    #endregion

    public class YamuiForm : Form {

        #region Constructor

        public YamuiForm() {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            TransparencyKey = Color.Fuchsia;
        }

        #endregion

        #region Fields

        private bool _isMovable = true;

        [Category("Yamui")]
        public bool Movable {
            get { return _isMovable; }
            set { _isMovable = value; }
        }

        public new Padding Padding {
            get { return base.Padding; }
            set {
                value.Top = Math.Max(value.Top, 35);
                base.Padding = value;
            }
        }

        protected override Padding DefaultPadding {
            get { return new Padding(20, 35, 20, 20); }
        }

        private bool _isResizable = true;

        [Category("Yamui")]
        public bool Resizable {
            get { return _isResizable; }
            set { _isResizable = value; }
        }

        private const int BorderWidth = 1;

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = ThemeManager.FormColor.BackColor();
            var foreColor = ThemeManager.FormColor.ForeColor();

            e.Graphics.Clear(backColor);

            /*
            // Top border
            using (SolidBrush b = ThemeManager.AccentColor)
            {
                Rectangle topRect = new Rectangle(0, 0, Width, borderWidth);
                e.Graphics.FillRectangle(b, topRect);
            }
            */

            // draw the border with Style color
            var rect = new Rectangle(new Point(0, 0), new Size(Width - BorderWidth, Height - BorderWidth));
            var pen = new Pen(ThemeManager.AccentColor, BorderWidth);
            e.Graphics.DrawRectangle(pen, rect);

            /*
            // draw my logo
            ColorMap[] colorMap = new ColorMap[1];
            colorMap[0] = new ColorMap();
            colorMap[0].OldColor = Color.Black;
            colorMap[0].NewColor = ThemeManager.AccentColor;
            ImageAttributes attr = new ImageAttributes();
            attr.SetRemapTable(colorMap);
            Image logoImage = Properties.Resources.bull_ant;
            rect = new Rectangle(ClientRectangle.Right - (100 + logoImage.Width), 0 + 2, logoImage.Width, logoImage.Height);
            e.Graphics.DrawImage(logoImage, rect, 0, 0, logoImage.Width, logoImage.Height, GraphicsUnit.Pixel, attr);
            //e.Graphics.DrawImage(Properties.Resources.bull_ant, ClientRectangle.Right - (100 + Properties.Resources.bull_ant.Width), 0 + 5);
            */

            // title
            var bounds = new Rectangle(10, 10, ClientRectangle.Width - 2*20, 25);
            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetLabelFont(LabelFunction.FormTitle), bounds, foreColor, TextFormatFlags.EndEllipsis | TextFormatFlags.Left);

            // draw the resize pixel stuff on the bottom right
            if (Resizable && (SizeGripStyle == SizeGripStyle.Auto || SizeGripStyle == SizeGripStyle.Show)) {
                using (var b = new SolidBrush(ThemeManager.FormColor.ForeColor())) {
                    var resizeHandleSize = new Size(2, 2);
                    e.Graphics.FillRectangles(b, new[] {
                        new Rectangle(new Point(ClientRectangle.Width - 6, ClientRectangle.Height - 6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 10, ClientRectangle.Height - 10), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 10, ClientRectangle.Height - 6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 6, ClientRectangle.Height - 10), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 14, ClientRectangle.Height - 6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 6, ClientRectangle.Height - 14), resizeHandleSize)
                    });
                }
            }
        }

        #endregion

        #region Management Methods

        protected override void OnClosing(CancelEventArgs e) {
            if (!(this is YamuiTaskWindow))
                YamuiTaskWindow.ForceClose();

            base.OnClosing(e);
        }

        [SecuritySafeCritical]
        public bool FocusMe() {
            return WinApi.SetForegroundWindow(Handle);
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (DesignMode) return;

            switch (StartPosition) {
                case FormStartPosition.CenterParent:
                    CenterToParent();
                    break;
                case FormStartPosition.CenterScreen:
                    if (IsMdiChild) {
                        CenterToParent();
                    } else {
                        CenterToScreen();
                    }
                    break;
            }

            RemoveCloseButton();

            if (ControlBox) {
                AddWindowButton(WindowButtons.Close);

                if (MaximizeBox)
                    AddWindowButton(WindowButtons.Maximize);

                if (MinimizeBox)
                    AddWindowButton(WindowButtons.Minimize);

                UpdateWindowButtonPosition();
            }
        }

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);

            Invalidate();
        }

        protected override void OnResizeEnd(EventArgs e) {
            base.OnResizeEnd(e);
            UpdateWindowButtonPosition();
        }

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg) {
                case (int) WinApi.Messages.WM_SYSCOMMAND:
                    var sc = m.WParam.ToInt32() & 0xFFF0;
                    switch (sc) {
                        case (int) WinApi.Messages.SC_MOVE:
                            if (!Movable) return;
                            break;
                        case (int) WinApi.Messages.SC_MAXIMIZE:
                            break;
                        case (int) WinApi.Messages.SC_RESTORE:
                            break;
                    }
                    break;

                case (int) WinApi.Messages.WM_NCLBUTTONDBLCLK:
                case (int) WinApi.Messages.WM_LBUTTONDBLCLK:
                    if (!MaximizeBox) return;
                    break;

                case (int) WinApi.Messages.WM_NCHITTEST:
                    var ht = HitTestNca(m.HWnd, m.WParam, m.LParam);
                    if (ht != WinApi.HitTest.HTCLIENT) {
                        m.Result = (IntPtr) ht;
                        return;
                    }
                    break;

                case (int) WinApi.Messages.WM_DWMCOMPOSITIONCHANGED:
                    break;

                case WmNcpaint: // box shadow
                    if (_mAeroEnabled) {
                        var v = 2;
                        DwmApi.DwmSetWindowAttribute(Handle, 2, ref v, 4);
                        var margins = new DwmApi.MARGINS(1, 1, 1, 1);
                        DwmApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
                    }
                    break;
            }

            base.WndProc(ref m);

            switch (m.Msg) {
                case (int) WinApi.Messages.WM_GETMINMAXINFO:
                    OnGetMinMaxInfo(m.HWnd, m.LParam);
                    break;
                case (int) WinApi.Messages.WM_SIZE:
                    if (_windowButtonList != null) {
                        YamuiFormButton btn;
                        _windowButtonList.TryGetValue(WindowButtons.Maximize, out btn);
                        if (WindowState == FormWindowState.Normal) {
                            btn.Text = "1";
                        }
                        if (WindowState == FormWindowState.Maximized) btn.Text = "2";
                    }
                    break;
            }

            if (m.Msg == WmNchittest && (int) m.Result == Htclient) // drag the form
                m.Result = (IntPtr) Htcaption;
        }

        [SecuritySafeCritical]
        private unsafe void OnGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            var pmmi = (WinApi.MINMAXINFO*) lParam;

            var s = Screen.FromHandle(hwnd);
            pmmi->ptMaxSize.x = s.WorkingArea.Width;
            pmmi->ptMaxSize.y = s.WorkingArea.Height;
            pmmi->ptMaxPosition.x = Math.Abs(s.WorkingArea.Left - s.Bounds.Left);
            pmmi->ptMaxPosition.y = Math.Abs(s.WorkingArea.Top - s.Bounds.Top);

            //if (MinimumSize.Width > 0) pmmi->ptMinTrackSize.x = MinimumSize.Width;
            //if (MinimumSize.Height > 0) pmmi->ptMinTrackSize.y = MinimumSize.Height;
            //if (MaximumSize.Width > 0) pmmi->ptMaxTrackSize.x = MaximumSize.Width;
            //if (MaximumSize.Height > 0) pmmi->ptMaxTrackSize.y = MaximumSize.Height;
        }

        private WinApi.HitTest HitTestNca(IntPtr hwnd, IntPtr wparam, IntPtr lparam) {
            //Point vPoint = PointToClient(new Point((int)lparam & 0xFFFF, (int)lparam >> 16 & 0xFFFF));
            //Point vPoint = PointToClient(new Point((Int16)lparam, (Int16)((int)lparam >> 16)));
            var vPoint = new Point((short) lparam, (short) ((int) lparam >> 16));
            var vPadding = Math.Max(Padding.Right, Padding.Bottom);

            if (Resizable) {
                if (RectangleToScreen(new Rectangle(ClientRectangle.Width - vPadding, ClientRectangle.Height - vPadding, vPadding, vPadding)).Contains(vPoint))
                    return WinApi.HitTest.HTBOTTOMRIGHT;
            }

            if (RectangleToScreen(new Rectangle(BorderWidth, BorderWidth, ClientRectangle.Width - 2*BorderWidth, 50)).Contains(vPoint))
                return WinApi.HitTest.HTCAPTION;

            return WinApi.HitTest.HTCLIENT;
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && Movable) {
                if (WindowState == FormWindowState.Maximized) return;
                //if (Width - borderWidth > e.Location.X && e.Location.X > borderWidth && e.Location.Y > borderWidth)
                //{
                MoveControl();
                //}
            }
        }

        [SecuritySafeCritical]
        private void MoveControl() {
            WinApi.ReleaseCapture();
            WinApi.SendMessage(Handle, (int) WinApi.Messages.WM_NCLBUTTONDOWN, (int) WinApi.HitTest.HTCAPTION, 0);
        }

        #endregion

        #region Window Buttons

        private enum WindowButtons {
            Minimize,
            Maximize,
            Close
        }

        private Dictionary<WindowButtons, YamuiFormButton> _windowButtonList;

        private void AddWindowButton(WindowButtons button) {
            if (_windowButtonList == null)
                _windowButtonList = new Dictionary<WindowButtons, YamuiFormButton>();

            if (_windowButtonList.ContainsKey(button))
                return;

            var newButton = new YamuiFormButton();

            if (button == WindowButtons.Close) {
                newButton.Text = "r";
            } else if (button == WindowButtons.Minimize) {
                newButton.Text = "0";
            } else if (button == WindowButtons.Maximize) {
                if (WindowState == FormWindowState.Normal)
                    newButton.Text = "1";
                else
                    newButton.Text = "2";
            }

            newButton.Tag = button;
            newButton.Size = new Size(25, 20);
            newButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newButton.TabStop = false; //remove the form controls from the tab stop
            newButton.Click += WindowButton_Click;
            Controls.Add(newButton);

            _windowButtonList.Add(button, newButton);
        }

        private void WindowButton_Click(object sender, EventArgs e) {
            var btn = sender as YamuiFormButton;
            if (btn != null) {
                var btnFlag = (WindowButtons) btn.Tag;
                if (btnFlag == WindowButtons.Close) {
                    Close();
                } else if (btnFlag == WindowButtons.Minimize) {
                    WindowState = FormWindowState.Minimized;
                } else if (btnFlag == WindowButtons.Maximize) {
                    if (WindowState == FormWindowState.Normal) {
                        WindowState = FormWindowState.Maximized;
                        btn.Text = "2";
                    } else {
                        WindowState = FormWindowState.Normal;
                        btn.Text = "1";
                    }
                }
            }
        }

        private void UpdateWindowButtonPosition() {
            if (!ControlBox) return;

            var priorityOrder = new Dictionary<int, WindowButtons>(3) {{0, WindowButtons.Close}, {1, WindowButtons.Maximize}, {2, WindowButtons.Minimize}};

            var firstButtonLocation = new Point(ClientRectangle.Width - BorderWidth - 25, BorderWidth);
            var lastDrawedButtonPosition = firstButtonLocation.X - 25;

            YamuiFormButton firstButton = null;

            if (_windowButtonList.Count == 1) {
                foreach (var button in _windowButtonList) {
                    button.Value.Location = firstButtonLocation;
                }
            } else {
                foreach (var button in priorityOrder) {
                    var buttonExists = _windowButtonList.ContainsKey(button.Value);

                    if (firstButton == null && buttonExists) {
                        firstButton = _windowButtonList[button.Value];
                        firstButton.Location = firstButtonLocation;
                        continue;
                    }

                    if (firstButton == null || !buttonExists) continue;

                    _windowButtonList[button.Value].Location = new Point(lastDrawedButtonPosition, BorderWidth);
                    lastDrawedButtonPosition = lastDrawedButtonPosition - 25;
                }
            }

            Refresh();
        }

        private class YamuiFormButton : Label {
            #region Constructor

            public YamuiFormButton() {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);
            }

            #endregion

            #region Paint Methods

            protected override void OnPaint(PaintEventArgs e) {
                Color backColor = ThemeManager.ButtonColors.BackGround(ThemeManager.FormColor.BackColor(), true, false, _isHovered, _isPressed, Enabled);
                Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, false, false, _isHovered, _isPressed, Enabled);

                e.Graphics.Clear(backColor);

                var buttonFont = new Font("Webdings", 9.25f);
                TextRenderer.DrawText(e.Graphics, Text, buttonFont, ClientRectangle, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            #endregion

            #region Fields

            private bool _isHovered;
            private bool _isPressed;

            #endregion

            #region Mouse Methods

            protected override void OnMouseDown(MouseEventArgs e) {
                if (e.Button == MouseButtons.Left) {
                    _isPressed = true;
                    Invalidate();
                }

                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e) {
                _isPressed = false;
                Invalidate();

                base.OnMouseUp(e);
            }

            protected override void OnMouseEnter(EventArgs e) {
                _isHovered = true;
                Invalidate();
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e) {
                _isHovered = false;
                Invalidate();

                base.OnMouseLeave(e);
            }

            #endregion
        }

        #endregion

        #region Shadows

        private bool _mAeroEnabled; // variables for box shadow
        private const int CsDropshadow = 0x00020000;
        private const int WmNcpaint = 0x0085;

        public struct Margins // struct for box shadow
        {
            public int BottomHeight;
            public int LeftWidth;
            public int RightWidth;
            public int TopHeight;
        }

        private const int WmNchittest = 0x84; // variables for dragging the form
        private const int Htclient = 0x1;
        private const int Htcaption = 0x2;

        protected override CreateParams CreateParams {
            get {
                _mAeroEnabled = CheckAeroEnabled();

                var cp = base.CreateParams;
                if (!_mAeroEnabled)
                    cp.ClassStyle |= CsDropshadow;

                return cp;
            }
        }

        private bool CheckAeroEnabled() {
            if (Environment.OSVersion.Version.Major >= 6) {
                var enabled = 0;
                DwmApi.DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1);
            }
            return false;
        }

        #endregion

        #region Helper Methods

        [SecuritySafeCritical]
        public void RemoveCloseButton() {
            var hMenu = WinApi.GetSystemMenu(Handle, false);
            if (hMenu == IntPtr.Zero) return;

            var n = WinApi.GetMenuItemCount(hMenu);
            if (n <= 0) return;

            WinApi.RemoveMenu(hMenu, (uint) (n - 1), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.RemoveMenu(hMenu, (uint) (n - 2), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.DrawMenuBar(Handle);
        }

        private Rectangle MeasureText(Graphics g, Rectangle clientRectangle, Font font, string text, TextFormatFlags flags) {
            var proposedSize = new Size(int.MaxValue, int.MinValue);
            var actualSize = TextRenderer.MeasureText(g, text, font, proposedSize, flags);
            return new Rectangle(clientRectangle.X, clientRectangle.Y, actualSize.Width, actualSize.Height);
        }

        #endregion
    }

    internal class YamuiFormDesigner : FormViewDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("Font");

            base.PreFilterProperties(properties);
        }
    }
}