#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlPanel.cs) is part of YamuiFramework.
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Yamui.Framework.Controls;
using Yamui.Framework.HtmlRenderer.Core.Core;
using Yamui.Framework.HtmlRenderer.Core.Core.Entities;
using Yamui.Framework.HtmlRenderer.Core.Core.Utils;
using Yamui.Framework.HtmlRenderer.WinForms.Utilities;
using Yamui.Framework.Themes;

namespace Yamui.Framework.HtmlRenderer.WinForms {

    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WinForms control that will render html content in it's client rectangle.<br/>
    /// If <see cref="AutoScroll"/> is true and the layout of the html resulted in its content beyond the client bounds 
    /// of the panel it will show scrollbars (horizontal/vertical) allowing to scroll the content.<br/>
    /// If <see cref="AutoScroll"/> is false html content outside the client bounds will be clipped.<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// <para>
    /// The major differential to use HtmlPanel or HtmlLabel is size and scrollbars.<br/>
    /// If the size of the control depends on the html content the HtmlLabel should be used.<br/>
    /// If the size is set by some kind of layout then HtmlPanel is more suitable, also shows scrollbars if the html contents is larger than the control client rectangle.<br/>
    /// </para>
    /// <para>
    /// <h4>AutoScroll:</h4>
    /// Allows showing scrollbars if html content is placed outside the visible boundaries of the panel.
    /// </para>
    /// <para>
    /// <h4>LinkClicked event:</h4>
    /// Raised when the user clicks on a link in the html.<br/>
    /// Allows canceling the execution of the link.
    /// </para>
    /// <para>
    /// <h4>StylesheetLoad event:</h4>
    /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
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
    public class HtmlPanel : YamuiScrollPanel {

        #region Fields and Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        protected HtmlContainer _htmlContainer;
        
        /// <summary>
        /// the raw base stylesheet data used in the control
        /// </summary>
        protected string _baseRawCssData;

        /// <summary>
        /// the base stylesheet data used in the control
        /// </summary>
        protected CssData _baseCssData;

        /// <summary>
        /// the current html text set in the control
        /// </summary>
        protected string _text;

        /// <summary>
        /// If to use cursors defined by the operating system or .NET cursors
        /// </summary>
        protected bool _useSystemCursors;

        /// <summary>
        /// The text rendering hint to be used for text rendering.
        /// </summary>
        protected TextRenderingHint _textRenderingHint = TextRenderingHint.SystemDefault;

        /// <summary>
        /// The last position of the scrollbars to know if it has changed to update mouse
        /// </summary>
        protected Point _lastScrollOffset;

        #endregion

        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        public HtmlPanel() {
            _htmlContainer = new HtmlContainer();
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.BoxClicked += OnBoxClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.ScrollChange += OnScrollChange;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;

            // subscribe to an event called when the BaseCss sheet changes
            YamuiThemeManager.OnCssChanged += YamuiThemeManagerOnOnCssChanged;
        }

        private void YamuiThemeManagerOnOnCssChanged() {
            if (_text != null)
                Text = _text;
        }

        /// <summary>
        /// Gets or sets the text of this panel
        /// </summary>
        [Browsable(true)]
        [Description("Sets the html of this control.")]
        public override string Text {
            get { return _text; }
            set {
                if (value == null) return;
                ;
                _text = value;
                base.Text = value;
                if (!IsDisposed) {
                    VerticalScroll.Value = 0;
                    _htmlContainer.SetHtml((_text.StartsWith(@"<html") ? _text : @"<html><body>" + _text + @"</body><html>"), YamuiThemeManager.CurrentThemeCss);
                    PerformLayout();
                    Invalidate();
                    InvokeMouseMove();
                }
            }
        }

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
        /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;

