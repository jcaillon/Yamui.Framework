using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    public class YamuiScrollControl : YamuiControl {

        #region Properties

        /// <summary>
        /// The width that should be used for this scrollbar
        /// </summary>
        [Category(nameof(Yamui))]
        public int ScrollBarWidth {
            get { return _scrollBarWidth; }
            set {
                if (_scrollBarWidth != value) {
                    _scrollBarWidth = value;
                    VerticalScroll.BarThickness = _scrollBarWidth;
                    HorizontalScroll.BarThickness = _scrollBarWidth;
                }
            }
        }

        /// <summary>
        /// Access the vertical scroll
        /// </summary>
        [Browsable(false)]
        public YamuiScrollHandler VerticalScroll { get; }
        
        /// <summary>
        /// Access the horizontal scroll
        /// </summary>
        [Browsable(false)]
        public YamuiScrollHandler HorizontalScroll { get; }

        /// <summary>
        /// Can this control have vertical scroll?
        /// </summary>
        [Category(nameof(Yamui))]
        public bool HScroll {
            get { return _hScroll; }
            set {
                _hScroll = value;
                HorizontalScroll.Enabled = _hScroll;
                PerformLayout();
            }
        }
        
        /// <summary>
        /// Can this control have vertical scroll?
        /// </summary>
        [Category(nameof(Yamui))]
        public bool VScroll {
            get { return _vScroll; }
            set {
                _vScroll = value;
                VerticalScroll.Enabled = _vScroll;
                PerformLayout();
            }
        }
        
        /// <summary>
        /// Can this control have scrolls?
        /// </summary>
        [Category(nameof(Yamui))]
        public bool AutoScroll {
            get { return _vScroll || _hScroll; }
            set {
                _vScroll = value;
                _hScroll = value;
                VerticalScroll.Enabled = _vScroll;
                HorizontalScroll.Enabled = _hScroll;
                PerformLayout();
            }
        }

        /// <summary>
        /// The scroll position, sets the content position to should be shown at the 0,0 location in the client rectangle
        /// </summary>
        [Browsable(false)]
        public Point AutoScrollPosition {
            get { return new Point(HorizontalScroll.HasScroll ? HorizontalScroll.Value : 0, VerticalScroll.HasScroll ? VerticalScroll.Value : 0); }
            set {
                if (HorizontalScroll.HasScroll)
                    HorizontalScroll.Value = value.X;
                if (VerticalScroll.HasScroll)
                    VerticalScroll.Value = value.Y;
            }
        }

        /// <summary>
        /// Sets/get the minimal "natural" size of the content to display, if this size exceeds the size available to display the content
        /// then scrollbars will be activated if possible
        /// </summary>
        [Browsable(false)]
        public Size AutoScrollMinSize {
            get { return new Size(HorizontalScroll.LengthToRepresentMinSize, VerticalScroll.LengthToRepresentMinSize); }
            set {
                HorizontalScroll.LengthToRepresentMinSize = value.Width;
                VerticalScroll.LengthToRepresentMinSize = value.Height;
            }
        }
        
        /// <summary>
        /// True if the control has at least 1 scoll active
        /// </summary>
        [Browsable(false)]
        public bool HasScroll => VerticalScroll.HasScroll || HorizontalScroll.HasScroll;

        /// <summary>
        /// Get prefered size, i.e. the size we would need to display all the child controls at the same time
        /// </summary>
        [Browsable(false)]
        protected Size NaturalSize {
            get {
                _naturalSize = GetNaturalSize();
                return _naturalSize;
            }
        }

        [DefaultValue(false)]
        [Browsable(false)]
        public virtual bool HasBorder {
            get { return _hasBorder; }
            set {
                if (_hasBorder != value) {
                    _hasBorder = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// The rectangle that is really used to display the content
        /// </summary>
        [Browsable(false)]
        public Rectangle ContentRectangle { get; private set; }

        [Browsable(false)]
        public Rectangle BorderRectangle { get; private set; }

        [Browsable(false)]
        public Rectangle NonBorderRectangle { get; private set; }

        [Browsable(false)]
        public virtual Color BorderColor => YamuiThemeManager.Current.AccentColor;

        [Browsable(false)]
        public override Color BackColor => Color.Yellow; // YamuiThemeManager.Current.FormBack;

        private int BorderPadding => (HasBorder ? BorderWidth : 0);

        #endregion
        
        #region Fields and Consts
        
        public const int BorderWidth = 1;   
        private int _scrollBarWidth = 12;
        protected Size _naturalSize;
        private Rectangle _leftoverBar;
        private bool _needBothScroll;
        protected bool _vScroll = true;
        protected bool _hScroll = true;
        private bool _hasBorder;

        #endregion

        #region Life and death

        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        public YamuiScrollControl() {
            TabStop = false;

            VerticalScroll = new YamuiScrollHandler(true, this) {
                SmallChange = 70,
                LargeChange = 400,
                BarThickness = ScrollBarWidth
            };
            HorizontalScroll = new YamuiScrollHandler(false, this) {
                SmallChange = 70,
                LargeChange = 400,
                BarThickness = ScrollBarWidth
            };

            VerticalScroll.OnValueChanged += OnScrollValueChanged;
            VerticalScroll.OnScrollbarsRedrawNeeded += OnScrollbarsRedrawNeeded;
            HorizontalScroll.OnValueChanged += OnScrollValueChanged;
            HorizontalScroll.OnScrollbarsRedrawNeeded += OnScrollbarsRedrawNeeded;
        }

        ~YamuiScrollControl() {
            VerticalScroll.OnValueChanged -= OnScrollValueChanged;
            VerticalScroll.OnScrollbarsRedrawNeeded -= OnScrollbarsRedrawNeeded;
            HorizontalScroll.OnValueChanged -= OnScrollValueChanged;
            HorizontalScroll.OnScrollbarsRedrawNeeded -= OnScrollbarsRedrawNeeded;
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e) {
            if (HasBorder && e.ClipRectangle.Contains(BorderRectangle)) {
                PaintBorder(e);
            }
            if (e.ClipRectangle.Contains(ContentRectangle)) {
                PaintContent(e);
            }           
            PaintScrollBars(e, null);
        }

        protected virtual void PaintBorder(PaintEventArgs e) {
            using (var p = new Pen(BorderColor, BorderWidth) {
                Alignment = PenAlignment.Inset
            }) {
                e.Graphics.DrawRectangle(p, BorderRectangle);
            }
        }

        protected virtual void PaintContent(PaintEventArgs e) {
            // paint background
            using (var b = new SolidBrush(BackColor)) {
                e.Graphics.FillRectangle(b, ContentRectangle);
            }
        }

        protected virtual void PaintScrollBars(PaintEventArgs e, YamuiScrollHandler yamuiScrollHandler) {
            if (e.ClipRectangle.Contains(VerticalScroll.BarRect)) {
                VerticalScroll.Paint(e);
            }
            if (_needBothScroll && e.ClipRectangle.Contains(_leftoverBar)) {
                using (var b = new SolidBrush(YamuiThemeManager.Current.ScrollNormalBack)) {
                    e.Graphics.FillRectangle(b, _leftoverBar);
                }
            }
            if (e.ClipRectangle.Contains(HorizontalScroll.BarRect)) {
                HorizontalScroll.Paint(e);
            }
        }

        #endregion
        
        #region ScrollHandler events

        protected virtual void OnScrollValueChanged(object sender, YamuiScrollHandlerValueChangedEventArgs e) {
            Invalidate(ContentRectangle);
        }
        
        protected virtual void OnScrollbarsRedrawNeeded(object sender, EventArgs eventArgs) {
            var scroll = (YamuiScrollHandler) sender;
            var invalidateRectangle = scroll.BarRect;
            if (_needBothScroll && scroll.IsVertical) {
                invalidateRectangle = Rectangle.Union(invalidateRectangle, _leftoverBar);
            }
            Invalidate(invalidateRectangle);
        }

        #endregion

        #region Base events

        /// <summary>
        /// redirect all input key to keydown
        /// </summary>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }
        
        /// <summary>
        /// Handle keydown
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e) {
            e.Handled = PerformKeyDown(e);
            if (!e.Handled) {
                base.OnKeyDown(e);
            }
        }
        
        protected override void OnMouseDown(MouseEventArgs e) {
            MouseLDownClientArea();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            MouseLUpClientArea();
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            MouseMoveClientArea();
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            MouseLeaveClientArea();
            base.OnMouseLeave(e);
        }

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch ((Window.Msg) m.Msg) {
                case Window.Msg.WM_NCCALCSIZE:

                    // Check WPARAM
                    if (m.WParam != IntPtr.Zero) {
                        // When TRUE, LPARAM Points to a NCCALCSIZE_PARAMS structure
                        var nccsp = (WinApi.NCCALCSIZE_PARAMS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.NCCALCSIZE_PARAMS));
                        OnSizeChanged(_naturalSize, nccsp.rectProposed.Size);
                        AdjustClientArea(ref nccsp.rectProposed);
                        Marshal.StructureToPtr(nccsp, m.LParam, true);
                    } else {
                        // When FALSE, LPARAM Points to a RECT structure
                        var clnRect = (WinApi.RECT) Marshal.PtrToStructure(m.LParam, typeof(WinApi.RECT));
                        OnSizeChanged(_naturalSize, clnRect.Size);
                        AdjustClientArea(ref clnRect);
                        Marshal.StructureToPtr(clnRect, m.LParam, true);
                    }

                    //Return Zero
                    m.Result = IntPtr.Zero;
                    break;

                case Window.Msg.WM_MOUSEWHEEL:
                    if (HasScroll) {
                        // delta negative when scrolling up
                        var delta = (short) (m.WParam.ToInt64() >> 16);
                        var mouseEvent1 = new MouseEventArgs(MouseButtons.None, 0, 0, 0, delta);
                        if (HorizontalScroll.IsHovered) {
                            HorizontalScroll.HandleScroll(null, mouseEvent1);
                        } else {
                            VerticalScroll.HandleScroll(null, mouseEvent1);
                        }
                    } else {
                        // propagate the event
                        base.WndProc(ref m);
                    }
                    break;

                /*
                case Window.Msg.WM_KEYDOWN:
                    // needto override OnPreviewKeyDown or IsInputKey to receive this
                    var key = (Keys) (m.WParam.ToInt64());
                    long context = m.LParam.ToInt64();

                    // on key down
                    if (!context.IsBitSet(31)) {
                        var e = new KeyEventArgs(key);
                        e.Handled = PerformKeyDown(e);
                        if (!e.Handled)
                            base.WndProc(ref m);
                    }

                    break;
                    */

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Modify the input rectangle to adjust the client area from the window size
        /// </summary>
        /// <param name="rect"></param>
        protected virtual void AdjustClientArea(ref WinApi.RECT rect) {
            // the whole control is a client area
        }

        protected virtual void MouseMoveClientArea() {
            VerticalScroll.HandleMouseMove(null, null);
            HorizontalScroll.HandleMouseMove(null, null);
        }

        protected virtual void MouseLDownClientArea() {
            VerticalScroll.HandleMouseDown(null, null);
            HorizontalScroll.HandleMouseDown(null, null);
        }

        protected virtual void MouseLUpClientArea() {
            VerticalScroll.HandleMouseUp(null, null);
            HorizontalScroll.HandleMouseUp(null, null);
        }

        protected virtual void MouseLeaveClientArea() {
            VerticalScroll.HandleMouseLeave(null, null);
            HorizontalScroll.HandleMouseLeave(null, null);
        }

        /// <summary>
        /// Here you should return the size that your control would take if it has an unlimited space available to display itself
        /// </summary>
        /// <remarks>if you specify an empty, the scrollbars will not show since the available space will always be superior to the natural size of the content</remarks>
        /// <returns></returns>
        protected virtual Size GetNaturalSize() {
            return new Size(0, 0);
        }

        /// <summary>
        /// Override, called at the end of initializeComponent() in forms made with the designer
        /// Should be called when your content size (i.e. <see cref="NaturalSize"/>) changes
        /// </summary>
        public new void PerformLayout() {
            OnSizeChanged(NaturalSize, Size);
            base.PerformLayout();
        }
        
        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            e.Handled = HorizontalScroll.HandleKeyDown(null, e) || VerticalScroll.HandleKeyDown(null, e);
            return e.Handled;
        }

        protected void OnSizeChanged(Size naturalSize, Size availableSize) {
            if (availableSize.IsEmpty) {
                // we are in this case when the form is minimized
                return;
            }
            BorderRectangle = new Rectangle(0, 0, availableSize.Width - BorderPadding, availableSize.Height - BorderPadding);
            NonBorderRectangle = new Rectangle(BorderPadding, BorderPadding, availableSize.Width - 2*BorderPadding, availableSize.Height - 2*BorderPadding);


            _needBothScroll = false;
            var needHorizontalScroll = HorizontalScroll.HasScroll;
            var needVerticalScroll = VerticalScroll.UpdateLength(naturalSize.Height, null, availableSize.Height - (needHorizontalScroll ? HorizontalScroll.BarThickness : 0), availableSize.Width);
            HorizontalScroll.UpdateLength(naturalSize.Width, null, availableSize.Width - (needVerticalScroll ? VerticalScroll.BarThickness : 0), availableSize.Height);

            if (needHorizontalScroll != HorizontalScroll.HasScroll) {
                needHorizontalScroll = HorizontalScroll.HasScroll;
                needVerticalScroll = VerticalScroll.UpdateLength(naturalSize.Height, null, availableSize.Height - (needHorizontalScroll ? HorizontalScroll.BarThickness : 0), availableSize.Width);
            }

            // compute the "left over" rectangle on the bottom right between the 2 scrolls
            _needBothScroll = needVerticalScroll && needHorizontalScroll;
            if (_needBothScroll) {
                _leftoverBar = new Rectangle(NonBorderRectangle.X + NonBorderRectangle.Width - VerticalScroll.BarThickness, NonBorderRectangle.Y + NonBorderRectangle.Height - HorizontalScroll.BarThickness, VerticalScroll.BarThickness, HorizontalScroll.BarThickness);
            }

            ContentRectangle = new Rectangle(NonBorderRectangle.X, NonBorderRectangle.Y, NonBorderRectangle.Width - (needVerticalScroll ? VerticalScroll.BarThickness : 0), NonBorderRectangle.Height - (needHorizontalScroll ? HorizontalScroll.BarThickness : 0));
        }

        #endregion

    }
}