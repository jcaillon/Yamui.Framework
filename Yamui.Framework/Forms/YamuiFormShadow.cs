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
using System.Timers;
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
        private System.Timers.Timer _timer;
        private bool _bordersVisible;

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
                CloseShadows();
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
                case Window.Msg.WM_ACTIVATEAPP:
                    UpdateFocus((int) m.WParam != 0);
                    break;

                case Window.Msg.WM_ACTIVATE:
                    UpdateFocus(((int) WinApi.WAFlags.WA_ACTIVE == (int)m.WParam || (int)WinApi.WAFlags.WA_CLICKACTIVE == (int)m.WParam));
                    break;

                case Window.Msg.WM_WINDOWPOSCHANGED:
                    _lastLocation = (WinApi.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.WINDOWPOS));
                    UpdateLocationAndSize(_lastLocation);

                    if (((int) _lastLocation.flags & (int) WinApi.SetWindowPosFlags.SWP_HIDEWINDOW) != 0) {
                        Show(false);
                    } else if (((int) _lastLocation.flags & (int) WinApi.SetWindowPosFlags.SWP_SHOWWINDOW) != 0) {
                        Show(!IsMaximized, true);
                    }
                    break;

                
                case Window.Msg.WM_SHOWWINDOW:
                    Show(m.WParam != IntPtr.Zero, true);
                    break;

                case Window.Msg.WM_CREATE:
                    InitBorders();
                    break;
                   /* 
                case Window.Msg.WM_MOVE:
                    var pos = GetWindowInfo().rcWindow;
                    var pos3 = new Point(m.LParam.LoWord(), m.LParam.HiWord());
                    foreach (YamuiShadowBorder sideGlow in _glows) {
                        sideGlow.SetLocationAndSize(pos.left, pos.top, pos.right - pos.left, pos.bottom - pos.top);
                    }
                    break;
                    */
                case Window.Msg.WM_SIZE:
                    /*
                    var pos2 = GetWindowInfo().rcWindow;
                    var newSize = new Size(m.LParam.LoWord(), m.LParam.HiWord());
                    foreach (YamuiShadowBorder sideGlow in _glows) {
                        sideGlow.SetLocationAndSize(pos2.left, pos2.top, pos2.right - pos2.left, pos2.bottom - pos2.top);
                    }
                    */
                    var state = (WinApi.WmSizeEnum) m.WParam;
                    switch (state) {
                        case WinApi.WmSizeEnum.SIZE_RESTORED:
                            Show(true);
                            break;
                        case WinApi.WmSizeEnum.SIZE_MINIMIZED:
                        case WinApi.WmSizeEnum.SIZE_MAXHIDE:
                        case WinApi.WmSizeEnum.SIZE_MAXIMIZED:
                        case WinApi.WmSizeEnum.SIZE_MAXSHOW:
                            Show(false);
                            break;
                    }
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
            CloseShadows();
            base.OnClosed(e);
        }

        private void InitBorders() {
            if (!HasShadow)
                return;

            _bordersVisible = false;
            _timer = new System.Timers.Timer {
                AutoReset = false,
                Interval = 200
            };
            _timer.Elapsed += TimerOnElapsed;

            _glows.Add(new YamuiShadowBorder(DockStyle.Top, Handle));
            _glows.Add(new YamuiShadowBorder(DockStyle.Left, Handle));
            _glows.Add(new YamuiShadowBorder(DockStyle.Bottom, Handle));
            _glows.Add(new YamuiShadowBorder(DockStyle.Right, Handle));
        }

        private void CloseShadows() {
            _timer?.Stop();
            _timer?.Dispose();
            foreach (var sideGlow in _glows) {
                sideGlow.Close();
                sideGlow.Dispose();
            }
            _glows.Clear();
        }

        private void Show(bool show, bool immediaty = false) {
            if (_bordersVisible == show)
                return;

            _timer?.Stop();
           if (immediaty || !show) {
                foreach (YamuiShadowBorder sideGlow in _glows) {
                    sideGlow.Show(show);
                }
               _bordersVisible = show;
            } else {
                _timer?.Start();
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e) {
            _timer?.Stop();
            Show(true, true);
        }
        
        private void UpdateFocus(bool isFocused) {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.ParentWindowIsFocused = isFocused;
            }
        }
        
        private void UpdateLocationAndSize(WinApi.WINDOWPOS pos) {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.SetLocationAndSize(pos.x, pos.y, pos.cx, pos.cy);
            }
        }
    }
}