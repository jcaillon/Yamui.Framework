using System;
using System.Drawing;
using System.Windows.Forms;
using Yamui.Framework.Helper;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {
    
    /// <summary>
    /// Store values needed to represent a scrollbar, also provides methods to handle user events and paint the scrollbar
    /// </summary>
    public class YamuiScrollHandler {

        /// <summary>
        /// Action that will be triggered when the value of the scroll changes : this, oldValue, newValue
        /// You should change the view of your client area when this event happens (do not paint scrollbars now)
        /// </summary>
        public event EventHandler<YamuiScrollHandlerValueChangedEventArgs> OnValueChange;

        /// <summary>
        /// Action sent when the scroll bar needs to be redraw
        /// </summary>
        public event EventHandler OnRedrawScrollBars;

        /// <summary>
        /// Read-only, is the scrollbar vertical
        /// </summary>
        public bool IsVertical { get; }

        /// <summary>
        /// Padding for the thumb within the bar
        /// </summary>
        public int ThumbPadding {
            get { return _thumbPadding; }
            set {
                _thumbPadding = value;
                InvalidateScrollBar();
            }
        }

        /// <summary>
        /// Is this scrollbar enabled
        /// </summary>
        public bool Enabled {
            get { return _enabled; }
            set {
                _enabled = value;
                AnalyzeScrollNeeded();
            }
        }

        /// <summary>
        /// Are the scroll buttons (up/down) enabled
        /// </summary>
        public bool ScrollButtonEnabled {
            get { return _scrollButtonEnabled; }
            set {
                _scrollButtonEnabled = value;
                AnalyzeScrollNeeded();
            }
        }

        /// <summary>
        /// Scrollbar thickness
        /// </summary>
        public int BarThickness {
            get { return _barThickness; }
            set {
                _barThickness = value.ClampMin(5);
                ComputeScrollFixedValues();
                InvalidateScrollBar();
            }
        }

        /// <summary>
        /// Will be added/substracted to the Value when using directional keys
        /// </summary>
        public int SmallChange {
            get { return _smallChange == 0 ? LengthAvailable / 10 : _smallChange; }
            set { _smallChange = value; }
        }

        /// <summary>
        /// Will be added/substracted to the Value when scrolling or page up/down
        /// </summary>
        public int LargeChange {
            get { return _largeChange == 0 ? LengthAvailable / 2 : _largeChange; }
            set { _largeChange = value; }
        }

        /// <summary>
        /// Forces a minimum value for the length to represent
        /// </summary>
        public int LengthToRepresentMinSize { get; set; }

        /// <summary>
        /// Exposes the state of the scroll bars, true if they are displayed
        /// </summary>
        public bool HasScroll { get; private set; }

        /// <summary>
        /// Exposes the state of the scroll buttons, true if displayed
        /// </summary>
        public bool HasScrollButtons { get; private set; }

        /// <summary>
        /// Maximum length of this panel if we wanted to show it all w/o scrolls (set with <see cref="UpdateLength"/>)
        /// </summary>
        public int LengthToRepresent { get; private set; }

        /// <summary>
        /// Maximum length really available to show the content (set with <see cref="UpdateLength"/>)
        /// </summary>
        public int LengthAvailable { get; private set; }
        
        public int DrawLength { get; private set; }

        public int OpposedDrawLength { get; private set; }

        /// <summary>
        /// Update the length to represent as well as the length really available.
        /// This effectively defines the ratio between thumb length and bar length. 
        /// Returns whether or not the scrollbar is needed
        /// </summary>
        /// <returns></returns>
        public bool UpdateLength(int lengthToRepresent, int? lengthAvailable, int drawLength, int opposedDrawLength) {
            LengthToRepresent = lengthToRepresent;
            if (LengthToRepresentMinSize > 0)
                LengthToRepresent = LengthToRepresent.ClampMin(LengthToRepresentMinSize);
            LengthAvailable = lengthAvailable ?? drawLength;
            DrawLength = drawLength;
            OpposedDrawLength = opposedDrawLength;

            ComputeScrollFixedValues();
            AnalyzeScrollNeeded();
            return HasScroll;
        } 

        /// <summary>
        /// Represents the current scroll value, limited by <see cref="MinimumValue"/> and by <see cref="MaximumValue"/>
        /// </summary>
        public int Value {
            get { return _value; }
            set {
                var previousValue = _value;
                _value = value.Clamp(MinimumValue, MaximumValue);

                // compute new thumb rectangle
                ThumbRect = IsVertical ? 
                    new Rectangle(BarOpposedOffset + ThumbPadding, ThumbOffset + (int) (BarScrollFreeSpace * ValuePercent), ThumbThickness, ThumbLenght) : 
                    new Rectangle(ThumbOffset + (int) (BarScrollFreeSpace * ValuePercent), BarOpposedOffset + ThumbPadding, ThumbLenght, ThumbThickness);

                InvalidateScrollBar();
                if (HasScroll)
                    OnValueChange?.Invoke(this, new YamuiScrollHandlerValueChangedEventArgs(previousValue, _value));
            }
        }
        
        /// <summary>
        /// The scroll value but represented in percent
        /// </summary>
        public double ValuePercent {
            get { return (double) Value / MaximumValue; }
            set { Value = (int) (MaximumValue * value); }
        }

        /// <summary>
        /// Is the thumb pressed
        /// </summary>
        public bool IsThumbPressed {
            get { return _isThumbPressed; }
            private set {
                if (_isThumbPressed != value) {
                    _isThumbPressed = value;
                    InvalidateScrollBar();
                }
            }
        }

        /// <summary>
        /// Is the mouse flying over the thumb
        /// </summary>
        public bool IsThumbHovered {
            get { return _isThumbHovered; }
            private set {
                if (_isThumbHovered != value) {
                    _isThumbHovered = value;
                    InvalidateScrollBar();
                }
            }
        }

        public bool IsButtonUpPressed {
            get { return _isButtonUpPressed; }
            private set {
                if (_isButtonUpPressed != value) {
                    _isButtonUpPressed = value;
                    InvalidateScrollBar();
                }
            }
        }

        public bool IsButtonUpHovered {
            get { return _isButtonUpHovered; }
            private set {
                if (_isButtonUpHovered != value) {
                    _isButtonUpHovered = value;
                    InvalidateScrollBar();
                }
            }
        }

        public bool IsButtonDownPressed {
            get { return _isButtonDownPressed; }
            private set {
                if (_isButtonDownPressed != value) {
                    _isButtonDownPressed = value;
                    InvalidateScrollBar();
                }
            }
        }

        public bool IsButtonDownHovered {
            get { return _isButtonDownHovered; }
            private set {
                if (_isButtonDownHovered != value) {
                    _isButtonDownHovered = value;
                    InvalidateScrollBar();
                }
            }
        }

        /// <summary>
        /// Is the mouse flying over the bar
        /// </summary>
        public bool IsHovered { get; private set; }
        
        public const int MinimumValue = 0;

        private const int BarOffset = 0;

        public int MaximumValue { get; set; }
        private int BarOpposedOffset { get; set; }
        private int ThumbThickness { get; set; }
        private int BarLength { get; set; }
        protected bool CanDisplayThumb { get; set; }
        protected int ScrollButtonSize { get; set; }
        private int ThumbLenght { get; set; }
        protected int ThumbOffset { get; set; }

        /// <summary>
        /// The total space available to move the thumb in the bar
        /// </summary>
        protected int BarScrollSpace { get; set; }

        /// <summary>
        /// The total space available to move the thumb in the bar minus the space occupied by the thumb
        /// </summary>
        protected int BarScrollFreeSpace { get; set; }

        /// <summary>
        /// Rect for the up button
        /// </summary>
        protected Rectangle ScrollButtonUp { get; set; }

        /// <summary>
        /// Rect for the down button
        /// </summary>
        protected Rectangle ScrollButtonDown { get; set; }

        /// <summary>
        /// represents the bar rectangle (that will be painted)
        /// </summary>
        public Rectangle BarRect { get; set; }

        /// <summary>
        /// represents the thumb rectangle (that will be painted)
        /// </summary>
        protected Rectangle ThumbRect { get; set; }

        private int _value;
        private Control _parent;
        private bool _isThumbPressed;
        private bool _isThumbHovered;
        private int _mouseMoveInThumbPosition;
        private int _smallChange;
        private int _largeChange;
        private bool _enabled = true;
        private int _thumbPadding = 2;
        private int _barThickness = 15;
        private bool _scrollButtonEnabled = true;
        private bool _isButtonUpHovered;
        private bool _isButtonDownHovered;
        private bool _isButtonUpPressed;
        private bool _isButtonDownPressed;

        public YamuiScrollHandler(bool isVertical, Control parent) {
            IsVertical = isVertical;
            _parent = parent;
        }

        #region Paint

        /// <summary>
        /// Paint the scroll bar in the parent client rectangle
        /// </summary>
        public void Paint(PaintEventArgs e) {
            if (!HasScroll)
                return;
            
            Color thumbColor = YamuiThemeManager.Current.ScrollBarsFg(false, IsThumbHovered, IsThumbPressed, Enabled);
            Color barColor = YamuiThemeManager.Current.ScrollBarsBg(false, IsThumbHovered, IsThumbPressed, Enabled);

            if (barColor != Color.Transparent) {
                using (var b = new SolidBrush(barColor)) {
                    e.Graphics.FillRectangle(b, BarRect);
                }
            }

            if (CanDisplayThumb) {
                using (var b = new SolidBrush(thumbColor)) {
                    e.Graphics.FillRectangle(b, ThumbRect);
                }
            }

            if (HasScrollButtons) {
                Color buttonColor = YamuiThemeManager.Current.ScrollBarsFg(false, IsButtonUpHovered, IsButtonUpPressed, Enabled);
                // draw the down arrow
                using (SolidBrush b = new SolidBrush(buttonColor)) {
                    e.Graphics.FillPolygon(b, Utilities.GetArrowPolygon(ScrollButtonUp, IsVertical ? AnchorStyles.Top : AnchorStyles.Left));
                }
                
                buttonColor = YamuiThemeManager.Current.ScrollBarsFg(false, IsButtonDownHovered, IsButtonDownPressed, Enabled);
                // draw the down arrow
                using (SolidBrush b = new SolidBrush(buttonColor)) {
                    e.Graphics.FillPolygon(b, Utilities.GetArrowPolygon(ScrollButtonDown, IsVertical ? AnchorStyles.Bottom : AnchorStyles.Right));
                }
            }
        }
        
        #endregion

        #region private

        /// <summary>
        /// Redraw the scrollbar
        /// </summary>
        private void InvalidateScrollBar() {
            if (!HasScroll)
                return;

            OnRedrawScrollBars?.Invoke(this, null);
        }

        /// <summary>
        /// move the thumb
        /// </summary>
        private void MoveThumb(int newThumbPos) {
            if (IsVertical) {
                ValuePercent = (double) (newThumbPos - ThumbOffset) / BarScrollFreeSpace;
            } else {
                ValuePercent = (double) (newThumbPos - ThumbOffset) / BarScrollFreeSpace;
            }
        }
        
        private void AnalyzeScrollNeeded() {
            // if the content is not too tall, no need to display the scroll bars
            if (MaximumValue <= 0 || !Enabled || LengthAvailable <= 0) {
                HasScroll = false;
                HasScrollButtons = false;
                Value = 0;
            } else {
                HasScroll = true;
                HasScrollButtons = ScrollButtonEnabled && BarLength > 4 * ScrollButtonSize && BarThickness >= 15;
                Value = Value;
            }
        }

        private void ComputeScrollFixedValues() {
            // compute fixed values
            MaximumValue = (LengthToRepresent - LengthAvailable).ClampMin(0);        
            BarOpposedOffset = OpposedDrawLength - BarThickness;
            ThumbThickness = BarThickness - ThumbPadding * 2;
            BarLength = DrawLength;        
            ScrollButtonSize = BarThickness;
            BarScrollSpace = BarLength - ThumbPadding * 2 - (HasScrollButtons ? 2 * ScrollButtonSize : 0);
            ThumbLenght = ((int) Math.Floor((BarScrollSpace) * ((double) LengthAvailable / LengthToRepresent))).ClampMin(BarThickness);
            ThumbOffset = BarOffset + (HasScrollButtons ? ScrollButtonSize : 0) + ThumbPadding;
            BarScrollFreeSpace = BarScrollSpace - ThumbLenght;
            ScrollButtonUp = IsVertical ? 
                new Rectangle(BarOpposedOffset, BarOffset, ScrollButtonSize, ScrollButtonSize) : 
                new Rectangle(BarOffset, BarOpposedOffset, ScrollButtonSize, ScrollButtonSize);
            ScrollButtonDown = IsVertical ? 
                new Rectangle(BarOpposedOffset, BarLength - ScrollButtonSize, ScrollButtonSize, ScrollButtonSize) : 
                new Rectangle(BarLength - ScrollButtonSize, BarOpposedOffset, ScrollButtonSize, ScrollButtonSize);
            BarRect = IsVertical ?
                new Rectangle(BarOpposedOffset, BarOffset, BarThickness, BarLength) : 
                new Rectangle(BarOffset, BarOpposedOffset, BarLength, BarThickness);
            CanDisplayThumb = BarScrollFreeSpace > 0;
        }

        #endregion

        #region public

        /// <summary>
        /// Mouse move
        /// </summary>
        public void HandleMouseMove(object sender, MouseEventArgs e) {
            if (!HasScroll)
                return;

            // hover bar
            var mousePosRelativeToThis = _parent.PointToClient(Cursor.Position);
            if (BarRect.Contains(mousePosRelativeToThis)) {
                IsHovered = true;
                
                // hover thumb
                if (ThumbRect.Contains(mousePosRelativeToThis)) {
                    IsThumbHovered = true;
                } else {
                    IsThumbHovered = false;

                    // hover button up
                    if (ScrollButtonUp.Contains(mousePosRelativeToThis)) {
                        IsButtonUpHovered = true;
                    } else {
                        IsButtonUpHovered = false;

                        // hover button down
                        if (ScrollButtonDown.Contains(mousePosRelativeToThis)) {
                            IsButtonDownHovered = true;
                        } else {
                            IsButtonDownHovered = false;
                        }
                    }
                }
            } else {
                IsHovered = false;
                IsButtonUpHovered = false;
                IsButtonDownHovered = false;
            }

            // move thumb
            if (IsThumbPressed) {
                MoveThumb(IsVertical ? mousePosRelativeToThis.Y - _mouseMoveInThumbPosition : mousePosRelativeToThis.X - _mouseMoveInThumbPosition);
            }
        }

        public void HandleMouseLeave(object sender, EventArgs e) {
            IsHovered = false;
            IsButtonUpHovered = false;
            IsButtonDownHovered = false;
        }

        /// <summary>
        /// Mouse down
        /// </summary>
        public void HandleMouseDown(object sender, MouseEventArgs e) {
            if (!HasScroll)
                return;
            if (e != null && e.Button != MouseButtons.Left) 
                return;

            var mousePosRelativeToThis = _parent.PointToClient(Cursor.Position);

            // mouse in scrollbar
            if (BarRect.Contains(mousePosRelativeToThis)) {
                var thumbRect = ThumbRect;
                if (IsVertical) {
                    thumbRect.X -= ThumbPadding;
                    thumbRect.Width += ThumbPadding * 2;
                } else {
                    thumbRect.Y -= ThumbPadding;
                    thumbRect.Height += ThumbPadding * 2;
                }

                // mouse in thumb
                if (thumbRect.Contains(mousePosRelativeToThis)) {
                    IsThumbPressed = true;
                    _mouseMoveInThumbPosition = IsVertical ? mousePosRelativeToThis.Y - thumbRect.Y : mousePosRelativeToThis.X - thumbRect.X;
                } else {

                    // hover button up
                    if (ScrollButtonUp.Contains(mousePosRelativeToThis)) {
                        IsButtonUpPressed = true;
                        Value -= SmallChange;
                    } else {

                        // hover button down
                        if (ScrollButtonDown.Contains(mousePosRelativeToThis)) {
                            IsButtonDownPressed = true;
                            Value += SmallChange;
                        } else {

                            // scroll to click position
                            MoveThumb(IsVertical ? mousePosRelativeToThis.Y : mousePosRelativeToThis.X);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mouse up
        /// </summary>
        public void HandleMouseUp(object sender, MouseEventArgs e) {
            if (!HasScroll)
                return;
            if (e != null && e.Button != MouseButtons.Left) 
                return;

            IsThumbPressed = false;
            IsButtonUpPressed = false;
            IsButtonDownPressed = false;
        }

        /// <summary>
        /// Handle scroll
        /// </summary>
        public void HandleScroll(object sender, MouseEventArgs e) {
            if (!HasScroll)
                return;

            // delta negative when scrolling up
            Value += -Math.Sign(e.Delta) * LengthAvailable / 2;
        }

        /// <summary>
        /// Keydown
        /// </summary>
        public bool HandleKeyDown(object sender, KeyEventArgs e) {
            if (!HasScroll)
                return false;

            bool handled = true;

            if (IsVertical) {
                if (e.KeyCode == Keys.Up) {
                    Value -= SmallChange;
                } else if (e.KeyCode == Keys.Down) {
                    Value += SmallChange;
                } else if (e.KeyCode == Keys.PageUp) {
                    Value -= LargeChange;
                } else if (e.KeyCode == Keys.PageDown) {
                    Value += LargeChange;
                } else if (e.KeyCode == Keys.End) {
                    Value = MaximumValue;
                } else if (e.KeyCode == Keys.Home) {
                    Value = MinimumValue;
                } else {
                    handled = false;
                }
            } else {
                if (e.KeyCode == Keys.Left) {
                    Value -= SmallChange;
                } else if (e.KeyCode == Keys.Right) {
                    Value += SmallChange;
                } else {
                    handled = false;
                }
            }

            return handled;
        }
        
        #endregion

        public override string ToString() {
            return $"Value = {Value}, Range = {MinimumValue}:{MaximumValue}, Change = {SmallChange}/{LargeChange}";
        }
    }

    public class YamuiScrollHandlerValueChangedEventArgs : EventArgs {

        public int OldValue { get; }

        public int NewValue { get; }

        public YamuiScrollHandlerValueChangedEventArgs(int oldValue, int newValue) {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}