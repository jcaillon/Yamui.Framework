﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (TabOrderManager.cs) is part of YamuiFramework.
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
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace Yamui.Framework.Helper {
    /// <summary>
    /// Dynamically determine and set a tab order for a container and children according to a given strategy.
    /// http://www.codeproject.com/Articles/8406/Automatic-Runtime-Tab-Order-Management-for-Windows
    /// / In constructor after InitializeComponent (or whatever other code might set  controls' TabIndex properties).
    /// (new TabOrderManager(this)).SetTabOrder(TabOrderManager.TabScheme.AcrossFirst);
    /// </summary>
    public class TabOrderManager {
        /// <summary>
        /// Compare two controls in the selected tab scheme.
        /// </summary>
        private class TabSchemeComparer : IComparer {
            private TabScheme _comparisonScheme;

            #region IComparer Members

            public int Compare(object x, object y) {
                Control control1 = x as Control;
                Control control2 = y as Control;

                if (control1 == null || control2 == null) {
                    Debug.Assert(false, "Attempting to compare a non-control");
                }

                if (_comparisonScheme == TabScheme.None) {
                    // Nothing to do.
                    return 0;
                }

                if (_comparisonScheme == TabScheme.AcrossFirst) {
                    // The primary direction to sort is the y direction (using the Top property).
                    // If two controls have the same y coordination, then we sort them by their x's.
                    if (control1.Top < control2.Top) {
                        return -1;
                    }
                    if (control1.Top > control2.Top) {
                        return 1;
                    }
                    return (control1.Left.CompareTo(control2.Left));
                }
                // The primary direction to sort is the x direction (using the Left property).
                // If two controls have the same x coordination, then we sort them by their y's.
                if (control1.Left < control2.Left) {
                    return -1;
                }
                if (control1.Left > control2.Left) {
                    return 1;
                }
                return (control1.Top.CompareTo(control2.Top));
            }

            #endregion

            /// <summary>
            /// Create a tab scheme comparer that compares using the given scheme.
            /// </summary>
            /// <param name="scheme"></param>
            public TabSchemeComparer(TabScheme scheme) {
                _comparisonScheme = scheme;
            }
        }

        /// <summary>
        /// The container whose tab order we manage.
        /// </summary>
        private Control _container;

        /// <summary>
        /// Hash of controls to schemes so that individual containers can have different ordering
        /// strategies than their parents.
        /// </summary>
        private Hashtable _schemeOverrides;

        /// <summary>
        /// The tab index we start numbering from when the tab order is applied.
        /// </summary>
        private int _curTabIndex;

        /// <summary>
        /// The general tab-ordering strategy (i.e. whether we tab across rows first, or down columns).
        /// </summary>
        public enum TabScheme {
            None,
            AcrossFirst,
            DownFirst
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">The container whose tab order we manage.</param>
        public TabOrderManager(Control container) {
            _container = container;
            _curTabIndex = 0;
            _schemeOverrides = new Hashtable();
        }

        /// <summary>
        /// Construct a tab order manager that starts numbering at the given tab index.
        /// </summary>
        /// <param name="container">The container whose tab order we manage.</param>
        /// <param name="curTabIndex">Where to start numbering.</param>
        /// <param name="schemeOverrides">List of controls with explicitly defined schemes.</param>
        private TabOrderManager(Control container, int curTabIndex, Hashtable schemeOverrides) {
            _container = container;
            _curTabIndex = curTabIndex;
            _schemeOverrides = schemeOverrides;
        }

        /// <summary>
        /// Explicitly set a tab order scheme for a given (presumably container) control.
        /// </summary>
        /// <param name="c">The control to set the scheme for.</param>
        /// <param name="scheme">The requested scheme.</param>
        public void SetSchemeForControl(Control c, TabScheme scheme) {
            _schemeOverrides[c] = scheme;
        }

        /// <summary>
        /// Recursively set the tab order on this container and all of its children.
        /// </summary>
        /// <param name="scheme">The tab ordering strategy to apply.</param>
        /// <returns>The next tab index to be used.</returns>
        public int SetTabOrder(TabScheme scheme) {
            // Tab order isn't important enough to ever cause a crash, so replace any exceptions
            // with assertions.
            try {
                ArrayList controlArraySorted = new ArrayList();
                controlArraySorted.AddRange(_container.Controls);
                controlArraySorted.Sort(new TabSchemeComparer(scheme));

                foreach (Control c in controlArraySorted) {
                    c.TabIndex = _curTabIndex++;
                    if (c.Controls.Count > 0) {
                        // Control has children -- recurse.
                        TabScheme childScheme = scheme;
                        if (_schemeOverrides.Contains(c)) {
                            childScheme = (TabScheme) _schemeOverrides[c];
                        }
                        _curTabIndex = (new TabOrderManager(c, _curTabIndex, _schemeOverrides)).SetTabOrder(childScheme);
                    }
                }

                return _curTabIndex;
            } catch (Exception e) {
                Debug.Assert(false, "Exception in TabOrderManager.SetTabOrder:  " + e.Message);
            }
            return 0;
        }
    }
}