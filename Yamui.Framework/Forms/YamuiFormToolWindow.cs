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
    public class YamuiFormToolWindow : YamuiFormButtons {
        
        protected override CreateParams CreateParams {
            get {

                var cp = base.CreateParams;

                if (DesignMode)
                    return cp;

                // below is what makes the windows borderless but resizable
                cp.Style = (int) WinApi.WindowStyles.WS_POPUP; // needed if we want the window to be able to aero snap on screen borders

                cp.ExStyle = (int) WinApi.WindowStylesEx.WS_EX_TOOLWINDOW
                             | (int) WinApi.WindowStylesEx.WS_EX_TOPMOST
                             | (int) WinApi.WindowStylesEx.WS_EX_LEFT
                             | (int) WinApi.WindowStylesEx.WS_EX_LTRREADING;

                cp.ClassStyle = (int) WinApi.WindowClassStyles.DropShadow;

                return cp;
            }
        }

    }
}