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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;

namespace Yamui.Framework.Forms {
    /// <summary>
    /// Form class that implements interesting utilities + shadow + onpaint + movable borderless
    /// </summary>
    public class YamuiFormBaseShadow : YamuiFormBase {
        
        #region WndProc

        protected override unsafe void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }
            
            switch (m.Msg) {
                case (int) WinApi.Messages.WM_DWMCOMPOSITIONCHANGED:
                    CheckDwmCompositionEnabled();
                    if (DwmCompositionEnabled) {
                        //EnableDwmComposition();
                    } else {
                        DisableDwmComposition();
                    }
                    break;

                case (int) WinApi.Messages.WM_GETMINMAXINFO:
                    if (DwmCompositionEnabled) {
                        // allows the window to be maximized at the size of the working area instead of the whole screen size
                        OnGetMinMaxInfo(m.HWnd, m.LParam);
                
                        //Return Zero
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case (int) WinApi.Messages.WM_NCCALCSIZE:
                    
                    if (DwmCompositionEnabled && m.WParam != IntPtr.Zero) {
                        // we respond to this message to say we do not want a non client area
                        // When TRUE, LPARAM Points to a NCCALCSIZE_PARAMS structure
                        
                        var nccsp = (WinApi.NCCALCSIZE_PARAMS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.NCCALCSIZE_PARAMS));
                        nccsp.rectProposed.left = nccsp.rectProposed.left + 0;
                        Marshal.StructureToPtr(nccsp, m.LParam, true);
                    
                        //Return Zero
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;
                    
                case (int) WinApi.Messages.WM_ACTIVATE:
                    if (DwmCompositionEnabled) {
                        //EnableDwmComposition();
                    }
                    break;

                case (int) WinApi.Messages.WM_CREATE:
                    if (DwmCompositionEnabled) {
                        WinApi.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, WinApi.SetWindowPosFlags.SWP_FRAMECHANGED | WinApi.SetWindowPosFlags.SWP_NOACTIVATE | WinApi.SetWindowPosFlags.SWP_NOMOVE | WinApi.SetWindowPosFlags.SWP_NOSIZE | WinApi.SetWindowPosFlags.SWP_NOZORDER);
                        // or GetWindowRect(hWnd, &rcClient); + SetWindowPos(hWnd, SWP_FRAMECHANGED);
                    }
                    break;

                case (int) WinApi.Messages.WM_NCACTIVATE:
                    if (DwmCompositionEnabled) {
                        /* Prevent Windows from drawing the default title bar by temporarily
                           toggling the WS_VISIBLE style. This is recommended in:
                           https://blogs.msdn.microsoft.com/wpfsdk/2008/09/08/custom-window-chrome-in-wpf/ */
                        var oldStyle = WindowStyle;
                        WindowStyle = oldStyle & ~WinApi.WindowStyles.WS_VISIBLE;
                        DefWndProc(ref m);
                        WindowStyle = oldStyle;
                    }

                    return;

                case (int) WinApi.Messages.WM_ERASEBKGND:
                    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms648055(v=vs.85).aspx
                    m.Result = IntPtr.Zero;
                    return;
            }

            base.WndProc(ref m);
        }
        
        #endregion

        protected override void OnPaint(PaintEventArgs e) {
           // e.Graphics.Clear(Color.Black);

            using (var b = new SolidBrush(Color.Yellow)) {
                e.Graphics.FillRectangle(b, new Rectangle(new Point(0, 0), Size));
            }
            //base.OnPaint(e);
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();
            EnableDwmComposition();
        }

        //protected override void OnShown(EventArgs e) {
        //    Refresh();
        //    base.OnShown(e);
        //}
        ///// <summary>
        ///// Prevents a bug that occurs while restoring the form after a minimize operation.
        ///// </summary>
        ///// <param name="x">The upper left corner x-coordinate.</param>
        ///// <param name="y">The upper left corner y-coordinate.</param>
        ///// <param name="width">The new width of the form (it is too large - use current Width value).</param>
        ///// <param name="height">The new height of the form (it is too large - use the current Height value).</param>
        ///// <param name="specified">What kind of boundaries should be changed?!</param>
        //protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
        //    base.SetBoundsCore(x, y, Width, Height, specified);
        //}

        private void OnGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            var minmaxinfo = (WinApi.MINMAXINFO) Marshal.PtrToStructure(lParam, typeof(WinApi.MINMAXINFO));
            var s = Screen.FromHandle(hwnd);
            minmaxinfo.ptMaxSize.x = s.WorkingArea.Width;
            minmaxinfo.ptMaxSize.y = s.WorkingArea.Height;
            minmaxinfo.ptMaxPosition.x = Math.Abs(s.WorkingArea.Left - s.Bounds.Left);
            minmaxinfo.ptMaxPosition.y = Math.Abs(s.WorkingArea.Top - s.Bounds.Top);
            Marshal.StructureToPtr(minmaxinfo, lParam, true);
        }
        
        public void DisableDwmComposition() {
            var margins = new WinApi.MARGINS(0, 0, 0, 0);
            WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        public void EnableDwmComposition() {
            var status = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(status, (int) WinApi.DWMNCRenderingPolicy.Enabled);
            WinApi.DwmSetWindowAttribute(Handle, WinApi.DWMWINDOWATTRIBUTE.NCRenderingPolicy, status, sizeof(int));

            var margins = new WinApi.MARGINS(-1, -1, -1, -1);
            WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

    }
}