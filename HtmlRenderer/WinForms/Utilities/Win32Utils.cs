#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (Win32Utils.cs) is part of YamuiFramework.
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
#if !MONO
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace YamuiFramework.HtmlRenderer.WinForms.Utilities {
    /// <summary>
    /// Utility for Win32 API.
    /// </summary>
    internal static class Win32Utils {
        public const int WsBorder = 0x00800000;

        public const int WsExClientEdge = 0x200;

        /// <summary>
        /// Const for BitBlt copy raster-operation code.
        /// </summary>
        public const int BitBltCopy = 0x00CC0020;

        /// <summary>
        /// Const for BitBlt paint raster-operation code.
        /// </summary>
        public const int BitBltPaint = 0x00EE0086;

        public const int WmSetCursor = 0x20;

        public const int IdcHand = 32649;

        public const int TextAlignDefault = 0;

        public const int TextAlignRtl = 256;

        public const int TextAlignBaseline = 24;

        public const int TextAlignBaselineRtl = 256 + 24;

        [DllImport("user32.dll")]
        public static extern int SetCursor(int hCursor);

        [DllImport("user32.dll")]
        public static extern int LoadCursor(int hInstance, int lpCursorName);

        /// <summary>
        /// Create a compatible memory HDC from the given HDC.<br/>
        /// The memory HDC can be rendered into without effecting the original HDC.<br/>
        /// The returned memory HDC and <paramref name="dib"/> must be released using <see cref="ReleaseMemoryHdc"/>.
        /// </summary>
        /// <param name="hdc">the HDC to create memory HDC from</param>
        /// <param name="width">the width of the memory HDC to create</param>
        /// <param name="height">the height of the memory HDC to create</param>
        /// <param name="dib">returns used bitmap memory section that must be released when done with memory HDC</param>
        /// <returns>memory HDC</returns>
        public static IntPtr CreateMemoryHdc(IntPtr hdc, int width, int height, out IntPtr dib) {
            // Create a memory DC so we can work off-screen
            IntPtr memoryHdc = CreateCompatibleDC(hdc);
            SetBkMode(memoryHdc, 1);

            // Create a device-independent bitmap and select it into our DC
            var info = new BitMapInfo();
            info.biSize = Marshal.SizeOf(info);
            info.biWidth = width;
            info.biHeight = -height;
            info.biPlanes = 1;
            info.biBitCount = 32;
            info.biCompression = 0; // BI_RGB
            IntPtr ppvBits;
            dib = CreateDIBSection(hdc, ref info, 0, out ppvBits, IntPtr.Zero, 0);
            SelectObject(memoryHdc, dib);

            return memoryHdc;
        }

        /// <summary>
        /// Release the given memory HDC and dib section created from <see cref="CreateMemoryHdc"/>.
        /// </summary>
        /// <param name="memoryHdc">Memory HDC to release</param>
        /// <param name="dib">bitmap section to release</param>
        public static void ReleaseMemoryHdc(IntPtr memoryHdc, IntPtr dib) {
            DeleteObject(dib);
            DeleteDC(memoryHdc);
        }

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromDC(IntPtr hdc);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <remarks>
        /// In conformance with conventions for the RECT structure, the bottom-right coordinates of the returned rectangle are exclusive. In other words, 
        /// the pixel at (right, bottom) lies immediately outside the rectangle.
        /// </remarks>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpRect">A pointer to a RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("User32", SetLastError = true)]
        public static extern int GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <remarks>
        /// In conformance with conventions for the RECT structure, the bottom-right coordinates of the returned rectangle are exclusive. In other words, 
        /// the pixel at (right, bottom) lies immediately outside the rectangle.
        /// </remarks>
        /// <param name="handle">A handle to the window.</param>
        /// <returns>RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window.</returns>
        public static Rectangle GetWindowRectangle(IntPtr handle) {
            Rectangle rect;
            GetWindowRect(handle, out rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        [DllImport("gdi32.dll")]
        public static extern int SetTextAlign(IntPtr hdc, uint fMode);

        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hdc, int mode);

        [DllImport("gdi32.dll")]
        public static extern int SelectObject(IntPtr hdc, IntPtr hgdiObj);

        [DllImport("gdi32.dll")]
        public static extern int SetTextColor(IntPtr hdc, int color);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetTextMetrics(IntPtr hdc, out TextMetric lptm);

        [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W")]
        public static extern int GetTextExtentPoint32(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string str, int len, ref Size size);

        [DllImport("gdi32.dll", EntryPoint = "GetTextExtentExPointW")]
        public static extern bool GetTextExtentExPoint(IntPtr hDc, [MarshalAs(UnmanagedType.LPWStr)] string str, int nLength, int nMaxExtent, int[] lpnFit, int[] alpDx, ref Size size);

        [DllImport("gdi32.dll", EntryPoint = "TextOutW")]
        public static extern bool TextOut(IntPtr hdc, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string str, int len);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern int GetClipBox(IntPtr hdc, out Rectangle lprc);

        [DllImport("gdi32.dll")]
        public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll", EntryPoint = "GdiAlphaBlend")]
        public static extern bool AlphaBlend(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, BlendFunction blendFunction);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BitMapInfo pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct BlendFunction {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;

        public BlendFunction(byte alpha) {
            BlendOp = 0;
            BlendFlags = 0;
            AlphaFormat = 0;
            SourceConstantAlpha = alpha;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct BitMapInfo {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
        public byte bmiColors_rgbBlue;
        public byte bmiColors_rgbGreen;
        public byte bmiColors_rgbRed;
        public byte bmiColors_rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TextMetric {
        public int tmHeight;
        public int tmAscent;
        public int tmDescent;
        public int tmInternalLeading;
        public int tmExternalLeading;
        public int tmAveCharWidth;
        public int tmMaxCharWidth;
        public int tmWeight;
        public int tmOverhang;
        public int tmDigitizedAspectX;
        public int tmDigitizedAspectY;
        public char tmFirstChar;
        public char tmLastChar;
        public char tmDefaultChar;
        public char tmBreakChar;
        public byte tmItalic;
        public byte tmUnderlined;
        public byte tmStruckOut;
        public byte tmPitchAndFamily;
        public byte tmCharSet;
    }
}

#endif