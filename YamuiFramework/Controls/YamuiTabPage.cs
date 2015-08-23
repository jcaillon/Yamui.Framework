using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Native;

namespace YamuiFramework.Controls {

    //[Designer("YamuiFramework.Controls.YamuiTabPageDesigner")]
    [ToolboxItem(false)]
    public class YamuiTabPage : TabPage {
        #region Fields

        private YamuiScrollBar _verticalScrollbar = new YamuiScrollBar(ScrollOrientation.Vertical);
        private YamuiScrollBar _horizontalScrollbar = new YamuiScrollBar(ScrollOrientation.Horizontal);

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
        /// Set this to true if the tab should be hidden on loading, you can access with 
        /// GoToPage method of the form, shouldn't be used for secondary tabs!
        /// </summary>
        private bool _hideThis;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool HideThis {
            get { return _hideThis; }
            set {_hideThis = value; }
        }

        private bool _showHorizontalScrollbar;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool HorizontalScrollbar {
            get { return _showHorizontalScrollbar; }
            set { _showHorizontalScrollbar = value; }
        }

        [Category("Yamui")]
        public int HorizontalScrollbarSize {
            get { return _horizontalScrollbar.ScrollbarSize; }
            set { _horizontalScrollbar.ScrollbarSize = value; }
        }

        [Category("Yamui")]
        public bool HorizontalScrollbarHighlightOnWheel {
            get { return _horizontalScrollbar.HighlightOnWheel; }
            set { _horizontalScrollbar.HighlightOnWheel = value; }
        }

        private bool _showVerticalScrollbar;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool VerticalScrollbar {
            get { return _showVerticalScrollbar; }
            set { _showVerticalScrollbar = value; }
        }

        [Category("Yamui")]
        public int VerticalScrollbarSize {
            get { return _verticalScrollbar.ScrollbarSize; }
            set { _verticalScrollbar.ScrollbarSize = value; }
        }

        [Category("Yamui")]
        public bool VerticalScrollbarHighlightOnWheel {
            get { return _verticalScrollbar.HighlightOnWheel; }
            set { _verticalScrollbar.HighlightOnWheel = value; }
        }

        [Category("Yamui")]
        [DefaultValue(false)]
        public new bool AutoScroll {
            get {
                return base.AutoScroll;
            }
            set {
                if (value) {
                    _showHorizontalScrollbar = true;
                    _showVerticalScrollbar = true;
                }

                base.AutoScroll = value;
            }
        }

        #endregion

        #region Constructor
        public YamuiTabPage() {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            if (AutoScroll) {
                Controls.Add(_verticalScrollbar);
                Controls.Add(_horizontalScrollbar);
            }

            _verticalScrollbar.Scroll += VerticalScrollbarScroll;
            _horizontalScrollbar.Scroll += HorizontalScrollbarScroll;

            SetStuff();
        }

        public void SetStuff() {
            Padding = (Function == TabFunction.Main) ? new Padding(0, 0, 0, 0) : new Padding(30, 25, 0, 0);
        }
        #endregion
        #region Paint
        
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
                Color backColor = ThemeManager.TabsColors.Normal.BackColor();
                if (backColor != Color.Transparent)
                    e.Graphics.Clear(backColor);
                else
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

            if (!AutoScroll) return;

            if (DesignMode) {
                _horizontalScrollbar.Visible = false;
                _verticalScrollbar.Visible = false;
                return;
            }

            UpdateScrollBarPositions();

            if (HorizontalScrollbar) {
                _horizontalScrollbar.Visible = HorizontalScroll.Visible;
            }
            if (HorizontalScroll.Visible) {
                _horizontalScrollbar.Minimum = HorizontalScroll.Minimum;
                _horizontalScrollbar.Maximum = HorizontalScroll.Maximum;
                _horizontalScrollbar.SmallChange = HorizontalScroll.SmallChange;
                _horizontalScrollbar.LargeChange = HorizontalScroll.LargeChange;
            }

            if (VerticalScrollbar) {
                _verticalScrollbar.Visible = VerticalScroll.Visible;
            }
            if (VerticalScroll.Visible) {
                _verticalScrollbar.Minimum = VerticalScroll.Minimum;
                _verticalScrollbar.Maximum = VerticalScroll.Maximum;
                _verticalScrollbar.SmallChange = VerticalScroll.SmallChange;
                _verticalScrollbar.LargeChange = VerticalScroll.LargeChange;
            }
        }

        protected override void OnBackColorChanged(EventArgs e) {
            if (Parent != null) {
                Parent.Invalidate(Bounds, true);
            }
            base.OnBackColorChanged(e);
        }

        protected override void OnParentBackColorChanged(EventArgs e) {
            Invalidate();
            base.OnParentBackColorChanged(e);
        }
        #endregion

        #region Scroll Events

        private void HorizontalScrollbarScroll(object sender, ScrollEventArgs e) {
            AutoScrollPosition = new Point(e.NewValue, _verticalScrollbar.Value);
            UpdateScrollBarPositions();
        }

        private void VerticalScrollbarScroll(object sender, ScrollEventArgs e) {
            AutoScrollPosition = new Point(_horizontalScrollbar.Value, e.NewValue);
            UpdateScrollBarPositions();
        }

        #endregion

        #region Overridden Methods
        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);
            if (!AutoScroll) return;

            _verticalScrollbar.Value = VerticalScroll.Value;
            _horizontalScrollbar.Value = HorizontalScroll.Value;
        }

        [SecuritySafeCritical]
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);

            if (!DesignMode && AutoScroll) {
                WinApi.ShowScrollBar(Handle, (int)WinApi.ScrollBar.SB_BOTH, 0);
            }
        }

        #endregion

        #region Management Methods
        private void UpdateScrollBarPositions() {
            if (DesignMode) {
                return;
            }

            if (!AutoScroll) {
                _verticalScrollbar.Visible = false;
                _horizontalScrollbar.Visible = false;
                return;
            }

            _verticalScrollbar.Location = new Point(ClientRectangle.Width - _verticalScrollbar.Width, ClientRectangle.Y);
            _verticalScrollbar.Height = ClientRectangle.Height;

            if (!VerticalScrollbar) {
                _verticalScrollbar.Visible = false;
            }

            _horizontalScrollbar.Location = new Point(ClientRectangle.X, ClientRectangle.Height - _horizontalScrollbar.Height);
            _horizontalScrollbar.Width = ClientRectangle.Width;

            if (!HorizontalScrollbar) {
                _horizontalScrollbar.Visible = false;
            }
        }

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
