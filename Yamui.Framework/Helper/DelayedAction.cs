﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (DelayedAction.cs) is part of YamuiFramework.
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
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Yamui.Framework.Helper {
    /// <summary>
    /// Simple class to delay an action
    /// </summary>
    public class DelayedAction : IDisposable {
        #region Static

        private static List<DelayedAction> _savedDelayedActions = new List<DelayedAction>();

        /// <summary>
        /// Start a new delayed action (starts a new task after the given delay)
        /// </summary>
        public static DelayedAction StartNew(int msDelay, Action toDo) {
            var created = new DelayedAction(msDelay, toDo);
            _savedDelayedActions.Add(created);
            return created;
        }

        /// <summary>
        /// Clean all delayed actions started
        /// </summary>
        public static void CleanAll() {
            foreach (var action in _savedDelayedActions.ToList().Where(action => action != null)) {
                action.Stop();
            }
        }

        #endregion

        #region private fields

        private Timer _timer;

        private Action _toDo;

        #endregion

        #region Life and death

        /// <summary>
        /// Use this class to do an action after a given delay
        /// </summary>
        private DelayedAction(int msDelay, Action toDo) {
            _savedDelayedActions.Add(this);
            _toDo = toDo;
            _timer = new Timer {
                AutoReset = false,
                Interval = msDelay
            };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        public void Dispose() {
            Stop();
        }

        #endregion

        #region private

        /// <summary>
        /// Stop the recurrent action
        /// </summary>
        private void Stop() {
            try {
                if (_timer != null) {
                    _timer.Stop();
                    _timer.Close();
                }
            } catch (Exception) {
                // clean up proc
            } finally {
                _savedDelayedActions.Remove(this);
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            Task.Factory.StartNew(_toDo);
            Stop();
        }

        #endregion
    }
}