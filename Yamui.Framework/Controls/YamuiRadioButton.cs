﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiRadioButton.cs) is part of YamuiFramework.
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Yamui.Framework.Fonts;
using Yamui.Framework.Themes;

// Credits to : http://sourceforge.net/projects/bsfccontrollibr/

namespace Yamui.Framework.Controls {

    [Designer(typeof(YamuiRadioButtonDesigner))]
    [ToolboxBitmap(typeof(RadioButton))]
    public class YamuiRadioButton : RadioButton, IScrollableControl {
        #region Fields

        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public bool UseCustomForeColor { get; set; }

        private bool _isHovered;
        private bool _isPressed;
        private bool _isFocused;

        #endregion

        #region Constructor

        public YamuiRadioButton() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);
        }

        #endregion

        #region Paint Methods

        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            graphics.Clear(Color.Transparent);
            if ((Parent != null)) {
                clipRect.Offset(Location);
                PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                GraphicsState state = graphics.Save();
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                try {
                    graphics.TranslateTransform(-Location.X, -Location.Y);
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                } finally {
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {}

        protected void CustomOnPaintBackground(PaintEventArgs e) {
            try {
                PaintTransparentBackground(e.Graphics, DisplayRectangle);
            } catch {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                CustomOnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {
            Color borderColor = YamuiThemeManager.Current.ButtonBorder(_isFocused, _isHovered, _isPressed, Enabled);
            Color foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, UseCustomForeColor, _isFocused, _isHovered, _isPressed, Enabled);
            Color backColor = YamuiThemeManager.Current.ButtonBg(BackColor, UseCustomBackColor, _isFocused, _isHovered, _isPressed, Enabled);

            // Paint the back + border of the checkbox
            using (SolidBrush b = new SolidBrush(backColor)) {
                Rectangle boxRect = new Rectangle(0, Height/2 - 6, 12, 12);
                e.Graphics.FillRectangle(b, boxRect);
            }

            if (borderColor != Color.Transparent)
                using (Pen p = new Pen(borderColor)) {
                    Rectangle boxRect = new Rectangle(0, Height/2 - 6, 12, 12);
                    e.Graphics.DrawRectangle(p, boxRect);
                }

            // paint the form inside
            if (Checked) {
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.AccentColor)) {
                    Rectangle boxRect = new Rectangle(4, Height/2 - 2, 5, 5);
                    e.Graphics.FillRectangle(b, boxRect);
                }
            }

            Rectangle textRect = new Rectangle(16, 0, Width - 16, Height);
            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRect, foreColor, FontManager.GetTextFormatFlags(TextAlign));
        }

        #endregion

        #region Managing isHovered, isPressed, isFocused

        #region Focus Methods

        protected override void OnEnter(EventArgs e) {
            _isFocused = true;
            Invalidate();

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            _isFocused = false;
            Invalidate();

            base.OnLeave(e);
        }

        #endregion

        #region Keyboard Methods

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyCode == Keys.Space) {
                _isPressed = true;
                Invalidate();
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            //Remove this code cause this prevents the focus color
            _isPressed = false;
            Invalidate();
            base.OnKeyUp(e);
        }

        #endregion

        #region Mouse Methods

        protected override void OnMouseEnter(EventArgs e) {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                _isPressed = true;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        #endregion

        #endregion

        #region Overridden Methods

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnCheckedChanged(EventArgs e) {
            base.OnCheckedChanged(e);
            Invalidate();
        }

        public override Size GetPreferredSize(Size proposedSize) {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics()) {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, Text, FontManager.GetStandardFont(), proposedSize, FontManager.GetTextFormatFlags(TextAlign));
                preferredSize.Width += 16;
            }

            return preferredSize;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            return e.Handled;
        }

        #endregion

        public void UpdateBoundsPublic() {
            UpdateBounds();
        }
    }

    internal class YamuiRadioButtonDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");

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