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
                if (_hScroll != value) {
                    _hScroll = value;
                    HorizontalScroll.Enabled = _hScroll;
                    RefreshLayout();
                }
            }
        }

        /// <summary>
        /// Can this control have vertical scroll?
        /// </summary>
        [Category(nameof(Yamui))]
        public bool VScroll {
            get { return _vScroll; }
            set {
                if (_vScroll != value) {
                    _vScroll = value;
                    VerticalScroll.Enabled = _vScroll;
                    RefreshLayout();
                }
            }
        }

        /// <summary>
        /// Can this control have scrolls?
        /// </summary>
        [Category(nameof(Yamui))]
        public bool AutoScroll {
            get { return _vScroll || _hScroll; }
            set {
                if ((value && (!_vScroll || !_hScroll)) || (!value && (_vScroll || _hScroll))) {
                    _vScroll = value;
                    _hScroll = value;
                    VerticalScroll.Enabled = _vScroll;
                    HorizontalScroll.Enabled = _hScroll;
                    RefreshLayout();
                }
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

        [Browsable(false)]
        public virtual bool HasBorder {
            get { return _hasBorder; }
            set {
                if (_hasBorder != value) {
                    _hasBorder = value;
                    VerticalScroll.Offset = new Point(BorderPadding, BorderPadding);
                    HorizontalScroll.Offset = new Point(BorderPadding, BorderPadding);
                    // this will change the effective size available for this control
                    OnSizeChanged(Size);
                    InvalidateBorder();
                }
            }
        }

        /// <summary>
        /// Get the size we would need to draw the whole content without scrollbars
        /// </summary>
        [Browsable(false)]
        protected virtual Size ContentNaturalSize { get; } = Size.Empty;

        /// <summary>
        /// The surface that is really available to draw the content (<see cref="ContentSurfaceWithScrolls"/> minus the scrollbars)
        /// </summary>
        /// <remarks>If this surface is superior or equals to <see cref="ContentNaturalSize"/> then no scrollbars are needed</remarks>
        [Browsable(false)]
        public Rectangle ContentSurface { get; private set; }

        /// <summary>
        /// The rectangle within the borders of this control (equals to the <see cref="WidgetSurface"/> if there are no borders)
        /// </summary>
        /// <remarks>This is also equivalent to the <see cref="ContentSurface"/> if there are no scrollbars shown</remarks>
        [Browsable(false)]
        public Rectangle ContentSurfaceWithScrolls { get; private set; }

        /// <summary>
        /// Represents the whole widget surface (=area), the position is relative and thus will always be (0,0)
        /// </summary>
        [Browsable(false)]
        public Rectangle WidgetSurface { get; private set; }


        [Browsable(false)]
        public virtual Color BorderColor => YamuiThemeManager.Current.AccentColor;

        [Browsable(false)]
        public override Color BackColor => Color.Yellow; // YamuiThemeManager.Current.FormBack;

        private int BorderPadding => (HasBorder ? BorderWidth : 0);

        #endregion

        #region Fields and Consts

        public const int BorderWidth = 1;
        private int _scrollBarWidth = 12;
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

        protected bool RectangleIntersectsWithBorder(Rectangle rectangle) {
            return rectangle.X < BorderWidth || 
                   rectangle.Y < BorderWidth || 
                   rectangle.Right > WidgetSurface.Width - BorderWidth || 
                   rectangle.Bottom > WidgetSurface.Height - BorderWidth;
        }

        protected override void OnPaint(PaintEventArgs e) {
            // I could use a Graphics.Save() / Restore() but I figure storing only the Clip is more efficient...
            Region originalRegion = e.Graphics.Clip.Clone();
            if (HasBorder && RectangleIntersectsWithBorder(e.ClipRectangle)) {
                e.Graphics.SetClip(ContentSurfaceWithScrolls, CombineMode.Exclude);
                PaintBorderRegion(e.Graphics);
                e.Graphics.Clip = originalRegion;
            }
            
            // to be more precise, we could use originalRegion.IsVisible(ContentSurface) here, but the dotnet framework
            // doesn't actually call GetUpdateRgn to get the real update region. Instead, we just get e.ClipRectangle that is the
            // bounds of the update region and the e.Graphics.Clip is equals to this ClipRectangle
            // So... TLDR : we will always get a rectangle to update, never a complex region
            if (ContentSurface.IntersectsWith(e.ClipRectangle)) {
                e.Graphics.SetClip(ContentSurface, CombineMode.Intersect);
                PaintContentSurface(e.Graphics);
                e.Graphics.Clip = originalRegion;
            }

            PaintScrollBars(e, originalRegion);
        }

        protected virtual void PaintBorderRegion(Graphics g) {
            g.PaintClipRegion(BorderColor);
        }

        protected virtual void PaintContentSurface(Graphics g) {
            g.PaintClipRegion(BackColor);
        }

        protected virtual void PaintScrollBars(PaintEventArgs e, Region originalRegion) {
            if (VerticalScroll.HasScroll && VerticalScroll.BarRect.IntersectsWith(e.ClipRectangle)) {
                e.Graphics.SetClip(VerticalScroll.BarRect, CombineMode.Intersect);
                VerticalScroll.Paint(e.Graphics);
                e.Graphics.Clip = originalRegion;
            }

            if (_needBothScroll && _leftoverBar.IntersectsWith(e.ClipRectangle)) {
                e.Graphics.SetClip(_leftoverBar, CombineMode.Intersect);
                e.Graphics.PaintClipRegion(YamuiThemeManager.Current.ScrollNormalBack);
                e.Graphics.Clip = originalRegion;
            }

            if (HorizontalScroll.HasScroll && HorizontalScroll.BarRect.IntersectsWith(e.ClipRectangle)) {
                e.Graphics.SetClip(HorizontalScroll.BarRect, CombineMode.Intersect);
                HorizontalScroll.Paint(e.Graphics);
                e.Graphics.Clip = originalRegion;
            }
        }

        #endregion

        #region ScrollHandler events

        protected virtual void OnScrollValueChanged(object sender, YamuiScrollHandlerValueChangedEventArgs e) {
            InvalidateContent();
        }

        protected virtual void OnScrollbarsRedrawNeeded(object sender, EventArgs eventArgs) {
            if (((YamuiScrollHandler) sender).IsVertical) {
                InvalidateVerticalScrollbar();
            } else {
                InvalidateHorizontalScrollbar();
            }
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
            MouseLDownClientArea(e);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            MouseLUpClientArea(e);
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            MouseMoveClientArea(e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            MouseLeaveClientArea(e);
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
                        if (!nccsp.rectProposed.Size.IsEmpty) {
                            // we are not in this case when the form is minimized
                            //OnSizeChanged(nccsp.rectProposed.Size);
                        }

                        AdjustClientArea(ref nccsp.rectProposed);
                        Marshal.StructureToPtr(nccsp, m.LParam, true);
                    } else {
                        // When FALSE, LPARAM Points to a RECT structure
                        var clnRect = (WinApi.RECT) Marshal.PtrToStructure(m.LParam, typeof(WinApi.RECT));
                        if (!clnRect.Size.IsEmpty) {
                            // we are not in this case when the form is minimized
                            //OnSizeChanged(clnRect.Size);
                        }

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
                case Window.Msg.WM_NCPAINT:
                    m.Result = IntPtr.Zero;
                    break;

                case Window.Msg.WM_ERASEBKGND:
                    m.Result = IntPtr.Zero;
                    return;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            OnSizeChanged(Size);
        }

        #endregion

        #region Methods

        public void InvalidateContent() {
            Invalidate(ContentSurface);
        }

        public void InvalidateBorder() {
            // TODO : here we probably want to custom paint only the border outside of the paint event
            // because we can't invalidate a complex region due to the limitation of the dotnet paint event
            var borderRegion = new Region(WidgetSurface);
            borderRegion.Exclude(ContentSurfaceWithScrolls);
            Invalidate(borderRegion);
        }

        public void InvalidateVerticalScrollbar() {
            Invalidate(_needBothScroll ? Rectangle.Union(VerticalScroll.BarRect, _leftoverBar) : VerticalScroll.BarRect);
        }

        public void InvalidateHorizontalScrollbar() {
            Invalidate(HorizontalScroll.BarRect);
        }
        
        /// <summary>
        /// Modify the input rectangle to adjust the client area from the window size
        /// </summary>
        /// <param name="rect"></param>
        protected virtual void AdjustClientArea(ref WinApi.RECT rect) {
            // the whole control is a client area
        }

        protected virtual void MouseMoveClientArea(MouseEventArgs mouseEventArgs) {
            VerticalScroll.HandleMouseMove(null, null);
            HorizontalScroll.HandleMouseMove(null, null);
        }

        protected virtual void MouseLDownClientArea(MouseEventArgs mouseEventArgs) {
            Focus();
            VerticalScroll.HandleMouseDown(null, null);
            HorizontalScroll.HandleMouseDown(null, null);
        }

        protected virtual void MouseLUpClientArea(MouseEventArgs mouseEventArgs) {
            VerticalScroll.HandleMouseUp(null, null);
            HorizontalScroll.HandleMouseUp(null, null);
        }

        protected virtual void MouseLeaveClientArea(EventArgs eventArgs) {
            VerticalScroll.HandleMouseLeave(null, null);
            HorizontalScroll.HandleMouseLeave(null, null);
        }

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            e.Handled = HorizontalScroll.HandleKeyDown(null, e) || VerticalScroll.HandleKeyDown(null, e);
            return e.Handled;
        }

        /// <summary>
        /// Should be called when the size of this control changes
        /// </summary>
        /// <param name="newSize"></param>
        protected virtual void OnSizeChanged(Size newSize) {
            // the (HasBorder && BorderWidth == 1 ? BorderWidth : 0) is there to compensate for an issue in dotnet that will never be fixed
            WidgetSurface = new Rectangle(0, 0, newSize.Width, newSize.Height);
            ContentSurfaceWithScrolls = new Rectangle(BorderPadding, BorderPadding, newSize.Width - 2 * BorderPadding, newSize.Height - 2 * BorderPadding);

            // we set the contentsurface taking as an hypothesis that the scrollbars will remain as they are, this is corrected
            ContentSurface = new Rectangle(ContentSurfaceWithScrolls.X, ContentSurfaceWithScrolls.Y, ContentSurfaceWithScrolls.Width - (VerticalScroll.HasScroll ? VerticalScroll.BarThickness : 0), ContentSurfaceWithScrolls.Height - (HorizontalScroll.HasScroll ? HorizontalScroll.BarThickness : 0));
            RefreshLayout();
        }


        /// <summary>
        /// Compute the layout of the control, ths position of each component of this control (for instance the scrollbars size/position)
        /// Called when the content changes or when the size of the control changes
        /// </summary>
        public virtual void RefreshLayout() {
            var initHasVerticalScroll = VerticalScroll.HasScroll;
            ComputeScrollbars(ContentNaturalSize, ContentSurfaceWithScrolls.Size);

            // as the display of the vertical scroll can reduce the available content width, maybe the content size 
            // has changed so we need to recompute the correct values for the scrollbars
            if (initHasVerticalScroll != VerticalScroll.HasScroll) {
                ComputeScrollbars(ContentNaturalSize, ContentSurfaceWithScrolls.Size);
            }
        }

        /// <summary>
        /// Translate a content size to control size taking into account stuff like scrollbars and border
        /// </summary>
        /// <param name="contentSize"></param>
        /// <returns></returns>
        protected Size GetControlSizeFromContentSize(Size contentSize) {
            contentSize.Width += (Size.Width - ContentSurface.Width);
            contentSize.Height += (Size.Height - ContentSurface.Height);
            return contentSize;
        }

        protected void ComputeScrollbars(Size naturalSize, Size availableSize) {
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
                _leftoverBar = new Rectangle(ContentSurfaceWithScrolls.X + ContentSurfaceWithScrolls.Width - VerticalScroll.BarThickness, ContentSurfaceWithScrolls.Y + ContentSurfaceWithScrolls.Height - HorizontalScroll.BarThickness, VerticalScroll.BarThickness, HorizontalScroll.BarThickness);
            }

            ContentSurface = new Rectangle(ContentSurfaceWithScrolls.X, ContentSurfaceWithScrolls.Y, ContentSurfaceWithScrolls.Width - (needVerticalScroll ? VerticalScroll.BarThickness : 0), ContentSurfaceWithScrolls.Height - (needHorizontalScroll ? HorizontalScroll.BarThickness : 0));
        }

        #endregion
    }
}