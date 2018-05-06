#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiButtonChar.cs) is part of YamuiFramework.
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Yamui.Framework.Fonts;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    public class YamuiButtonChar : YamuiButton {
        #region Fields

        public IconFontNameEnum IconFontName { get; set; }

        [DefaultValue("ç")]
        [Category(nameof(Yamui))]
        public string ButtonChar { get; set; }

        private bool _fakeDisabled;

        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public bool FakeDisabled {
            get { return _fakeDisabled; }
            set {
                _fakeDisabled = value;
                Invalidate();
            }
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            try {
                Color backColor = YamuiThemeManager.Current.ButtonBg(BackColor, UseCustomBackColor, IsFocused, IsHovered, IsPressed, Enabled && !FakeDisabled);
                Color borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, IsPressed, Enabled && !FakeDisabled);
                Color foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, UseCustomForeColor, IsFocused, IsHovered, IsPressed, Enabled && !FakeDisabled);

                var designRect = ClientRectangle;
                designRect.Width -= 2;
                designRect.Height -= 2;

                PaintTransparentBackground(e.Graphics, DisplayRectangle);
                if (backColor != Color.Transparent) {
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    using (SolidBrush b = new SolidBrush(backColor)) {
                        e.Graphics.FillEllipse(b, designRect);
                    }
                    e.Graphics.SmoothingMode = SmoothingMode.Default;
                }

                if (borderColor != Color.Transparent) {
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    using (Pen b = new Pen(borderColor)) {
                        e.Graphics.DrawEllipse(b, designRect);
                    }
                    e.Graphics.SmoothingMode = SmoothingMode.Default;
                }

                designRect.Width += 2;
                designRect.Height += 2;
                TextRenderer.DrawText(e.Graphics, ButtonChar, FontManager.GetOtherFont(IconFontName.ToString().Replace("_", " "), FontStyle.Regular, (float) (Height*0.45)), designRect, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            } catch {
                // ignored
            }
        }

        #endregion

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum IconFontNameEnum {
            Webdings,
            Wingdings,
            Wingdings_2,
            Wingdings_3
        }
    }

}