        /// <summary>
        /// Gets or sets a value indicating if anti-aliasing should be avoided for geometry like backgrounds and borders (default - false).
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("If anti-aliasing should be avoided for geometry like backgrounds and borders")]
        public virtual bool AvoidGeometryAntialias {
            get { return _htmlContainer.AvoidGeometryAntialias; }
            set { _htmlContainer.AvoidGeometryAntialias = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if image loading only when visible should be avoided (default - false).<br/>
        /// True - images are loaded as soon as the html is parsed.<br/>
        /// False - images that are not visible because of scroll location are not loaded until they are scrolled to.
        /// </summary>
        /// <remarks>
        /// Images late loading improve performance if the page contains image outside the visible scroll area, especially if there is large 
        /// amount of images, as all image loading is delayed (downloading and loading into memory).<br/>
        /// Late image loading may effect the layout and actual size as image without set size will not have actual size until they are loaded
        /// resulting in layout change during user scroll.<br/>
        /// Early image loading may also effect the layout if image without known size above the current scroll location are loaded as they
        /// will push the html elements down.
        /// </remarks>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("If image loading only when visible should be avoided")]
        public virtual bool AvoidImagesLateLoading {
            get { return _htmlContainer.AvoidImagesLateLoading; }
            set { _htmlContainer.AvoidImagesLateLoading = value; }
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
        [DefaultValue(false)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Description("If to use GDI+ text rendering to measure/draw text, false - use GDI")]
        public bool UseGdiPlusTextRendering {
            get { return _htmlContainer.UseGdiPlusTextRendering; }
            set { _htmlContainer.UseGdiPlusTextRendering = value; }
        }

        /// <summary>
        /// The text rendering hint to be used for text rendering.
        /// </summary>
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(TextRenderingHint.SystemDefault)]
        [Description("The text rendering hint to be used for text rendering.")]
        public TextRenderingHint TextRenderingHint {
            get { return _textRenderingHint; }
            set { _textRenderingHint = value; }
        }

        /// <summary>
        /// If to use cursors defined by the operating system or .NET cursors
        /// </summary>
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(false)]
        [Description("If to use cursors defined by the operating system or .NET cursors")]
        public bool UseSystemCursors {
            get { return _useSystemCursors; }
            set { _useSystemCursors = value; }
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
        public virtual bool IsSelectionEnabled {
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
        public virtual bool IsContextMenuEnabled {
            get { return _htmlContainer.IsContextMenuEnabled; }
            set { _htmlContainer.IsContextMenuEnabled = value; }
        }

        /// <summary>
        /// Set base stylesheet to be used by html rendered in the panel.
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set base stylesheet to be used by html rendered in the control.")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public virtual string BaseStylesheet {
            get { return _baseRawCssData; }
            set {
                //_baseRawCssData = value;
                //_baseCssData = HtmlRender.ParseStyleSheet(value);
                //_htmlContainer.SetHtml(_text, _baseCssData);
            }
        }
        
        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        [Browsable(false)]
        public virtual string SelectedText {
            get { return _htmlContainer.SelectedText; }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        [Browsable(false)]
        public virtual string SelectedHtml {
            get { return _htmlContainer.SelectedHtml; }
        }

        /// <summary>
        /// Get html from the current DOM tree with inline style.
        /// </summary>
        /// <returns>generated html</returns>
        public virtual string GetHtml() {
            return _htmlContainer != null ? _htmlContainer.GetHtml() : null;
        }

        /// <summary>
        /// Get the rectangle of html element as calculated by html layout.<br/>
        /// Element if found by id (id attribute on the html element).<br/>
        /// Note: to get the screen rectangle you need to adjust by the hosting control.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to get its rectangle</param>
        /// <returns>the rectangle of the element or null if not found</returns>
        public virtual RectangleF? GetElementRectangle(string elementId) {
            return _htmlContainer != null ? _htmlContainer.GetElementRectangle(elementId) : null;
        }

        /// <summary>
        /// Adjust the scrollbar of the panel on html element by the given id.<br/>
        /// The top of the html element rectangle will be at the top of the panel, if there
        /// is not enough height to scroll to the top the scroll will be at maximum.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to scroll to</param>
        public virtual void ScrollToElement(string elementId) {
            ArgChecker.AssertArgNotNullOrEmpty(elementId, "elementId");

            if (_htmlContainer != null) {
                var rect = _htmlContainer.GetElementRectangle(elementId);
                if (rect.HasValue) {
                    UpdateScroll(Point.Round(rect.Value.Location));
                    _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons, 0, MousePosition.X, MousePosition.Y, 0));
                }
            }
        }

        #region Private methods

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override void OnLayout(LayoutEventArgs levent) {
            PerformHtmlLayout();

            base.OnLayout(levent);

            // to handle if vertical scrollbar is appearing or disappearing
            if (_htmlContainer != null && Math.Abs(_htmlContainer.MaxSize.Width - ClientSize.Width) > 0.1) {
                PerformHtmlLayout();
                base.OnLayout(levent);
            }
        }

        /// <summary>
        /// Perform html container layout by the current panel client size.
        /// </summary>
        protected void PerformHtmlLayout() {
            if (_htmlContainer != null) {
                _htmlContainer.MaxSize = new SizeF(ClientSize.Width - Padding.Horizontal, 0);

                using (var g = CreateGraphics()) {
                    _htmlContainer.PerformLayout(g);
                }

                AutoScrollMinSize = Size.Round(new SizeF(_htmlContainer.ActualSize.Width + Padding.Horizontal, _htmlContainer.ActualSize.Height));
            }
        }

        /// <summary>
        /// Perform paint of the html in the control.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            if (_htmlContainer != null) {
                e.Graphics.TextRenderingHint = _textRenderingHint;
                e.Graphics.SetClip(ClientRectangle);

                _htmlContainer.Location = new PointF(Padding.Left, Padding.Top);
                _htmlContainer.ScrollOffset = AutoScrollPosition;
                _htmlContainer.PerformPaint(e.Graphics);

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
            UpdateScroll(new Point((int) e.X, (int) e.Y));
        }

