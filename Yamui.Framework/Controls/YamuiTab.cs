﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiTab.cs) is part of YamuiFramework.
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Yamui.Framework.Animations.Transitions;
using Yamui.Framework.Fonts;
using Yamui.Framework.Forms;
using Yamui.Framework.Helper;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    /// <summary>
    /// Basically, this class is a better tabControl, documentation to be done later 
    /// </summary>
    public sealed class YamuiTab : YamuiControl {

        #region Fields

        public bool GoBackButtonHasTabStop = false;

        /// <summary>
        /// Content of the form, the page displayed in it
        /// </summary>
        private List<YamuiMainMenu> _content;

        private bool _showingHidden;
        private Point _currentPoint = new Point(0, 0);
        private YamuiPage _currentPage;
        private YamuiButtonChar _goBackButton;
        private YamuiTabButtons _mainButtons;
        private YamuiTabButtons _secondaryButtons;

        /// <summary>
        /// Go back feature
        /// </summary>
        private Stack<Point> _formHistory = new Stack<Point>();

        private const int XOffsetTabButton = 32;
        private const int XOffsetPage = XOffsetTabButton + 25;
        private const int YOffsetPage = 50 + 15;

        #endregion

        #region Constructor

        public YamuiTab(List<YamuiMainMenu> content, YamuiMainAppli owner) {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);
            _content = content;
            _owner = owner;

            // padding
            TabStop = false;
        }

        public void Init() {
            // draw the go back button
            _goBackButton = new YamuiButtonChar {
                IconFontName = YamuiButtonChar.IconFontNameEnum.Wingdings,
                ButtonChar = "ç",
                FakeDisabled = true,
                Size = new Size(27, 27),
                TabStop = false
            };
            _goBackButton.ButtonPressed += GoBackButtonOnButtonPressed;
            Controls.Add(_goBackButton);
            _goBackButton.Location = new Point(0, 6);

            // draw the menus
            _mainButtons = new YamuiTabButtons(CurrentMainMenuList, 0) {
                Font = FontManager.GetFont(FontFunction.MenuMain),
                Height = 32,
                SpaceBetweenText = 15,
                Location = new Point(XOffsetTabButton, 0),
                Width = Width - XOffsetTabButton,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            _mainButtons.TabPressed += MainButtonsOnTabPressed;
            Controls.Add(_mainButtons);
            _secondaryButtons = new YamuiTabButtons(CurrentSecondaryMenuList, 0) {
                Font = FontManager.GetFont(FontFunction.MenuSecondary),
                Height = 18,
                SpaceBetweenText = 13,
                Location = new Point(XOffsetTabButton + 15, 32),
                Width = Width - XOffsetTabButton - 15,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            _secondaryButtons.TabPressed += SecondaryButtonsOnTabPressed;
            Controls.Add(_secondaryButtons);

            // display the page
            CurrentPage = new Point(0, 0);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Show the given page
        /// </summary>
        /// <param name="pageName"></param>
        public void ShowPage(string pageName) {
            PushHistory();
            CurrentPage = FindPage(pageName);
        }

        /// <summary>
        /// Execute the OnClose of the current page
        /// </summary>
        public void ExecuteOnClose() {
            _currentPage.OnHide();
        }

        #endregion

        #region Mechanic

        private Point CurrentPage {
            get { return _currentPoint; }
            set {
                // execute the OnClose for the current page
                if (_currentPage != null) {
                    _currentPage.OnHide();

                    // remove the current page
                    Controls.Remove(_currentPage);
                }

                // update private
                _currentPoint = value;
                _currentPage = _content[_currentPoint.X].SecTabs[_currentPoint.Y].Page;

                // tab animation (initialize)
                var doAnimate = TabAnimatorInit();

                // Update main menu
                if (_showingHidden != _content[_currentPoint.X].Hidden) {
                    _showingHidden = !_showingHidden;
                    _mainButtons.UpdateList(CurrentMainMenuList, CurrentMainMenuIndex);
                } else {
                    _mainButtons.UpdateList(null, CurrentMainMenuIndex);
                }

                // update secondary menu
                _secondaryButtons.UpdateList(CurrentSecondaryMenuList, _currentPoint.Y);

                // display the new page
                _currentPage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                _currentPage.Location = new Point(XOffsetPage, YOffsetPage);
                _currentPage.Size = new Size(Width - XOffsetPage, Height - YOffsetPage);
                Controls.Add(_currentPage);

                Application.DoEvents();

                // tab animation (do if needed)
                if (doAnimate)
                    TabAnimatorStart();
                
                _currentPage.Focus();

                // Execute the page OnShow method
                _currentPage.OnShow();
            }
        }

        private void GoBackButtonOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            if (_formHistory.Count > 0) {
                CurrentPage = _formHistory.Pop();
                if (_formHistory.Count == 0) {
                    _goBackButton.FakeDisabled = true;
                    _goBackButton.TabStop = false;
                }
            }
        }

        private void MainButtonsOnTabPressed(object sender, TabPressedEventArgs tabPressedEventArgs) {
            var wantedIndex = tabPressedEventArgs.SelectedIndex;
            if (wantedIndex == CurrentMainMenuIndex)
                return;
            PushHistory();

            // save last visited page
            _content[CurrentPage.X].LastVisitedPage = CurrentPage.Y;

            // update current page
            var realIndex = 0;
            foreach (var mainMenu in _content) {
                if (!mainMenu.Hidden)
                    wantedIndex--;
                if (wantedIndex < 0)
                    break;
                realIndex++;
            }
            CurrentPage = new Point(realIndex, _content[realIndex].LastVisitedPage);

            // focus secondary menu
        }

        private void SecondaryButtonsOnTabPressed(object sender, TabPressedEventArgs tabPressedEventArgs) {
            if (tabPressedEventArgs.SelectedIndex == CurrentPage.Y)
                return;
            PushHistory();

            CurrentPage = new Point(CurrentPage.X, tabPressedEventArgs.SelectedIndex);
        }

        private List<string> CurrentMainMenuList {
            get { return _content.Where(tab => _showingHidden == tab.Hidden).Select(tab => tab.Name).ToList(); }
        }

        private List<string> CurrentSecondaryMenuList {
            get { return _content[CurrentPage.X].SecTabs.Select(tab => tab.Name).ToList(); }
        }

        private int CurrentMainMenuIndex {
            get { return _showingHidden ? 0 : _content.Where(menu => !menu.Hidden).ToList().FindIndex(menu => menu.RefName.Equals(_content[_currentPoint.X].RefName)); }
        }

        private void PushHistory() {
            // remember current page (if different from the previous one)
            if (_formHistory.Count == 0 || !_currentPoint.Equals(_formHistory.Peek())) {
                _formHistory.Push(_currentPoint);
                _goBackButton.FakeDisabled = false;
                _goBackButton.TabStop = GoBackButtonHasTabStop;
            }
        }

        /// <summary>
        /// find the secondary tab point corresponding to the input pageName
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        private Point FindPage(string pageName) {
            var output = new Point(0, 0);
            foreach (var mainTab in _content) {
                foreach (var secTab in mainTab.SecTabs) {
                    if (secTab.RefName.Equals(pageName))
                        return output;
                    output.Y++;
                }
                output.Y = 0;
                output.X++;
            }
            return new Point(0, 0);
        }

        // the following reference is used to always know the size and position of a secondary tabpage (for animation purposes)
        private static YamuiTabAnimation _animSmokeScreen;
        private YamuiMainAppli _owner;

        private bool TabAnimatorInit() {
            if (!YamuiThemeManager.TabAnimationAllowed) return false;

            // the principle is easy, we create a foreground form on top of our form with the same back ground,
            // and we animate its opacity value from 1 to 0 to effectivly create a fade in animation
            if (_animSmokeScreen == null) {
                _animSmokeScreen = new YamuiTabAnimation(_owner, new Rectangle(Left + Padding.Left, Top + Padding.Top, Width - Padding.Left - Padding.Right, Height - Padding.Top - Padding.Bottom)) {Opacity = 0d};
                return false;
            }
            _animSmokeScreen.Refresh();
            _animSmokeScreen.GoHide = false;

            return true;
        }

        private void TabAnimatorStart() {
            if (!YamuiThemeManager.TabAnimationAllowed) return;
            var t = new Transition(new TransitionType_Acceleration(500));
            t.add(_animSmokeScreen, "Opacity", 0d);
            t.TransitionCompletedEvent += (sender, args) => _animSmokeScreen.SafeSyncInvoke(form => form.GoHide = true);
            t.run();
        }

        #endregion

        public new Padding Padding {
            get {
                return new Padding(XOffsetPage, YOffsetPage, 0, 0);
            }
            set {
                base.Padding = value;
            }
        }

        #region Paint

        protected override void OnPaintBackground(PaintEventArgs e) {}

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.Clear(YamuiThemeManager.Current.FormBack);
        }

        #endregion

        #region YamuiTabAnimation

        internal class YamuiTabAnimation : YamuiSmokeScreen {
            #region fields

            /// <summary>
            /// Show the background image.. or not?
            /// </summary>
            public bool DontShowBackGroundImage {
                get { return _dontShowBackGroundImage; }
                set {
                    _dontShowBackGroundImage = value;
                    Invalidate();
                }
            }

            private bool _dontShowBackGroundImage = true;

            #endregion

            #region implement constructor

            public YamuiTabAnimation(Form owner, Rectangle pageRectangle) : base(owner, pageRectangle) {}

            #endregion

            #region Override paint method

            protected override void OnPaint(PaintEventArgs e) {
                e.Graphics.Clear(YamuiThemeManager.Current.FormBack);
                // background image?
                if (!DontShowBackGroundImage) {
                    var img = YamuiThemeManager.Current.BackgroundImage;
                    if (img != null) {
                        Rectangle rect = new Rectangle(ClientRectangle.Right - img.Width, ClientRectangle.Height - img.Height, img.Width, img.Height);
                        e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                    }
                }
            }

            #endregion
        }

        #endregion
        
    }

    #region Menu specs

    public class YamuiMainMenu {
        public string Name { get; private set; }
        public string RefName { get; private set; }
        public bool Hidden { get; private set; }
        public List<YamuiSecMenu> SecTabs { get; private set; }
        public int LastVisitedPage { get; set; }

        public YamuiMainMenu(string name, string refName, bool hidden, List<YamuiSecMenu> secTabs) {
            Name = name;
            Hidden = hidden;
            SecTabs = secTabs;
            RefName = refName ?? name;
        }
    }

    public class YamuiSecMenu {
        public string Name { get; private set; }
        public string RefName { get; private set; }
        public YamuiPage Page { get; private set; }

        public YamuiSecMenu(string name, string refName, YamuiPage page) {
            Name = name.ToUpper();
            Page = page;
            RefName = refName ?? name;
        }
    }

    #endregion
}