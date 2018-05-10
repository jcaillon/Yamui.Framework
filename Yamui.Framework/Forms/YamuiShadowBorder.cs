using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Yamui.Framework.Helper;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Forms {

    /// <summary>
    /// This is a layered window that renders the border "visual studio like"
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/ms633556(v=vs.85).aspx</remarks>
    /// <remarks>http://www.nuonsoft.com/blog/2009/05/27/how-to-use-updatelayeredwindow/</remarks>
    /// <remarks>http://simostro.synology.me/simone/2016/04/04/glow-window-effect/</remarks>
    public class YamuiShadowBorder : IDisposable, IWin32Window {
        #region private

        private const int Thickness = 12;

        //private readonly byte[] _alphas = {64, 46, 25, 19, 10, 07, 02, 01, 00};
        private readonly byte[] _alphas = {65, 50, 35, 20, 15, 8, 5, 3, 2, 1, 1, 0};
        private readonly IntPtr _parentHandle;
        private readonly Color _transparent = Color.FromArgb(0);

        private WinApi.POINT _ptZero = new WinApi.POINT(0, 0);
        private bool _disposed;
        private WinApi.WndProcHandler _wndProcDelegate;
        
        private readonly DockStyle _side;

        private bool _parentWindowIsFocused;
        private WinApi.BLENDFUNCTION _blend;

        private WinApi.WNDCLASS _windowClass;
        private bool _registeredClass;
        
        public Color ActiveColor => YamuiThemeManager.Current.AccentColor;
        public Color InactiveColor => YamuiThemeManager.Current.FormBorder;

        #endregion

        #region Properties
        
        public bool Visible { get; private set; }

        public Point Location { get; private set; }

        public Size Size { get; private set; }
        
        public IntPtr Handle { get; private set; }

        public bool ParentWindowIsFocused {
            set {
                if (_parentWindowIsFocused != value) {
                    _parentWindowIsFocused = value;
                    WinApi.SetWindowPos(Handle, _parentHandle, 0, 0, 0, 0,  WinApi.SetWindowPosFlags.SWP_NOMOVE | WinApi.SetWindowPosFlags.SWP_NOSIZE | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
                    Render();
                }
            }
        }

        public void Show(bool show) {
            if (Visible != show) {
                Visible = show;
                WinApi.ShowWindow(new HandleRef(this, Handle), show ? WinApi.ShowWindowStyle.SW_SHOWNOACTIVATE : WinApi.ShowWindowStyle.SW_HIDE);
            }
        }

        #endregion

        #region constuctor

        internal YamuiShadowBorder(DockStyle side, IntPtr parent) {
            _side = side;
            _parentHandle = parent;
            _blend = new WinApi.BLENDFUNCTION(255);
            CreateWindow();
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

        #region public

        public void SetLocationAndSize(int left, int top, int width, int height) {
            switch (_side) {
                case DockStyle.Top:
                    left = left - Thickness;
                    top = top - Thickness;
                    break;
                case DockStyle.Bottom:
                    left = left - Thickness;
                    top = top + height;
                    break;
                case DockStyle.Left:
                    left = left - Thickness;
                    top = top - Thickness;
                    break;
                case DockStyle.Right:
                    left = left + width;
                    top = top - Thickness;
                    break;
            }
            switch (_side) {
                case DockStyle.Top:
                case DockStyle.Bottom:
                    height = Thickness;
                    width = width + Thickness * 2;
                    break;
                default:
                    width = Thickness;
                    height = height + Thickness * 2;
                    break;
            }

            var locationHasChanged = false;
            if (left != Location.X || top != Location.Y) {
                Location = new Point(left, top);
                locationHasChanged = true;
            }

            var sizeHasChanged = false;
            if (width != Size.Width || height != Size.Height) {
                Size = new Size(width, height);
                sizeHasChanged = true;
            }

            if (locationHasChanged && sizeHasChanged) {
                WinApi.SetWindowPos(Handle, _parentHandle, left, top, width, height, WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
            } else if (sizeHasChanged) {
                WinApi.SetWindowPos(Handle, _parentHandle, 0, 0, width, height, WinApi.SetWindowPosFlags.SWP_NOMOVE | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
            } else if (locationHasChanged) {
                WinApi.SetWindowPos(Handle, _parentHandle, left, top, 0, 0, WinApi.SetWindowPosFlags.SWP_NOSIZE | WinApi.SetWindowPosFlags.SWP_NOACTIVATE);
            }
            
            if (sizeHasChanged) {
                Render();
            }
        }

        public void Close() {
            WinApi.CloseWindow(Handle);
            if (Handle != IntPtr.Zero) {
                WinApi.DestroyWindow(new HandleRef(this, Handle));
            }

            if (_registeredClass) {
                _registeredClass = false;
                WinApi.UnregisterClass(_windowClass.lpszClassName, new HandleRef(this, WinApi.GetModuleHandle(null)));
            }
        }

        #endregion

        #region private

        private void RegisterClass() {
            _wndProcDelegate = CustomWndProc;
            var appDomain = Convert.ToString(AppDomain.CurrentDomain.GetHashCode(), 16);
            _windowClass = new WinApi.WNDCLASS {
                lpszClassName = $"{nameof(YamuiShadowBorder)}.{_side}_{_parentHandle}.{VersioningHelper.MakeVersionSafeName(appDomain, ResourceScope.Process, ResourceScope.AppDomain)}",
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate)
            };

            var classAtom = WinApi.RegisterClassW(ref _windowClass);
            int lastError = Marshal.GetLastWin32Error();
            if (classAtom == 0) {
                throw new Exception("Could not register window class : " + lastError);
            }

            _registeredClass = true;
        }

        private void CreateWindow() {
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
                //new HandleRef(this, _parentHandle),
                WinApi.NullHandleRef,
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

        private Bitmap GetWindowBitmap(int width, int height) {
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
                Color gradientColor = _parentWindowIsFocused ? ActiveColor : InactiveColor;
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

    }
}