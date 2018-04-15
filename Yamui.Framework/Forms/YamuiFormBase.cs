#region header

// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormBase.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================

#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Forms {
    /// <summary>
    /// Form class that implements interesting utilities + shadow + onpaint + movable/resizable borderless
    /// </summary>
    public class YamuiFormBase : Form {
        #region Constants

        public const int BorderWidth = 2;
        public const int ResizeHitDetectionSize = 8;

        #endregion

        #region private fields

        private bool? _dwmCompositionEnabled;
        private bool _reverseX;
        private bool _reverseY;
        private FormWindowState _lastWindowState;
        protected Padding _nonClientAreaPadding = new Padding(5, 20, 5, 5);
        private int _savedHeight;
        private int _savedWidth;
        private bool _maximized;
        private Size? _windowBorderSize;
        private WinApi.MINMAXINFO _lastMinMaxInfo;
        private bool _resizing;
        private bool _moving;

        #endregion

        #region Properties

        [Category("Yamui")]
        public bool Movable { get; set; } = true;

        [Category("Yamui")]
        public bool Resizable { get; set; } = true;

        [Browsable(false)]
        public virtual bool DwmCompositionEnabled {
            get {
                if (_dwmCompositionEnabled == null) {
                    CheckDwmCompositionEnabled();
                }

                return _dwmCompositionEnabled ?? false;
            }
        }

        [Browsable(false)]
        public virtual Padding NonClientAreaPadding => _nonClientAreaPadding;

        /// <summary>
        /// Window border size seen by windows
        /// </summary>
        private Size WindowBorderSize {
            get {
                // here we assume that the window border will never change
                if (_windowBorderSize == null) {
                    var winfo = GetWindowInfo();
                    _windowBorderSize = new Size(unchecked((int) winfo.cxWindowBorders), unchecked((int) winfo.cyWindowBorders));
                }

                return _windowBorderSize ?? new Size();
            }
        }

        [Browsable(false)]
        public bool IsActive { get; set; }

        #endregion

        #region constructor

        #region CreateParams

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;

                if (DesignMode)
                    return cp;

                if (DwmCompositionEnabled) {
                    // below is what makes the windows borderless but resizable
                    cp.Style = (int) WinApi.WindowStyles.WS_CAPTION // needed to have animation on resize for instance
                               | (int) WinApi.WindowStyles.WS_CLIPCHILDREN
                               | (int) WinApi.WindowStyles.WS_CLIPSIBLINGS
                               | (int) WinApi.WindowStyles.WS_OVERLAPPED
                               | (int) WinApi.WindowStyles.WS_SYSMENU
                               | (int) WinApi.WindowStyles.WS_MINIMIZEBOX // need to be able to minimize from taskbar
                               | (int) WinApi.WindowStyles.WS_MAXIMIZEBOX
                               | (int) WinApi.WindowStyles.WS_THICKFRAME; // needed if we want the window to be able to aero snap on screen borders
                }

                cp.ExStyle = (int) WinApi.WindowStylesEx.WS_EX_COMPOSITED
                             | (int) WinApi.WindowStylesEx.WS_EX_LEFT
                             | (int) WinApi.WindowStylesEx.WS_EX_LTRREADING
                             | (int) WinApi.WindowStylesEx.WS_EX_APPWINDOW
                             | (int) WinApi.WindowStylesEx.WS_EX_WINDOWEDGE;

                cp.ClassStyle = 0;

                return cp;
            }
        }

        #endregion

        public YamuiFormBase() {
            // why those styles? check here: 
            // https://sites.google.com/site/craigandera/craigs-stuff/windows-forms/flicker-free-control-drawing
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.Opaque, true);
            
            // icon
            if (YamuiThemeManager.GlobalIcon != null)
                Icon = YamuiThemeManager.GlobalIcon;
        }

        #endregion

        #region OnPaint

        protected override void OnPaint(PaintEventArgs e) {
            using (var b = new SolidBrush(Color.Yellow)) {
                e.Graphics.FillRectangle(b, new Rectangle(new Point(0, 0), Size));
            }
            return;
            var backColor = YamuiThemeManager.Current.FormBack;
            var borderColor = YamuiThemeManager.Current.FormBorder;

            e.Graphics.Clear(backColor);

            // draw the border with Style color
            var rect = new Rectangle(new Point(0, 0), new Size(Width, Height));
            var pen = new Pen(borderColor, BorderWidth) {
                Alignment = PenAlignment.Inset
            };
            e.Graphics.DrawRectangle(pen, rect);
        }

        #endregion

        protected override void OnCreateControl() {
            base.OnCreateControl();
            if (DwmCompositionEnabled) {
                EnableDwmComposition();
            }
        }

        #region WndProc

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg) {
                case (int) WinApi.Messages.WM_SYSCOMMAND:
                    var sc = (int) m.WParam;
                    switch (sc) {
                        // prevent the window from moving
                        case (int) WinApi.SysCommands.SC_MOVE:
                            if (!Movable)
                                return;
                            break;
                        case (int) WinApi.SysCommands.SC_RESTORE:
                        case (int) WinApi.SysCommands.SC_RESTOREDBLCLICK:
                            //SuspendLayout();
                            //Height = _savedHeight;
                            //Width = _savedWidth;
                            //ResumeLayout(false);
                            break;
                        case (int) WinApi.SysCommands.SC_MINIMIZE:
                            var f2 = GetWindowInfo();
                            //_savedHeight = Height;
                            //_savedWidth = Width;
                            break;
                        case (int) WinApi.SysCommands.SC_MAXIMIZE:
                        case (int) WinApi.SysCommands.SC_MAXIMIZEDBLCLICK:
                            //_savedHeight = Height;
                            //_savedWidth = Width;
                            break;
                    }

                    break;

                case (int) WinApi.Messages.WM_NCHITTEST:
                    // here we return on which part of the window the cursor currently is :
                    // this allows to resize the windows when grabbing edges
                    // and allows to move/double click to maximize the window if the cursor is on the caption (title) bar
                    var ht = HitTestNca(m.LParam);
                    if (ht != WinApi.HitTest.HTNOWHERE) {
                        if (Resizable || ht == WinApi.HitTest.HTCAPTION && Movable || ht != WinApi.HitTest.HTCAPTION) {
                            // when maximized, we don't need the rezising hitTest, it all becomes caption
                            if (WindowState == FormWindowState.Maximized && ht != WinApi.HitTest.HTCAPTION) {
                                ht = WinApi.HitTest.HTNOWHERE;
                            }
                        }
                    }
                    m.Result = (IntPtr) ht;
                    return;

                case (int) WinApi.Messages.WM_WINDOWPOSCHANGING:
                    // https://msdn.microsoft.com/fr-fr/library/windows/desktop/ms632653(v=vs.85).aspx?f=255&MSPPError=-2147217396
                    // https://blogs.msdn.microsoft.com/oldnewthing/20080116-00/?p=23803
                    // The WM_WINDOWPOSCHANGING message is sent early in the window state changing process, unlike WM_WINDOWPOSCHANGED, 
                    // which tells you about what already happened
                    // A crucial difference (aside from the timing) is that you can influence the state change by handling 
                    // the WM_WINDOWPOSCHANGING message and modifying the WINDOWPOS structure
                    var fuck2 = GetWindowInfo();
                    var windowpos = (WinApi.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.WINDOWPOS));
                    Marshal.StructureToPtr(windowpos, m.LParam, true);
                    //Return Zero
                    m.Result = IntPtr.Zero;
                    break;

                case (int) WinApi.Messages.WM_ENTERSIZEMOVE:
                    // Useful if your window contents are dependent on window size but expensive to compute, as they give you a way to defer paints until the end of the resize action. We found WM_WINDOWPOSCHANGING/ED wasn’t reliable for that purpose.
                    //WindowStyle &= ~WinApi.WindowStyles.WS_CAPTION;
                    break;
                case (int) WinApi.Messages.WM_EXITSIZEMOVE:
                    if (_resizing) {
                        // restore caption style
                        WindowStyle |= WinApi.WindowStyles.WS_CAPTION;
                    }
                    _resizing = false;
                    _moving = false;
                    break;

                case (int) WinApi.Messages.WM_SIZING:
                    if (!_resizing) {
                        // disable caption style to avoid seeing it when resizing 
                        WindowStyle &= ~WinApi.WindowStyles.WS_CAPTION;
                    }
                    _resizing = true;
                    break;

                case (int) WinApi.Messages.WM_MOVING:
                    _moving = true;
                    break;

                case (int) WinApi.Messages.WM_SIZE:
                    var state = (WinApi.WmSizeEnum) m.WParam;
                    var wid = m.LParam.LoWord();
                    var h = m.LParam.HiWord();
                    break;

                case (int) WinApi.Messages.WM_NCPAINT:
                    //Return Zero
                    m.Result = IntPtr.Zero;
                    break;

                case (int) WinApi.Messages.WM_GETMINMAXINFO:
                    if (DwmCompositionEnabled) {
                        // allows the window to be maximized at the size of the working area instead of the whole screen size
                        OnGetMinMaxInfo(m.HWnd, m.LParam);

                        //Return Zero
                        m.Result = IntPtr.Zero;
                        return;
                    }

                    break;

                case (int) WinApi.Messages.WM_NCCALCSIZE:
                    if (DwmCompositionEnabled) {
                        // we respond to this message to say we do not want a non client area
                        if (OnNcCalcSize(ref m))
                            return;
                    }

                    break;

                case (int) WinApi.Messages.WM_ACTIVATEAPP:
                    IsActive = (int) m.WParam != 0;
                    break;

                case (int) WinApi.Messages.WM_ACTIVATE:
                    IsActive = ((int)WinApi.WAFlags.WA_ACTIVE == (int)m.WParam || (int)WinApi.WAFlags.WA_CLICKACTIVE == (int)m.WParam);
                    break;

                case (int) WinApi.Messages.WM_CREATE:
                    if (DwmCompositionEnabled) {
                        //WinApi.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, WinApi.SetWindowPosFlags.SWP_FRAMECHANGED | WinApi.SetWindowPosFlags.SWP_NOACTIVATE | WinApi.SetWindowPosFlags.SWP_NOMOVE | WinApi.SetWindowPosFlags.SWP_NOSIZE | WinApi.SetWindowPosFlags.SWP_NOZORDER);
                        // or GetWindowRect(hWnd, &rcClient); + SetWindowPos(hWnd, SWP_FRAMECHANGED);
                    }
                    break;

                case (int) WinApi.Messages.WM_SHOWWINDOW:
                    break;

                case (int) WinApi.Messages.WM_NCACTIVATE:
                    if (DwmCompositionEnabled) {
                        /* Prevent Windows from drawing the default title bar by temporarily
                           toggling the WS_VISIBLE style. This is recommended in:
                           https://blogs.msdn.microsoft.com/wpfsdk/2008/09/08/custom-window-chrome-in-wpf/ */
                        var oldStyle = WindowStyle;
                        WindowStyle = oldStyle & ~WinApi.WindowStyles.WS_VISIBLE;
                        DefWndProc(ref m);
                        WindowStyle = oldStyle;
                    }
                    return;

                case (int) WinApi.Messages.WM_WINDOWPOSCHANGED:
                    //var newwindowpos = (WinApi.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.WINDOWPOS));
                    DefWndProc(ref m);
                    UpdateBounds();
                    m.Result = IntPtr.Zero;
                    return;

                case (int) WinApi.Messages.WM_ERASEBKGND:
                    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms648055(v=vs.85).aspx
                    m.Result = IntPtr.Zero;
                    return;

                case (int) WinApi.Messages.WM_DWMCOMPOSITIONCHANGED:
                    CheckDwmCompositionEnabled();
                    if (DwmCompositionEnabled) {
                        EnableDwmComposition();
                    } else {
                        DisableDwmComposition();
                    }

                    break;
            }

            base.WndProc(ref m);
        }



        //SetClassLong won't work correctly for 64-bit: we should use SetClassLongPtr instead.  On
        //32-bit, SetClassLongPtr is just #defined as SetClassLong.  SetClassLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr SetClassLong(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetClassLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetClassLong")]
        public static extern IntPtr SetClassLongPtr32(HandleRef hwnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetClassLongPtr")]
        public static extern IntPtr SetClassLongPtr64(HandleRef hwnd, int nIndex, IntPtr dwNewLong);


        /// <summary>
        /// Handles WM_NCCALCSIZE
        /// </summary>
        private bool OnNcCalcSize(ref Message m) {
            // by default, the proposed rect is already equals to the window size (=no NC area) so we just respond 0
            // however, in a default windows, the non client border of the window "hangs" outside the screen
            // since we don't have this non client border we don't want the window to go outside the screen...
            // we handle it here

            if (m.WParam != IntPtr.Zero) {
                // When TRUE, LPARAM Points to a NCCALCSIZE_PARAMS structure
                var nccsp = (WinApi.NCCALCSIZE_PARAMS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.NCCALCSIZE_PARAMS));
                if (OnNcCalcSize_ModifyProposedRectangle(ref nccsp.rectProposed)) {
                    Marshal.StructureToPtr(nccsp, m.LParam, true);
                }
            } else {
                // When FALSE, LPARAM Points to a RECT structure
                var clnRect = (WinApi.RECT) Marshal.PtrToStructure(m.LParam, typeof(WinApi.RECT));
                if (OnNcCalcSize_ModifyProposedRectangle(ref clnRect)) {
                    Marshal.StructureToPtr(clnRect, m.LParam, true);
                }
            }

            //Return Zero (we can return flags, see the manual)
            m.Result = IntPtr.Zero;
            return true;
        }

        private bool OnNcCalcSize_ModifyProposedRectangle(ref WinApi.RECT rect) {
            UpdateWindowState();
            if (WindowState == FormWindowState.Maximized) {
                // the proposed rect is the maximized position/size that window suggest using the "out of screen" borders
                // we change that here
                rect.left = _lastMinMaxInfo.ptMaxPosition.x;
                rect.right = _lastMinMaxInfo.ptMaxPosition.x + _lastMinMaxInfo.ptMaxSize.x;
                rect.top = _lastMinMaxInfo.ptMaxPosition.y;
                rect.bottom = _lastMinMaxInfo.ptMaxPosition.y + _lastMinMaxInfo.ptMaxSize.y;
                return true;
            }

            return false;
        }

        private void UpdateWindowState() {
            var wp = GetWindowPlacement();
            switch (wp.showCmd) {
                case (int) WinApi.ShowWindowCommands.SW_NORMAL:
                case (int) WinApi.ShowWindowCommands.SW_RESTORE:
                case (int) WinApi.ShowWindowCommands.SW_SHOW:
                case (int) WinApi.ShowWindowCommands.SW_SHOWNA:
                case (int) WinApi.ShowWindowCommands.SW_SHOWNOACTIVATE:
                    if (WindowState != FormWindowState.Normal)
                        WindowState = FormWindowState.Normal;
                    break;
                case (int) WinApi.ShowWindowCommands.SW_SHOWMAXIMIZED:
                    if (WindowState != FormWindowState.Maximized) {
                        WindowState = FormWindowState.Maximized;
                    }
                    break;
                case (int) WinApi.ShowWindowCommands.SW_SHOWMINIMIZED:
                case (int) WinApi.ShowWindowCommands.SW_MINIMIZE:
                case (int) WinApi.ShowWindowCommands.SW_SHOWMINNOACTIVE:
                    if (WindowState != FormWindowState.Minimized)
                        WindowState = FormWindowState.Minimized;
                    break;
            }
        }

        /// <summary>
        /// test in which part of the form the cursor is in, it allows to resize a borderless window
        /// </summary>
        protected virtual WinApi.HitTest HitTestNca(IntPtr lparam) {
            var cursorLocation = PointToClient(new Point(lparam.ToInt32()));

            bool resizeBorder = false;
            int uRow = 1;
            int uCol = 1;

            // Determine if the point is at the top or bottom of the window
            if (cursorLocation.Y >= 0 && cursorLocation.Y <= NonClientAreaPadding.Top) {
                resizeBorder = cursorLocation.Y <= ResizeHitDetectionSize;
                uRow = 0;
            } else if (cursorLocation.Y <= Height && cursorLocation.Y >= Height - ResizeHitDetectionSize) {
                uRow = 2;
            }

            // Determine if the point is at the left or right of the window
            if (cursorLocation.X >= 0 && cursorLocation.X <= ResizeHitDetectionSize) {
                uCol = 0;
            } else if (cursorLocation.X <= Width && cursorLocation.X >= Width - ResizeHitDetectionSize) {
                uCol = 2;
            }

            var hitTests = new[] {
                new[] {
                    WinApi.HitTest.HTTOPLEFT, resizeBorder && WindowState != FormWindowState.Maximized ? WinApi.HitTest.HTTOP : WinApi.HitTest.HTCAPTION, WinApi.HitTest.HTTOPRIGHT
                },
                new[] {
                    WinApi.HitTest.HTLEFT, WinApi.HitTest.HTNOWHERE, WinApi.HitTest.HTRIGHT
                },
                new[] {
                    WinApi.HitTest.HTBOTTOMLEFT, WinApi.HitTest.HTBOTTOM, WinApi.HitTest.HTBOTTOMRIGHT
                },
            };

            return hitTests[uRow][uCol];
        }

        #endregion

        #region Methods

        private void OnGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            _lastMinMaxInfo = (WinApi.MINMAXINFO) Marshal.PtrToStructure(lParam, typeof(WinApi.MINMAXINFO));
            var s = Screen.FromHandle(hwnd);
            _lastMinMaxInfo.ptMaxSize.x = s.WorkingArea.Width;
            _lastMinMaxInfo.ptMaxSize.y = s.WorkingArea.Height;
            _lastMinMaxInfo.ptMaxPosition.x = Math.Abs(s.WorkingArea.Left - s.Bounds.Left);
            _lastMinMaxInfo.ptMaxPosition.y = Math.Abs(s.WorkingArea.Top - s.Bounds.Top);
            Marshal.StructureToPtr(_lastMinMaxInfo, lParam, true);
        }

        public void DisableDwmComposition() {
            var margins = new WinApi.MARGINS(0, 0, 0, 0);
            WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        public void EnableDwmComposition() {
            var status = (int) WinApi.DWMNCRenderingPolicy.Enabled;
            WinApi.DwmSetWindowAttribute(Handle, WinApi.DWMWINDOWATTRIBUTE.NCRenderingPolicy, ref status, sizeof(int));

            var margins = new WinApi.MARGINS(-1, -1, -1, -1);
            WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        /// <summary>
        /// Called when the window state changes
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnWindowStateChanged(EventArgs e) { }

        /// <summary>
        /// Returns the best position for a window centered in another one
        /// </summary>
        public Point GetBestCenteredPosition(Rectangle winRect) {
            return new Point(winRect.X + (winRect.Width / 2 - Width / 2), winRect.Y + (winRect.Height / 2 - Height / 2));
        }

        /// <summary>
        /// Returns the best position (for the starting position of the form) given the spawn location
        /// (spawn location being the mouse location for a menu for instance)
        /// Example:
        /// If the spawn point if too far on the right of the screen, the location returned will spawn the menu
        /// on the left of the spawn point
        /// </summary>
        public Point GetBestMenuPosition(Point spawnLocation) {
            var screen = Screen.FromPoint(spawnLocation);
            if (spawnLocation.X > screen.WorkingArea.X + screen.WorkingArea.Width / 2) {
                spawnLocation.X = spawnLocation.X - Width;
                _reverseX = true;
            } else
                _reverseX = false;

            if (spawnLocation.Y > screen.WorkingArea.Y + screen.WorkingArea.Height / 2) {
                spawnLocation.Y = spawnLocation.Y - Height;
                _reverseY = true;
            } else
                _reverseY = false;

            return spawnLocation;
        }

        /// <summary>
        /// Returns the best position (for the starting position of the form) given the spawn location
        /// (spawn location being the mouse location for a menu for instance) for an autocompletion form
        /// </summary>
        public Point GetBestAutocompPosition(Point spawnLocation, int lineHeight) {
            var screen = Screen.FromPoint(spawnLocation);
            return GetBestAutocompPosition(spawnLocation, lineHeight, screen.WorkingArea);
        }

        /// <summary>
        /// Returns the best position (for the starting position of the form) given the spawn location
        /// (spawn location being the mouse location for a menu for instance) for an autocompletion form
        /// </summary>
        public Point GetBestAutocompPosition(Point spawnLocation, int lineHeight, Rectangle winRect) {
            // position the window smartly
            if (spawnLocation.X + Width > winRect.X + winRect.Width) {
                spawnLocation.X -= (spawnLocation.X + Width) - (winRect.X + winRect.Width);
                _reverseX = true;
            } else
                _reverseX = false;

            if (spawnLocation.Y + Height > winRect.Y + winRect.Height) {
                spawnLocation.Y = spawnLocation.Y - Height - lineHeight;
                _reverseY = true;
            } else
                _reverseY = false;

            return spawnLocation;
        }

        /// <summary>
        /// Returns the location that should be used for a window child relative to this window
        /// the location of the childRectangle should be the "default" position of the child menu
        /// (i.e. somewhere on the right of this form and between this.XY and this.Y + this.Height
        /// </summary>
        public Point GetChildBestPosition(Rectangle childRectangle, int parentLineHeight) {
            return new Point(_reverseX ? (childRectangle.X - childRectangle.Width - Width) : childRectangle.X, childRectangle.Y);
        }

        /// <summary>
        /// Returns the location that should be used for a tooltip window relative to this window
        /// </summary>
        public Point GetToolTipBestPosition(Size childSize) {
            var screen = Screen.FromPoint(Location);
            return new Point((Location.X + Width + childSize.Width > screen.WorkingArea.X + screen.WorkingArea.Width) ? Location.X - childSize.Width : Location.X + Width, _reverseY ? Location.Y + Height - childSize.Height : Location.Y);
        }

        /// <summary>
        /// Resizes the form so that it doesn't go out of screen
        /// </summary>
        protected void ResizeFormToFitScreen() {
            var loc = Location;
            loc.Offset(Width / 2, Height / 2);
            var screen = Screen.FromPoint(loc);
            if (Location.X < screen.WorkingArea.X) {
                var rightPos = Location.X + Width;
                Location = new Point(screen.WorkingArea.X, Location.Y);
                Width = rightPos - Location.X;
            }

            if (Location.X + Width > screen.WorkingArea.X + screen.WorkingArea.Width) {
                Width -= (Location.X + Width) - (screen.WorkingArea.X + screen.WorkingArea.Width);
            }

            if (Location.Y < screen.WorkingArea.Y) {
                var bottomPos = Location.Y + Height;
                Location = new Point(Location.X, screen.WorkingArea.Y);
                Height = bottomPos - Location.Y;
            }

            if (Location.Y + Height > screen.WorkingArea.Y + screen.WorkingArea.Height) {
                Height -= (Location.Y + Height) - (screen.WorkingArea.Y + screen.WorkingArea.Height);
            }
        }

        /// <summary>
        /// Check if the desktop window manager composition is enabled
        /// </summary>
        protected void CheckDwmCompositionEnabled() {
            bool enabled;
            WinApi.DwmIsCompositionEnabled(out enabled);
            _dwmCompositionEnabled = Environment.OSVersion.Version.Major >= 6 && enabled;
        }

        /// <summary>
        /// Set/get current window exstyle
        /// </summary>
        protected WinApi.WindowStylesEx WindowEStyle {
            get { return (WinApi.WindowStylesEx) unchecked((int) (long) WinApi.GetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_EXSTYLE)); }
            set { WinApi.SetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_EXSTYLE, new HandleRef(null, (IntPtr) (int) value)); }
        }

        /// <summary>
        /// Set/get current window style
        /// </summary>
        protected WinApi.WindowStyles WindowStyle {
            get { return (WinApi.WindowStyles) unchecked((int) (long) WinApi.GetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_STYLE)); }
            set { WinApi.SetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_STYLE, new HandleRef(null, (IntPtr) (int) value)); }
        }

        /// <summary>
        /// Retrieves information about this window
        /// </summary>
        protected WinApi.WINDOWINFO GetWindowInfo() {
            WinApi.WINDOWINFO info = new WinApi.WINDOWINFO();
            info.cbSize = (uint) Marshal.SizeOf(info);
            WinApi.GetWindowInfo(Handle, ref info);
            return info;
        }

        /// <summary>
        /// Retrieves placement information about this window
        /// </summary>
        protected WinApi.WINDOWPLACEMENT GetWindowPlacement() {
            WinApi.WINDOWPLACEMENT wp = new WinApi.WINDOWPLACEMENT {
                length = Marshal.SizeOf(typeof(WinApi.WINDOWPLACEMENT))
            };
            WinApi.GetWindowPlacement(new HandleRef(this, Handle), ref wp);
            return wp;
        }

        #endregion

        #region KeyDown helper

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            this.SafeSyncInvoke(form => form.OnKeyDown(e));
            return e.Handled;
        }

        #endregion
    }
}