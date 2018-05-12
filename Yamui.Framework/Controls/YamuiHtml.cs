#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlLabel.cs) is part of YamuiFramework.
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
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Yamui.Framework.Helper;
using Yamui.Framework.HtmlRenderer.Core.Adapters.Entities;
using Yamui.Framework.HtmlRenderer.Core.Core.Entities;
using Yamui.Framework.HtmlRenderer.Core.Core.Utils;
using Yamui.Framework.HtmlRenderer.WinForms;
using Yamui.Framework.HtmlRenderer.WinForms.Adapters;
using Yamui.Framework.HtmlRenderer.WinForms.Utilities;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WinForms control that will render html content in it's client rectangle.<br/>
    /// Using <see cref="AutoSize"/> and <see cref="AutoSizeHeightOnly"/> client can control how the html content effects the
    /// size of the label. Either case scrollbars are never shown and html content outside of client bounds will be clipped.
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// <para>
    /// The major differential to use HtmlPanel or HtmlLabel is size and scrollbars.<br/>
    /// If the size of the control depends on the html content the HtmlLabel should be used.<br/>
    /// If the size is set by some kind of layout then HtmlPanel is more suitable, also shows scrollbars if the html contents is larger than the control client rectangle.<br/>
    /// </para>
    /// <para>
    /// <h4>AutoSize:</h4>
    /// <u>AutoSize = AutoSizeHeightOnly = false</u><br/>
    /// The label size will not change by the html content. MaximumSize and MinimumSize are ignored.<br/>
    /// <br/>
    /// <u>AutoSize = true</u><br/>
    /// The width and height is adjustable by the html content, the width will be longest line in the html, MaximumSize.Width will restrict it but it can be lower than that.<br/>
    /// <br/>
    /// <u>AutoSizeHeightOnly = true</u><br/>
    /// The width of the label is set and will not change by the content, the height is adjustable by the html content with restrictions to the MaximumSize.Height and MinimumSize.Height values.<br/>
    /// </para>
    /// <para>
    /// <h4>LinkClicked event</h4>
    /// Raised when the user clicks on a link in the html.<br/>
    /// Allows canceling the execution of the link.
    /// </para>
    /// <para>
    /// <h4>StylesheetLoad event:</h4>
    /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
    /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </para>
    /// <para>
    /// <h4>ImageLoad event:</h4>
    /// Raised when an image is about to be loaded by file path or URI.<br/>
    /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
    /// </para>
    /// <para>
    /// <h4>RenderError event:</h4>
    /// Raised when an error occurred during html rendering.<br/>
    /// </para>
    /// </summary>
    [Designer(typeof(YamuiHtmlLabelDesigner))]
    public class YamuiHtml : YamuiScrollControl {

        #region Fields and Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        protected HtmlContainer _htmlContainer;

        protected bool _autoSizeHeight;
        protected bool _autoSize = true;
     
        /// <summary>
        /// The last position of the scrollbars to know if it has changed to update mouse
        /// </summary>
        protected Point _lastScrollOffset;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// Raised when the user clicks on a box with the attribute "clickable"
        /// </summary>
        public event EventHandler<BoxClickedEventArgs> BoxClicked;

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError;

        /// <summary>
        /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;

        #endregion

        /// <summary>
        /// Creates a new HTML Label
        /// </summary>
        public YamuiHtml() {
            SuspendLayout();

            _htmlContainer = new HtmlContainer {
                AvoidImagesLateLoading = true,
            };
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.BoxClicked += OnBoxClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;
            _htmlContainer.ScrollChange += OnScrollChange;

            ResumeLayout(false);

            base.AutoSize = _autoSize;
            TabStop = false;

            // subscribe to an event called when the BaseCss sheet changes
            YamuiThemeManager.OnCssChanged += YamuiThemeManagerOnOnCssChanged;
        }

        private void YamuiThemeManagerOnOnCssChanged() {
            Text = base.Text;
        }

        /// <summary>
        /// Release the html container resources.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (_htmlContainer != null) {
                _htmlContainer.LinkClicked -= OnLinkClicked;
                _htmlContainer.BoxClicked -= OnBoxClicked;
                _htmlContainer.RenderError -= OnRenderError;
                _htmlContainer.Refresh -= OnRefresh;
                _htmlContainer.StylesheetLoad -= OnStylesheetLoad;
                _htmlContainer.ImageLoad -= OnImageLoad;
                _htmlContainer.ScrollChange -= OnScrollChange;
                _htmlContainer.Dispose();
                _htmlContainer = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets or sets a value indicating if anti-aliasing should be avoided for geometry like backgrounds and borders (default - false).
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("If anti-aliasing should be avoided for geometry like backgrounds and borders")]
        public bool AvoidGeometryAntialias {
            get { return _htmlContainer.AvoidGeometryAntialias; }
            set { _htmlContainer.AvoidGeometryAntialias = value; }
        }

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.<br/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// GDI+ text rendering is less smooth than GDI text rendering but it natively supports alpha channel
        /// thus allows creating transparent images.
        /// </para>
        /// <para>
        /// While using GDI+ text rendering you can control the text rendering using <see cref="Graphics.TextRenderingHint"/>, note that
        /// using <see cref="System.Drawing.Text.TextRenderingHint.ClearTypeGridFit"/> doesn't work well with transparent background.
        /// </para>
        /// </remarks>
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(false)]
        [Description("If to use GDI+ text rendering to measure/draw text, false - use GDI")]
        public bool UseGdiPlusTextRendering {
            get { return _htmlContainer.UseGdiPlusTextRendering; }
            set { _htmlContainer.UseGdiPlusTextRendering = value; }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Is content selection is enabled for the rendered html.")]
        public bool IsSelectionEnabled {
            get { return _htmlContainer.IsSelectionEnabled; }
            set { _htmlContainer.IsSelectionEnabled = value; }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Is the build-in context menu enabled and will be shown on mouse right click.")]
        public bool IsContextMenuEnabled {
            get { return _htmlContainer.IsContextMenuEnabled; }
            set { _htmlContainer.IsContextMenuEnabled = value; }
        }
        
        /// <summary>
        /// Automatically sets the size of the label by content size
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Automatically sets the size of the label by content size.")]
        public override bool AutoSize {
            get { return _autoSize; }
            set {
                if (_autoSize != value) {
                    _autoSize = value;
                    base.AutoSize = _autoSize;
                    if (value) {
                        _autoSizeHeight = false;
                        RefreshLayout();
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Automatically sets the height of the label by content height (width is not effected).
        /// </summary>
        [Browsable(true)]
        [Category("Layout")]
        [Description("Automatically sets the height of the label by content height (width is not effected)")]
        public bool AutoSizeHeightOnly {
            get { return _autoSizeHeight; }
            set {
                if (_autoSizeHeight != value) {
                    _autoSizeHeight = value;
                    if (value) {
                        _autoSize = false;
                        RefreshLayout();
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the html of this control.
        /// </summary>
        [Description("Sets the html of this control.")]
        public override string Text {
            get { return base.Text; }
            set {
                base.Text = value ?? string.Empty;
                if (!IsDisposed) {
                    SetText(base.Text);
                    if (AutoSize || AutoSizeHeightOnly) {
                        var newSize = GetControlSizeFromContentSize(GetContentNaturalSize());
                        if (AutoSizeHeightOnly) {
                            newSize.Width = Size.Width;
                        }
                        if (Size != newSize) {         
                            Size = newSize;
                        }
                    } else {
                        RefreshLayout();
                    }
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        [Browsable(false)]
        public virtual string SelectedText => _htmlContainer.SelectedText;

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        [Browsable(false)]
        public virtual string SelectedHtml => _htmlContainer.SelectedHtml;

        /// <summary>
        /// Get html from the current DOM tree with inline style.
        /// </summary>
        /// <returns>generated html</returns>
        public virtual string GetHtml => _htmlContainer.GetHtml();

        /// <summary>
        /// Get the rectangle of html element as calculated by html layout.<br/>
        /// Element if found by id (id attribute on the html element).<br/>
        /// Note: to get the screen rectangle you need to adjust by the hosting control.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to get its rectangle</param>
        /// <returns>the rectangle of the element or null if not found</returns>
        public RectangleF? GetElementRectangle(string elementId) => _htmlContainer.GetElementRectangle(elementId);
        
        /// <summary>
        /// adapts width to content (the label needs to be in AutoSizeHeight only)
        /// </summary>
        public void SetNeededSize(string content, int minWidth, int maxWidth, bool dontSquareIt = false) {

            SetText(content);

            // this should retrieve the best width, however it will not be the case if there are some width='100%' for instance
            //var calcWidth = MeasureContentSize(new RSize(9999, 0)).Width;
            var calcWidth = MeasureContentSize(RSize.Empty).Width;

            if (calcWidth >= 9999) {
                // get the minimum size required to display everything
                minWidth = MeasureContentSize(new RSize(10, 0)).Width.Clamp(minWidth, maxWidth);

                // set to max Width, get the height at max Width
                var sizef = MeasureContentSize(new RSize(maxWidth, 0));
                var prefHeight = sizef.Height;

                // now we got the final height, resize width until height changes
                int j = 0;
                int detla = maxWidth/2;
                calcWidth = maxWidth;
                do {
                    calcWidth -= detla;
                    calcWidth = calcWidth.Clamp(minWidth, maxWidth);

                    sizef = MeasureContentSize(new RSize(calcWidth, 0));

                    if (sizef.Height > prefHeight) {
                        calcWidth += detla;
                        detla /= 2;
                    }

                    if (calcWidth == maxWidth || calcWidth == minWidth)
                        break;

                    j++;
                } while (j < 6);
            }

            Width = GetControlSizeFromContentSize(new Size(calcWidth.Clamp(minWidth, maxWidth), 0)).Width;
            
            // make it more square shaped if possible
            if (!dontSquareIt && Width > Height) {
                Width = ((int) Math.Sqrt(Width*Height)).Clamp(minWidth, maxWidth);
            }
        }

        
        protected Size MeasureContentSize(RSize maxSizeConstraint) {
            Size contentSize;
            using (Graphics g = CreateGraphics()) {
                using (var ga = new GraphicsAdapter(g, _htmlContainer.UseGdiPlusTextRendering)) {
                    _htmlContainer.HtmlContainerInt.MaxSize = maxSizeConstraint;
                    _htmlContainer.HtmlContainerInt.PerformLayout(ga);
                    contentSize = Utils.ConvertRound(_htmlContainer.HtmlContainerInt.ActualSize);
                    contentSize.Width += Padding.Horizontal;
                    contentSize.Height += Padding.Vertical;
                }
            }
            return contentSize;
        }
        
        #region Private methods
        
        private void SetText(string text) {
            _htmlContainer.SetHtml(text.StartsWith(@"<html") ? text : @"<html><body>" + text + @"</body><html>", YamuiThemeManager.CurrentThemeCss);
        }

        /// <summary>
        /// Adjust the scrollbar of the panel on html element by the given id.<br/>
        /// The top of the html element rectangle will be at the top of the panel, if there
        /// is not enough height to scroll to the top the scroll will be at maximum.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to scroll to</param>
        public void ScrollToElement(string elementId) {
            ArgChecker.AssertArgNotNullOrEmpty(elementId, "elementId");

            if (_htmlContainer != null) {
                var rect = _htmlContainer.GetElementRectangle(elementId);
                if (rect.HasValue) {
                    AutoScrollPosition = Point.Round(rect.Value.Location);
                    _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons, 0, MousePosition.X, MousePosition.Y, 0));
                }
            }
        }
        

        /// <summary>
        /// call mouse move to handle paint after scroll or html change affecting mouse cursor.
        /// </summary>
        private void InvokeMouseMove() {
            var mp = PointToClient(MousePosition);
            _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0));
        }
        
        /// <summary>
        /// Will return the natural size of the html while trying to constraint its width if AutoSizeHeightOnly or not AutoSize
        /// </summary>
        /// <returns></returns>
        protected override Size GetContentNaturalSize() {
            var naturalSize = MeasureContentSize(AutoSizeHeightOnly || !AutoSize ? new RSize(ContentRectangle.Width - Padding.Horizontal, 0) : new RSize(0, 0));
            if ((AutoSizeHeightOnly || !AutoSize) && naturalSize.Width > ContentRectangle.Width - Padding.Horizontal) {
                // we are in a case were the html contains an unbreakable line and thus the needed width is superior to the available width
                // horizontal scrollbar will be displayed. We compute the natural size from the minimum width so that wrappable lines
                // dont look too squished compared to the unbreakable line...
                // UNBREAKABLELINEISHUGE
                // other
                // lines 
                // would 
                // look
                // like
                // this
                // ----- <- this is ContentRectangle.Width - Padding.Horizontal
                // --------------------- <- instead we give this minimum to all lines, which is the width of the unbreakable line
                naturalSize = MeasureContentSize(AutoSizeHeightOnly || !AutoSize ? new RSize(naturalSize.Width, 0) : new RSize(0, 0));
            }
            return naturalSize;
        }
        
        protected override void PaintContent(PaintEventArgs e) {
            base.PaintContent(e);
            if (_htmlContainer != null) {
                _htmlContainer.ScrollOffset = new Point(ContentRectangle.Left + Padding.Left - HorizontalScroll.Value, ContentRectangle.Top + Padding.Top - VerticalScroll.Value);
                var clipRect = new Rectangle(ContentRectangle.X, ContentRectangle.Y, ContentRectangle.Width, ContentRectangle.Height);
                clipRect.Intersect(e.ClipRectangle);
                e.Graphics.SetClip(clipRect);
                _htmlContainer.PerformPaint(e.Graphics);
                e.Graphics.SetClip(e.ClipRectangle);

                if (!_lastScrollOffset.Equals(_htmlContainer.ScrollOffset)) {
                    _lastScrollOffset = _htmlContainer.ScrollOffset;
                    InvokeMouseMove();
                }
            }
        }

        /// <summary>
        /// On html renderer scroll request adjust the scrolling of the panel to the requested location.
        /// </summary>
        protected virtual void OnScrollChange(HtmlScrollEventArgs e) {
            AutoScrollPosition = new Point((int) e.X, (int) e.Y);
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection. 
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (ContentRectangle.Contains(e.Location)) {
                _htmlContainer?.HandleMouseMove(this, e);
            } else {
                Cursor = DefaultCursor;
            }
        }

        /// <summary>
        /// Handle mouse down to handle selection. 
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            if (ContentRectangle.Contains(e.Location)) {
                _htmlContainer?.HandleMouseDown(this, e);
            }
        }

        /// <summary>
        /// Handle mouse leave to handle cursor change.
        /// </summary>
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            _htmlContainer?.HandleMouseLeave(this);
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click. 
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e) {
            if (ContentRectangle.Contains(e.Location)) {
                OnMouseClick(e);
            }
            _htmlContainer?.HandleMouseUp(this, e);
            base.OnMouseUp(e);
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse. 
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            base.OnMouseDoubleClick(e);
            if (ContentRectangle.Contains(e.Location)) {
                _htmlContainer?.HandleMouseDoubleClick(this, e);
            }
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e) {
            LinkClicked?.Invoke(this, e);
        }

        /// <summary>
        /// Propagate the Render Error event from root container.
        /// </summary>
        protected virtual void OnRenderError(HtmlRenderErrorEventArgs e) {
            RenderError?.Invoke(this, e);
        }

        /// <summary>
        /// Propagate the stylesheet load event from root container.
        /// </summary>
        protected virtual void OnStylesheetLoad(HtmlStylesheetLoadEventArgs e) {
            StylesheetLoad?.Invoke(this, e);
        }

        /// <summary>
        /// Propagate the image load event from root container.
        /// </summary>
        protected virtual void OnImageLoad(HtmlImageLoadEventArgs e) {
            ImageLoad?.Invoke(this, e);
            if (!e.Handled)
                YamuiThemeManager.GetHtmlImages(e);
        }

        /// <summary>
        /// Handle html renderer invalidate and re-layout as requested.
        /// </summary>
        protected virtual void OnRefresh(HtmlRefreshEventArgs e) {
            if (e.Layout) {
                RefreshLayout();
            }
            Invalidate(ContentRectangle);
        }

        /// <summary>
        /// Propagate the BoxClicked event from root container.
        /// </summary>
        protected virtual void OnBoxClicked(BoxClickedEventArgs e) {
            BoxClicked?.Invoke(this, e);
        }

        #region Private event handlers

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e) {
            OnLinkClicked(e);
        }

        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e) {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => OnRenderError(e)));
            else
                OnRenderError(e);
        }

        private void OnStylesheetLoad(object sender, HtmlStylesheetLoadEventArgs e) {
            OnStylesheetLoad(e);
        }

        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e) {
            OnImageLoad(e);
        }

        private void OnRefresh(object sender, HtmlRefreshEventArgs e) {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => OnRefresh(e)));
            else
                OnRefresh(e);
        }

        private void OnBoxClicked(object sender, BoxClickedEventArgs e) {
            OnBoxClicked(e);
        }

        private void OnScrollChange(object sender, HtmlScrollEventArgs e) {
            OnScrollChange(e);
        }

        #endregion

        #endregion
    }

    #region YamuiHtmlLabelDesigner

    internal class YamuiHtmlLabelDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("Font");
            properties.Remove("ForeColor");
            properties.Remove("BackColor");
            properties.Remove("AllowDrop");
            properties.Remove("RightToLeft");
            properties.Remove("Cursor");
            properties.Remove("UseWaitCursor");
            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("CausesValidation");
            properties.Remove("ContextMenuStrip");
            properties.Remove("ImeMode");
            base.PreFilterProperties(properties);
        }
    }

    #endregion

}