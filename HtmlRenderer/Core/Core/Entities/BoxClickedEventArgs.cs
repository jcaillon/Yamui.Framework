﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (BoxClickedEventArgs.cs) is part of YamuiFramework.
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

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities {
    public sealed class BoxClickedEventArgs : EventArgs {
        /// <summary>
        /// The value of the attribute "clickable" of the box
        /// </summary>
        public readonly string Attribute;

        /// <summary>
        /// Number of clicks that triggers this shit
        /// </summary>
        public readonly int NumberOfClicks;

        public bool Handled;

        public BoxClickedEventArgs(string attribute, int numberOfClicks) {
            Attribute = attribute;
            NumberOfClicks = numberOfClicks;
        }
    }
}