        /// <summary>
        /// Adjust the scrolling of the panel to the requested location.
        /// </summary>
        /// <param name="location">the location to adjust the scroll to</param>
        protected virtual void UpdateScroll(Point location) {
            AutoScrollPosition = location;
            _htmlContainer.ScrollOffset = AutoScrollPosition;
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection. 
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseMove(this, e);
        }

        /// <summary>
        /// Handle mouse leave to handle cursor change.
        /// </summary>
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseLeave(this);
        }

        /// <summary>
        /// Handle mouse down to handle selection. 
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDown(this, e);
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click. 
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e) {
            OnMouseClick(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseUp(this, e);
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse. 
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            base.OnMouseDoubleClick(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDoubleClick(this, e);
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e) {
            var handler = LinkClicked;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the BoxClicked event from root container.
        /// </summary>
        protected virtual void OnBoxClicked(BoxClickedEventArgs e) {
            var handler = BoxClicked;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the Render Error event from root container.
        /// </summary>
        protected virtual void OnRenderError(HtmlRenderErrorEventArgs e) {
            var handler = RenderError;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the stylesheet load event from root container.
        /// </summary>
        protected virtual void OnStylesheetLoad(HtmlStylesheetLoadEventArgs e) {
            var handler = StylesheetLoad;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the image load event from root container.
        /// </summary>
        protected virtual void OnImageLoad(HtmlImageLoadEventArgs e) {
            var handler = ImageLoad;
            if (handler != null)
                handler(this, e);
            if (!e.Handled)
                YamuiThemeManager.GetHtmlImages(e);
        }

        /// <summary>
        /// Handle html renderer invalidate and re-layout as requested.
        /// </summary>
        protected virtual void OnRefresh(HtmlRefreshEventArgs e) {
            if (e.Layout)
                PerformLayout();
            Invalidate();
        }
        
        /// <summary>
        /// call mouse move to handle paint after scroll or html change affecting mouse cursor.
        /// </summary>
        protected virtual void InvokeMouseMove() {
            var mp = PointToClient(MousePosition);
            _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0));
        }

#if !MONO
        /// <summary>
        /// Override the proc processing method to set OS specific hand cursor.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message"/> to process. </param>
        [DebuggerStepThrough]
        protected override void WndProc(ref Message m) {
            if (_useSystemCursors && m.Msg == Win32Utils.WmSetCursor && Cursor == Cursors.Hand) {
                try {
                    // Replace .NET's hand cursor with the OS cursor
                    Win32Utils.SetCursor(Win32Utils.LoadCursor(0, Win32Utils.IdcHand));
                    m.Result = IntPtr.Zero;
                    return;
                } catch (Exception ex) {
                    OnRenderError(this, new HtmlRenderErrorEventArgs(HtmlRenderErrorType.General, "Failed to set OS hand cursor", ex));
                }
            }
            base.WndProc(ref m);
        }
#endif

        /// <summary>
        /// Release the html container resources.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (_htmlContainer != null) {
                _htmlContainer.LinkClicked -= OnLinkClicked;
                _htmlContainer.BoxClicked -= OnBoxClicked;
                _htmlContainer.RenderError -= OnRenderError;
                _htmlContainer.Refresh -= OnRefresh;
                _htmlContainer.ScrollChange -= OnScrollChange;
                _htmlContainer.StylesheetLoad -= OnStylesheetLoad;
                _htmlContainer.ImageLoad -= OnImageLoad;
                _htmlContainer.Dispose();
                _htmlContainer = null;
            }
            base.Dispose(disposing);
        }

        #region Private event handlers

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e) {
            OnLinkClicked(e);
        }

        private void OnBoxClicked(object sender, BoxClickedEventArgs e) {
            OnBoxClicked(e);
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

        private void OnScrollChange(object sender, HtmlScrollEventArgs e) {
            OnScrollChange(e);
        }

        #endregion

        #region Hide not relevant properties from designer

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Font Font {
            get { return base.Font; }
            set { base.Font = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Color ForeColor {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override bool AllowDrop {
            get { return base.AllowDrop; }
            set { base.AllowDrop = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override RightToLeft RightToLeft {
            get { return base.RightToLeft; }
            set { base.RightToLeft = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Cursor Cursor {
            get { return base.Cursor; }
            set { base.Cursor = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public new bool UseWaitCursor {
            get { return base.UseWaitCursor; }
            set { base.UseWaitCursor = value; }
        }

        #endregion

        #endregion
    }
}