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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Forms {

    /// <summary>
    /// Form class that implements interesting utilities + shadow + onpaint + movable/resizable borderless
    /// </summary>
    public abstract partial class YamuiForm : Form {

        #region Constants

        public const int BorderWidth = 1;

        #endregion

        #region private fields

        private bool _reverseX;
        private bool _reverseY;
        protected Padding _nonClientAreaPadding = new Padding(5, 5, 5, 5);
        private WinApi.MINMAXINFO _lastMinMaxInfo;
        private bool _isActive;
        private WinApi.HitTest[][] _hitTests;
        private bool _needRedraw;

        #endregion

        #region Properties

        [Category("Yamui")]
        public bool Movable { get; set; } = true;

        [Category("Yamui")]
        public bool Resizable { get; set; } = true;

        [Browsable(false)]
        public virtual Padding NonClientAreaPadding => _nonClientAreaPadding;

        [Browsable(false)]
        public virtual int TitleBarHeight => 20;

        [Browsable(false)]
        public bool Resizing { get; private set; }

        [Browsable(false)]
        public bool Moving { get; private set; }

        [Browsable(false)]
        public bool IsActive {
            get { return _isActive; }
            set {
                if (_isActive != value) {
                    _isActive = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        protected bool IsMaximized => GetWindowPlacement().showCmd == WinApi.ShowWindowStyle.SW_SHOWMAXIMIZED;

        [Browsable(false)]
        public bool AlwaysOnTop { get; }

        [Browsable(false)]
        public bool IsPopup { get; }

        [Browsable(false)]
        public bool DontShowInAltTab { get; }

        [Browsable(false)]
        public bool DontActivateOnShow { get; }

        [Browsable(false)]
        public bool Unselectable { get; }

        [Browsable(false)]
        public override Color BackColor {
            get { return YamuiThemeManager.Current.FormBack; }
            set { base.BackColor = value; }
        }

        [Browsable(false)]
        public override Color ForeColor {
            get { return YamuiThemeManager.Current.FormFore; }
            set { base.ForeColor = value; }
        }
        
        [Browsable(false)]
        public virtual Color BorderActiveColor => YamuiThemeManager.Current.AccentColor;

        [Browsable(false)]
        public virtual Color BorderInactiveColor => YamuiThemeManager.Current.FormBorder;

        /// <summary>
        /// This indicates that the form should not take focus when shown
        /// </summary>
        [Browsable(false)]
        protected override bool ShowWithoutActivation {
            get { return DontActivateOnShow; }
        }

        [Browsable(false)]
        public new FormWindowState WindowState {
            get { return IsMaximized ? FormWindowState.Maximized : base.WindowState; }
            set { base.WindowState = value; }
        }

        #endregion

        #region constructor
        
        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;

                if (DesignMode)
                    return cp;

                if (IsPopup) {
                    cp.Style = (int) WinApi.WindowStyles.WS_POPUP;
                    cp.ExStyle = (int) (WinApi.WindowStylesEx.WS_EX_COMPOSITED
                        | WinApi.WindowStylesEx.WS_EX_LEFT
                        | WinApi.WindowStylesEx.WS_EX_LTRREADING);
                } else {
                    // below is what makes the windows borderless but resizable
                    cp.Style = (int) (WinApi.WindowStyles.WS_CAPTION // enables aero minimize animation/transition
                        | WinApi.WindowStyles.WS_CLIPCHILDREN
                        | WinApi.WindowStyles.WS_CLIPSIBLINGS
                        | WinApi.WindowStyles.WS_OVERLAPPED
                        | WinApi.WindowStyles.WS_SYSMENU // enables the context menu with the move, close, maximize, minize... commands (shift + right-click on the task bar item)
                        | WinApi.WindowStyles.WS_MINIMIZEBOX // need to be able to minimize from taskbar
                        | WinApi.WindowStyles.WS_MAXIMIZEBOX // same for maximize
                        | WinApi.WindowStyles.WS_THICKFRAME); // without this the window cannot be resized and so aero snap, de-maximizing and minimizing won't work

                    cp.ExStyle = (int) (WinApi.WindowStylesEx.WS_EX_COMPOSITED
                        | WinApi.WindowStylesEx.WS_EX_LEFT
                        | WinApi.WindowStylesEx.WS_EX_LTRREADING
                        | WinApi.WindowStylesEx.WS_EX_APPWINDOW
                        | WinApi.WindowStylesEx.WS_EX_WINDOWEDGE);
                }

                if (DontShowInAltTab) {
                    cp.ExStyle = cp.ExStyle | (int) WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
                }

                if (AlwaysOnTop) {
                    cp.ExStyle = cp.ExStyle | (int) WinApi.WindowStylesEx.WS_EX_TOPMOST;
                }

                if (Unselectable) {
                    cp.ExStyle = cp.ExStyle | (int) WinApi.WindowStylesEx.WS_EX_TRANSPARENT;
                    cp.ExStyle = cp.ExStyle | (int) WinApi.WindowStylesEx.WS_EX_NOACTIVATE;
                    cp.Style = cp.Style | (int) WinApi.WindowStyles.WS_DISABLED;
                }
                
                cp.ClassStyle = 0;

                return cp;
            }
        }

        protected YamuiForm(YamuiFormOption formOptions) {
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

            // init hittest table
            _hitTests = new[] {
                new[] {
                    WinApi.HitTest.HTTOPLEFT, WinApi.HitTest.HTTOP, WinApi.HitTest.HTTOPRIGHT
                },
                new[] {
                    WinApi.HitTest.HTLEFT, WinApi.HitTest.HTCLIENT, WinApi.HitTest.HTRIGHT
                },
                new[] {
                    WinApi.HitTest.HTBOTTOMLEFT, WinApi.HitTest.HTBOTTOM, WinApi.HitTest.HTBOTTOMRIGHT
                },
            };

            AlwaysOnTop = formOptions.HasFlag(YamuiFormOption.AlwaysOnTop);
            IsPopup = formOptions.HasFlag(YamuiFormOption.IsPopup);
            DontShowInAltTab = formOptions.HasFlag(YamuiFormOption.DontShowInAltTab);
            DontActivateOnShow = formOptions.HasFlag(YamuiFormOption.DontActivateOnShow);
            Unselectable = formOptions.HasFlag(YamuiFormOption.Unselectable);

            if (IsPopup) {
                ShowInTaskbar = false;
            }
            SizeGripStyle = SizeGripStyle.Hide;
        }

        

        #endregion

        #region OnPaint

        protected override void OnPaint(PaintEventArgs e) {
            using (var b = new SolidBrush(BackColor)) {
                e.Graphics.FillRectangle(b, ClientRectangle);
            }
            var borderRect = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            using (var p = new Pen(IsActive ? BorderActiveColor : BorderInactiveColor, BorderWidth) {
                Alignment = PenAlignment.Inset
            }) {
                e.Graphics.DrawRectangle(p, borderRect);
            }
        }

        #endregion
        
        #region WndProc

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }
            
            switch ((Window.Msg) m.Msg) {

                case Window.Msg.WM_SYSCOMMAND:
                    switch ((WinApi.SysCommands) m.WParam) {
                        // prevent the window from moving
                        case WinApi.SysCommands.SC_MOVE:
                            if (!Movable)
                                return;
                            break;
                    }
                    break;

                case Window.Msg.WM_NCHITTEST:
                    // here we return on which part of the window the cursor currently is :
                    // this allows to resize the windows when grabbing edges
                    // and allows to move/double click to maximize the window if the cursor is on the caption (title) bar
                    var ht = HitTestNca(m.LParam);
                    if (!Movable && ht == WinApi.HitTest.HTCAPTION) {
                        ht = WinApi.HitTest.HTNOWHERE;
                    }
                    if (!Resizable && (int)ht >= (int)WinApi.HitTest.HTRESIZESTARTNUMBER && (int)ht <= (int)WinApi.HitTest.HTRESIZEENDNUMBER) {
                        ht = WinApi.HitTest.HTNOWHERE;
                    }
                    m.Result = (IntPtr) ht;
                    return;

                //case Window.Msg.WM_WINDOWPOSCHANGING:
                    // https://msdn.microsoft.com/fr-fr/library/windows/desktop/ms632653(v=vs.85).aspx?f=255&MSPPError=-2147217396
                    // https://blogs.msdn.microsoft.com/oldnewthing/20080116-00/?p=23803
                    // The WM_WINDOWPOSCHANGING message is sent early in the window state changing process, unlike WM_WINDOWPOSCHANGED, 
                    // which tells you about what already happened
                    // A crucial difference (aside from the timing) is that you can influence the state change by handling 
                    // the WM_WINDOWPOSCHANGING message and modifying the WINDOWPOS structure
                    // var windowpos = (WinApi.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.WINDOWPOS));
                    //break;

                case Window.Msg.WM_ENTERSIZEMOVE:
                    // Useful if your window contents are dependent on window size but expensive to compute, as they give you a way to defer paints until the end of the resize action. We found WM_WINDOWPOSCHANGING/ED wasn’t reliable for that purpose.
                    break;

                case Window.Msg.WM_EXITSIZEMOVE:
                    if (Resizing) {
                        // restore caption style
                        WindowStyle |= WinApi.WindowStyles.WS_CAPTION;
                    }
                    Resizing = false;
                    Moving = false;
                    break;

                case Window.Msg.WM_SIZING:
                    if (!Resizing) {
                        // disable caption style to avoid seeing it when resizing 
                        WindowStyle &= ~WinApi.WindowStyles.WS_CAPTION;
                    }
                    Resizing = true;
                    break;

                case Window.Msg.WM_MOVING:
                    Moving = true;
                    break;

                case Window.Msg.WM_MBUTTONDOWN:
                    //var state = (WinApi.WmSizeEnum) m.WParam;
                    //if (state == WinApi.WmSizeEnum.SIZE_MAXIMIZED || state == WinApi.WmSizeEnum.SIZE_MAXSHOW) {
                        Refresh();
                    //}
                    // var wid = m.LParam.LoWord();
                    // var h = m.LParam.HiWord();
                    break;

                case Window.Msg.WM_SIZE:
                    var state = (WinApi.WmSizeEnum) m.WParam;
                    if (state == WinApi.WmSizeEnum.SIZE_MAXIMIZED || state == WinApi.WmSizeEnum.SIZE_MAXSHOW || state == WinApi.WmSizeEnum.SIZE_RESTORED) {
                        _needRedraw = true;
                    }
                    // var wid = m.LParam.LoWord();
                    // var h = m.LParam.HiWord();
                    break;

                case Window.Msg.WM_NCPAINT:
                    //Return Zero
                    m.Result = IntPtr.Zero;
                    break;

                case Window.Msg.WM_GETMINMAXINFO:
                    // allows the window to be maximized at the size of the working area instead of the whole screen size
                    OnGetMinMaxInfo(ref m);
                    //Return Zero
                    m.Result = IntPtr.Zero;
                    return;

                case Window.Msg.WM_NCCALCSIZE:
                    // we respond to this message to say we do not want a non client area
                    if (OnNcCalcSize(ref m))
                        return;
                    break;

                case Window.Msg.WM_ACTIVATEAPP:
                    IsActive = (int) m.WParam != 0;
                    break;

                case Window.Msg.WM_ACTIVATE:
                    IsActive = ((int)WinApi.WAFlags.WA_ACTIVE == (int)m.WParam || (int)WinApi.WAFlags.WA_CLICKACTIVE == (int)m.WParam);
                    break;
                    
                case Window.Msg.WM_NCACTIVATE:
                    /* Prevent Windows from drawing the default title bar by temporarily
                        toggling the WS_VISIBLE style. This is recommended in:
                        https://blogs.msdn.microsoft.com/wpfsdk/2008/09/08/custom-window-chrome-in-wpf/ */
                    var oldStyle = WindowStyle;
                    WindowStyle = oldStyle & ~WinApi.WindowStyles.WS_VISIBLE;
                    DefWndProc(ref m);
                    WindowStyle = oldStyle;
                    return;

                case Window.Msg.WM_WINDOWPOSCHANGED:
                    // the default From handler for this message messes up the restored height/width for non client area window
                    // var newwindowpos = (WinApi.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.WINDOWPOS));
                    DefWndProc(ref m);
                    UpdateBounds();
                    m.Result = IntPtr.Zero;
                    return;

                case Window.Msg.WM_ERASEBKGND:
                    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms648055(v=vs.85).aspx
                    m.Result = IntPtr.Zero;
                    // hack thing to correctly redraw th window after maximizing it
                    if (_needRedraw) {
                        _needRedraw = false;
                        WinApi.RedrawWindow(new HandleRef(this, Handle), IntPtr.Zero, IntPtr.Zero, WinApi.RedrawWindowFlags.Invalidate);
                    }
                    return;
            }

            base.WndProc(ref m);
        }

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
            if (IsMaximized) {
                var s = Screen.FromHandle(Handle);
                // the proposed rect is the maximized position/size that window suggest using the "out of screen" borders
                // we change that here
                rect.Set(s.WorkingArea);
                return true;
            }
            return false;
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
            if (cursorLocation.Y >= 0 && cursorLocation.Y <= TitleBarHeight) {
                resizeBorder = cursorLocation.Y <= NonClientAreaPadding.Top && WindowState != FormWindowState.Maximized;
                uRow = 0;
            } else if (cursorLocation.Y <= Height && cursorLocation.Y >= Height - NonClientAreaPadding.Bottom) {
                uRow = 2;
            }

            // Determine if the point is at the left or right of the window
            if (cursorLocation.X >= 0 && cursorLocation.X <= NonClientAreaPadding.Left) {
                uCol = 0;
            } else if (cursorLocation.X <= Width && cursorLocation.X >= Width - NonClientAreaPadding.Right) {
                uCol = 2;
            } else if (uRow == 0 && !resizeBorder) {
                return WinApi.HitTest.HTCAPTION;
            }

            return _hitTests[uRow][uCol];

        }

        private void OnGetMinMaxInfo(ref Message m) {
            _lastMinMaxInfo = (WinApi.MINMAXINFO) Marshal.PtrToStructure(m.LParam, typeof(WinApi.MINMAXINFO));
            var s = Screen.FromHandle(Handle);
            _lastMinMaxInfo.ptMaxPosition.x = Math.Abs(s.WorkingArea.Left - s.Bounds.Left);
            _lastMinMaxInfo.ptMaxPosition.y = Math.Abs(s.WorkingArea.Top - s.Bounds.Top);
            _lastMinMaxInfo.ptMaxSize.x = s.WorkingArea.Width;
            _lastMinMaxInfo.ptMaxSize.y = s.WorkingArea.Height;
            Marshal.StructureToPtr(_lastMinMaxInfo, m.LParam, true);
        }

        #endregion

        #region Methods
        
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
        /// Set/get current window exstyle
        /// </summary>
        protected WinApi.WindowStylesEx WindowExStyle {
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