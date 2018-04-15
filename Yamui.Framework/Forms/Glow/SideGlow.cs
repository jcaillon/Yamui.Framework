using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;

namespace WpfGlowWindow.Glow {

    /// <summary>
    /// A SideGlow window is a layered window that
    /// renders the "glowing effect" on one of the sides.
    /// ref : http://simostro.synology.me/simone/2016/04/04/glow-window-effect/
    /// </summary>
    internal class SideGlow : IDisposable {
        #region private

        private const int ErrorClassAlreadyExists = 1410;
        private const int AcSrcOver = 0x00;
        private const int AcSrcAlpha = 0x01;
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
        private Color _inactiveColor = Color.LightGray;
        private WinApi.BLENDFUNCTION _blend;

        public Point Location { get; private set; }
        public Size Size { get; private set; }

        #endregion

        #region constuctor

        internal SideGlow(DockStyle side, IntPtr parent) {
            _side = side;
            _parentHandle = parent;

            _blend = new WinApi.BLENDFUNCTION {
                BlendOp = AcSrcOver,
                BlendFlags = 0,
                SourceConstantAlpha = 255,
                AlphaFormat = AcSrcAlpha
            };
            
            CreateWindow($"GlowSide_{side}_{parent}");
        }

        #endregion

        #region internal

