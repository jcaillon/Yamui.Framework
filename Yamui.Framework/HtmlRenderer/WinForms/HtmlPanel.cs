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
using System.Drawing;
using System.Windows.Forms;
using Yamui.Framework.Controls;
using Yamui.Framework.HtmlRenderer.Core.Core.Entities;
using Yamui.Framework.HtmlRenderer.Core.Core.Utils;

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
    public class HtmlPanel : HtmlLabel {

        [DefaultValue(15)]
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

        [Browsable(false)]
        public YamuiScrollHandler VerticalScroll { get; }
        
        [Browsable(false)]
        public YamuiScrollHandler HorizontalScroll { get; }

        /// <summary>
        /// Can this control have vertical scroll?
        /// </summary>
        [DefaultValue(true)]
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
        [DefaultValue(true)]
        public bool VScroll {
            get { return _vScroll; }
            set {
                _vScroll = value;
                VerticalScroll.Enabled = _vScroll;
                PerformLayout();
            }
        }

        private bool _vScroll = true;
        private bool _hScroll = true;

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

        [Browsable(false)]
        public Size AutoScrollMinSize {
            get { return new Size(HorizontalScroll.LengthToRepresentMinSize, VerticalScroll.LengthToRepresentMinSize); }
            set {
                HorizontalScroll.LengthToRepresentMinSize = value.Width;
                VerticalScroll.LengthToRepresentMinSize = value.Height;
            }
        }
        
        [Browsable(false)]
        public bool HasScroll => VerticalScroll.HasScroll || HorizontalScroll.HasScroll;

        #region ScrollHandler events

        private void ScrollOnOnValueChanged(object sender, YamuiScrollHandlerValueChangedEventArgs e) {
            SetDisplayRectLocation(sender as YamuiScrollHandler, e.NewValue);
        }
        
        private void OnScrollbarsRedrawNeeded(object sender, EventArgs eventArgs) {
            if (HasScroll) {
                Invalidate(); // help against flickering
                PaintScrollBars(sender as YamuiScrollHandler);
            }
        }

        private void SetDisplayRectLocation(YamuiScrollHandler yamuiScrollHandler, int newValue) {
            _htmlContainer.ScrollOffset = AutoScrollPosition;
        }

        private void PaintScrollBars(YamuiScrollHandler yamuiScrollHandler) {
            
        }

        #endregion




        #region Fields and Consts
        /// <summary>
        /// The last position of the scrollbars to know if it has changed to update mouse
        /// </summary>
        protected Point _lastScrollOffset;

        private int _scrollBarWidth = 12;

        #endregion

        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        public HtmlPanel() {
            _htmlContainer.ScrollChange += OnScrollChange;

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
            VerticalScroll.OnValueChanged += ScrollOnOnValueChanged;
            VerticalScroll.OnScrollbarsRedrawNeeded += OnScrollbarsRedrawNeeded;
            HorizontalScroll.OnValueChanged += ScrollOnOnValueChanged;
            HorizontalScroll.OnScrollbarsRedrawNeeded += OnScrollbarsRedrawNeeded;
        }

        ~HtmlPanel() {
            VerticalScroll.OnValueChanged -= ScrollOnOnValueChanged;
            VerticalScroll.OnScrollbarsRedrawNeeded -= OnScrollbarsRedrawNeeded;
            HorizontalScroll.OnValueChanged -= ScrollOnOnValueChanged;
            HorizontalScroll.OnScrollbarsRedrawNeeded -= OnScrollbarsRedrawNeeded;
        }

        /// <summary>
        /// Gets or sets the text of this panel
        /// </summary>
        [Browsable(true)]
        [Description("Sets the html of this control.")]
        public override string Text {
            get { return base.Text; }
            set {
                if (!IsDisposed) {
                    VerticalScroll.Value = 0;
                }
                base.Text = value;
                if (!IsDisposed) {
                    InvokeMouseMove();
                }
            }
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
                    AutoScrollPosition = Point.Round(rect.Value.Location);
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
                e.Graphics.SetClip(ClientRectangle);

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
            AutoScrollPosition = new Point((int) e.X, (int) e.Y);
        }

        /// <summary>
        /// call mouse move to handle paint after scroll or html change affecting mouse cursor.
        /// </summary>
        protected virtual void InvokeMouseMove() {
            var mp = PointToClient(MousePosition);
            _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0));
        }

        /// <summary>
        /// Release the html container resources.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (_htmlContainer != null) {
                _htmlContainer.ScrollChange -= OnScrollChange;
                _htmlContainer.Dispose();
                _htmlContainer = null;
            }
            base.Dispose(disposing);
        }

        #region Private event handlers

        private void OnScrollChange(object sender, HtmlScrollEventArgs e) {
            OnScrollChange(e);
        }

        #endregion

        #endregion
    }
}