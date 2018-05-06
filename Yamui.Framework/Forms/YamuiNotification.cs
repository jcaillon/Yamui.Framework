﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiNotification.cs) is part of YamuiFramework.
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
using System.Linq;
using System.Windows.Forms;
using Yamui.Framework.Animations.Transitions;
using Yamui.Framework.Controls;
using Yamui.Framework.Helper;
using Yamui.Framework.HtmlRenderer.Core.Core.Entities;
using Yamui.Framework.HtmlRenderer.WinForms;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Forms {

    public sealed class YamuiNotification : YamuiFormButtons {

        #region Static

        private static List<YamuiNotification> _openNotifications = new List<YamuiNotification>();

        public static void CloseEverything() {
            try {
                foreach (var yamuiNotification in _openNotifications.ToList()) {
                    if (yamuiNotification != null) {
                        yamuiNotification.Close();
                        yamuiNotification.Dispose();
                    }
                }
            } catch (Exception) {
                // nothing much to do if this crashes
            }
        }

        /// <summary>
        /// The user clicked on the button to close all visible notifications
        /// </summary>
        private static void CloseAllNotif(object sender, EventArgs eventArgs) {
            CloseEverything();
        }

        #endregion

        #region Private
        
        private const int SpaceBetweenNotifications = 5;
        private const int PositionHorizontalMargin = 5;
        private const int PositionVerticalMargin = 5;

        private int _duration;
        private YamuiSimplePanel _progressSimplePanel;
        private Transition _closingTransition;
        private Screen _screen;

        private YamuiScrollPanel contentPanel;
        private HtmlLabel contentLabel;
        private HtmlLabel titleLabel;

        #endregion

        #region MyRegion

        [Category(nameof(Yamui))]
        [DefaultValue(NotificationPosition.BottomRight)]
        public NotificationPosition OnScreenPosition { get; set; }

        #endregion

        #region Constructor
        
        /// <summary>
        /// Create a new notification, to be displayed with Show() later
        /// </summary>
        public YamuiNotification(string htmlTitle, string htmlMessage, int duration, Screen screenToUse = null, int formMinWidth = 0, int formMaxWidth = 0, int formMaxHeight = 0, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null) : base(YamuiFormOption.IsPopup | YamuiFormOption.WithDropShadow | YamuiFormOption.AlwaysOnTop | YamuiFormOption.DontShowInAltTab | YamuiFormOption.DontActivateOnShow) {

            // close all notif button
            CloseAllBox = true;
            OnCloseAllNotif = CloseAllNotif;
            Movable = false;
            Resizable = false;
            ShowIcon = false;
            ShowInTaskbar = false;

            InitializeComponent();

            // correct input if needed
            _screen = screenToUse ?? Screen.PrimaryScreen;
            if (formMaxWidth == 0)
                formMaxWidth = _screen.WorkingArea.Width - 20;
            if (formMaxHeight == 0)
                formMaxHeight = _screen.WorkingArea.Height - 20;
            if (formMinWidth == 0)
                formMinWidth = 300;
            
            // Set title, it will define a new minimum width for the message box
            titleLabel.Location = new Point(5, 5);
            var space = FormButtonWidth + BorderWidth*2 + titleLabel.Location.X + 5;
            titleLabel.SetNeededSize(htmlTitle, formMinWidth - space, formMaxWidth - space, true);
            formMinWidth = formMinWidth.ClampMin(titleLabel.Width + space);
            var newPadding = Padding;
            newPadding.Bottom = newPadding.Bottom + (duration > 0 ? 8 : 0);
            newPadding.Top = titleLabel.Height + 10;
            Padding = newPadding;

            // set content label
            space = Padding.Left + Padding.Right;
            contentLabel.SetNeededSize(htmlMessage, formMinWidth - space, formMaxWidth - space, true);
            if (onLinkClicked != null)
                contentLabel.LinkClicked += onLinkClicked;

            // set form size
            Size = new Size(contentLabel.Width + space, (Padding.Top + Padding.Bottom + contentLabel.Height).ClampMax(formMaxHeight));
            if (contentPanel.VerticalScroll.HasScroll)
                Width += 10;
            MinimumSize = Size;

            // do we need to animate a panel on the bottom to visualise time left?
            if (duration > 0) {
                _progressSimplePanel = new YamuiSimplePanel {
                    BackColor = YamuiThemeManager.Current.AccentColor,
                    AutoScroll = false,
                    Location = new Point(1, Height - 9),
                    Name = "progressPanel",
                    Size = new Size(Width - 2, 8),
                    TabStop = false,
                    UseCustomBackColor = true
                };
                Controls.Add(_progressSimplePanel);
                _duration = duration*1000;
                
                _closingTransition = new Transition(new TransitionType_Linear(_duration));
                _closingTransition.add(_progressSimplePanel, "Width", 0);
                _closingTransition.TransitionCompletedEvent += (o, args) => { Close(); };
            } else
                _duration = 0;

        }

        private void InitializeComponent() {
            contentPanel = new YamuiScrollPanel();
            contentLabel = new HtmlLabel();
            titleLabel = new HtmlLabel();
            contentPanel.SuspendLayout();
            SuspendLayout();

            contentPanel.Controls.Add(contentLabel);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(5, 50);
            contentPanel.Name = "contentPanel";
            contentPanel.DisableBackgroundImage = true;
            contentPanel.Size = new Size(290, 270);
            contentPanel.TabIndex = 4;

            contentLabel.AutoSize = false;
            contentLabel.AutoSizeHeightOnly = true;
            contentLabel.BackColor = Color.Transparent;
            contentLabel.BaseStylesheet = null;
            contentLabel.Location = new Point(0, 0);
            contentLabel.Name = "contentLabel";
            contentLabel.Size = new Size(245, 15);
            contentLabel.TabIndex = 4;
            contentLabel.TabStop = false;
            contentLabel.Text = "contentLabel";

            titleLabel.AutoSize = false;
            titleLabel.AutoSizeHeightOnly = true;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.BaseStylesheet = null;
            titleLabel.CausesValidation = false;
            titleLabel.Enabled = false;
            titleLabel.IsContextMenuEnabled = false;
            titleLabel.IsSelectionEnabled = false;
            titleLabel.Location = new Point(5, 5);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(243, 15);
            titleLabel.TabIndex = 6;
            titleLabel.TabStop = false;
            titleLabel.Text = "titleLabel";

            Controls.Add(titleLabel);
            Controls.Add(contentPanel);
            Padding = new Padding(5, 50, 5, 5);
            contentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Point GetSpawnLocation() {
            Point output;
            switch (OnScreenPosition) {
                case NotificationPosition.TopLeft:
                    output = new Point(_screen.WorkingArea.X + PositionHorizontalMargin, _screen.WorkingArea.Y + PositionVerticalMargin);
                    break;
                case NotificationPosition.BottomLeft:
                    output = new Point(_screen.WorkingArea.X + PositionHorizontalMargin, _screen.WorkingArea.Y + _screen.WorkingArea.Height - Height - PositionVerticalMargin);
                    break;
                case NotificationPosition.TopRight:
                    output = new Point(_screen.WorkingArea.X + _screen.WorkingArea.Width - Width - PositionHorizontalMargin, _screen.WorkingArea.Y + PositionVerticalMargin);
                    break;
                default:
                    output = new Point(_screen.WorkingArea.X + _screen.WorkingArea.Width - Width - PositionHorizontalMargin, _screen.WorkingArea.Y + _screen.WorkingArea.Height - Height - PositionVerticalMargin);
                    break;
            }
            return output;
        }

        #region override

        /// <summary>
        /// A key was pressed on the form
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Escape:
                    // close the form
                    Close();
                    e.Handled = true;
                    break;
            }
            if (!e.Handled)
                base.OnKeyDown(e);
        }

        // get activated window
        protected override void OnShown(EventArgs e) {
            // Start the timer animation on the bottom of the notif
            _closingTransition?.run();
            base.OnShown(e);
        }
        
        protected override void OnActivated(EventArgs e) {
            // when the form is activated (i.e. when it's clicked on), remove the bottom animation
            if (_closingTransition != null) {
                _closingTransition.removeProperty(_closingTransition.TransitionedProperties.FirstOrDefault());
                _closingTransition = null;
                Controls.Remove(_progressSimplePanel);
                _progressSimplePanel.Dispose();
            }
            base.OnActivated(e);
        }

        protected override void OnLoad(EventArgs e) {
            // Display the form just above the system tray.
            Location = GetSpawnLocation();

            // Move each open form upwards to make room for this one
            foreach (YamuiNotification openForm in _openNotifications) {
                if (OnScreenPosition == NotificationPosition.TopLeft || OnScreenPosition == NotificationPosition.TopRight) {
                    openForm.Top += Height + SpaceBetweenNotifications;
                } else {
                    openForm.Top -= Height + SpaceBetweenNotifications;
                }
            }
            _openNotifications.Add(this);
            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e) {
            // Move down any open forms above this one
            foreach (YamuiNotification openForm in _openNotifications) {
                if (openForm == this) {
                    // Remaining forms are below this one
                    break;
                }
                if (OnScreenPosition == NotificationPosition.TopLeft || OnScreenPosition == NotificationPosition.TopRight) {
                    openForm.Top -= Height + SpaceBetweenNotifications;
                } else {
                    openForm.Top += Height + SpaceBetweenNotifications;
                }
            }
            _openNotifications.Remove(this);
            base.OnClosed(e);
        }

        #endregion
    }

    public enum NotificationPosition {
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight
    }
}