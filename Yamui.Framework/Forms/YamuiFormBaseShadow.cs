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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;

namespace Yamui.Framework.Forms {
    /// <summary>
    /// Form class that implements interesting utilities + shadow + onpaint + movable borderless
    /// </summary>
    public class YamuiFormBaseShadow : YamuiFormBase {

        #region private fields
        
        private bool? _dwmCompositionEnabled;

        public bool DwmCompositionEnabled {
            get {
                if (_dwmCompositionEnabled == null) {
                    CheckDwmCompositionEnabled();
                }
                return _dwmCompositionEnabled ?? false;
            }
        }

        #endregion

        #region WndProc

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            if (DwmCompositionEnabled) {
                IntPtr result;
                int dwmHandled = WinApi.DwmDefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam, out result);
                if (dwmHandled == 1) {
                    m.Result = result;
                    return;
                }
            }

            switch (m.Msg) {
                case (int) WinApi.Messages.WM_DWMCOMPOSITIONCHANGED:
                    CheckDwmCompositionEnabled();
                    if (DwmCompositionEnabled) {
                        EnableDwmComposition();
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
                    if (DwmCompositionEnabled) {
                        // We can't use BorderStyle None with Dwm so we respond to this message to say we do not want
                        // a non client area
                
                        //Return Zero
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;


                //case (int) WinApi.Messages.WM_ACTIVATE:
                //    if (DwmCompositionEnabled) {
                //        EnableDwmComposition();
                //    }
                //    break;
            }

            base.WndProc(ref m);
        }

        #endregion

        protected override void OnControlCreation() {
            base.OnControlCreation();
            EnableDwmComposition();
        }

        private void OnGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            var minmaxinfo = (WinApi.MINMAXINFO) Marshal.PtrToStructure(lParam, typeof(WinApi.MINMAXINFO));
            var s = Screen.FromHandle(hwnd);
            minmaxinfo.ptMaxSize.x = s.WorkingArea.Width;
            minmaxinfo.ptMaxSize.y = s.WorkingArea.Height;
            minmaxinfo.ptMaxPosition.x = Math.Abs(s.WorkingArea.Left - s.Bounds.Left);
            minmaxinfo.ptMaxPosition.y = Math.Abs(s.WorkingArea.Top - s.Bounds.Top);
            Marshal.StructureToPtr(minmaxinfo, lParam, true);
        }

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
       
        public void DisableDwmComposition() {
            var margins = new WinApi.MARGINS(0, 0, 0, 0);
            WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
            FormBorderStyle = FormBorderStyle.None;
        }

        public void EnableDwmComposition() {
            var status = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(status, (int) WinApi.DWMNCRenderingPolicy.Enabled);
            WinApi.DwmSetWindowAttribute(Handle, WinApi.DWMWINDOWATTRIBUTE.NCRenderingPolicy, status, sizeof(int));

            var margins = new WinApi.MARGINS(1, 1, 1, 1);
            WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
            FormBorderStyle = FormBorderStyle.Sizable;
        }
        
        private void CheckDwmCompositionEnabled() {
            bool enabled;
            WinApi.DwmIsCompositionEnabled(out enabled);
            _dwmCompositionEnabled = Environment.OSVersion.Version.Major >= 6 && enabled;
        }
    }
}