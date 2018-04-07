﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiButtonLink.cs) is part of YamuiFramework.
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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Yamui.Framework.Fonts;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    [ToolboxBitmap(typeof(LinkLabel))]
    public class YamuiButtonLink : YamuiButton {
        #region Fields

        private FontFunction _function = FontFunction.Link;

        [DefaultValue(FontFunction.Normal)]
        [Category("Yamui")]
        public FontFunction Function {
            get { return _function; }
            set { _function = value; }
        }

        #endregion

        #region Constructor

        public YamuiButtonLink() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint, true);
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            // background
            Color backColor = YamuiThemeManager.Current.LabelsBg(BackColor, UseCustomBackColor);
            if (backColor != Color.Transparent)
                e.Graphics.Clear(backColor);
            else
                PaintTransparentBackground(e.Graphics, DisplayRectangle);

            // foreground
            Color foreColor = YamuiThemeManager.Current.LabelsFg(ForeColor, UseCustomForeColor, IsFocused, IsHovered, IsPressed, Enabled);
            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetFont(Function), ClientRectangle, foreColor, FontManager.GetTextFormatFlags(TextAlign));
        }

        #endregion
    }

}