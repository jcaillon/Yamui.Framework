#region header

// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormBaseShadow.cs) is part of YamuiFramework.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Forms {

    /// <summary>
    /// Form class that implements interesting utilities + shadow + onpaint + movable borderless
    /// </summary>
    public abstract class YamuiFormShadow : YamuiForm {

        private readonly List<YamuiShadowBorder> _glows = new List<YamuiShadowBorder>();
        private WinApi.WINDOWPOS _lastLocation;

        #region Constructor

        [Browsable(false)]
        public bool HasShadow { get; }

        [Browsable(false)]
        public bool HasDropShadow { get; }

        [Browsable(false)]
        public Color BorderActiveColor => YamuiThemeManager.Current.AccentColor;

        [Browsable(false)]
        public Color BorderInactiveColor => YamuiThemeManager.Current.FormBorder;

        #endregion

        #region Life and death

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;

                if (DesignMode)
                    return cp;

                if (HasDropShadow) {
                    cp.ClassStyle = (int) WinApi.WindowClassStyles.DropShadow;
                }

                return cp;
            }
        }

        protected YamuiFormShadow(YamuiFormOption formOptions) : base(formOptions) {
            HasShadow = formOptions.HasFlag(YamuiFormOption.WithShadow);
            HasDropShadow = formOptions.HasFlag(YamuiFormOption.WithDropShadow);
            if (HasDropShadow) {
                HasShadow = false;
            }
        }

        private bool _isDisposed;

        /// <summary>
        /// Standard Dispose
        /// </summary>
        public new void Dispose() {
            if (!_isDisposed) {
                _isDisposed = true;
                CloseGlows();
                GC.SuppressFinalize(this);
            }
            base.Dispose();
        }

        #endregion

        protected override void WndProc(ref Message m) {
            if (!HasShadow || DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch ((Window.Msg) m.Msg) {
                //case Window.Msg.WM_SYSCOMMAND:
                //    switch ((WinApi.SysCommands) m.WParam) {
                //        case WinApi.SysCommands.SC_RESTORE:
                //        case WinApi.SysCommands.SC_RESTOREDBLCLICK:
                //            // we have to handle this restore to not have the normal restore animation on the window
                //            // otherwise the borders are displayed at the restored location BEFORE the window finishes its animation
                //            var hdlRef = new HandleRef(null, m.HWnd);
                //            WinApi.WINDOWPLACEMENT wp = new WinApi.WINDOWPLACEMENT {
                //                length = Marshal.SizeOf(typeof(WinApi.WINDOWPLACEMENT))
                //            };
                //            WinApi.GetWindowPlacement(hdlRef, ref wp);
                //            if (wp.showCmd == WinApi.ShowWindowStyle.SW_SHOWMINIMIZED) {
                //                wp.showCmd = WinApi.ShowWindowStyle.SW_RESTORE;
                //                WinApi.SetWindowPlacement(hdlRef, ref wp);
                //                m.Result = IntPtr.Zero;
                //                return;
                //            }
                //            break;
                //    }
                //    break;
                    
                case Window.Msg.WM_ACTIVATEAPP:
                    UpdateFocus((int) m.WParam != 0);
                    break;

                case Window.Msg.WM_ACTIVATE:
                    UpdateFocus(((int) WinApi.WAFlags.WA_ACTIVE == (int)m.WParam || (int)WinApi.WAFlags.WA_CLICKACTIVE == (int)m.WParam));
                    break;

                case Window.Msg.WM_WINDOWPOSCHANGED:
                    _lastLocation = (WinApi.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.WINDOWPOS));
                    UpdateLocationAndSize(_lastLocation);
                    break;

                case Window.Msg.WM_CREATE:
                    InitBorders();
                    break;

            }
            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e) {
            using (var b = new SolidBrush(YamuiThemeManager.Current.FormBack)) {
                e.Graphics.FillRectangle(b, ClientRectangle);
            }
            var borderRect = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            using (var p = new Pen(IsActive ? BorderActiveColor : BorderInactiveColor, BorderWidth) {
                Alignment = PenAlignment.Inset
            }) {
                e.Graphics.DrawRectangle(p, borderRect);
            }
        }

        protected override void OnClosed(EventArgs e) {
            CloseGlows();
            base.OnClosed(e);
        }

        private void InitBorders() {
            if (!HasShadow)
                return;

            _glows.Add(new YamuiShadowBorder(DockStyle.Top, Handle, BorderActiveColor, BorderInactiveColor));
            _glows.Add(new YamuiShadowBorder(DockStyle.Left, Handle, BorderActiveColor, BorderInactiveColor));
            _glows.Add(new YamuiShadowBorder(DockStyle.Bottom, Handle, BorderActiveColor, BorderInactiveColor));
            _glows.Add(new YamuiShadowBorder(DockStyle.Right, Handle, BorderActiveColor, BorderInactiveColor));
        }
        
        private void CloseGlows() {
            foreach (var sideGlow in _glows) {
                sideGlow.Close();
                sideGlow.Dispose();
            }
            _glows.Clear();
        }

        private void Show(bool show) {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.Show(show);
            }
        }

        private void UpdateColors() {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.SetColors(BorderActiveColor, BorderInactiveColor);
            }
        }

        private void UpdateFocus(bool isFocused) {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.ParentWindowIsFocused = isFocused;
            }
        }
        
        private void UpdateLocationAndSize(WinApi.WINDOWPOS pos) {
            var isMaximized = IsMaximized;
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.SetLocationAndSize(pos.x, pos.y, pos.cx, pos.cy);
            }
            if (((int) pos.flags & (int) WinApi.SetWindowPosFlags.SWP_HIDEWINDOW) != 0) {
                Show(false);
            } else if (((int) pos.flags & (int) WinApi.SetWindowPosFlags.SWP_SHOWWINDOW) != 0) {
                Show(!isMaximized);
            } else if (Visible) {
                Show(!isMaximized);
            }
        }
    }
}