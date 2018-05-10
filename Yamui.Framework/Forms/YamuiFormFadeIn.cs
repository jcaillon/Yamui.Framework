#region header

// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormBaseFadeIn.cs) is part of YamuiFramework.
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
using System.Windows.Forms;
using Yamui.Framework.Animations.Transitions;

namespace Yamui.Framework.Forms {
    /// <summary>
    /// Form class that adds a fade in/out animation on form show/close
    /// </summary>
    public abstract class YamuiFormFadeIn : YamuiFormShadow {
        #region Private

        private bool _closingAnimationOnGoing;

        #endregion

        #region Life and death

        protected YamuiFormFadeIn(YamuiFormOption formOptions) : base(formOptions) {
            
        }

        #endregion

        #region Field

        /// <summary>
        /// Milliseconds duration for the fade in/fade out animation
        /// </summary>
        public virtual int AnimationDuration { get; set; }

        /// <summary>
        /// This field is used for the fade in/out animation, shouldn't be used by the user
        /// </summary>
        public virtual double AnimationOpacity {
            get { return Opacity; }
            set {
                try {
                    if (value < 0) {
                        Close();
                        return;
                    }
                    Opacity = value;
                } catch (Exception) {
                    // ignored
                }
            }
        }

        #endregion

        #region On closing

        protected override void OnClosing(CancelEventArgs e) {
            // cancel initialise close to run an animation, after that allow it
            if (!_closingAnimationOnGoing) {
                _closingAnimationOnGoing = true;
                e.Cancel = true;
                if (AnimationDuration > 0) {
                    Transition.run(this, "AnimationOpacity", 1d, -0.01d, new TransitionType_Acceleration(AnimationDuration), (o, args1) => { Dispose(); });
                } else {
                    Close();
                    Dispose();
                }
            } else {
                base.OnClosing(e);
            }
        }

        #endregion

        #region Forceclose

        public virtual void ForceClose() {
            _closingAnimationOnGoing = true;
            Close();
            Dispose();
        }

        #endregion

        #region Show

        /// <summary>
        /// Call this method to show the notification
        /// </summary>
        public new void Show() {
            if (AnimationDuration > 0)
                Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(AnimationDuration));
            base.Show();
        }

        public new void ShowDialog() {
            if (AnimationDuration > 0)
                Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(AnimationDuration));
            base.ShowDialog();
        }

        public new void Show(IWin32Window owner) {
            if (AnimationDuration > 0)
                Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(AnimationDuration));
            base.Show(owner);
        }

        public new void ShowDialog(IWin32Window owner) {
            if (AnimationDuration > 0)
                Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(AnimationDuration));
            base.ShowDialog(owner);
        }

        #endregion
    }
}