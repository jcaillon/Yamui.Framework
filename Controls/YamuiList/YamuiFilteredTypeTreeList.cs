﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFilteredTypeTreeList.cs) is part of YamuiFramework.
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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {
    /// <summary>
    /// This class is the most complicated, obviously
    /// The difficulty being handling the filter string correctly;
    /// basically, we have two separate modes (see SearchMode)
    /// This is not the conventional tree searching but this makes more sense to me
    /// </summary>
    public class YamuiFilteredTypeTreeList : YamuiFilteredTypeList {
        #region constants

        /// <summary>
        /// Width allowed to draw the arrow/tree branches
        /// </summary>
        protected const int TreeWidth = 8;

        #endregion

        #region private fields

        /// <summary>
        /// Allows to save the expansion state for each node of the tree
        /// </summary>
        private Dictionary<string, bool> _savedState = new Dictionary<string, bool>();

        /// <summary>
        /// Root items passed to the SetItems method
        /// </summary>
        protected List<FilteredTypeTreeListItem> _treeRootItems;

        private SearchModeOption _searchMode;

        /// <summary>
        /// True when in FilterSortWithNoParent mode + filter string not empty
        /// </summary>
        protected bool _isSearching;

        private bool _showTreeBranches = true;
        protected int _nodeExpandClickMargin = 20;

        #endregion

        #region public properties

        /// <summary>
        /// Two modes can be active when the filter string isn't empty :
        /// - either we filter the item + we include their parent and still display the list as a tree
        /// - or we filter + sort and display the list as a simple filtered list with types
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SearchModeOption SearchMode {
            get { return _searchMode; }
            set {
                _searchMode = value;
                FilterString = FilterString;
            }
        }

        /// <summary>
        /// Set this to filter the list with the given text
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string FilterString {
            get { return _filterString; }
            set {
                _filterString = value.Trim();
                if (SetIsSearching(_filterString, SearchMode))
                    // base.FilterString = value; is done in SetIsSearching
                    return;
                base.FilterString = value;
            }
        }

        /// <summary>
        /// If true, will display the lines representing the tree branches
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowTreeBranches {
            get { return _showTreeBranches; }
            set { _showTreeBranches = value; }
        }

        /// <summary>
        /// Root items passed to the SetItems method
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected List<FilteredTypeTreeListItem> TreeRootItems {
            get { return _treeRootItems; }
            set { _treeRootItems = value; }
        }

        /// <summary>
        /// When the user click on the arrow of a row, it either collapse or expand the node
        /// Here, you can increase the margin taken to know if the click is on the arrow or not
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int NodeExpandClickMargin {
            get { return _nodeExpandClickMargin; }
            set { _nodeExpandClickMargin = value; }
        }

        #endregion

        #region Enum

        private enum ForceExpansion {
            Idle,
            ForceExpand,
            ForceCollapse
        }

        /// <summary>
        /// Two modes can be active when the filter string isn't empty :
        /// - either we filter the item + we include their parent and still display the list as a tree
        /// - or we filter + sort and display the list as a simple filtered list with types
        /// </summary>
        public enum SearchModeOption {
            SearchSortWithNoParent,
            FilterOnlyAndIncludeParent
        }

        #endregion

        #region Set

        /// <summary>
        /// Set the items that will be displayed in the list
        /// </summary>
        public override void SetItems(List<ListItem> listItems) {
            var firstItem = listItems.FirstOrDefault();
            if (firstItem != null && !(firstItem is FilteredTypeTreeListItem))
                throw new Exception("listItems should contain objects of type FilteredTypeTreeListItem");

            TreeRootItems = listItems.Cast<FilteredTypeTreeListItem>().ToList();

            foreach (var item in TreeRootItems) {
                item.ResetItemProperties();
            }

            if (_isSearching)
                base.SetItems(GetFullItemsList(TreeRootItems));
            else
                base.SetItems(GetExpandedItemsList(TreeRootItems, ForceExpansion.Idle));
        }

        /// <summary>
        /// Returns the full list of items to be displayed, taking into account expanded items
        /// </summary>
        private List<ListItem> GetExpandedItemsList(List<FilteredTypeTreeListItem> list, ForceExpansion forceExpansion) {
            if (list == null)
                return new List<ListItem>();

            var outList = new List<ListItem>();

            foreach (var item in list) {
                outList.Add(item);
                var descriptor = item.PathDescriptor;
                if (item.CanExpand) {
                    // force expand/collapse?
                    if (forceExpansion != ForceExpansion.Idle) {
                        if (_savedState.ContainsKey(descriptor))
                            _savedState[descriptor] = forceExpansion == ForceExpansion.ForceExpand;
                        else
                            _savedState.Add(descriptor, forceExpansion == ForceExpansion.ForceExpand);
                    }

                    // restore the expand state of the item if needed
                    if (_savedState.ContainsKey(descriptor))
                        item.IsExpanded = _savedState[descriptor];

                    if (item.IsExpanded) {
                        var children = GetExpandedItemsList(item.GetItemChildren(), forceExpansion);
                        if (children != null)
                            outList.AddRange(children);
                    } else if (forceExpansion != ForceExpansion.Idle) {
                        // to also force the savedState of the children node
                        GetExpandedItemsList(item.GetItemChildren(), forceExpansion);
                    }
                }
            }

            return outList;
        }

        /// <summary>
        /// Returns the full list of items in the tree
        /// </summary>
        private List<ListItem> GetFullItemsList(List<FilteredTypeTreeListItem> list) {
            if (list == null)
                return new List<ListItem>();
            var outList = new List<ListItem>();
            foreach (var item in list) {
                outList.Add(item);
                if (item.CanExpand) {
                    var children = GetFullItemsList(item.GetItemChildren());
                    if (children != null)
                        outList.AddRange(children);
                }
            }
            return outList;
        }

        #endregion

        #region Expand/Retract

        /// <summary>
        /// Call this method to expand the whole tree
        /// </summary>
        public void ForceAllToExpand() {
            if (!_isSearching)
                base.SetItems(GetExpandedItemsList(TreeRootItems, ForceExpansion.ForceExpand));
        }

        /// <summary>
        /// Call this method to collapse the whole tree
        /// </summary>
        public void ForceAllToCollapse() {
            if (!_isSearching)
                base.SetItems(GetExpandedItemsList(TreeRootItems, ForceExpansion.ForceCollapse));
        }

        /// <summary>
        /// ReCompute the whole list while taking into account expanded items, to be called when you change the 
        /// IsExpanded property of items on the list
        /// </summary>
        public void ApplyExpansionState() {
            if (!_isSearching)
                base.SetItems(GetExpandedItemsList(TreeRootItems, ForceExpansion.Idle));
        }

        /// <summary>
        /// The list automatically saves the expansion state of a node when it is changed,
        /// if you want the states to be forgotten, call this method
        /// </summary>
        public void ClearSavedExpansionState() {
            _savedState.Clear();
        }

        /// <summary>
        /// Toggle expand/collapse for the an item at the given index
        /// </summary>
        private bool ExpandCollapse(int itemIndex, ForceExpansion forceExpansion) {
            var selectedItem = GetItem(itemIndex);
            if (selectedItem == null)
                return false;

            // handles a node expansion
            var currentItem = selectedItem as FilteredTypeTreeListItem;
            if (currentItem != null && currentItem.CanExpand) {
                string currentItemPathDescriptor = currentItem.PathDescriptor;

                // switch state
                if (forceExpansion != ForceExpansion.Idle)
                    currentItem.IsExpanded = (forceExpansion == ForceExpansion.ForceExpand);
                else
                    currentItem.IsExpanded = !currentItem.IsExpanded;

                // saves expansion state
                if (_savedState.ContainsKey(currentItemPathDescriptor))
                    _savedState[currentItemPathDescriptor] = currentItem.IsExpanded;
                else
                    _savedState.Add(currentItemPathDescriptor, currentItem.IsExpanded);

                ApplyExpansionState();

                return true;
            }

            return false;
        }

        #endregion

        #region Draw list

        /// <summary>
        /// Called by default to paint the row if no OnRowPaint is defined
        /// </summary>
        protected override void RowPaint(ListItem item, YamuiListRow row, PaintEventArgs e) {
            // background
            var backColor = item.IsSeparator ?
                YamuiThemeManager.Current.MenuBg(false, false, !item.IsDisabled) :
                YamuiThemeManager.Current.MenuBg(row.IsSelected, row.IsHovered, !item.IsDisabled);
            e.Graphics.Clear(backColor);

            var curItem = item as FilteredTypeTreeListItem;
            if (curItem != null) {
                var drawRect = row.ClientRectangle;
                drawRect.Height = RowHeight;
                var shiftedDrawRect = drawRect;

                // draw the tree structure
                if (!_isSearching)
                    shiftedDrawRect = RowPaintTree(e.Graphics, curItem, drawRect, row);

                // case of a separator
                if (item.IsSeparator)
                    RowPaintSeparator(e.Graphics, curItem.Level == 0 ? drawRect : shiftedDrawRect);
                else
                    DrawFilteredTypeRow(e.Graphics, curItem, shiftedDrawRect, row);
            }
        }

        protected virtual Rectangle RowPaintTree(Graphics g, FilteredTypeTreeListItem curItem, Rectangle drawRect, YamuiListRow row) {
            var foreColor = YamuiThemeManager.Current.MenuFg(row.IsSelected, row.IsHovered, !curItem.IsDisabled);
            var shiftedDrawRect = drawRect;

            // draw the branches of the tree
            if (ShowTreeBranches) {
                using (var linePen = new Pen(!(curItem.IsDisabled || curItem.IsSeparator) ? YamuiThemeManager.Current.SubTextFore : foreColor, 1.5f) {DashStyle = DashStyle.Solid}) {
                    var pos = drawRect.X + TreeWidth/2;
                    if (curItem.Level >= 1)
                        pos += (curItem.Level - 1)*TreeWidth;

                    // Draw the horizontal line that goes to the arrow
                    if (curItem.Level > 0 && !curItem.IsSeparator) {
                        g.DrawLine(linePen, pos, drawRect.Y + drawRect.Height/2 - 1, pos + (curItem.CanExpand ? TreeWidth/2 : TreeWidth), drawRect.Y + drawRect.Height/2 - 1);
                    }

                    // draw the vertical lines
                    var familyNode = curItem;
                    while (familyNode != null && familyNode.Level > 0) {
                        // the current item is the last item of its parent
                        if (familyNode.Level == curItem.Level && familyNode.IsLastItem)
                            g.DrawLine(linePen, pos, drawRect.Y, pos, drawRect.Y + drawRect.Height/2 - 1);
                        else if (!familyNode.IsLastItem)
                            g.DrawLine(linePen, pos, drawRect.Y, pos, drawRect.Y + drawRect.Height);
                        familyNode = familyNode.ParentNode;
                        pos -= TreeWidth;
                    }
                }
            }

            for (int i = 0; i <= curItem.Level; i++) {
                shiftedDrawRect.X += TreeWidth;
                shiftedDrawRect.Width -= TreeWidth;
            }

            // Draw the arrow icon indicating if the node is expanded or not
            if (curItem.CanExpand) {
                Color arrowColor;
                if (row.IsHovered && new Rectangle(new Point(0, 0), new Size(shiftedDrawRect.X + NodeExpandClickMargin, drawRect.Height)).Contains(row.PointToClient(MousePosition)))
                    arrowColor = YamuiThemeManager.Current.AccentColor;
                else
                    arrowColor = curItem.IsExpanded ? YamuiThemeManager.Current.MenuNormalFore : YamuiThemeManager.Current.MenuDisabledFore;

                var rect = new Rectangle(shiftedDrawRect.X - TreeWidth + 1, shiftedDrawRect.Y, TreeWidth - 2, shiftedDrawRect.Height);
                rect.Y = rect.Y + (rect.Height - rect.Width)/2;
                rect.Height = rect.Width;

                g.PixelOffsetMode = PixelOffsetMode.Half;
                using (SolidBrush brush = new SolidBrush(arrowColor)) {
                    if (!curItem.IsExpanded)
                        g.FillPolygon(brush, new[] {new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height/2), new Point(rect.X, rect.Y + rect.Height)});
                    else
                        g.FillPolygon(brush, new[] {new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height)});
                }
            }

            return shiftedDrawRect;
        }

        #endregion

        #region ApplyFilter

        /// <summary>
        /// Returns a list of items that meet the FilterPredicate requirement as well as the filter string requirement,
        /// it also sorts the list of items
        /// We override this for trees so that if an item survives the filter, its parent should be displayed as well
        /// </summary>
        protected override List<ListItem> GetFilteredAndSortedList(List<FilteredListItem> listItems) {
            // when searching, the tree must actually behave like a FilteredTypeList
            if (_isSearching) {
                // sort alphabetically
                var list = listItems.Where(item => !((FilteredTypeTreeListItem)item).HideWhileSearching).OrderBy(item => item.DisplayText, StringComparer).ToList();
                //list.Sort(SortingClass); // make sure to sort the "initial" list here
                return base.GetFilteredAndSortedList(list);
            }

            var outList = new List<FilteredTypeTreeListItem>();
            var parentsToInclude = new HashSet<string>();

            var nbItems = listItems.Count;
            for (int i = nbItems - 1; i >= 0; i--) {
                var item = listItems[i] as FilteredTypeTreeListItem;
                if (item == null) continue;

                // the item must be included
                if (parentsToInclude.Contains(item.PathDescriptor) || (item.InternalFilterFullyMatch && (FilterPredicate == null || FilterPredicate(item)))) {
                    outList.Add(item);

                    // we register its parent to be included as well
                    var lastIdx = item.PathDescriptor.LastIndexOf(FilteredTypeTreeListItem.TreePathSeparator, StringComparison.CurrentCultureIgnoreCase);
                    if (lastIdx > -1) {
                        var parentPath = item.PathDescriptor.Substring(0, lastIdx);
                        if (!parentsToInclude.Contains(parentPath))
                            parentsToInclude.Add(parentPath);
                    }
                }
            }

            // we have our list completed, but we need to reverse it before delivering it
            outList.Reverse();

            return outList.Cast<ListItem>().ToList();
        }

        /// <summary>
        /// Allows to update the _isSearching value, if it does update, switches the list from
        /// a tree view to a flat list where we applied the classic filter from the filteredtypelist
        /// </summary>
        private bool SetIsSearching(string stringFilter, SearchModeOption searchMode) {
            var newIsSearching = !string.IsNullOrEmpty(stringFilter) && searchMode == SearchModeOption.SearchSortWithNoParent;
            if (newIsSearching != _isSearching) {
                _isSearching = newIsSearching;

                // we went from searching to not searching or the contrary
                if (_isSearching)
                    base.SetItems(GetFullItemsList(TreeRootItems));
                else
                    base.SetItems(GetExpandedItemsList(TreeRootItems, ForceExpansion.Idle));
                return true;
            }
            return false;
        }

        #endregion

        #region Events pushed from the button rows

        protected override void OnRowMouseMove(object sender, MouseEventArgs args) {
            ((YamuiListRow) sender).Invalidate(); // force to redraw on mouse move
            base.OnRowMouseMove(sender, args);
        }

        /// <summary>
        /// Click on an item, SelectedItem is usable at this time
        /// </summary>
        protected override void OnItemClick(object sender, MouseEventArgs eventArgs) {
            // handles node expansion
            if (!_isSearching) {
                var curItem = SelectedItem as FilteredTypeTreeListItem;
                if (curItem != null && curItem.CanExpand && eventArgs.Button == MouseButtons.Left) {
                    var widthOfArrow = TreeWidth + NodeExpandClickMargin;
                    for (int i = 0; i <= curItem.Level; i++) {
                        widthOfArrow += TreeWidth;
                    }
                    if (eventArgs.X <= widthOfArrow) {
                        if (eventArgs.Clicks == 1)
                            ExpandCollapse(SelectedItemIndex, ForceExpansion.Idle);
                        return;
                    }
                }
            }
            base.OnItemClick(sender, eventArgs);
        }

        #endregion

        #region HandleKeyDown

        /// <summary>
        /// Called when a key is pressed
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Left:
                    if (!_isSearching && !ModifierKeys.HasFlag(Keys.Alt)) {
                        var curItem = SelectedItem as FilteredTypeTreeListItem;
                        if (curItem != null) {
                            if (curItem.CanExpand && curItem.IsExpanded)
                                // collapse the current item
                                e.Handled = ExpandCollapse(SelectedItemIndex, ForceExpansion.ForceCollapse);
                            else {
                                // select its parent
                                var lastSep = curItem.PathDescriptor.LastIndexOf(FilteredTypeTreeListItem.TreePathSeparator, StringComparison.CurrentCultureIgnoreCase);
                                if (lastSep >= 0) {
                                    var parentPath = curItem.PathDescriptor.Substring(0, lastSep);
                                    var idx = SelectedItemIndex;
                                    FilteredTypeTreeListItem itemAbove;
                                    do {
                                        idx--;
                                        itemAbove = GetItem(idx) as FilteredTypeTreeListItem;
                                    } while (itemAbove != null && !itemAbove.PathDescriptor.Equals(parentPath));
                                    SelectedItemIndex = idx;
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                    break;

                case Keys.Right:
                    if (!_isSearching && !ModifierKeys.HasFlag(Keys.Alt)) {
                        // expand the current item
                        e.Handled = ExpandCollapse(SelectedItemIndex, ForceExpansion.ForceExpand);
                    }
                    break;
            }
            if (!e.Handled)
                base.OnKeyDown(e);
        }

        #endregion
    }
}