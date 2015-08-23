using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Native;

namespace YamuiFramework.Controls {
    [Designer("YamuiFramework.Controls.YamuiToggleDesigner")]
    [ToolboxBitmap(typeof(CheckBox))]

    public class YamuiToggle : CheckBox {
        #region Fields
        private bool _isHovered;
        private bool _isPressed;
        private bool _isFocused;
        #endregion

        #region Constructor
        public YamuiToggle() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint, true);
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

        protected override void OnPaintBackground(PaintEventArgs e) {
            try {
                PaintTransparentBackground(e.Graphics, DisplayRectangle);
            } catch {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                OnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {
            Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, false, _isFocused, _isHovered, _isPressed, Enabled);

            Rectangle textRect = new Rectangle(42, 0, Width - 42, Height);
            Rectangle backRect = new Rectangle(0, 0, 39, Height);

            Color borderColor = ThemeManager.ButtonColors.BorderColor(_isFocused, _isHovered, _isPressed, Enabled);

            using (Pen p = new Pen(borderColor)) {
                Rectangle boxRect = new Rectangle(0, 0, backRect.Width - 1, backRect.Height - 1);
                e.Graphics.DrawRectangle(p, boxRect);
            }

            Color fillColor = Checked ? ThemeManager.AccentColor : ThemeManager.ButtonColors.Normal.BackColor();

            using (SolidBrush b = new SolidBrush(fillColor)) {
                Rectangle boxRect = new Rectangle(2, 2, backRect.Width - 4, backRect.Height - 4);
                e.Graphics.FillRectangle(b, boxRect);
            }

            Color backColor = BackColor;

            using (SolidBrush b = new SolidBrush(backColor)) {
                int left = Checked ? backRect.Width - 11 : 0;

                Rectangle boxRect = new Rectangle(left, 0, 11, backRect.Height);
                e.Graphics.FillRectangle(b, boxRect);
            }
            using (SolidBrush b = new SolidBrush(ThemeManager.ButtonColors.Hover.BackColor())) {
                int left = Checked ? backRect.Width - 10 : 0;

                Rectangle boxRect = new Rectangle(left, 0, 10, backRect.Height);
                e.Graphics.FillRectangle(b, boxRect);
            }

            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRect, foreColor, FontManager.GetTextFormatFlags(TextAlign));
        }

        #endregion

        #region Managing isHovered, isPressed, isFocused

        #region Focus Methods
        protected override void OnGotFocus(EventArgs e) {
            _isFocused = true;
            Invalidate();

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e) {
            _isFocused = false;
            Invalidate();

            base.OnLostFocus(e);
        }

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
                preferredSize.Width += 42;
            }

            return preferredSize;
        }

        #endregion
    }

    internal class YamuiToggleDesigner : ControlDesigner {
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

            properties.Remove("ThreeState");

            base.PreFilterProperties(properties);
        }
    }
}
