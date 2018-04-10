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
    public class YamuiFormBaseShadow2 : YamuiFormBase {

        #region CreateParams

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;
                //cp.ExStyle &= ~((int) WinApi.WindowStylesEx.WS_EX_RIGHTSCROLLBAR);
                //cp.Style |= (int) WinApi.WindowStyles.WS_POPUP;
                //cp.Style |= (int) WinApi.WindowStyles.WS_CAPTION;
                //cp.Style |= (int) WinApi.WindowStyles.WS_THICKFRAME;
                ////cp.Style &= ~(int) WinApi.WindowStyles.WS_THICKFRAME;
                return cp;
            }
        }

        #endregion

        #region WndProc

        //protected override void WndProc(ref Message m) {
        //    if (DesignMode) {
        //        base.WndProc(ref m);
        //        return;
        //    }
        //
        //    base.WndProc(ref m);
        //
        //    switch (m.Msg) {
        //        case (int) WinApi.Messages.WM_NCPAINT:
        //            // Allow to display the shadows
        //            if (DwmCompositionEnabled) {
        //                var status = Marshal.AllocHGlobal(sizeof(int));
        //                Marshal.WriteInt32(status, (int) WinApi.DWMNCRenderingPolicy.Enabled);
        //                WinApi.DwmSetWindowAttribute(Handle, WinApi.DWMWINDOWATTRIBUTE.NCRenderingPolicy, status, sizeof(int));
        //
        //                var margins = new WinApi.MARGINS(1, 1, 1, 1);
        //                WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
        //            }
        //            break;
        //    }
        //}

        #endregion

        protected override void OnCreateControl() {
            base.OnCreateControl();
            // Allow to display the shadows
            if (DwmCompositionEnabled) {
                var status = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(status, (int) WinApi.DWMNCRenderingPolicy.Enabled);
                WinApi.DwmSetWindowAttribute(Handle, WinApi.DWMWINDOWATTRIBUTE.NCRenderingPolicy, status, sizeof(int));

                var margins = new WinApi.MARGINS(1, 1, 1, 1);
                WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
            }
        }
    }

}