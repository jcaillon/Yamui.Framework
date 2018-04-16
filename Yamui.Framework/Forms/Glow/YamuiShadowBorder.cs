using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Yamui.Framework.Helper;

namespace Yamui.Framework.Forms.Glow {

    /// <summary>
    /// A SideGlow window is a layered window that
    /// renders the "glowing effect" on one of the sides
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/ms633556(v=vs.85).aspx?f=255&MSPPError=-2147217396</remarks>
    /// <remarks>http://www.nuonsoft.com/blog/2009/05/27/how-to-use-updatelayeredwindow/</remarks>
    /// <remarks>http://simostro.synology.me/simone/2016/04/04/glow-window-effect/</remarks>
    internal class YamuiShadowBorder : IDisposable, IWin32Window {
        #region private

        private const int Thickness = 9;

        private readonly byte[] _alphas = {64, 46, 25, 19, 10, 07, 02, 01, 00};
        private readonly IntPtr _parentHandle;
        private readonly Color _transparent = Color.FromArgb(0);

        private WinApi.POINT _ptZero = new WinApi.POINT(0, 0);
        private bool _disposed;
        private WinApi.WndProcHandler _wndProcDelegate;
        
        private readonly DockStyle _side;
        private const WinApi.SetWindowPosFlags NoSizeNoMove = WinApi.SetWindowPosFlags.SWP_NOSIZE | WinApi.SetWindowPosFlags.SWP_NOMOVE;

        private bool _parentWindowIsFocused;
        private Color _activeColor = Color.Cyan;
        private Color _inactiveColor = Color.Orange;
        private WinApi.BLENDFUNCTION _blend;

        private WinApi.WNDCLASS _windowClass;

        public Point Location { get; private set; }
        public Size Size { get; private set; }

        internal Color InactiveColor {
            get { return _inactiveColor; }
            set {
                _inactiveColor = value;
                Render();
            }
        }

        internal Color ActiveColor {
            get { return _activeColor; }
            set {
                _activeColor = value;
                Render();
            }
        }

        public IntPtr Handle { get; private set; }

        internal bool ParentWindowIsFocused {
            set {
                _parentWindowIsFocused = value;
                Render();
            }
        }

        internal bool IsTopMost { get; set; }

        internal void Show(bool show) {
            WinApi.ShowWindow(new HandleRef(this, Handle), show ? WinApi.ShowWindowStyle.SW_SHOWNOACTIVATE : WinApi.ShowWindowStyle.SW_HIDE);
        }

        #endregion

        #region constuctor

        internal YamuiShadowBorder(DockStyle side, IntPtr parent) {
            _side = side;
            _parentHandle = parent;

            AppDomain.CurrentDomain.ProcessExit += OnShutdown;
            AppDomain.CurrentDomain.DomainUnload += OnShutdown;

            _blend = new WinApi.BLENDFUNCTION(255);
            
            CreateWindow();
        }

        private void OnShutdown(object sender, EventArgs eventArgs) {
            Close();
            WinApi.UnregisterClass(_windowClass.lpszClassName, new HandleRef(this, WinApi.GetModuleHandle(null)));
        }

        #endregion

