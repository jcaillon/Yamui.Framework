﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiButton.cs) is part of YamuiFramework.
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
using Yamui.Framework.Helper;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    [Designer(typeof(YamuiButtonDesigner))]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent(nameof(ButtonPressed))]
    public class YamuiButton : Button, IScrollableControl {

        #region Properties

        /// <summary>
        /// Set to true if you wish to use the BackColor property
        /// </summary>
        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public bool UseCustomBackColor { get; set; }

        /// <summary>
        /// Set to true if you wish to use the ForeColor property
        /// </summary>
        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public bool UseCustomForeColor { get; set; }

        /// <summary>
        /// Highlight this button by giving it a thicker border
        /// </summary>
        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public bool Highlight { get; set; }

        /// <summary>
        /// Allows the ButtonPressed event to also be activated by a right/middle click on the button
        /// </summary>
        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public virtual bool AcceptsAnyClick { get; set; }

        /// <summary>
        /// Set the image to use for this button
        /// </summary>
        [Category(nameof(Yamui))]
        public Image BackGrndImage {
            get { return _backGrndImage; }
            set {
                _backGrndImage = value;
                Invalidate();
            }
        }

        private Image _backGrndImage;

        /// <summary>
        /// Set the image size to visualize the image position in the designer mode
        /// </summary>
        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public Size SetImgSize { get; set; }

        /// <summary>
        /// Set to true to use grey scale only for the image (happens by default if the button is disabled)
        /// </summary>
        [DefaultValue(false)]
        [Category(nameof(Yamui))]
        public bool UseGreyScale {
            get { return _useGreyScale; }
            set {
                _useGreyScale = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Returns a grey scale version of the image
        /// </summary>
        public Image GreyScaleBackGrndImage {
            get { return _greyScaleBackGrndImage ?? (_greyScaleBackGrndImage = (BackGrndImage != null ? BackGrndImage.MakeGreyscale3() : null)); }
            set { _greyScaleBackGrndImage = value; }
        }

        /// <summary>
        /// This public prop is only defined so we can set it from the transitions (animation component)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool VisuallyPressButton {
            get { return IsPressed; }
            set {
                IsPressed = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The control has focus?
        /// </summary>
        public bool IsFocused { get; set; }

        /// <summary>
        /// The control is hovered by the cursor?
        /// </summary>
        public bool IsHovered { get; set; }

        /// <summary>
        /// The control is pressed?
        /// </summary>
        public bool IsPressed { get; set; }

        #endregion

        #region Public events

        /// <summary>
        /// You should register to this event to know when the button has been pressed (clicked or enter or space)
        /// The EventArgs can be casted to KeyEventArgs or MouseEventArgs depending on how this was triggered
        /// You can analyse the MouseEventArgs.Clicks number to know if it's a simple or double click
        /// </summary>
        [Category(nameof(Yamui))]
        public event EventHandler<EventArgs> ButtonPressed;

        #endregion

        #region private fields

        private bool _useGreyScale;

        private Image _greyScaleBackGrndImage;

        #endregion

        #region Constructor

        public YamuiButton() {
            // why those styles? check here: https://sites.google.com/site/craigandera/craigs-stuff/windows-forms/flicker-free-control-drawing
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);
        }

        #endregion

        #region Methods

        protected virtual void OnButtonPressed(EventArgs eventArgs) {
            // we could do something here, like preventing the user to click the button when the OnClick is being ran
            if (ButtonPressed != null) {
                ButtonPressed(this, eventArgs);
                var e = eventArgs as KeyEventArgs;
                if (e != null)
                    e.Handled = true;
            }
        }

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            return e.Handled;
        }

        #endregion

        #region Overridden Methods

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        /// <summary>
        /// hides the default method
        /// </summary>
        public new void PerformClick() {
            OnButtonPressed(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            base.PerformClick();
        }

        #endregion

        #region Paint Methods

        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            graphics.Clear(Color.Transparent);
            if ((Parent != null)) {
                clipRect.Offset(Location);
                var e = new PaintEventArgs(graphics, clipRect);
                var state = graphics.Save();
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

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = YamuiThemeManager.Current.ButtonBg(BackColor, UseCustomBackColor, IsFocused, IsHovered, IsPressed, Enabled);
            var borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, IsPressed, Enabled);
            var foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, UseCustomForeColor, IsFocused, IsHovered, IsPressed, Enabled);

            // background
            if (backColor != Color.Transparent)
                e.Graphics.Clear(backColor);
            else
                PaintTransparentBackground(e.Graphics, DisplayRectangle);

            // border?
            if (borderColor != Color.Transparent)
                using (var p = new Pen(borderColor)) {
                    var borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
                    e.Graphics.DrawRectangle(p, borderRect);
                }

            // highlight is a border with more width
            if (Highlight && !IsHovered && !IsPressed && Enabled) {
                using (var p = new Pen(YamuiThemeManager.Current.AccentColor, 4)) {
                    var borderRect = new Rectangle(2, 2, Width - 4, Height - 4);
                    e.Graphics.DrawRectangle(p, borderRect);
                }
            }

            // text + image
            if (BackGrndImage != null || !SetImgSize.IsEmpty) {
                // ReSharper disable once PossibleNullReferenceException
                Size imgSize = !SetImgSize.IsEmpty ? SetImgSize : BackGrndImage.Size;
                float gap = ((float) ClientRectangle.Height - imgSize.Height)/2;
                var rectImg = new RectangleF(gap, gap, imgSize.Width, imgSize.Height);

                if (DesignMode || BackGrndImage == null) {
                    // in design mode
                    using (SolidBrush b = new SolidBrush(Color.Fuchsia))
                        e.Graphics.FillRectangle(b, rectImg);
                } else {
                    // draw main image, in greyscale if not activated
                    if (BackGrndImage != null)
                        e.Graphics.DrawImage((!Enabled || UseGreyScale) ? GreyScaleBackGrndImage : BackGrndImage, rectImg);
                }

                // text
                int xPos = (int) (gap*2 + 0.5) + imgSize.Width;
                TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), new Rectangle(xPos, 0, ClientRectangle.Width - xPos - (int) (gap + 0.5), ClientRectangle.Height), foreColor, FontManager.GetTextFormatFlags(TextAlign));
            } else {
                // text only
                TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), ClientRectangle, foreColor, FontManager.GetTextFormatFlags(TextAlign));
            }
        }

        #endregion

        #region Managing isHovered, isPressed, isFocused

        #region Focus Methods

        protected override void OnEnter(EventArgs e) {
            IsFocused = true;
            Invalidate();
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            IsFocused = false;
            Invalidate();
            base.OnLeave(e);
        }

        #endregion

        #region Keyboard Methods

        // This is mandatory to be able to handle the ENTER key in key events!!
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            if (e.KeyCode == Keys.Enter)
                e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) {
                IsPressed = true;
                Invalidate();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if (IsPressed) {
                OnButtonPressed(e);
            }
            IsPressed = false;
            Invalidate();
            base.OnKeyUp(e);
        }

        #endregion

        #region Mouse Methods

        protected override void OnMouseEnter(EventArgs e) {
            IsHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left || AcceptsAnyClick) {
                IsPressed = true;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (IsPressed) {
                OnButtonPressed(e);
            }
            IsPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            IsPressed = false;
            IsHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        #endregion

        #endregion

        public void UpdateBoundsPublic() {
            UpdateBounds();
        }
    }

    internal class YamuiButtonDesigner : ControlDesigner {
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