﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ListItem.cs) is part of YamuiFramework.
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

using System.Collections.Generic;
using System.Drawing;

namespace Yamui.Framework.Controls.YamuiList {

    #region ListItem

    /// <summary>
    /// Describes a basic item of a scroll list
    /// </summary>
    public class ListItem {
        #region private

        #endregion

        #region Virtual properties

        /// <summary>
        /// The piece of text displayed in the list
        /// </summary>
        public virtual string DisplayText { get; set; }

        /// <summary>
        /// The item is disabled or not (a separator is necesseraly disabled)
        /// </summary>
        public virtual bool IsDisabled { get; set; }

        /// <summary>
        /// true if the item is a separator
        /// </summary>
        public virtual bool IsSeparator { get; set; }

        #endregion
    }

    #endregion

    #region FilteredItem

    /// <summary>
    /// Adds attributes that allow to filter the list of items to display based on a filter string, 
    /// the method FilterApply allows to compute the attributes
    /// </summary>
    public class FilteredListItem : ListItem {
        #region Constructor

        public FilteredListItem() {
            InternalFilterFullyMatch = true;
        }

        #endregion

        #region internal filter mechanism

        /// <summary>
        /// The dispersion level with which the filterString matches the DisplayText
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public int InternalFilterDispertionLevel { get; private set; }

        /// <summary>
        /// True of the filterString fully matches DisplayText
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public bool InternalFilterFullyMatch { get; private set; }

        /// <summary>
        /// The way filterString matches DisplayText
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public List<CharacterRange> InternalFilterMatchedRanges { get; private set; }

        /// <summary>
        /// Call this method to compute the value of
        /// FilterDispertionLevel, FilterFullyMatch, FilterMatchedRanges
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public void InternalFilterApply(string filterString, YamuiFilteredList.FilterCaseType caseType) {

            InternalFilterMatchedRanges = new List<CharacterRange>();
            InternalFilterFullyMatch = true;
            InternalFilterDispertionLevel = 0;

            // not filtering, everything should be included
            if (string.IsNullOrEmpty(filterString))
                return;

            // exclude the separator items and empty text from a match when searching
            if (IsSeparator || string.IsNullOrEmpty(DisplayText)) {
                InternalFilterFullyMatch = false;
                return;
            }

            var lcText = DisplayText;
            var textLenght = lcText.Length;
            var filterLenght = filterString.Length;

            if (caseType == YamuiFilteredList.FilterCaseType.Insensitive) {
                lcText = lcText.ToLower();
                filterString = filterString.ToLower();
            }

            int pos = 0;
            int posFilter = 0;
            bool matching = false;
            int startMatch = 0;

            while (pos < textLenght) {

                // remember matching state at the beginning of the loop
                bool wasMatching = matching;

                // we match the current char of the filter
                if (lcText[pos] == filterString[posFilter]) {
                    if (!matching) {
                        matching = true;
                        startMatch = pos;
                    }
                    posFilter++;
                    // we matched the entire filter
                    if (posFilter >= filterLenght) {
                        InternalFilterMatchedRanges.Add(new CharacterRange(startMatch, pos - startMatch + 1));
                        break;
                    }
                } else {
                    matching = false;

                    // gap between match mean more penalty than finding the match later in the string
                    if (posFilter > 0) {
                        InternalFilterDispertionLevel += 900;
                    } else {
                        InternalFilterDispertionLevel += 30;
                    }
                }
                // we stopped matching, remember matching range
                if (!matching && wasMatching)
                    InternalFilterMatchedRanges.Add(new CharacterRange(startMatch, pos - startMatch));
                pos++;
            }

            // put the exact matches first
            if (filterLenght != textLenght)
                InternalFilterDispertionLevel += 1;

            // we reached the end of the input, if we were matching stuff, remember matching range
            if (pos >= textLenght && matching)
                InternalFilterMatchedRanges.Add(new CharacterRange(startMatch, pos - 1 - startMatch));

            // we didn't match the entire filter
            if (posFilter < filterLenght)
                InternalFilterFullyMatch = false;
        }

        #endregion
    }

    #endregion

    #region FilteredTypeItem

    /// <summary>
    /// Adds a layer "type" for each item, they are then categorized in a particular "type" which can
    /// be used to quickly filter the items through a set of "type" buttons
    /// </summary>
    public class FilteredTypeListItem : FilteredListItem {
        #region Virtual properties

        /// <summary>
        /// to override, should return the image to display for this item
        /// If null, the image corresponding to ItemTypeImage will be used instead
        /// </summary>
        public virtual Image ItemImage { get; set; }

        /// <summary>
        /// to override, should return this item type (a unique int for each item type)
        /// if the value is strictly inferior to 0, the button for this type will not appear
        /// on the bottom of list
        /// </summary>
        public virtual int ItemType { get; set; }

