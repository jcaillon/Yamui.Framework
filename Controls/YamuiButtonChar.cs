﻿#region header
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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer("YamuiFramework.Controls.YamuiGoBackButtonDesigner")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("ButtonPressed")]
    public class YamuiButtonChar : YamuiButton {
        #region Fields

        public IconFontNameEnum IconFontName { get; set; }

        [DefaultValue("ç")]
        [Category("Yamui")]
        public string ButtonChar { get; set; }

        private bool _fakeDisabled;

        [DefaultValue(false)]
        [Category("Yamui")]
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

    internal class YamuiGoBackButtonDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");
            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");
            properties.Remove("UseVisualStyleBackColor");
            properties.Remove("Font");
            properties.Remove("RightToLeft");
            base.PreFilterProperties(properties);
        }
    }
}