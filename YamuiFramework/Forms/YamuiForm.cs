using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Web.UI.Design.WebControls;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Native;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {

    public class YamuiForm : Form {

        #region Fields
        [Category("Yamui")]
        public bool UseGoBackButton { get; set; }

        private bool _isMovable = true;

        [Category("Yamui")]
        public bool Movable {
            get { return _isMovable; }
            set { _isMovable = value; }
        }

        public new Padding Padding {
            get { return base.Padding; }
            set {
                value.Top = Math.Max(value.Top, 40);
                base.Padding = value;
            }
        }

        protected override Padding DefaultPadding {
            get { return new Padding(40, 40, BorderWidth + 10, BorderWidth + 10); }
        }

        private bool _isResizable = true;

        [Category("Yamui")]
        public bool Resizable {
            get { return _isResizable; }
            set { _isResizable = value; }
        }

        private const int BorderWidth = 1;

        private List<int[]> _formHistory = new List<int[]>();

        private YamuiGoBackButton _goBackButton;
        #endregion

        #region Constructor

        public YamuiForm() {
            // why those styles? check here: https://sites.google.com/site/craigandera/craigs-stuff/windows-forms/flicker-free-control-drawing
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            TransparencyKey = Color.Fuchsia;

            Shown += OnShown;
        }
        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = ThemeManager.Current.FormColorBackColor;
            var foreColor = ThemeManager.Current.FormColorForeColor;

            e.Graphics.Clear(backColor);

            /*
            // Top border
            using (SolidBrush b = ThemeManager.AccentColor)
            {
                Rectangle topRect = new Rectangle(0, 0, Width, borderWidth);
                e.Graphics.FillRectangle(b, topRect);
            }
            */
            /*
             * Color.FromArgb(40,
                ((basic.R / 255 * diluant.R) + diluant.R) / 2,
                ((basic.G / 255 * diluant.G) + diluant.G) / 2,
                ((basic.B / 255 * diluant.B) + diluant.B) / 2);
             * 
            // gradient
            Rectangle headeRectangle = new Rectangle(0, 0, ClientRectangle.Width, 20);
            using (LinearGradientBrush brush = new LinearGradientBrush(headeRectangle, MergeColors(backColor, MetroPaint.GetStyleColor(Style)), backColor, LinearGradientMode.Vertical)) {
                e.Graphics.FillRectangle(brush, headeRectangle);
            }
             * */
            /*
            if (ThemeManager.ImageTheme != null) {
                Rectangle imgRectangle = new Rectangle(ClientRectangle.Right - ThemeManager.ImageTheme.Width, ClientRectangle.Height - ThemeManager.ImageTheme.Height, ThemeManager.ImageTheme.Width, ThemeManager.ImageTheme.Height);
                e.Graphics.DrawImage(ThemeManager.ImageTheme, imgRectangle, 0, 0, ThemeManager.ImageTheme.Width, ThemeManager.ImageTheme.Height, GraphicsUnit.Pixel);
            }*/

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

            // draw the resize pixel stuff on the bottom right
            if (Resizable && (SizeGripStyle == SizeGripStyle.Auto || SizeGripStyle == SizeGripStyle.Show)) {
                using (var b = new SolidBrush(ThemeManager.Current.FormColorForeColor)) {
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

        #region For the user
        /// <summary>
        /// Go to page pagename
        /// </summary>
        /// <param name="pageName"></param>
        public void GoToPage(string pageName) {
            try {
                YamuiTabPage page = null;
                foreach (var control in GetAll(this, typeof(YamuiTabPage))) {
                    if (control.Name == pageName)
                        page = (YamuiTabPage)control;
                }
                if (page == null) return;
                YamuiTabControl secControl = (YamuiTabControl)page.Parent;
                YamuiTabControl mainControl = (YamuiTabControl)secControl.Parent.Parent;
                GoToPage(mainControl, (YamuiTabPage)secControl.Parent, secControl, page, true);
            } catch (Exception) {
                // ignored
            }
        }

        public void GoToPage(int pageMainInt, int pageSecInt) {
            YamuiTabControl mainControl = (YamuiTabControl)GetFirst(this, typeof(YamuiTabControl));
            if (mainControl == null || mainControl.TabCount < pageMainInt || mainControl.TabPages[pageMainInt] == null) return;
            YamuiTabControl secControl = (YamuiTabControl)GetFirst(mainControl.TabPages[pageMainInt], typeof(YamuiTabControl));
            if (secControl == null) return;
            GoToPage(mainControl, (YamuiTabPage)mainControl.TabPages[pageMainInt], secControl, (YamuiTabPage)secControl.TabPages[pageSecInt], false);
        }

        public void GoToPage(YamuiTabControl tabMain, YamuiTabPage pageMain, YamuiTabControl tabSecondary, YamuiTabPage pageSecondary, bool histoSave) {

            // if we want to display a hidden page            
            if (pageMain.HiddenPage)
                pageMain.HiddenState = false;

            if (pageSecondary.HiddenPage)
                pageSecondary.HiddenState = false;

            var pageMainInt = tabMain.GetIndexOf(pageMain);
            var pageSecInt = tabSecondary.GetIndexOf(pageSecondary);

            if (histoSave)
                SaveCurrentPathInHistory();

            // if we change both pages, we can't do the animation for both!
            if (pageMainInt != tabMain.SelectIndex && pageSecInt != tabSecondary.SelectIndex) {
                ThemeManager.AnimationAllowed = false;
                tabSecondary.SelectIndex = pageSecInt;
                ThemeManager.AnimationAllowed = true;
            } else
                tabSecondary.SelectIndex = pageSecInt;
            tabMain.SelectIndex = pageMainInt;
        }

        /// <summary>
        /// Use the history list to go back to previous tabs
        /// </summary>
        public void GoBack() {
            if (_formHistory.Count == 0) return;
            var lastPage = _formHistory.Last();
            GoToPage(lastPage[0], lastPage[1]);
            _formHistory.Remove(_formHistory.Last());
            if (_formHistory.Count == 0 && _goBackButton != null) _goBackButton.FakeDisabled = true;
        }

        /// <summary>
        /// Keep an history of the tabs visited through a list handled here
        /// </summary>
        public void SaveCurrentPathInHistory() {
            YamuiTabControl mainControl = (YamuiTabControl)GetFirst(this, typeof(YamuiTabControl));
            if (mainControl == null) return;
            var pageMainInt = mainControl.SelectedIndex;
            YamuiTabControl secControl = (YamuiTabControl)GetFirst(mainControl.TabPages[pageMainInt], typeof(YamuiTabControl));
            if (secControl == null) return;
            var pageSecInt = secControl.SelectedIndex;
            _formHistory.Add(new[] { pageMainInt, pageSecInt });
            if (_goBackButton.FakeDisabled) _goBackButton.FakeDisabled = false;
        }

        private IEnumerable<Control> GetAll(Control control, Type type) {
            var controls = control.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
        }

        private Control GetFirst(Control control, Type type) {
            foreach (var control1 in control.Controls) {
                if (control1.GetType() == type) return (Control)control1;
            }
            return null;
        }
        #endregion

        #region Management Methods
        /// <summary>
        /// allows to do stuff only when everything is fully loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnShown(object sender, EventArgs eventArgs) {
            // Processes all Windows messages currently in the message queue.
            Application.DoEvents();

            // only then activate the animations
            ThemeManager.AnimationAllowed = true;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (UseGoBackButton) {
                _goBackButton = new YamuiGoBackButton();
                Controls.Add(_goBackButton);
                _goBackButton.Location = new Point(8, Padding.Top + 6);
                _goBackButton.Size = new Size(27, 27);
            }

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


            // add the fonts to renderer
            try {
                HtmlRender.AddFontFamily(GetFontFamily("SEGOEUI"));
                HtmlRender.AddFontFamily(GetFontFamily("SEGOEUII"));
                HtmlRender.AddFontFamily(GetFontFamily("SEGOEUIB"));
            } catch (Exception) {
                // ignored
            }
        }

        private readonly PrivateFontCollection _fontCollection = new PrivateFontCollection();

        private FontFamily GetFontFamily(string familyName) {
            lock (_fontCollection) {
                foreach (FontFamily fontFamily in _fontCollection.Families)
                    if (fontFamily.Name == familyName) return fontFamily;

                string resourceName = GetType().Namespace + ".Fonts." + familyName.Replace(' ', '_') + ".ttf";

                Stream fontStream = null;
                IntPtr data = IntPtr.Zero;
                try {
                    fontStream = GetType().Assembly.GetManifestResourceStream(resourceName);
                    if (fontStream != null) {
                        int bytes = (int) fontStream.Length;
                        data = Marshal.AllocCoTaskMem(bytes);
                        byte[] fontdata = new byte[bytes];
                        fontStream.Read(fontdata, 0, bytes);
                        Marshal.Copy(fontdata, 0, data, bytes);
                        _fontCollection.AddMemoryFont(data, bytes);
                    }
                    return _fontCollection.Families[_fontCollection.Families.Length - 1];
                } finally {
                    if (fontStream != null) fontStream.Dispose();
                    if (data != IntPtr.Zero) Marshal.FreeCoTaskMem(data);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            if (!(this is YamuiTaskWindow))
                YamuiTaskWindow.ForceClose();
            base.OnClosing(e);
        }

        [SecuritySafeCritical]
        public bool FocusMe() {
            return WinApi.SetForegroundWindow(Handle);
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
                case (int)WinApi.Messages.WM_SYSCOMMAND:
                    var sc = m.WParam.ToInt32() & 0xFFF0;
                    switch (sc) {
                        case (int)WinApi.Messages.SC_MOVE:
                            if (!Movable) return;
                            break;
                        case (int)WinApi.Messages.SC_MAXIMIZE:
                            break;
                        case (int)WinApi.Messages.SC_RESTORE:
                            break;
                    }
                    break;

                case (int)WinApi.Messages.WM_NCLBUTTONDBLCLK:
                case (int)WinApi.Messages.WM_LBUTTONDBLCLK:
                    if (!MaximizeBox) return;
                    break;

                case (int)WinApi.Messages.WM_NCHITTEST:
                    var ht = HitTestNca(m.HWnd, m.WParam, m.LParam);
                    if (ht != WinApi.HitTest.HTCLIENT) {
                        m.Result = (IntPtr)ht;
                        return;
                    }
                    break;

                case (int)WinApi.Messages.WM_DWMCOMPOSITIONCHANGED:
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
                case (int)WinApi.Messages.WM_GETMINMAXINFO:
                    OnGetMinMaxInfo(m.HWnd, m.LParam);
                    break;
                case (int)WinApi.Messages.WM_SIZE:
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

            if (m.Msg == WmNchittest && (int)m.Result == Htclient) // drag the form
                m.Result = (IntPtr)Htcaption;
        }

        [SecuritySafeCritical]
        private unsafe void OnGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            var pmmi = (WinApi.MINMAXINFO*)lParam;

            var s = Screen.FromHandle(hwnd);
            pmmi->ptMaxSize.x = s.WorkingArea.Width;
            pmmi->ptMaxSize.y = s.WorkingArea.Height;
            pmmi->ptMaxPosition.x = Math.Abs(s.WorkingArea.Left - s.Bounds.Left);
            pmmi->ptMaxPosition.y = Math.Abs(s.WorkingArea.Top - s.Bounds.Top);
        }

        private WinApi.HitTest HitTestNca(IntPtr hwnd, IntPtr wparam, IntPtr lparam) {
            var vPoint = new Point((short)lparam, (short)((int)lparam >> 16));
            var vPadding = Math.Max(Padding.Right, Padding.Bottom);

            if (Resizable) {
                if (RectangleToScreen(new Rectangle(ClientRectangle.Width - vPadding, ClientRectangle.Height - vPadding, vPadding, vPadding)).Contains(vPoint))
                    return WinApi.HitTest.HTBOTTOMRIGHT;
            }

            if (RectangleToScreen(new Rectangle(BorderWidth, BorderWidth, ClientRectangle.Width - 2 * BorderWidth, 50)).Contains(vPoint))
                return WinApi.HitTest.HTCAPTION;

            return WinApi.HitTest.HTCLIENT;
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && Movable) {
                if (WindowState == FormWindowState.Maximized) return;
                MoveControl();
            }
        }

        [SecuritySafeCritical]
        private void MoveControl() {
            WinApi.ReleaseCapture();
            WinApi.SendMessage(Handle, (int)WinApi.Messages.WM_NCLBUTTONDOWN, (int)WinApi.HitTest.HTCAPTION, 0);
        }

        #endregion

        #region go back button
        private class YamuiGoBackButton : YamuiCharButton {
            public YamuiGoBackButton() {
                UseWingdings = true;
                ButtonChar = "ç";
                FakeDisabled = true;
                KeyUp += (sender, args) => {
                    if (args.KeyCode == Keys.Space && !FakeDisabled) {
                        TryToGoBack();
                    }
                };
                MouseDown += (sender, args) => {
                    if (args.Button == MouseButtons.Left && !FakeDisabled) {
                        TryToGoBack();
                    }
                };
            }

            private void TryToGoBack() {
                try {
                    YamuiForm ownerForm = (YamuiForm)FindForm();
                    if (ownerForm != null) ownerForm.GoBack();
                } catch (Exception) {
                    // ignored
                }
            }
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
                var btnFlag = (WindowButtons)btn.Tag;
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

            var priorityOrder = new Dictionary<int, WindowButtons>(3) { { 0, WindowButtons.Close }, { 1, WindowButtons.Maximize }, { 2, WindowButtons.Minimize } };

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
            protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
                graphics.Clear(Color.Transparent);
                if ((Parent != null)) {
                    clipRect.Offset(Location);
                    PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                    GraphicsState state = graphics.Save();
                    graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    try {
                        graphics.TranslateTransform(-Location.X, -Location.Y);
                        InvokePaintBackground(Parent, e);
                        InvokePaint(Parent, e);
                    } finally {
                        graphics.Restore(state);
                        clipRect.Offset(-Location.X, -Location.Y);
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs e) {
                if (_isPressed)
                    e.Graphics.Clear(ThemeManager.AccentColor);
                else if (_isHovered)
                    e.Graphics.Clear(ThemeManager.Current.ButtonColorsHoverBackColor);
                else
                    PaintTransparentBackground(e.Graphics, DisplayRectangle);

                Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, false, false, _isHovered, _isPressed, Enabled);
                TextRenderer.DrawText(e.Graphics, Text, new Font("Webdings", 9.25f), ClientRectangle, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
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

            WinApi.RemoveMenu(hMenu, (uint)(n - 1), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.RemoveMenu(hMenu, (uint)(n - 2), WinApi.MfByposition | WinApi.MfRemove);
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
            properties.Remove("Text");
            base.PreFilterProperties(properties);
        }
    }
}