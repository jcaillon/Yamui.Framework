using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.Native;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {

    public partial class YamuiNotifications : YamuiForm {

        #region fields
        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        private static List<YamuiNotifications> _openNotifications = new List<YamuiNotifications>();
        private bool _allowFocus;
        private IntPtr _currentForegroundWindow;
        //private Timer _lifeTimer;
        private const int DefaultWidth = 300;
        private int _duration;
        #endregion

        public YamuiNotifications(string body, int duration) {
            InitializeComponent();

            Load += YamuiNotificationsLoad;
            Activated += YamuiNotificationsActivated;
            Shown += YamuiNotificationsShown;
            FormClosed += YamuiNotificationsFormClosed;
            contentLabel.LinkClicked += OnLinkClicked;

            // resize form
            int j = 0;
            int compWidth = DefaultWidth;
            do {
                Width = compWidth;
                contentLabel.Text = body;
                var compHeight = contentLabel.Height + 20;
                compHeight = Math.Min(compHeight, Screen.PrimaryScreen.WorkingArea.Height);
                Size = new Size(compWidth, compHeight);
                compWidth = compWidth * (compHeight / compWidth);
                j++;
            } while (j < 2 && Height > Width);

            progressPanel.UseCustomBackColor = true;
            progressPanel.BackColor = ThemeManager.Current.AccentColor;
            progressPanel.Width = Width - 2;
            _duration = duration*1000;
            //_lifeTimer = new Timer();
            //_lifeTimer.Tick += (sender, args) => Close();
            //if (duration < 0)
            //    duration = int.MaxValue;
            //else
            //    duration = duration * 1000;
            //_lifeTimer.Interval = duration;

            // fade out animation
            Opacity = 0d;
            Tag = false;
            Closing += (sender, args) => {
                if ((bool)Tag) return;
                args.Cancel = true;
                Tag = true;
                var t = new Transition(new TransitionType_Acceleration(200));
                t.add(this, "Opacity", 0d);
                t.TransitionCompletedEvent += (o, args1) => { Close(); };
                t.run();
            };
            // fade in animation
            Transition.run(this, "Opacity", 1d, new TransitionType_Acceleration(200));
        }

        #region Methods

        /// <summary>
        /// Displays the form
        /// </summary>
        /// <remarks>
        /// Required to allow the form to determine the current foreground window before being displayed
        /// </remarks>
        public new void Show() {
            // Determine the current foreground window so it can be reactivated each time this form tries to get the focus
            _currentForegroundWindow = WinApi.GetForegroundWindow();
            base.Show();
        }

        #endregion // Methods

        #region Event Handlers

        protected override void OnMouseDown(MouseEventArgs e) {
            Close();
        }

        private void YamuiNotificationsLoad(object sender, EventArgs e) {
            // Display the form just above the system tray.
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width,
                                      Screen.PrimaryScreen.WorkingArea.Height - Height - 5);

            // Move each open form upwards to make room for this one
            foreach (YamuiNotifications openForm in _openNotifications) {
                openForm.Top -= Height + 5;
            }

            _openNotifications.Add(this);
            //_lifeTimer.Start();
        }

        private void YamuiNotificationsActivated(object sender, EventArgs e) {
            // Prevent the form taking focus when it is initially shown
            if (!_allowFocus) {
                // Activate the window that previously had focus
                WinApi.SetForegroundWindow(_currentForegroundWindow);
            }
        }

        private void YamuiNotificationsShown(object sender, EventArgs e) {
            // Once the animation has completed the form can receive focus
            _allowFocus = true;
            var t = new Transition(new TransitionType_Linear(_duration));
            t.add(progressPanel, "Width", 0);
            t.TransitionCompletedEvent += (o, args) => { Close(); };
            t.run();
        }

        private void YamuiNotificationsFormClosed(object sender, FormClosedEventArgs e) {
            // Move down any open forms above this one
            foreach (YamuiNotifications openForm in _openNotifications) {
                if (openForm == this) {
                    // Remaining forms are below this one
                    break;
                }
                openForm.Top += Height + 5;
            }
            _openNotifications.Remove(this);
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e) {
            var handler = LinkClicked;
            if (handler != null)
                handler(this, e);
        }

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e) {
            OnLinkClicked(e);
        }

        #endregion // Event Handlers
    }
}
