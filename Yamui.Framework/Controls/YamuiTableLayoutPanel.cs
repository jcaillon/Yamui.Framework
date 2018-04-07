#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiTableLayoutPanel.cs) is part of YamuiFramework.
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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    [ToolboxBitmap(typeof(TableLayoutPanel))]
    public class YamuiTableLayoutPanel : TableLayoutPanel, IScrollableControl {
        #region Fields

        [Category("Yamui")]
        [DefaultValue(false)]
        public bool DisableTransparentBackGround { get; set; }

        #endregion

        #region Constructor

        public YamuiTableLayoutPanel() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.Opaque, true);
        }

        #endregion

        #region Paint

        /// <summary>
        /// Paint transparent background
        /// </summary>
        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            if (Parent != null) {
                clipRect.Offset(Location);
                PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                GraphicsState state = graphics.Save();
                //graphics.SmoothingMode = SmoothingMode.HighSpeed;
                try {
                    graphics.TranslateTransform(-Location.X, -Location.Y);
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                } finally {
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            } else {
                graphics.Clear(YamuiThemeManager.Current.FormBack);
            }
        }
        
        protected override void OnPaint(PaintEventArgs e) {
            if (YamuiThemeManager.Current.NeedTransparency && !DisableTransparentBackGround) 
                PaintTransparentBackground(e.Graphics, DisplayRectangle);
            else
                e.Graphics.Clear(YamuiThemeManager.Current.FormBack);
        }

        #endregion

        public void UpdateBoundsPublic() {
            UpdateBounds();
        }
    }
}