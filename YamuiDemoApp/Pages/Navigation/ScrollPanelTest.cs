﻿#region header

// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiScrollPage.cs) is part of YamuiFramework.
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
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages.Navigation {
    
    public class ScrollPanelTest : YamuiControl {

        #region fields

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool NoBackgroundImage { get; set; }

        /*
        [DefaultValue(2)]
        [Category("Yamui")]
        public int ThumbPadding { get; set; }

        [DefaultValue(10)]
        [Category("Yamui")]
        public int ScrollBarWidth { get; set; }
        */

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

        #endregion

        #region constructor

        public ScrollPanelTest() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.Selectable |
                ControlStyles.Opaque, true);

            VerticalScroll = new YamuiScrollHandler(true, this) {
                SmallChange = 70,
                LargeChange = 400
            };
            HorizontalScroll = new YamuiScrollHandler(false, this){
                SmallChange = 35,
                LargeChange = 200
            };
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e) {
            
            // paint background
            e.Graphics.Clear(Color.Black);
            VerticalScroll.Paint(e);
            HorizontalScroll.Paint(e);

        }

        #endregion

        #region handle windows events
        
        /// <summary>
        /// Handle keydown
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e) {
            e.Handled = HorizontalScroll.HandleKeyDown(e) || VerticalScroll.HandleKeyDown(e);
            if (!e.Handled)
                base.OnKeyDown(e);
        }

        /// <summary>
        /// redirect all input key to keydown
        /// </summary>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// Handle mouse wheel
        /// </summary>
        protected override void OnMouseWheel(MouseEventArgs e) {
            if (HorizontalScroll.IsActive) {
                HorizontalScroll.HandleScroll(e);
            } else {
                VerticalScroll.HandleScroll(e);
            }
            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            HorizontalScroll.HandleMouseDown(e);
            VerticalScroll.HandleMouseDown(e);
            Focus();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            HorizontalScroll.HandleMouseUp(e);
            VerticalScroll.HandleMouseUp(e);
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            HorizontalScroll.HandleMouseMove(e);
            VerticalScroll.HandleMouseMove(e);
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            return e.Handled;
        }
        
        protected override void OnResize(EventArgs e) {
            HorizontalScroll.UpdateLength(HorizontalScroll.LengthToRepresent, HorizontalScroll.LengthAvailable);
            VerticalScroll.UpdateLength(VerticalScroll.LengthToRepresent, VerticalScroll.LengthAvailable);
            base.OnResize(e);
        }


        /// <summary>
        /// Correct original padding as we need extra space for the scrollbars
        /// </summary>
        public new Padding Padding {
            get {
                var basePadding = base.Padding;
                if (!DesignMode) {
                    if (HorizontalScroll.HasScroll) {
                        basePadding.Bottom = basePadding.Bottom + HorizontalScroll.BarThickness;
                    }

                    if (VerticalScroll.HasScroll) {
                        basePadding.Right = basePadding.Right + VerticalScroll.BarThickness;
                    }
                }
                return basePadding;
            }
            set {
                base.Padding = value;
            }
        }

        /// <summary>
        /// Very important to display the correct scroll value when coming back to a scrolled panel.
        /// Try without it and watch for yourself
        /// </summary>
        public override Rectangle DisplayRectangle {
            get {
                Rectangle rect = ClientRectangle;
                if (VerticalScroll.HasScroll)
                    rect.Y = -VerticalScroll.Value;
                    rect.Width -= HorizontalScroll.BarThickness;
                if (HorizontalScroll.HasScroll) {
                    rect.X = -HorizontalScroll.Value;
                    rect.Height -= VerticalScroll.BarThickness;
                }
                return rect;
            }
        }

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
            get {
                return new Size(HorizontalScroll.MaximumValue, VerticalScroll.MaximumValue);
            }
            set {
                // TODO : 
            }
        }

        public void ScrollControlIntoView(Control control) {
            
        }

        #endregion


    }
}