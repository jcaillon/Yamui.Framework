using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;

namespace Yamui.Framework.Forms.Glow {
    internal class GlowDecorator : IDisposable {
        #region [private]

        private IntPtr _parentWindowHndl;
        private Form _window;
        private readonly List<YamuiShadowBorder> _glows = new List<YamuiShadowBorder>();
        private Color _activeColor = Color.Cyan;
        private Color _inactiveColor = Color.Gray;
        private bool _isAttached;
        private bool _isEnabled;
        private bool _setTopMost;

        #endregion

        #region [internal] API and Properties

        internal Color ActiveColor {
            get { return _activeColor; }

            set {
                _activeColor = value;
                foreach (YamuiShadowBorder sideGlow in _glows) {
                    sideGlow.ActiveColor = _activeColor;
                }
            }
        }

        internal Color InactiveColor {
            get { return _inactiveColor; }

            set {
                _inactiveColor = value;
                foreach (YamuiShadowBorder sideGlow in _glows) {
                    sideGlow.InactiveColor = _inactiveColor;
                }
            }
        }

        internal bool IsEnabled {
            get { return _isEnabled; }
        }

        internal void Attach(Form window, bool enable = true) {
            if (_isAttached) {
                return;
            }

            _isAttached = true;
            _window = window;
            _parentWindowHndl = window.Handle;

            _glows.Add(new YamuiShadowBorder(DockStyle.Top, _parentWindowHndl));
            _glows.Add(new YamuiShadowBorder(DockStyle.Left, _parentWindowHndl));
            _glows.Add(new YamuiShadowBorder(DockStyle.Bottom, _parentWindowHndl));
            _glows.Add(new YamuiShadowBorder(DockStyle.Right, _parentWindowHndl));

            foreach (var yamuiShadowBorder in _glows) {
                yamuiShadowBorder.Show(true);
            }
            
            _isEnabled = false;
            AlignSideGlowTopMost();
            Enable(true);
        }

        private WinApi.WINDOWPOS _lastLocation;

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (!_isEnabled) 
                return (IntPtr) 0;

            switch ((WinApi.Messages) msg) {
                case WinApi.Messages.WM_ENTERSIZEMOVE:
                    // Useful if your window contents are dependent on window size but expensive to compute, as they give you a way to defer paints until the end of the resize action. We found WM_WINDOWPOSCHANGING/ED wasn’t reliable for that purpose.
                    //Show(false);
                    break;

                case WinApi.Messages.WM_EXITSIZEMOVE:
                    //Show(true);
                    break;

                case WinApi.Messages.WM_SIZING:

                    break;

                case WinApi.Messages.WM_MOVING:
                    break;

                case WinApi.Messages.WM_SIZE:
                    // var state = (WinApi.WmSizeEnum) m.WParam;
                    // var wid = m.LParam.LoWord();
                    // var h = m.LParam.HiWord();
                    // Size(wParam, lParam);
                    break;

                case WinApi.Messages.WM_ACTIVATEAPP:
                    UpdateFocus((int) wParam != 0);
                    break;

                case WinApi.Messages.WM_ACTIVATE:
                    UpdateFocus(((int)WinApi.WAFlags.WA_ACTIVE == (int)wParam || (int)WinApi.WAFlags.WA_CLICKACTIVE == (int)wParam));
                    break;
                    
                case WinApi.Messages.WM_SHOWWINDOW:
                    break;

                case WinApi.Messages.WM_WINDOWPOSCHANGING:
                    // the default From handler for this message messes up the restored height/width for non client area window
                    // var newwindowpos = (WinApi.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.WINDOWPOS));
                    _lastLocation = (WinApi.WINDOWPOS) Marshal.PtrToStructure(lParam, typeof(WinApi.WINDOWPOS));
                    UpdateLocations(_lastLocation);
                    UpdateSizes(_lastLocation.cx, _lastLocation.cy);
                    break;

                case WinApi.Messages.WM_SETFOCUS:
                    //UpdateFocus();
                    //UpdateZOrder();
                    break;
                case WinApi.Messages.WM_KILLFOCUS:
                    //KillFocus();
                    //UpdateZOrder();
                    break;
            }

            return (IntPtr) 0;
        }

        internal void Detach() {
            _isAttached = false;
            Show(false);
        }

        /// <summary>
        /// Enables or disables the glow effect on the window
        /// </summary>
        /// <param name="enable">Enable mode</param>
        internal void Enable(bool enable) {
            if (_isEnabled && !enable) {
                Show(false);
            } else if (!_isEnabled && enable) {
                if (_window != null) {
                    UpdateLocations(new WinApi.WINDOWPOS {
                        x = _window.Left,
                        y = _window.Top,
                        cx = _window.Width,
                        cy = _window.Height,
                        flags = WinApi.SetWindowPosFlags.SWP_SHOWWINDOW
                    });
                    UpdateSizes(_window.Width, _window.Height);
                    UpdateFocus(_window.Focused);
                }
            }

            _isEnabled = enable;
        }

        #endregion

        #region [private]

        private void DestroyGlows() {
            _parentWindowHndl = IntPtr.Zero;
            CloseGlows();
            _window = null;
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

        private void UpdateZOrder() {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.UpdateZOrder();
            }
        }

        private void UpdateFocus(bool isFocused) {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.ParentWindowIsFocused = isFocused;
            }
        }

        private void UpdateSizes(int width, int height) {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.SetSize(width, height);
            }
        }

        private void UpdateLocations(WinApi.WINDOWPOS location) {
            foreach (YamuiShadowBorder sideGlow in _glows) {
                sideGlow.SetLocation(location);
            }
            if (((int) location.flags & (int) WinApi.SetWindowPosFlags.SWP_HIDEWINDOW) != 0) {
                Show(false);
            } else if (((int) location.flags & (int) WinApi.SetWindowPosFlags.SWP_SHOWWINDOW) != 0) {
                Show(true);
                UpdateZOrder();
            }
        }

        private void AlignSideGlowTopMost() {
            if (_glows == null) {
                return;
            }
            foreach (YamuiShadowBorder glow in _glows) {
                glow.IsTopMost = _setTopMost;
                glow.UpdateZOrder();
            }
        }

        #endregion
        
        #region Dispose

        private bool _isDisposed;

        /// <summary>
        /// Standard Dispose
        /// </summary>
        public void Dispose() {
            if (!_isDisposed) {
                _isDisposed = true;
                Detach();
                DestroyGlows();
                _window = null;
            }
            GC.SuppressFinalize(this);
        }
        
        #endregion
    }
}