        internal bool ExternalResizeEnable { get; set; }
        
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
            WinApi.SetWindowPos(Handle, !IsTopMost ? WinApi.SpecialWindowHandles.HWND_NOTOPMOST : WinApi.SpecialWindowHandles.HWND_TOPMOST, left, top, 0, 0, 0);
            UpdateZOrder(left, top, WinApi.SetWindowPosFlags.SWP_NOSIZE | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        internal void UpdateZOrder(int left, int top, WinApi.SetWindowPosFlags flags) {
            WinApi.SetWindowPos(Handle, !IsTopMost ? WinApi.SpecialWindowHandles.HWND_NOTOPMOST : WinApi.SpecialWindowHandles.HWND_TOPMOST, left, top, 0, 0, flags);
            WinApi.SetWindowPos(Handle, _parentHandle, 0, 0, 0, 0, NoSizeNoMove | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        internal void UpdateZOrder() {
            WinApi.SetWindowPos(Handle, !IsTopMost ? WinApi.SpecialWindowHandles.HWND_NOTOPMOST : WinApi.SpecialWindowHandles.HWND_TOPMOST, 0, 0, 0, 0, NoSizeNoMove | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
            WinApi.SetWindowPos(Handle, _parentHandle, 0, 0, 0, 0, NoSizeNoMove | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
        }

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

        internal IntPtr Handle { get; private set; }

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

        internal void Close() {
            WinApi.CloseWindow(Handle);
            WinApi.SetParent(Handle, IntPtr.Zero);
            WinApi.DestroyWindow(Handle);
        }

        #endregion

        #region private

        private void CreateWindow(string className) {
            _wndProcDelegate = CustomWndProc;

            // Create WNDCLASS
            WinApi.WNDCLASS windClass = new WinApi.WNDCLASS {
                lpszClassName = className,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate)
            };

            ushort classAtom = WinApi.RegisterClassW(ref windClass);
            int lastError = Marshal.GetLastWin32Error();
            if (classAtom == 0 && lastError != ErrorClassAlreadyExists) {
                throw new Exception("Could not register window class");
            }

            var extendedStyle = (
                WinApi.WindowStylesEx.WS_EX_LEFT |
                WinApi.WindowStylesEx.WS_EX_LTRREADING |
                WinApi.WindowStylesEx.WS_EX_RIGHTSCROLLBAR |
                WinApi.WindowStylesEx.WS_EX_LAYERED |
                WinApi.WindowStylesEx.WS_EX_TOOLWINDOW);

            var style = (
                WinApi.WindowStyles.WS_CLIPSIBLINGS |
                WinApi.WindowStyles.WS_CLIPCHILDREN |
                WinApi.WindowStyles.WS_POPUP);

            // Create window
            Handle = WinApi.CreateWindowEx(
                (int) extendedStyle,
                className,
                className,
                (int) style,
                0,
                0,
                0,
                0,
                new HandleRef(this, _parentHandle),
                new HandleRef(this, IntPtr.Zero),
                new HandleRef(this, IntPtr.Zero),
                IntPtr.Zero
            );

            if (Handle == IntPtr.Zero) {
                return;
            }
        }

        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) {
            return WinApi.DefWindowProcW(hWnd, msg, wParam, lParam);
        }

        public Bitmap GetBitmap(int width, int height) {
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

            Graphics g = Graphics.FromImage(bmp);
            //List<Color> colorMap = _parentWindowIsFocused ? _activeColors : _inactiveColors;
            Color gradientColor = _activeColor;

            if (_side == DockStyle.Top || _side == DockStyle.Bottom) {
                for (int i = 0; i < _alphas.Length; i++) {
                    Color color = Color.FromArgb(_alphas[i], gradientColor.R, gradientColor.G, gradientColor.B);
                    Pen pen = new Pen(color);
                    int y = (_side == DockStyle.Top) ? Thickness - 1 - i : i;
                    const int xLeft = Thickness * 2 - 1;
                    int xRight = width - Thickness * 2;
                    g.DrawLine(pen, new Point(xLeft, y), new Point(xRight, y));
                    double a = _alphas[i] / (Thickness + i);
                    for (int j = 0; j < Thickness - 1; j++) {
                        double al = Math.Max(0, _alphas[i] - a * j);
                        color = Color.FromArgb((int) al, gradientColor.R, gradientColor.G, gradientColor.B);
                        Brush b = new SolidBrush(color);
                        g.FillRectangle(b, xLeft - 1 - j, y, 1, 1);
                        g.FillRectangle(b, xRight + 1 + j, y, 1, 1);
                    }

                    for (int j = Thickness - 1; j < Thickness + 1 + i; j++) {
                        double al = Math.Max(0, _alphas[i] - a * j) / 2;
                        color = Color.FromArgb((int) al, gradientColor.R, gradientColor.G, gradientColor.B);
                        Brush b = new SolidBrush(color);
                        g.FillRectangle(b, xLeft - 1 - j, y, 1, 1);
                        g.FillRectangle(b, xRight + 1 + j, y, 1, 1);
                    }
                }
            } else {
                for (int i = 0; i < _alphas.Length; i++) {
                    Color color = Color.FromArgb(_alphas[i], gradientColor.R, gradientColor.G, gradientColor.B);
                    Pen pen = new Pen(color);
                    int x = (_side == DockStyle.Right) ? i : Thickness - i - 1;
                    const int yTop = Thickness * 2;
                    int yBottom = height - Thickness * 2 - 1;
                    g.DrawLine(pen, new Point(x, yTop), new Point(x, yBottom));

                    double a = _alphas[i] / (Thickness + i);
                    for (int j = 0; j < Thickness; j++) {
                        double al = Math.Max(0, _alphas[i] - a * j);
                        color = Color.FromArgb((int) al, gradientColor.R, gradientColor.G, gradientColor.B);
                        Brush b = new SolidBrush(color);
                        g.FillRectangle(b, x, yTop - 1 - j, 1, 1);
                        g.FillRectangle(b, x, yBottom + 1 + j, 1, 1);
                    }

                    for (int j = Thickness; j < Thickness + i; j++) {
                        double al = Math.Max(0, _alphas[i] - a * j) / 2;
                        color = Color.FromArgb((int) al, gradientColor.R, gradientColor.G, gradientColor.B);
                        Brush b = new SolidBrush(color);
                        g.FillRectangle(b, x, yTop - 1 - j, 1, 1);
                        g.FillRectangle(b, x, yBottom + 1 + j, 1, 1);
                    }
                }
            }

            g.Flush();
            return bmp;
        }

        private void Render() {
            WinApi.POINT newLocation = new WinApi.POINT(Location);
            WinApi.SIZE newSize = new WinApi.SIZE(Size);
            IntPtr screenDc = WinApi.GetDC(IntPtr.Zero);
            IntPtr memDc = WinApi.CreateCompatibleDC(screenDc);
            using (Bitmap bmp = GetBitmap(Size.Width, Size.Height)) {
                IntPtr hBitmap = bmp.GetHbitmap(_transparent);
                IntPtr hOldBitmap = WinApi.SelectObject(memDc, hBitmap);

                WinApi.UpdateLayeredWindow(Handle, screenDc, ref newLocation, ref newSize, memDc, ref _ptZero, 0, ref _blend, 0x02);

                WinApi.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero) {
                    WinApi.SelectObject(memDc, hOldBitmap);
                    WinApi.DeleteObject(hBitmap);
                }
            }

            WinApi.DeleteDC(memDc);
        }
        
        #endregion

        #region Dispose

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) return;
            _disposed = true;
            if (Handle == IntPtr.Zero) return;

            WinApi.DestroyWindow(Handle);
            Handle = IntPtr.Zero;
        }

        #endregion
    }
}