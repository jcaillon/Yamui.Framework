﻿#region header

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
    public class YamuiFormBase : Form {
        //TODO : see what is done here..
        // https://github.com/Sardau/Sardauscan/tree/master/Sardauscan/Gui/Forms
        // https://github.com/mganss/BorderlessForm

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

        public virtual Padding NonClientAreaPadding {
            get { return _nonClientAreaPadding; }
        }

        #endregion

        #region constructor

        #region CreateParams

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;

                if (DesignMode)
                    return cp;

                cp.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_COMPOSITED;

                // below is what makes the windows borderless but resizable
                cp.Style &= ~(int) WinApi.WindowStyles.WS_SYSMENU;
                cp.Style &= ~(int) WinApi.WindowStyles.WS_CAPTION;
                cp.Style &= ~(int) WinApi.WindowStyles.WS_BORDER;
                cp.Style |= (int) WinApi.WindowStyles.WS_MINIMIZEBOX;
                cp.Style |= (int) WinApi.WindowStyles.WS_MAXIMIZEBOX;
                cp.Style |= (int) WinApi.WindowStyles.WS_THICKFRAME;
                
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
            base.OnPaint(e);
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

        #region WndProc

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg) {
                case (int) WinApi.Messages.WM_SYSCOMMAND:
                    var sc = m.WParam.ToInt64() & 0xFFF0;
                    switch (sc) {
                        // prevent the window from moving
                        case (int) WinApi.SysCommands.SC_MOVE:
                            if (!Movable)
                                return;
                            break;
                        case (int) WinApi.SysCommands.SC_RESTORE:
                            SuspendLayout();
                            Height = _savedHeight;
                            Width = _savedWidth;
                            ResumeLayout(false);
                            break;
                        case (int) WinApi.SysCommands.SC_MINIMIZE:
                        case (int) WinApi.SysCommands.SC_MAXIMIZE:
                            _savedHeight = Height;
                            _savedWidth = Width;
                            break;
                    }
                    break;

                case (int) WinApi.Messages.WM_NCHITTEST:
                    // Allows to resize the form
                    if (Resizable) {
                        var ht = HitTestNca(m.LParam);
                        if (ht != WinApi.HitTest.HTNOWHERE) {
                            m.Result = (IntPtr) ht;
                            return;
                        }
                    }

                    break;

                case (int) WinApi.Messages.WM_SIZE:
                    // Allows to resize the form
                    if (WindowState != _lastWindowState) {
                        OnWindowStateChanged(null);
                        _lastWindowState = WindowState;
                    }

                    break;

                case (int) WinApi.Messages.WM_NCPAINT:
                    // Allow to display the shadows
                    //Return Zero
                    m.Result = IntPtr.Zero;
                    break;
            }

            base.WndProc(ref m);
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
                    WinApi.HitTest.HTTOPLEFT, resizeBorder ? WinApi.HitTest.HTTOP : WinApi.HitTest.HTCAPTION, WinApi.HitTest.HTTOPRIGHT
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

        #region Events

        /// <summary>
        /// override to make a borderless window movable
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e) {
            if (Movable && e.Button == MouseButtons.Left) {
                if (WindowState == FormWindowState.Maximized)
                    return;

                // do as if the cursor was on the title bar
                WinApi.ReleaseCapture();
                WinApi.SendMessage(Handle, (uint) WinApi.Messages.WM_NCLBUTTONDOWN, new IntPtr((int) WinApi.HitTest.HTCAPTION), new IntPtr(0));
            }

            base.OnMouseDown(e);
        }

        #endregion

        #region Methods

        protected override void OnCreateControl() {
            Text = null;
            base.OnCreateControl();
        }

        protected void MakeToolWindow(CreateParams cp) {
            cp.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
            cp.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOPMOST;
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

        protected void CheckDwmCompositionEnabled() {
            bool enabled;
            WinApi.DwmIsCompositionEnabled(out enabled);
            _dwmCompositionEnabled = Environment.OSVersion.Version.Major >= 6 && enabled;
        }

        /// <devdoc>
        ///     The current exStyle of the hWnd
        /// </devdoc>
        /// <internalonly/>
        protected WinApi.WindowStyles WindowExStyle {
            get { return (WinApi.WindowStyles) unchecked((int) (long) WinApi.GetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_EXSTYLE)); }
            set { WinApi.SetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_EXSTYLE, new HandleRef(null, (IntPtr) (int) value)); }
        }

        /// <devdoc>
        ///     The current style of the hWnd
        /// </devdoc>
        /// <internalonly/>
        protected WinApi.WindowStylesEx WindowStyle {
            get { return (WinApi.WindowStylesEx) unchecked((int) (long) WinApi.GetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_STYLE)); }
            set { WinApi.SetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_STYLE, new HandleRef(null, (IntPtr) (int) value)); }
        }

        protected void SetWindowStyle(int flag, bool value) {
            int styleFlags = unchecked((int) ((long) WinApi.GetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_STYLE)));
            WinApi.SetWindowLong(new HandleRef(this, Handle), WinApi.WindowLongParam.GWL_STYLE, new HandleRef(null, (IntPtr) (value ? styleFlags | flag : styleFlags & ~flag)));
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