        #region internal

        
        public void SetSize(int width, int height) {
            if (_side == DockStyle.Top || _side == DockStyle.Bottom) {
                height = Thickness;
                width = width + Thickness * 2;
            } else {
                width = Thickness;
                height = height + Thickness * 2;
            }

            Size = new Size(width, height);
            const WinApi.SetWindowPosFlags flags = (WinApi.SetWindowPosFlags.SWP_NOMOVE | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
            WinApi.SetWindowPos(Handle, WinApi.SpecialWindowHandles.HWND_NOTOPMOST, 0, 0, width, height, flags);
            Render();
        }

        public void SetLocation(WinApi.WINDOWPOS pos) {
            int left = 0;
            int top = 0;
            switch (_side) {
                case DockStyle.Top:
                    left = pos.x - Thickness;
                    top = pos.y - Thickness;
                    break;
                case DockStyle.Bottom:
                    left = pos.x - Thickness;
                    top = pos.y + pos.cy;
                    break;
                case DockStyle.Left:
                    left = pos.x - Thickness;
                    top = pos.y - Thickness;
                    break;
                case DockStyle.Right:
                    left = pos.x + pos.cx;
                    top = pos.y - Thickness;
                    break;
            }

            Location = new Point(left, top);
            WinApi.SetWindowPos(Handle, !IsTopMost ? WinApi.SpecialWindowHandles.HWND_NOTOPMOST : WinApi.SpecialWindowHandles.HWND_TOPMOST, left, top, 0, 0, WinApi.SetWindowPosFlags.SWP_NOSIZE | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private void UpdateZOrder(int left, int top, WinApi.SetWindowPosFlags flags) {
            WinApi.SetWindowPos(Handle, !IsTopMost ? WinApi.SpecialWindowHandles.HWND_NOTOPMOST : WinApi.SpecialWindowHandles.HWND_TOPMOST, left, top, 0, 0, flags);
            WinApi.SetWindowPos(Handle, _parentHandle, 0, 0, 0, 0, NoSizeNoMove | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        internal void UpdateZOrder() {
            //WinApi.SetWindowPos(Handle, !IsTopMost ? WinApi.SpecialWindowHandles.HWND_NOTOPMOST : WinApi.SpecialWindowHandles.HWND_TOPMOST, 0, 0, 0, 0, NoSizeNoMove | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
            WinApi.SetWindowPos(Handle, _parentHandle, 0, 0, 0, 0, NoSizeNoMove | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
        }


        internal void Close() {
            WinApi.CloseWindow(Handle);
            if (Handle != IntPtr.Zero)
                WinApi.DestroyWindow(new HandleRef(this, Handle));
        }

        #endregion

        #region private

        private void RegisterClass() {
            _wndProcDelegate = CustomWndProc;
            String appDomain = Convert.ToString(AppDomain.CurrentDomain.GetHashCode(), 16);
            _windowClass = new WinApi.WNDCLASS {
                lpszClassName = $"{nameof(YamuiShadowBorder)}.{_side}_{_parentHandle}.{VersioningHelper.MakeVersionSafeName(appDomain, ResourceScope.Process, ResourceScope.AppDomain)}",
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate)
            };

            var classAtom = WinApi.RegisterClassW(ref _windowClass);
            int lastError = Marshal.GetLastWin32Error();
            if (classAtom == 0) {
                throw new Exception("Could not register window class : " + lastError);
            }
        }

        private void CreateWindow() {
            _wndProcDelegate = CustomWndProc;

            // Create WNDCLASS
            RegisterClass();
            var extendedStyle = (
                WinApi.WindowStylesEx.WS_EX_LEFT |
                WinApi.WindowStylesEx.WS_EX_LTRREADING |
                WinApi.WindowStylesEx.WS_EX_RIGHTSCROLLBAR |
                WinApi.WindowStylesEx.WS_EX_LAYERED |
                WinApi.WindowStylesEx.WS_EX_TRANSPARENT |
                WinApi.WindowStylesEx.WS_EX_TOOLWINDOW);

            var style = (
                WinApi.WindowStyles.WS_CLIPSIBLINGS |
                WinApi.WindowStyles.WS_CLIPCHILDREN |
                WinApi.WindowStyles.WS_POPUP);

            // Create window
            Handle = WinApi.CreateWindowEx(
                (int) extendedStyle,
                _windowClass.lpszClassName,
                _windowClass.lpszClassName,
                (int) style,
                0,
                0,
                0,
                0,
                new HandleRef(this, _parentHandle),
                WinApi.NullHandleRef,
                WinApi.NullHandleRef,
                IntPtr.Zero
            );
        }

        private IntPtr CustomWndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam) {

            return WinApi.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void Render() {
            var screenDc = WinApi.GetDC(WinApi.NullHandleRef);
            if (screenDc == IntPtr.Zero) {
                return;
            }
            try {           
                var memDc = WinApi.CreateCompatibleDC(new HandleRef(null, screenDc));
                if (memDc == IntPtr.Zero) {
                    return;
                }
                try {
                    using (Bitmap bmp = GetWindowBitmap(Size.Width, Size.Height)) {
                        IntPtr hBitmap = bmp.GetHbitmap(_transparent);
                        IntPtr hOldBitmap = WinApi.SelectObject(memDc, hBitmap);

                        WinApi.POINT newLocation = new WinApi.POINT(Location);
                        WinApi.SIZE newSize = new WinApi.SIZE(Size);
                        WinApi.UpdateLayeredWindow(Handle, screenDc, ref newLocation, ref newSize, memDc, ref _ptZero, 0, ref _blend, WinApi.BlendingFlags.ULW_ALPHA);

                        if (hBitmap != IntPtr.Zero) {
                            WinApi.SelectObject(memDc, hOldBitmap);
                            WinApi.DeleteObject(hBitmap);
                        }
                    }
                } finally {
                    WinApi.DeleteDC(new HandleRef(null, memDc));
                }
            } finally {
                WinApi.ReleaseDC(WinApi.NullHandleRef, new HandleRef(null, screenDc));
            }
        }

        public Bitmap GetWindowBitmap(int width, int height) {
            Bitmap bmp;
            switch (_side) {
                case DockStyle.Top:
                case DockStyle.Bottom:
                    bmp = new Bitmap(width, Thickness, PixelFormat.Format32bppArgb);
                    break;
                case DockStyle.Left:
                case DockStyle.Right:
                    bmp = new Bitmap(Thickness, height, PixelFormat.Format32bppArgb);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            using (Graphics g = Graphics.FromImage(bmp)) {
                Color gradientColor = _parentWindowIsFocused ? _activeColor : _inactiveColor;
                for (int i = 0; i < Thickness; i++) {
                    var color = Color.FromArgb(_alphas[i], gradientColor.R, gradientColor.G, gradientColor.B);
                    using (Pen pen = new Pen(color, 1f)) {
                        pen.Alignment = PenAlignment.Center;
                        if (_side == DockStyle.Top || _side == DockStyle.Bottom) {
                            int y = (_side == DockStyle.Top) ? Thickness - 1 - i : i;
                            int xLeft = i == 0 ? Thickness - 1 : Thickness; // case of i==0 is to draw the missing pixel
                            int xRight = width - xLeft - 1;
                            g.DrawLine(pen, new Point(xLeft, y), new Point(xRight, y));

                            if (i > 0) { // can't draw this line, it's too short
                                var color2 = Color.FromArgb(_alphas[i + 1 > _alphas.Length - 1 ? _alphas.Length - 1 : i + 1], gradientColor.R, gradientColor.G, gradientColor.B);
                                using (Pen pen2 = new Pen(color2, 1f)) {
                                    xLeft = Thickness - 1 - i;
                                    var yLeft = _side == DockStyle.Top ? Thickness - 1 : 0;
                                    xRight = Thickness - 1;
                                    g.DrawLine(pen2, xLeft, yLeft, xRight, y);
                                    g.DrawLine(pen2, width - 1 - xLeft, yLeft, width - 1 - xRight, y);
                                }
                            }
                        } else {
                            int x = (_side == DockStyle.Right) ? i : Thickness - i - 1;
                            int yTop = Thickness;
                            int yBottom = height - yTop - 1;
                            g.DrawLine(pen, new Point(x, yTop), new Point(x, yBottom));
                        }
                    }
                }
                g.Flush();
            }
            return bmp;
        }

        
        #endregion

        #region Dispose

        public void Dispose() {
            if (_disposed) 
                return;
            _disposed = true;
            if (Handle == IntPtr.Zero) 
                return;
            WinApi.DestroyWindow(new HandleRef(this, Handle));
            Handle = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}