﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiProgressBar.cs) is part of YamuiFramework.
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
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Yamui.Framework.Fonts;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    [ToolboxBitmap(typeof(ProgressBar))]
    public class YamuiProgressBar : YamuiControl {

        #region fields

        private float _progress;

        [DefaultValue(0), Description("The value ranging from 0-100 which represents progress"), Category("Yamui")]
        public float Progress {
            get { return _progress; }
            set {
                if (value >= 100) {
                    _progress = 100;
                    OnProgressCompleted();
                } else if (value < 0) _progress = 0;
                else _progress = value;

                Invalidate();
                OnProgressChanged();
            }
        }

        private ProgressStyle _progressStyle;

        [DefaultValue(ProgressStyle.Normal), Description("The progress animation style"), Category("Yamui")]
        public ProgressStyle Style {
            get { return _progressStyle; }
            set {
                _progressStyle = value;
                Invalidate();
            }
        }

        private CenterElement _centerElement;

        [DefaultValue(CenterElement.None), Description("The element to draw at the center"), Category("Yamui")]
        public CenterElement CenterText {
            get { return _centerElement; }
            set {
                _centerElement = value;
                Invalidate();
            }
        }

        private bool _vertical;

        [DefaultValue(false), Description("Determines the orientation of the progress bar"), Category("Yamui")]
        public bool Vertical {
            get { return _vertical; }
            set {
                bool changed = _vertical != value;

                if (changed) {
                    _vertical = value;
                    UpdateLgBrushes();
                    Invalidate();
                }
            }
        }

        private bool _useMarquee;

        [DefaultValue(false), Description("Determines whether to use the marquee in opposed to direction"), Category("Yamui")]
        public bool UseMarquee {
            get { return _useMarquee; }
            set {
                _useMarquee = value;
                _tmrMarquee.Enabled = value;
                Invalidate();
            }
        }

        [DefaultValue(80), Description("The width of the animated marquee"), Category("Yamui")]
        public int MarqueeWidth { get; set; }

        private int _gradientIntensity;

        [DefaultValue(20), Description("The intensity of the outer color in relation to the inner"), Category("Yamui")]
        public int GradientIntensity {
            get { return _gradientIntensity; }
            set {
                _gradientIntensity = value;
                UpdateLgBrushes();
                Invalidate();
            }
        }

        private bool _autoSetOrientation = true;

        [DefaultValue(true), Description("When true, automatically sets the Vertical property according to the controls size"), Category("Yamui")]
        public bool AutoSetOrientation {
            get { return _autoSetOrientation; }
            set {
                _autoSetOrientation = value;
                if (_autoSetOrientation) AutoSetIsVertical();
                Invalidate();
            }
        }

        private readonly BufferedGraphicsContext _bufContext = BufferedGraphicsManager.Current;
        private readonly Timer _tmrMarquee = new Timer();
        private LinearGradientBrush _foreLgb, _backLgb;
        private BufferedGraphics _bufGraphics;
        private int _marqueePos;

        #endregion

        #region Constructor

        public YamuiProgressBar() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            // this usercontrol should not be able to get the focus
            SetStyle(ControlStyles.Selectable, false);

            _tmrMarquee.Tick += _tmrMarquee_Tick;

            // Set defaults
            GradientIntensity = 20;
            MarqueeWidth = 50;
            _tmrMarquee.Interval = 30;
            Size = new Size(200, 23);
        }

        #endregion

        #region Virtual and Overridden

        [Description("Occurs when the progress property has changed and the control has invalidated")]
        public event EventHandler ProgressChanged;

        /// <summary>
        /// Raises the ProgressChanged event
        /// </summary>
        protected virtual void OnProgressChanged() {
            if (ProgressChanged != null)
                ProgressChanged(this, EventArgs.Empty);
        }

        [Description("Occurs when progress reaches 100%")]
        public event EventHandler ProgressCompleted;

        /// <summary>
        /// Raises the ProgressCompleted event
        /// </summary>
        protected virtual void OnProgressCompleted() {
            if (ProgressCompleted != null)
                ProgressCompleted(this, EventArgs.Empty);
        }

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            UpdateLgBrushes();
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e) {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            if (_autoSetOrientation) AutoSetIsVertical();
            // Update back buffer
            _bufContext.MaximumBuffer = new Size(Width + 1, Height + 1);
            _bufGraphics = _bufContext.Allocate(CreateGraphics(), ClientRectangle);
            UpdateLgBrushes();
        }

        protected override void OnPaintBackground(PaintEventArgs e) {}

        protected override void OnPaint(PaintEventArgs e) {
            //Draw grey backdrop
            _bufGraphics.Graphics.FillRectangle(_backLgb, ClientRectangle);

            if (_useMarquee) DrawMarquee();
            else if (_vertical) DrawVerticalProgress();
            else DrawHorizProgress();

            // Draw border
            if (YamuiThemeManager.Current.ButtonNormalBorder != Color.Transparent) {
                Rectangle rect = ClientRectangle;
                _bufGraphics.Graphics.DrawRectangle(new Pen(YamuiThemeManager.Current.ButtonNormalBorder, 1f), rect.X,
                    rect.Y, rect.Width - 1, rect.Height - 1);
            }

            DrawCenterElement();
            _bufGraphics.Render(e.Graphics);
        }

        #endregion

        #region Private methods

        private void AutoSetIsVertical() {
            Vertical = (Width < Height);
        }

        private void _tmrMarquee_Tick(object sender, EventArgs e) {
            if (Visible && Enabled) {
                if ((_vertical && _marqueePos < Height) ||
                    (!_vertical && _marqueePos < Width)) {
                    _marqueePos += 2;
                } else _marqueePos = 0;
                Invalidate();
            }
        }

        private void UpdateLgBrushes() {
            Color backColor = YamuiThemeManager.Current.ButtonBg(BackColor, false, false, false, false, Enabled);

            if (Width <= 0 || Height <= 0) return;

            int angle = (_vertical) ? 0 : 90;
            Color darkColor;
            Color darkColor2;

            if (Enabled) {
                darkColor = ChangeColorBrightness(backColor, _gradientIntensity);
                darkColor2 = ChangeColorBrightness(YamuiThemeManager.Current.AccentColor, _gradientIntensity);
            } else {
                darkColor = ChangeColorBrightness(Desaturate(backColor), _gradientIntensity);
                darkColor2 = ChangeColorBrightness(Desaturate(YamuiThemeManager.Current.AccentColor), _gradientIntensity);
            }

            Rectangle rect = (!_vertical) ? new Rectangle(0, 0, Width, Height/2) :
                new Rectangle(0, 0, Width/2, Height);

            if (Enabled) {
                _backLgb = new LinearGradientBrush(rect, backColor, darkColor, angle, true);
                _foreLgb = new LinearGradientBrush(rect, YamuiThemeManager.Current.AccentColor, darkColor2, angle, true);
            } else {
                _backLgb = new LinearGradientBrush(rect, Desaturate(backColor), darkColor, angle, true);
                _foreLgb = new LinearGradientBrush(rect, Desaturate(YamuiThemeManager.Current.AccentColor), darkColor2, angle, true);
            }

            _backLgb.WrapMode = WrapMode.TileFlipX;
            _foreLgb.WrapMode = WrapMode.TileFlipX;
        }

        #endregion

        #region Drawing

        private static Color ChangeColorBrightness(Color color, int factor) {
            int r = (color.R + factor > 255) ? 255 : color.R + factor;
            int g = (color.G + factor > 255) ? 255 : color.G + factor;
            int b = (color.B + factor > 255) ? 255 : color.B + factor;
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            return Color.FromArgb(r, g, b);
        }

        private static Color Desaturate(Color color) {
            int b = (int) (255*color.GetBrightness());
            return Color.FromArgb(b, b, b);
        }

        private void DrawMarquee() {
            if (_vertical) {
                int smallSect = Height - _marqueePos;
                int largeSect = MarqueeWidth - smallSect;

                if (largeSect > 0)
                    _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, 0, Width, largeSect);

                _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, _marqueePos, Width, MarqueeWidth);
            } else {
                int smallSect = Width - _marqueePos;
                int largeSect = MarqueeWidth - smallSect;

                if (largeSect > 0)
                    _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, 0, largeSect, Height);

                _bufGraphics.Graphics.FillRectangle(_foreLgb, _marqueePos, 0, MarqueeWidth, Height);
            }
        }

        private void DrawHorizProgress() {
            if (Style == ProgressStyle.Normal) {
                float width = Width*(_progress/100f);
                _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, 0, width, Height);
            } else if (Style == ProgressStyle.Reversed) {
                float pixelPercent = Width*(_progress/100f);
                _bufGraphics.Graphics.FillRectangle(_foreLgb, Width - pixelPercent, 0, pixelPercent, Height);
            } else if (Style == ProgressStyle.Inwards) {
                float pixelPercent = Width*(_progress/100f)/2f;
                _bufGraphics.Graphics.FillRectangle(_foreLgb, Width - pixelPercent, 0, pixelPercent, Height);
                _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, 0, pixelPercent, Height);
            } else if (Style == ProgressStyle.Outwards) {
                float pixelPercent = Width*(_progress/100f);
                float x = Width/2f - pixelPercent/2f;
                _bufGraphics.Graphics.FillRectangle(_foreLgb, x, 0, pixelPercent, Height);
            }
        }

        private void DrawCenterElement() {
            if (_centerElement == CenterElement.None) return;
            string text;

            if (_centerElement == CenterElement.Percent)
                text = ((int) (_progress + 0.5f)).ToString(CultureInfo.InvariantCulture) + '%';
            else
                text = Text;

            Color foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, false, false, false, false, Enabled);
            Font font = FontManager.GetFont(FontStyle.Bold, 10f);

            float strWidth = _bufGraphics.Graphics.MeasureString(text, font).Width;
            float strHeight = _bufGraphics.Graphics.MeasureString(text, font).Height;
            float xPos = Width/2f - strWidth/2f;
            float yPos = Height/2f - strHeight/2f;
            Point p1 = new Point((int) xPos, (int) yPos);
            TextRenderer.DrawText(_bufGraphics.Graphics, text, font, p1, foreColor);
        }

        private void DrawVerticalProgress() {
            float pixelPercent = Height*(_progress/100f);

            if (Style == ProgressStyle.Normal) {
                _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, 0, Width, pixelPercent);
            } else if (Style == ProgressStyle.Reversed) {
                _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, Height - pixelPercent, Width, pixelPercent);
            } else if (Style == ProgressStyle.Inwards) {
                float temp = pixelPercent/2f;
                _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, Height - temp, Width, temp);
                _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, 0, Width, temp);
            } else if (Style == ProgressStyle.Outwards) {
                float y = Height/2f - pixelPercent/2f;
                _bufGraphics.Graphics.FillRectangle(_foreLgb, 0, y, Width, pixelPercent);
            }
        }

        #endregion
        
    }

    public enum CenterElement {
        /// <summary>
        /// Draw nothing in the center
        /// </summary>
        None,

        /// <summary>
        /// Draw the value of the Text property in the center
        /// </summary>
        Text,

        /// <summary>
        /// Draw the value of the Percent property in the center
        /// </summary>
        Percent
    }

    /// <summary>
    /// Specifies what style to use for a SuperProgressBar
    /// </summary>
    public enum ProgressStyle {
        /// <summary>
        /// The control will animate from left to right or top to bottom
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The control will animate from right to left or bottom to top
        /// </summary>
        Reversed,

        /// <summary>
        /// The control will animate from the outer edges inward
        /// </summary>
        Inwards,

        /// <summary>
        /// The control will animate from the center to the edge of the control
        /// </summary>
        Outwards
    }

}