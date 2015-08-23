using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace YamuiFramework.Controls
{
    [Designer("YamuiFramework.Controls.YamuiTileDesigner")]
    [ToolboxBitmap(typeof(Button))]
    public class YamuiTile : Button, IContainerControl
    {
        #region Interface
        [Browsable(false)]
        public Control ActiveControl { get; set; }

        public bool ActivateControl(Control ctrl)
        {
            if (Controls.Contains(ctrl))
            {
                ctrl.Select();
                ActiveControl = ctrl;
                return true;
            }

            return false;
        }
        #endregion

        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        private bool _paintTileCount = true;
        [DefaultValue(true)]
        [Category("Yamui")]
        public bool PaintTileCount
        {
            get { return _paintTileCount; }
            set { _paintTileCount = value; }
        }

        private int _tileCount;
        [DefaultValue(0)]
        public int TileCount
        {
            get { return _tileCount; }
            set { _tileCount = value; }
        }

        [DefaultValue(ContentAlignment.BottomLeft)]
        public new ContentAlignment TextAlign
        {
            get { return base.TextAlign; }
            set { base.TextAlign = value; }
        }

        private Image _tileImage;
        [DefaultValue(null)]
        [Category("Yamui")]
        public Image TileImage
        {
            get { return _tileImage; }
            set { _tileImage = value; }
        }

        private bool _useTileImage;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseTileImage
        {
            get { return _useTileImage; }
            set { _useTileImage = value; }
        }

        private ContentAlignment _tileImageAlign = ContentAlignment.TopLeft;
        [DefaultValue(ContentAlignment.TopLeft)]
        [Category("Yamui")]
        public ContentAlignment TileImageAlign
        {
            get { return _tileImageAlign; }
            set { _tileImageAlign = value; }
        }

        private bool _isHovered;
        private bool _isPressed;
        private bool _isFocused;

        #endregion

        #region Constructor

        public YamuiTile()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            TextAlign = ContentAlignment.BottomLeft;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.ButtonColors.BackGround(BackColor, UseCustomBackColor, _isFocused, _isHovered, _isPressed, Enabled);
                if (backColor == Color.Transparent) return;
                e.Graphics.Clear(backColor);
            } catch {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                if (GetStyle(ControlStyles.AllPaintingInWmPaint))
                    OnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e)
        {
            Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, UseCustomForeColor, _isFocused, _isHovered, _isPressed, Enabled);

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            if (_useTileImage)
            {
                if (_tileImage != null)
                {
                    Rectangle imageRectangle;
                    switch (_tileImageAlign)
                    {
                        case ContentAlignment.BottomLeft:
                            imageRectangle = new Rectangle(new Point(0, Height - TileImage.Height), new Size(TileImage.Width, TileImage.Height));
                            break;

                        case ContentAlignment.BottomCenter:
                            imageRectangle = new Rectangle(new Point(Width / 2 - TileImage.Width / 2, Height - TileImage.Height), new Size(TileImage.Width, TileImage.Height));
                            break;

                        case ContentAlignment.BottomRight:
                            imageRectangle = new Rectangle(new Point(Width - TileImage.Width, Height - TileImage.Height), new Size(TileImage.Width, TileImage.Height));
                            break;

                        case ContentAlignment.MiddleLeft:
                            imageRectangle = new Rectangle(new Point(0, Height / 2 - TileImage.Height / 2), new Size(TileImage.Width, TileImage.Height));
                            break;

                        case ContentAlignment.MiddleCenter:
                            imageRectangle = new Rectangle(new Point(Width / 2 - TileImage.Width / 2, Height / 2 - TileImage.Height / 2), new Size(TileImage.Width, TileImage.Height));
                            break;

                        case ContentAlignment.MiddleRight:
                            imageRectangle = new Rectangle(new Point(Width - TileImage.Width, Height / 2 - TileImage.Height / 2), new Size(TileImage.Width, TileImage.Height));
                            break;

                        case ContentAlignment.TopLeft:
                            imageRectangle = new Rectangle(new Point(0, 0), new Size(TileImage.Width, TileImage.Height));
                            break;

                        case ContentAlignment.TopCenter:
                            imageRectangle = new Rectangle(new Point(Width / 2 - TileImage.Width / 2, 0), new Size(TileImage.Width, TileImage.Height));
                            break;

                        case ContentAlignment.TopRight:
                            imageRectangle = new Rectangle(new Point(Width - TileImage.Width, 0), new Size(TileImage.Width, TileImage.Height));
                            break;

                        default:
                            imageRectangle = new Rectangle(new Point(0, 0), new Size(TileImage.Width, TileImage.Height));
                            break;
                    }

                    e.Graphics.DrawImage(TileImage, imageRectangle);
                }
            }

            if (TileCount > 0 && _paintTileCount)
            {
                Size countSize = TextRenderer.MeasureText(TileCount.ToString(), FontManager.GetFont(FontStyle.Regular, 44f));

                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                TextRenderer.DrawText(e.Graphics, TileCount.ToString(), FontManager.GetFont(FontStyle.Regular, 44f), new Point(Width - countSize.Width, 0), foreColor);
                e.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
            }

            TextFormatFlags flags = FontManager.GetTextFormatFlags(TextAlign) | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.EndEllipsis;
            Rectangle textRectangle = ClientRectangle;
            if (_isPressed)
            {
                textRectangle.Inflate(-2, -2);
            }

            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRectangle, foreColor, flags);
        }

        #endregion

        #region "Managing isHovered, isPressed, isFocused"

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

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        #endregion
    }

    internal class YamuiTileDesigner : ParentControlDesigner {
        public override bool CanParent(Control control) {
            return (control is YamuiLabel || control is YamuiProgressSpinner);
        }

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

            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("UseVisualStyleBackColor");

            properties.Remove("Font");
            properties.Remove("RightToLeft");

            base.PreFilterProperties(properties);
        }
    }
}
