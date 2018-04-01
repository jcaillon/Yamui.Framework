﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (TransitionType_Deceleration.cs) is part of YamuiFramework.
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

namespace Yamui.Framework.Animations.Transitions {
    /// <summary>
    /// Manages a transition starting from a high speed and decelerating to zero by
    /// the end of the transition.
    /// </summary>
    public class TransitionType_Deceleration : ITransitionType {
        #region Public methods

        /// <summary>
        /// Constructor. You pass in the time that the transition 
        /// will take (in milliseconds).
        /// </summary>
        public TransitionType_Deceleration(int iTransitionTime) {
            if (iTransitionTime <= 0) {
                throw new Exception("Transition time must be greater than zero.");
            }
            m_dTransitionTime = iTransitionTime;
        }

        #endregion

        #region ITransitionMethod Members

        /// <summary>
        /// Works out the percentage completed given the time passed in.
        /// This uses the formula:
        ///   s = ut + 1/2at^2
        /// The initial velocity is 2, and the acceleration to get to 1.0
        /// at t=1.0 is -2, so the formula becomes:
        ///   s = t(2-t)
        /// </summary>
        public void onTimer(int iTime, out double dPercentage, out bool bCompleted) {
            // We find the percentage time elapsed...
            double dElapsed = iTime/m_dTransitionTime;
            dPercentage = dElapsed*(2.0 - dElapsed);
            if (dElapsed >= 1.0) {
                dPercentage = 1.0;
                bCompleted = true;
            } else {
                bCompleted = false;
            }
        }

        #endregion

        #region Private data

        private double m_dTransitionTime;

        #endregion
    }
}