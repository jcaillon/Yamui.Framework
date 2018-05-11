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

        protected const int FormButtonWidth = 30;
        protected const int FormButtonHeight = 20;
        protected const int ResizeIconSize = 8;

        #endregion

        #region Private

        /// <summary>
        /// Tooltip for close buttons
        /// </summary>
        private HtmlToolTip _mainFormToolTip = new HtmlToolTip();

        private Dictionary<WindowButtons, YamuiFormButton> _windowButtonList = new Dictionary<WindowButtons, YamuiFormButton>();

        private Rectangle _resizeRectangle;
        private bool _trackLeave;

        #endregion

        #region Properties

        /// <summary>
        /// The top part of the window that makes it dragable
        /// </summary>
        [Browsable(false)]
        public override int TitleBarHeight => FormButtonHeight + BorderWidth;

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
            get { return new Padding(BorderWidth, TitleBarHeight, BorderWidth + ResizeIconSize, BorderWidth + ResizeIconSize); }
        }

        /// <summary>
        /// The x position of the first (i.e. leftmost) button of this form
        /// </summary>
        [Browsable(false)]
        public int FirstButtonPosition { get; private set; }
        
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
                Color foreColor = YamuiThemeManager.Current.ButtonFg(YamuiThemeManager.Current.FormFore, false, false, false, false, IsActive);
                _resizeRectangle = new Rectangle(ClientRectangle.Right - ResizeIconSize - BorderWidth * 2, ClientRectangle.Bottom - ResizeIconSize - BorderWidth * 2, ResizeIconSize, ResizeIconSize);
                e.Graphics.PaintCachedImage(_resizeRectangle, ImageDrawerType.WindowResizeIcon, new Size(ResizeIconSize, ResizeIconSize), foreColor);
            }

            if (ControlBox) {
                int x = ClientRectangle.Width - BorderWidth - FormButtonWidth;
                DrawButton(ref x, WindowButtons.Close, e, true);
                DrawButton(ref x, WindowButtons.CloseAllVisible, e, CloseAllBox);
                DrawButton(ref x, WindowButtons.Maximize, e, MaximizeBox && Resizable);
                DrawButton(ref x, WindowButtons.Minimize, e, MinimizeBox && ShowInTaskbar);
                FirstButtonPosition = x + FormButtonWidth;
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
                    _trackLeave = false;
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

            if (result == WinApi.HitTest.HTCAPTION || result == WinApi.HitTest.HTTOP || result == WinApi.HitTest.HTTOPRIGHT) {
                var cursorLocation = PointToClient(new Point(lparam.ToInt32()));
                foreach (var yamuiFormButton in _windowButtonList.Values) {
                    if (cursorLocation.X >= yamuiFormButton.ClientRectangle.Left && cursorLocation.X <= yamuiFormButton.ClientRectangle.Right) {
                        yamuiFormButton.IsHovered = true;
                        result = WinApi.HitTest.HTBORDER;

                        // track mouse leaving (otherwise the WM_NCMOUSELEAVE message would not fire)
                        if (!_trackLeave) {
                            WinApi.TRACKMOUSEEVENT tme = new WinApi.TRACKMOUSEEVENT();
                            tme.cbSize = (uint) Marshal.SizeOf(tme);
                            tme.dwFlags = (uint) (WinApi.TMEFlags.TME_LEAVE | WinApi.TMEFlags.TME_NONCLIENT);
                            tme.hwndTrack = Handle;
                            WinApi.TrackMouseEvent(tme);
                            _trackLeave = true;
                        }
                    } else {
                        yamuiFormButton.IsHovered = false;
                        yamuiFormButton.IsPressed = false;
                    }
                }
            } else if (result == WinApi.HitTest.HTCLIENT && Resizable) {
                var cursorLocation2 = PointToClient(new Point(lparam.ToInt32()));
                if (cursorLocation2.X >= _resizeRectangle.Left && cursorLocation2.Y >= _resizeRectangle.Top) {
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
                button.ClientRectangle = new Rectangle(x, BorderWidth, FormButtonWidth, FormButtonHeight);
                button.OnPaint(e);
                x -= button.ClientRectangle.Width;
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
                    OnCloseAllNotif?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

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
                e.Graphics.PaintRectangle(ClientRectangle, BackColor);

                Color foreColor = YamuiThemeManager.Current.ButtonFg(YamuiThemeManager.Current.FormFore, false, false, IsHovered, IsPressed, _parent.IsActive);

                var imageSize = ClientRectangle.Height / 2;
                imageSize = imageSize - (imageSize % 2); // ensure imageSize is pair
                var imagRectangle = new Rectangle(ClientRectangle.X + ClientRectangle.Width / 2 - imageSize / 2, ClientRectangle.Y + ClientRectangle.Height / 2 - imageSize / 2, imageSize, imageSize);
                e.Graphics.PaintCachedImage(imagRectangle, ImageType, new Size(imageSize, imageSize), foreColor);
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

            private ImageDrawerType ImageType {
                get {
                    switch (Type) {
                        case WindowButtons.Minimize:
                            return ImageDrawerType.Minimize;
                        case WindowButtons.Restore:
                            return ImageDrawerType.Restore;
                        case WindowButtons.Maximize:
                            return ImageDrawerType.Maximize;
                        case WindowButtons.CloseAllVisible:
                            return ImageDrawerType.CloseAll;
                        case WindowButtons.Close:
                            return ImageDrawerType.Close;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            private Color BackColor {
                get {
                    if (IsPressed)
                        return YamuiThemeManager.Current.AccentColor;
                    if (IsHovered)
                        return YamuiThemeManager.Current.ButtonHoverBack;
                    return YamuiThemeManager.Current.FormBack;
                }
            }

            //public WinApi.HitTest HitTest {
            //    get {
            //        switch (Type) {
            //            case WindowButtons.Minimize:
            //                return WinApi.HitTest.HTMINBUTTON;
            //            case WindowButtons.Restore:
            //            case WindowButtons.Maximize:
            //                return WinApi.HitTest.HTMAXBUTTON;
            //            case WindowButtons.CloseAllVisible:
            //                return WinApi.HitTest.HTZOOM;
            //            case WindowButtons.Close:
            //                return WinApi.HitTest.HTCLOSE;
            //            default:
            //                throw new ArgumentOutOfRangeException();
            //        }
            //    }
            //}


        }

        #endregion

        #endregion
    }
}