        /// <summary>
        /// to override, should return the image that will be used to identify this item
        /// type, it will be used for the bottom buttons of the list
        /// All items of a given type should return the same image! The image used for the 
        /// bottom buttons will be that of the first item found for the given type
        /// </summary>
        public virtual Image ItemTypeImage { get; set; }

        /// <summary>
        /// The text that will displayed in the tooltip of the button corresponding to this item's type
        /// </summary>
        public virtual string ItemTypeText { get; set; }

        /// <summary>
        /// to override, should return true if the item is to be highlighted
        /// </summary>
        public virtual bool IsRowHighlighted { get; set; }

        /// <summary>
        /// to override, should return a string containing the subtext to display
        /// </summary>
        public virtual string SubText { get; set; }

        /// <summary>
        /// to override, should return a list of images to be displayed (in reverse order) for the item
        /// </summary>
        public virtual List<Image> TagImages { get; set; }

        #endregion
    }

    #endregion

    #region FilteredTypeItemTree

    /// <summary>
    /// Each item is now view as a node of the tree, allows to view the list as a tree
    /// </summary>
    public class FilteredTypeTreeListItem : FilteredTypeListItem {
        #region constant

        // used for the PathDescriptor property
        public const string TreePathSeparator = "||";

        #endregion

        #region Virtual properties

        /// <summary>
        /// to override, that should return the list of the children for this item (if any) or null
        /// </summary>
        public virtual List<FilteredTypeTreeListItem> Children { get; set; }

        /// <summary>
        /// Should this item be hidden when in searching mode?
        /// </summary>
        public virtual bool HideWhileSearching { get; set; }

        #endregion

        #region internal mechanism

        /// <summary>
        /// Is this item expanded? (useful only if has children), should only be used in read mode
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Returns the list of the children for this item, to be used internally by the YamuiList as it
        /// also sets properties for each child
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public List<FilteredTypeTreeListItem> GetItemChildren() {
            var list = Children;
            if (list != null && list.Count > 0) {
                var count = 0;
                foreach (var itemTree in list) {
                    itemTree.ParentNode = this;
                    itemTree.IsLastItem = false;
                    itemTree.ComputeItemProperties();
                    count++;
                }
                list[count - 1].IsLastItem = true;
            }
            return list;
        }

        /// <summary>
        /// Compute the path descriptor/level for this item (not required for root items)
        /// </summary>
        private void ComputeItemProperties() {
            // compute path descriptor
            _pathDescriptor = string.Empty;
            // compute node level
            Level = 0;

            var loopParent = ParentNode;
            while (loopParent != null) {
                _pathDescriptor = loopParent.DisplayText + "(" + loopParent.ItemType + ")" + TreePathSeparator + _pathDescriptor;
                Level++;
                loopParent = loopParent.ParentNode;
            }
            _pathDescriptor = _pathDescriptor + DisplayText + "(" + ItemType + ")";
        }

        /// <summary>
        /// Reset the properties of this item, to correctly draw the tree
        /// This method needs to be called on each root item when setting items
        /// </summary>
        public void ResetItemProperties() {
            ParentNode = null;
            _pathDescriptor = null;
            Level = 0;
        }

        /// <summary>
        /// does this item have children?
        /// </summary>
        public virtual bool CanExpand {
            get { return Children != null && Children.Count > 0; }
        }

        /// <summary>
        /// Parent node for this item (can be null if the item is on the root of the tree)
        /// </summary>
        public FilteredTypeTreeListItem ParentNode { get; private set; }

        /// <summary>
        /// This is the last item of the tree or the last item of its branch
        /// </summary>
        public bool IsLastItem { get; private set; }

        /// <summary>
        /// A list of this object ancestors (PARENT) node (null for root items)
        /// </summary>
        public List<FilteredTypeTreeListItem> Ancestors {
            get {
                if (ParentNode != null) {
                    var outList = new List<FilteredTypeTreeListItem>();
                    var loopParent = ParentNode;
                    while (loopParent != null) {
                        outList.Add(loopParent);
                        loopParent = loopParent.ParentNode;
                    }
                    return outList;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns a string describing the place of this item in the tree in the form of a path (using displaytext + (type)) :
        /// rootitem(10)/parent(2)/this(4)
        /// </summary>
        public string PathDescriptor {
            get { return _pathDescriptor ?? DisplayText + "(" + ItemType + ")"; }
        }

        private string _pathDescriptor;

        /// <summary>
        /// The level of the item defines its place in the tree, level 0 is the root, 1 is deeper and so on...
        /// </summary>
        public int Level { get; private set; }

        #endregion
    }

    #endregion
}