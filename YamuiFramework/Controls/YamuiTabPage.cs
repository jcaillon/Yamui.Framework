using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    //[Designer("YamuiFramework.Controls.YamuiTabPageDesigner")]
    [ToolboxItem(false)]
    public class YamuiTabPage : TabPage {
        #region Fields
        private TabFunction _function = TabFunction.Main;
        [DefaultValue(TabFunction.Main)]
        [Category("Yamui")]
        public TabFunction Function {
            get { return _function; }
            set {
                _function = value;
                SetStuff();
            }
        }

        /// <summary>
        /// Read only after the initialisation! Set to true to hide this sheet in normal mode
        /// </summary>
        private bool _hiddenPage;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool HiddenPage {
            get { return _hiddenPage; }
            set {
                _hiddenPage = value;
                HiddenState = _hiddenPage;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool HiddenState { get; set; }
        #endregion

        #region Constructor
        public YamuiTabPage() {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            SetStuff();
        }

        public void SetStuff() {
            Padding = (Function == TabFunction.Main) ? new Padding(0, 0, 0, 0) : new Padding(15, 25, 0, 0);
            UseVisualStyleBackColor = false;
        }
        #endregion

        #region Paint

        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            var myParent = (YamuiTabControl)Parent;
            graphics.Clear(Color.Transparent);
            if ((myParent != null)) {
                //clipRect.Offset(myParent.Location);
                //clipRect.Offset(0, myParent.ItemSize.Height);
                PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                GraphicsState state = graphics.Save();
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                try {
                    graphics.TranslateTransform(-myParent.Location.X, -myParent.Location.Y);
                    InvokePaintBackground(myParent, e);
                    InvokePaint(myParent, e);
                } finally {
                    graphics.Restore(state);
                    //clipRect.Offset(-myParent.Location.X, -myParent.Location.Y);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected void CustomOnPaintBackground(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.Current.TabsColorsNormalBackColor;
                if (backColor != Color.Transparent) {
                    e.Graphics.Clear(backColor);
                    var img = ThemeManager.ThemePageImage;
                    if (img != null) {
                        Rectangle rect = new Rectangle(ClientRectangle.Right - img.Width, ClientRectangle.Height - img.Height, img.Width, img.Height);
                        e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                    }
                } else
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

        protected virtual void OnPaintForeground(PaintEventArgs e) { }
        #endregion
    }

    #region YamuiTabPageDesigner
    internal class YamuiTabPageDesigner : ControlDesigner {
        #region Fields

        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("UseVisualStyleBackColor");
            properties.Remove("Padding");
            properties.Remove("Font");

            base.PreFilterProperties(properties);
        }

        #endregion
    }

    #endregion
}
