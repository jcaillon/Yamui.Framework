using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Yamui.Framework.Helper;

namespace WpfGlowWindow.Glow
{
    internal class GlowDecorator : IDisposable
    {
        #region [private]

        private IntPtr _parentWindowHndl;
        private Form _window;
        private SideGlow _topGlow;
        private SideGlow _leftGlow;
        private SideGlow _bottomGlow;
        private SideGlow _rightGlow;
        private readonly List<SideGlow> _glows = new List<SideGlow>();
        private Color _activeColor = Color.Cyan;
        private Color _inactiveColor = Color.Gray;
        private bool _isAttached;
        private bool _isEnabled;
        private bool _setTopMost;

        #endregion

        #region [internal] API and Properties

        internal bool SetTopMost
        {
            get
            {
                return _setTopMost;
            }
            set
            {
                _setTopMost = value;
                AlignSideGlowTopMost();
            }
        }

        internal Color ActiveColor
        {
            get
            {
                return _activeColor;
            }

            set
            {
                _activeColor = value;
                foreach (SideGlow sideGlow in _glows)
                {
                    sideGlow.ActiveColor = _activeColor;
                }
            }
        }

        internal Color InactiveColor
        {
            get
            {
                return _inactiveColor;
            }

            set
            {
                _inactiveColor = value;
                foreach (SideGlow sideGlow in _glows)
                {
                    sideGlow.InactiveColor = _inactiveColor;
                }
            }
        }

        internal bool IsEnabled
        {
            get { return _isEnabled; }
        }

        internal void Attach(Form window, bool enable = true)
        {
            if (_isAttached)
            {
                return;
            }

            _isAttached = true;
            _window = window;
            _parentWindowHndl = window.Handle;

            _topGlow = new SideGlow(DockStyle.Top, _parentWindowHndl);
            _leftGlow = new SideGlow(DockStyle.Left, _parentWindowHndl);
            _bottomGlow = new SideGlow(DockStyle.Bottom, _parentWindowHndl);
            _rightGlow = new SideGlow(DockStyle.Right, _parentWindowHndl);

            _glows.Add(_topGlow);
            _glows.Add(_leftGlow);
            _glows.Add(_bottomGlow);
            _glows.Add(_rightGlow);

            WinApi.ShowWindow(new HandleRef(_topGlow, _topGlow.Handle), WinApi.ShowWindowStyle.SW_SHOWNOACTIVATE);
            WinApi.ShowWindow(new HandleRef(_topGlow, _leftGlow.Handle), WinApi.ShowWindowStyle.SW_SHOWNOACTIVATE);
            WinApi.ShowWindow(new HandleRef(_bottomGlow, _leftGlow.Handle), WinApi.ShowWindowStyle.SW_SHOWNOACTIVATE);
            WinApi.ShowWindow(new HandleRef(_rightGlow, _leftGlow.Handle), WinApi.ShowWindowStyle.SW_SHOWNOACTIVATE);

            _isEnabled = false;
            AlignSideGlowTopMost();
            Enable(true);
        }

        private WinApi.WINDOWPOS _lastLocation;

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!_isEnabled) return (IntPtr)0;

