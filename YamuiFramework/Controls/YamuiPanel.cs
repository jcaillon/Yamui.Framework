using System;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Native;

namespace YamuiFramework.Controls
{
    [ToolboxBitmap(typeof(Panel))]
    public class YamuiPanel : Panel
    {
        #region Fields

        private YamuiScrollBar _verticalScrollbar = new YamuiScrollBar(ScrollOrientation.Vertical);
        private YamuiScrollBar _horizontalScrollbar = new YamuiScrollBar(ScrollOrientation.Horizontal);

        private bool _showHorizontalScrollbar;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool HorizontalScrollbar
        {
            get { return _showHorizontalScrollbar; }
            set { _showHorizontalScrollbar = value; }
        }

        [Category("Yamui")]
        public int HorizontalScrollbarSize
        {
            get { return _horizontalScrollbar.ScrollbarSize; }
            set { _horizontalScrollbar.ScrollbarSize = value; }
        }

        [Category("Yamui")]
        public bool HorizontalScrollbarBarColor
        {
            get { return _horizontalScrollbar.UseBarColor; }
            set { _horizontalScrollbar.UseBarColor = value; }
        }

        [Category("Yamui")]
        public bool HorizontalScrollbarHighlightOnWheel
        {
            get { return _horizontalScrollbar.HighlightOnWheel; }
            set { _horizontalScrollbar.HighlightOnWheel = value; }
        }

        private bool _showVerticalScrollbar;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool VerticalScrollbar
        {
            get { return _showVerticalScrollbar; }
            set { _showVerticalScrollbar = value; }
        }

        [Category("Yamui")]
        public int VerticalScrollbarSize
        {
            get { return _verticalScrollbar.ScrollbarSize; }
            set { _verticalScrollbar.ScrollbarSize = value; }
        }

        [Category("Yamui")]
        public bool VerticalScrollbarBarColor
        {
            get { return _verticalScrollbar.UseBarColor; }
            set { _verticalScrollbar.UseBarColor = value; }
        }

        [Category("Yamui")]
        public bool VerticalScrollbarHighlightOnWheel
        {
            get { return _verticalScrollbar.HighlightOnWheel; }
            set { _verticalScrollbar.HighlightOnWheel = value; }
        }

        [Category("Yamui")]
        public new bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                _showHorizontalScrollbar = value;
                _showVerticalScrollbar = value;

                base.AutoScroll = value;
            }
        }

        #endregion

        #region Constructor

        public YamuiPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Controls.Add(_verticalScrollbar);
            Controls.Add(_horizontalScrollbar);

            _verticalScrollbar.UseBarColor = true;
            _horizontalScrollbar.UseBarColor = true;

            _verticalScrollbar.Visible = false;
            _horizontalScrollbar.Visible = false;

            _verticalScrollbar.Scroll += VerticalScrollbarScroll;
            _horizontalScrollbar.Scroll += HorizontalScrollbarScroll;
        }

        #endregion

        #region Scroll Events

        private void HorizontalScrollbarScroll(object sender, ScrollEventArgs e)
        {
            AutoScrollPosition = new Point(e.NewValue, _verticalScrollbar.Value);
            UpdateScrollBarPositions();
        }

        private void VerticalScrollbarScroll(object sender, ScrollEventArgs e)
        {
            AutoScrollPosition = new Point(_horizontalScrollbar.Value, e.NewValue);
            UpdateScrollBarPositions();
        }

        #endregion

        #region Overridden Methods

        protected override void OnPaintBackground(PaintEventArgs e) {
            try {
                e.Graphics.Clear(ThemeManager.FormColor.BackColor());
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
            if (DesignMode)
            {
                _horizontalScrollbar.Visible = false;
                _verticalScrollbar.Visible = false;
                return;
            }

            UpdateScrollBarPositions();

            if (HorizontalScrollbar)
            {
                _horizontalScrollbar.Visible = HorizontalScroll.Visible;
            }
            if (HorizontalScroll.Visible)
            {
                _horizontalScrollbar.Minimum = HorizontalScroll.Minimum;
                _horizontalScrollbar.Maximum = HorizontalScroll.Maximum;
                _horizontalScrollbar.SmallChange = HorizontalScroll.SmallChange;
                _horizontalScrollbar.LargeChange = HorizontalScroll.LargeChange;
            }

            if (VerticalScrollbar)
            {
                _verticalScrollbar.Visible = VerticalScroll.Visible;
            }
            if (VerticalScroll.Visible)
            {
                _verticalScrollbar.Minimum = VerticalScroll.Minimum;
                _verticalScrollbar.Maximum = VerticalScroll.Maximum;
                _verticalScrollbar.SmallChange = VerticalScroll.SmallChange;
                _verticalScrollbar.LargeChange = VerticalScroll.LargeChange;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            _verticalScrollbar.Value = Math.Abs(VerticalScroll.Value);
            _horizontalScrollbar.Value = Math.Abs(HorizontalScroll.Value);
        }

        [SecuritySafeCritical]
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (!DesignMode)
            {
                WinApi.ShowScrollBar(Handle, (int)WinApi.ScrollBar.SB_BOTH, 0);
            }
        }

        #endregion

        #region Management Methods

        private void UpdateScrollBarPositions()
        {
            if (DesignMode)
            {
                return;
            }

            if (!AutoScroll)
            {
                _verticalScrollbar.Visible = false;
                _horizontalScrollbar.Visible = false;
                return;
            }

            _verticalScrollbar.Location = new Point(ClientRectangle.Width - _verticalScrollbar.Width, ClientRectangle.Y);
            _verticalScrollbar.Height = ClientRectangle.Height - _horizontalScrollbar.Height;

            if (!VerticalScrollbar)
            {
                _verticalScrollbar.Visible = false;
            }

            _horizontalScrollbar.Location = new Point(ClientRectangle.X, ClientRectangle.Height - _horizontalScrollbar.Height);
            _horizontalScrollbar.Width = ClientRectangle.Width - _verticalScrollbar.Width;

            if (!HorizontalScrollbar)
            {
                _horizontalScrollbar.Visible = false;
            }
        }

        #endregion
    }
}
