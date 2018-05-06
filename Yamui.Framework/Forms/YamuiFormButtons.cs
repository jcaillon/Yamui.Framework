#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormButtons.cs) is part of YamuiFramework.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Yamui.Framework.Helper;
using Yamui.Framework.HtmlRenderer.WinForms;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Forms {

    /// <summary>
    /// Form class that adds the top right buttons + resize
    /// </summary>
    public abstract class YamuiFormButtons : YamuiFormFadeIn {

        #region constants

        protected const int FormButtonWidth = 25;
        protected const int ResizeIconSize = 14;

        #endregion

        #region Private

        /// <summary>
        /// Tooltip for close buttons
        /// </summary>
        private HtmlToolTip _mainFormToolTip = new HtmlToolTip();

        private Dictionary<WindowButtons, YamuiFormButton> _windowButtonList = new Dictionary<WindowButtons, YamuiFormButton>();

        private YamuiFormResizeIcon _resizeIcon = new YamuiFormResizeIcon();

        #endregion

        #region Properties

        [Browsable(false)]
        public override int TitleBarHeight => 25;

        /// <summary>
        /// Set this to true to show the "close all notifications button",
        /// to use with OnCloseAllVisible
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        public bool CloseAllBox { get; set; }

        /// <summary>
        /// To use with ShowCloseAllVisibleButton,
        /// Action to do when the user click the button
        /// </summary>
        [Browsable(false)]
        public EventHandler OnCloseAllNotif { get; set; }

        protected override Padding DefaultPadding {
            get { return new Padding(BorderWidth, 20, BorderWidth + ResizeIconSize, BorderWidth + ResizeIconSize); }
        }
        
        #endregion

        #region Enum

        internal enum WindowButtons : byte {
            Minimize = 0,
            Maximize,
            CloseAllVisible,
            Close,

            Restore = 10,
        }

        #endregion

        #region Constructor

        protected YamuiFormButtons(YamuiFormOption options) : base(options) {
            _mainFormToolTip.ShowAlways = true;
            
            AddWindowButton(WindowButtons.Close);
            AddWindowButton(WindowButtons.CloseAllVisible);
            AddWindowButton(WindowButtons.Maximize);  
            AddWindowButton(WindowButtons.Minimize);
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if (DesignMode) {
                return;
            }

            // draw the resize pixels icon on the bottom right
            if (Resizable) {
                _resizeIcon.ClientRectangle = new Rectangle(ClientRectangle.Right - ResizeIconSize, ClientRectangle.Bottom - ResizeIconSize, ResizeIconSize, ResizeIconSize);
                _resizeIcon.OnPaint(e);
            }

            if (ControlBox) {
                int x = ClientRectangle.Width - 1 - FormButtonWidth;
                DrawButton(ref x, WindowButtons.Close, e, true);
                DrawButton(ref x, WindowButtons.CloseAllVisible, e, CloseAllBox);
                DrawButton(ref x, WindowButtons.Maximize, e, MaximizeBox && Resizable);
                DrawButton(ref x, WindowButtons.Minimize, e, MinimizeBox && ShowInTaskbar);
            }
        }

        #endregion

        #region WndProc

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch ((Window.Msg) m.Msg) {
                case Window.Msg.WM_SIZE:
                    var state = (WinApi.WmSizeEnum) m.WParam;
                    switch (state) {
                        case WinApi.WmSizeEnum.SIZE_RESTORED:
                            _windowButtonList[WindowButtons.Maximize].Type = WindowButtons.Maximize;
                            break;
                        case WinApi.WmSizeEnum.SIZE_MAXIMIZED:
                        case WinApi.WmSizeEnum.SIZE_MAXSHOW:
                            _windowButtonList[WindowButtons.Maximize].Type = WindowButtons.Restore;
                            break;
                    }
                    break;

                case Window.Msg.WM_NCLBUTTONDOWN:
                    foreach (var formButton in _windowButtonList.Values) {
                        if (formButton.IsHovered) {
                            formButton.IsPressed = true;
                        }
                    }
                    break;

                case Window.Msg.WM_NCLBUTTONUP:
                case Window.Msg.WM_LBUTTONUP:
                    foreach (var formButton in _windowButtonList.Values) {
                        if (formButton.IsHovered && formButton.IsPressed) {
                            OnWindowButtonClick(formButton.Type);
                        }
                        formButton.IsPressed = false;
                    }
                    break;

                case Window.Msg.WM_NCMOUSELEAVE:
                    foreach (var formButton in _windowButtonList.Values) {
                        formButton.IsHovered = false;
                        formButton.IsPressed = false;
                    }
                    break;

            }

            base.WndProc(ref m);
        }

        #endregion

        #region Events
        
        /// <remarks>my first idea was to return the correct hittest to let windows handle the click, but it shows the ugly default buttons</remarks>
        protected override WinApi.HitTest HitTestNca(IntPtr lparam) {
            var result = base.HitTestNca(lparam);

            if (result == WinApi.HitTest.HTCAPTION) {
                var cursorLocation = PointToClient(new Point(lparam.ToInt32()));
                foreach (var yamuiFormButton in _windowButtonList.Values) {
                    if (cursorLocation.X >= yamuiFormButton.ClientRectangle.Left && cursorLocation.X <= yamuiFormButton.ClientRectangle.Right) {
                        yamuiFormButton.IsHovered = true;
                        result = WinApi.HitTest.HTBORDER;

                        // track mouse leaving (otherwise the WM_NCMOUSELEAVE message would not fire)
                        WinApi.TRACKMOUSEEVENT tme = new WinApi.TRACKMOUSEEVENT();
                        tme.cbSize = (uint) Marshal.SizeOf(tme);
                        tme.dwFlags = (uint) (WinApi.TMEFlags.TME_LEAVE | WinApi.TMEFlags.TME_NONCLIENT);
                        tme.hwndTrack = Handle;
                        WinApi.TrackMouseEvent(tme);
                    } else {
                        yamuiFormButton.IsHovered = false;
                        yamuiFormButton.IsPressed = false;
                    }
                }
            } else if (result == WinApi.HitTest.HTCLIENT && Resizable) {
                var cursorLocation2 = PointToClient(new Point(lparam.ToInt32()));
                if (cursorLocation2.X >= _resizeIcon.ClientRectangle.Left && cursorLocation2.Y >= _resizeIcon.ClientRectangle.Top) {
                    result = WinApi.HitTest.HTBOTTOMRIGHT;
                }
            }

            return result;
        }

        /// <summary>
        /// On load of the form
        /// </summary>
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (DesignMode) return;

            switch (StartPosition) {
                case FormStartPosition.CenterParent:
                    CenterToParent();
                    break;
                case FormStartPosition.CenterScreen:
                    if (IsMdiChild) {
                        CenterToParent();
                    } else {
                        CenterToScreen();
                    }
                    break;
            }
        }

        #endregion

        #region Window Buttons

        /// <summary>
        /// Add a particular button on the right top of the form
        /// </summary>
        private void AddWindowButton(WindowButtons button) {
            var newButton = new YamuiFormButton(this) {
                Type = button,
            };
            _windowButtonList.Add(button, newButton);
        }
        
        private void DrawButton(ref int x, WindowButtons buttonType, PaintEventArgs e, bool show) {
            var button = _windowButtonList[buttonType];
            button.Show = show;
            if (show) {
                button.ClientRectangle = new Rectangle(x, 1, FormButtonWidth, FormButtonWidth - 1);
                button.OnPaint(e);
                x -= FormButtonWidth;
            }
        }
        
        /// <summary>
        /// Triggered when a button is clicked
        /// </summary>
        private void OnWindowButtonClick(WindowButtons type) {
            switch (type) {
                case WindowButtons.Close:
                    Close();
                    break;
                case WindowButtons.Minimize:
                    WindowState = FormWindowState.Minimized;
                    break;
                case WindowButtons.Maximize:
                    WindowState = FormWindowState.Maximized;
                    break;
                case WindowButtons.Restore:
                    WindowState = FormWindowState.Normal;
                    break;
                case WindowButtons.CloseAllVisible:
                    OnCloseAllNotif?.Invoke(this, null);
                    break;
            }
        }

        #region YamuiFormResizeIcon

        private class YamuiFormResizeIcon {

            public Rectangle ClientRectangle { get; set; }

            public void OnPaint(PaintEventArgs e) {
                var foreColor = YamuiThemeManager.Current.FormFore;
                using (var b = new SolidBrush(foreColor)) {
                    var resizeHandleSize = new Size(2, 2);
                    e.Graphics.FillRectangles(b, new[] {
                        new Rectangle(new Point(ClientRectangle.Right - 4, ClientRectangle.Bottom - 4), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Right - 8, ClientRectangle.Bottom - 8), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Right - 8, ClientRectangle.Bottom - 4), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Right - 4, ClientRectangle.Bottom - 8), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Right - 12, ClientRectangle.Bottom - 4), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Right - 4, ClientRectangle.Bottom - 12), resizeHandleSize)
                    });
                }
            }

        }

        #endregion

        #region YamuiFormButton

        private class YamuiFormButton {
                        
            private readonly YamuiFormButtons _parent;
            private bool _isHovered;
            private bool _isPressed;
            
            public WindowButtons Type { get; set; }
            public Rectangle ClientRectangle { get; set; }


            public bool IsHovered {
                get { return _isHovered; }
                set {
                    if (_isHovered != value) {
                        _isHovered = value;
                        if (Show) {
                            _parent.Invalidate(ClientRectangle, false);
                        }
                    }
                }
            }
            public bool IsPressed {
                get { return _isPressed; }
                set {
                    if (_isPressed != value) {
                        _isPressed = value;
                        if (Show) {
                            _parent.Invalidate(ClientRectangle, false);
                        }
                    }
                }
            }

            public bool Show { get; set; }


            public YamuiFormButton(YamuiFormButtons parent) {
                _parent = parent;
            }
            
            public void OnPaint(PaintEventArgs e) {
                using (var b = new SolidBrush(BackColor)) {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                Color foreColor = YamuiThemeManager.Current.ButtonFg(YamuiThemeManager.Current.FormFore, false, false, IsHovered, IsPressed, _parent.Enabled);

                using (var font = new Font("Webdings", 9.25f)) {
                    TextRenderer.DrawText(e.Graphics, Text, font, ClientRectangle, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }
            
            public string TooltipText {
                get {
                    switch (Type) {
                        case WindowButtons.Close:
                            return "<b>Close</b> this window";
                        case WindowButtons.Minimize:
                            return "<b>Minimize</b> this window";
                        case WindowButtons.Maximize:
                            return "<b>Maximize</b> this window";
                        case WindowButtons.Restore:
                            return "<b>Restore</b> this window";
                        case WindowButtons.CloseAllVisible:
                            return "<b>Close all</b> notification windows";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public string Text {
                get {
                    switch (Type) {
                        case WindowButtons.Minimize:
                            return @"0";
                        case WindowButtons.Restore:
                            return @"2";
                        case WindowButtons.Maximize:
                            return @"1";
                        case WindowButtons.CloseAllVisible:
                            return ((char) (126)).ToString();
                        case WindowButtons.Close:
                            return @"r";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public Color BackColor {
                get {
                    if (IsPressed)
                        return YamuiThemeManager.Current.AccentColor;
                    if (IsHovered)
                        return YamuiThemeManager.Current.ButtonHoverBack;
                    return YamuiThemeManager.Current.FormBack;
                }
            }

            public WinApi.HitTest HitTest {
                get {
                    switch (Type) {
                        case WindowButtons.Minimize:
                            return WinApi.HitTest.HTMINBUTTON;
                        case WindowButtons.Restore:
                        case WindowButtons.Maximize:
                            return WinApi.HitTest.HTMAXBUTTON;
                        case WindowButtons.CloseAllVisible:
                            return WinApi.HitTest.HTZOOM;
                        case WindowButtons.Close:
                            return WinApi.HitTest.HTCLOSE;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }


        }

        #endregion

        #endregion
    }
}