            switch (msg)
            {
                case (int)WinApi.Messages.WM_WINDOWPOSCHANGED:
                    _lastLocation = (WinApi.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WinApi.WINDOWPOS));
                    WindowPosChanged(_lastLocation);
                    break;
                case (int)WinApi.Messages.WM_SETFOCUS:
                    SetFocus();
                    break;
                case (int)WinApi.Messages.WM_KILLFOCUS:
                    KillFocus();
                    break;
                case (int)WinApi.Messages.WM_SIZE:
                    Size(wParam, lParam);
                    break;
            }

            return (IntPtr)0;
        }

        internal void Detach()
        {
            _isAttached = false;

            Show(false);

            UnregisterEvents();
        }

        /// <summary>
        /// Enables or disables the glow effect on the window
        /// </summary>
        /// <param name="enable">Enable mode</param>
        internal void Enable(bool enable)
        {
            if (_isEnabled && !enable)
            {
                Show(false);
                UnregisterEvents();
            }
            else if (!_isEnabled && enable)
            {
                RegisterEvents();
                if (_window != null)
                {
                    UpdateLocations(new WinApi.WINDOWPOS
                    {
                        x = (int)_window.Left,
                        y = (int)_window.Top,
                        cx = (int)_window.Width,
                        cy = (int)_window.Height,
                        flags = WinApi.SetWindowPosFlags.SWP_SHOWWINDOW
                    });

                    UpdateSizes((int)_window.Width, (int)_window.Height);
                }
            }

            _isEnabled = enable;
        }

        internal void EnableResize(bool enable)
        {
            foreach (SideGlow sideGlow in _glows)
            {
                sideGlow.ExternalResizeEnable = enable;
            }
        }

        #endregion

        #region [private]

        private void DestroyGlows()
        {
            _parentWindowHndl = IntPtr.Zero;

            CloseGlows();

            _window = null;
        }

        private void HandleWindowVisibleChanged(object sender, EventArgs e)
        {
            Show(_window.Visible);
        }

        private void RegisterEvents()
        {
            if (_window != null)
            {
                _window.VisibleChanged += HandleWindowVisibleChanged;
            }
        }

        private void UnregisterEvents()
        {
            if (_window != null)
            {
                _window.VisibleChanged -= HandleWindowVisibleChanged;
            }
        }
        
        private void CloseGlows()
        {
            foreach (var sideGlow in _glows)
            {
                sideGlow.Close();
            }

            _glows.Clear();

            _topGlow = null;
            _bottomGlow = null;
            _leftGlow = null;
            _rightGlow = null;
        }

        private void Show(bool show)
        {
            foreach (SideGlow sideGlow in _glows) sideGlow.Show(show);
        }

        private void UpdateZOrder()
        {
            foreach (SideGlow sideGlow in _glows)
            {
                sideGlow.UpdateZOrder();
            }
        }

        private void UpdateFocus(bool isFocused)
        {
            foreach (SideGlow sideGlow in _glows)
            {
                sideGlow.ParentWindowIsFocused = isFocused;
            }
        }

        private void UpdateSizes(int width, int height)
        {
            foreach (SideGlow sideGlow in _glows)
            {
                sideGlow.SetSize(width, height);
            }
        }

        private void UpdateLocations(WinApi.WINDOWPOS location)
        {
            foreach (SideGlow sideGlow in _glows)
            {
                sideGlow.SetLocation(location);
            }

            if (((int) location.flags & (int)WinApi.SetWindowPosFlags.SWP_HIDEWINDOW) != 0)
            {
                Show(false);
            }
            else if (((int) location.flags & (int)WinApi.SetWindowPosFlags.SWP_SHOWWINDOW) != 0)
            {
                Show(true);
                UpdateZOrder();
            }
        }

        private void AlignSideGlowTopMost()
        {
            if (_glows == null)
            {
                return;
            }

            foreach (SideGlow glow in _glows)
            {
                glow.IsTopMost = _setTopMost;
                glow.UpdateZOrder();
            }
        }

        #endregion

        #region [WM_events handlers]

        public void SetFocus()
        {
            if (!_isEnabled) return;
            UpdateFocus(true);
            UpdateZOrder();
        }

        public void KillFocus()
        {
            if (!_isEnabled) return;
            UpdateFocus(false);
            UpdateZOrder();
        }

        public void WindowPosChanged(WinApi.WINDOWPOS location)
        {
            if (!_isEnabled) return;
            UpdateLocations(location);
        }

        public void Activate(bool isActive)
        {
            if (!_isEnabled) return;
            UpdateZOrder();
        }

        public void Size(IntPtr wParam, IntPtr lParam)
        {
            if (!_isEnabled) return;
            if ((int)wParam == 2 || (int)wParam == 1) // maximized/minimized
            {
                Show(false);
            }
            else
            {
                Show(true);
                int width = lParam.LoWord();
                int height = lParam.HiWord();
                UpdateSizes(width, height);
            }
        }

        #endregion

        #region Dispose

        private bool _isDisposed;

        /// <summary>
        /// IsDisposed status
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        /// <summary>
        /// Standard Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">True if disposing, false otherwise</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // release unmanaged resources
                }

                _isDisposed = true;

                Detach();
                DestroyGlows();

                UnregisterEvents();

                _window = null;
            }
        }

        #endregion
    }
}
