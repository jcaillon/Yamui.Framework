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
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using Yamui.Framework.Helper;

namespace Yamui.Framework.Forms {

    public abstract class YamuiFormShadow : YamuiForm {

        private readonly List<YamuiShadowBorder> _shadows = new List<YamuiShadowBorder>();
        private volatile bool _bordersVisible;
        private System.Timers.Timer _restoreAnimationTimer;
        
        private bool _shownOnce;
        private bool _isDisposed;
        private bool _isInitialized;

        #region Constructor

        [Browsable(false)]
        public bool HasShadow { get; }

        [Browsable(false)]
        public bool HasDropShadow { get; }

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
                    UpdateFocus(((int) WinApi.WAFlags.WA_ACTIVE == (int) m.WParam || (int) WinApi.WAFlags.WA_CLICKACTIVE == (int) m.WParam));
                    break;

                case Window.Msg.WM_WINDOWPOSCHANGED:
                    var windowPos = (WinApi.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.WINDOWPOS));
                    UpdateLocationAndSize(windowPos);

                    if (((int) windowPos.flags & (int) WinApi.SetWindowPosFlags.SWP_HIDEWINDOW) != 0) {
                        ShowShadows(false, true);
                    } else if (((int) windowPos.flags & (int) WinApi.SetWindowPosFlags.SWP_SHOWWINDOW) != 0) {
                        ShowShadows(!IsMaximized, true);
                    }

                    break;

                case Window.Msg.WM_SHOWWINDOW:
                    ShowShadows(m.WParam != IntPtr.Zero, true);
                    break;

                case Window.Msg.WM_CREATE:
                    if (!_isInitialized) {
                        InitShadows();
                        _isInitialized = true;
                    }
                    break;

                case Window.Msg.WM_SIZE:
                    var state = (WinApi.WmSizeEnum) m.WParam;
                    switch (state) {
                        case WinApi.WmSizeEnum.SIZE_RESTORED:
                            ShowShadows(true, false);
                            break;
                        case WinApi.WmSizeEnum.SIZE_MINIMIZED:
                        case WinApi.WmSizeEnum.SIZE_MAXHIDE:
                        case WinApi.WmSizeEnum.SIZE_MAXIMIZED:
                        case WinApi.WmSizeEnum.SIZE_MAXSHOW:
                            ShowShadows(false, true);
                            break;
                    }

                    break;

                //case Window.Msg.WM_MOVE:
                //var pos = GetWindowInfo().rcWindow;
                //var pos3 = new Point(m.LParam.LoWord(), m.LParam.HiWord());
                //foreach (YamuiShadowBorder sideGlow in _glows) {
                //    sideGlow.SetLocationAndSize(pos.left, pos.top, pos.right - pos.left, pos.bottom - pos.top);
                //}
                //break;
            }

            base.WndProc(ref m);
        }

        protected override void OnClosed(EventArgs e) {
            CloseShadows();
            base.OnClosed(e);
        }

        protected override void OnShown(EventArgs e) {
            _shownOnce = true;
            if (Visible) {
                ShowShadows(true, false);
            }
            base.OnShown(e);
        }

        private void InitShadows() {
            if (!HasShadow)
                return;

            _bordersVisible = false;
            _restoreAnimationTimer = new System.Timers.Timer {
                AutoReset = false,
                Interval = 500
            };
            _restoreAnimationTimer.Elapsed += RestoreAnimationTimerOnElapsed;

            _shadows.Add(new YamuiShadowBorder(DockStyle.Top, Handle));
            _shadows.Add(new YamuiShadowBorder(DockStyle.Left, Handle));
            _shadows.Add(new YamuiShadowBorder(DockStyle.Bottom, Handle));
            _shadows.Add(new YamuiShadowBorder(DockStyle.Right, Handle));
        }

        private void CloseShadows() {
            _restoreAnimationTimer?.Stop();
            _restoreAnimationTimer?.Dispose();
            foreach (var shadowBorder in _shadows) {
                shadowBorder.Close();
                shadowBorder.Dispose();
            }

            _shadows.Clear();
        }

        private void ShowShadows(bool show, bool immediaty) {
            _restoreAnimationTimer.Enabled = false;
            
            if (_bordersVisible == show)
                return;
            if (!_shownOnce)
                return;

            // why this animation? Because of the restore animation in windows : the borders would be shown at the restored location
            // before the parent finishes its animation

            if (immediaty || !show) {
                foreach (YamuiShadowBorder sideGlow in _shadows) {
                    sideGlow.Show(show);
                }

                _bordersVisible = show;
            } else {
                _restoreAnimationTimer?.Start();
            }
        }

        private void RestoreAnimationTimerOnElapsed(object sender, ElapsedEventArgs e) {
            _restoreAnimationTimer?.Stop();
            ShowShadows(true, true);
        }

        private void UpdateFocus(bool isFocused) {
            foreach (YamuiShadowBorder shadowBorder in _shadows) {
                shadowBorder.ParentWindowIsFocused = isFocused;
            }
        }

        private void UpdateLocationAndSize(WinApi.WINDOWPOS pos) {
            foreach (YamuiShadowBorder shadowBorder in _shadows) {
                shadowBorder.SetLocationAndSize(pos.x, pos.y, pos.cx, pos.cy);
            }
        }
    }
}