﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiMenu.cs) is part of YamuiFramework.
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
using Yamui.Framework.Controls;
using Yamui.Framework.Controls.YamuiList;
using Yamui.Framework.Fonts;
using Yamui.Framework.Helper;
using Yamui.Framework.HtmlRenderer.WinForms;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Forms {
    public class YamuiMenu : YamuiFormBaseShadow {
        #region Private

        private Action<YamuiMenuItem> _clickItemWrapper;
        private Size _formMinSize = new Size(0, 20);
        private bool _isExpanded = true;

        #endregion

        #region public fields
        
        /// <summary>
        /// When an item is clicked, it will be fed to this method that should, in term, be calling .OnClic of said item
        /// Use this as a wrapper to handle errors for instance
        /// </summary>
        public Action<YamuiMenuItem> ClicItemWrapper {
            get {
                return _clickItemWrapper ?? (item => {
                    if (item.OnClic != null) {
                        item.OnClic(item);
                    }
                });
            }
            set { _clickItemWrapper = value; }
        }

        /// <summary>
        /// Location from where the menu will be generated
        /// </summary>
        public Point SpawnLocation { get; set; }

        /// <summary>
        /// If != 0, positions this menu not as a classic menu but as an autocompletion for a given text,
        /// set this value to the line height of said text
        /// </summary>
        public int AutocompletionLineHeight { get; set; }

        /// <summary>
        /// If width > 0 then this menu will be centered in this rectangle
        /// </summary>
        public Rectangle ParentWindowRectangle { get; set; }

        /// <summary>
        /// List of the item to display in the menu
        /// </summary>
        public List<YamuiMenuItem> MenuList { get; set; }

        /// <summary>
        /// Title of the menu (or null)
        /// </summary>
        public string HtmlTitle { get; set; }

        /// <summary>
        /// Should we display a filter box?
        /// </summary>
        public bool DisplayFilterBox { get; set; }

        public bool DisplayNbItems { get; set; }

        /// <summary>
        /// Set a minimum size for this menu
        /// </summary>
        public Size FormMinSize {
            get { return _formMinSize; }
            set { _formMinSize = value; }
        }

        /// <summary>
        /// Set a maximum size for this menu
        /// </summary>
        public Size FormMaxSize { get; set; }

        /// <summary>
        /// Accessor to the list
        /// </summary>
        public YamuiFilteredTypeTreeListForMenuPopup YamuiList { get; private set; }

        /// <summary>
        /// Accessor to the filter box
        /// </summary>
        public YamuiFilterBox FilterBox { get; private set; }

        /// <summary>
        /// Initial filter string used (only if DisplayFilterBox is true)                                                                                                                        
        /// </summary>
        public string InitialFilterString { get; set; }

        #endregion

        #region Don't show in ATL+TAB + topmost

        /// <summary>
        /// The form should also set ShowInTaskbar = false; for this to work
        /// </summary>
        protected override CreateParams CreateParams {
            get {
                var createParams = base.CreateParams;
                createParams.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
                createParams.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOPMOST;
                return createParams;
            }
        }

        #endregion

        #region Life and death

        public YamuiMenu() {
            YamuiList = new YamuiFilteredTypeTreeListForMenuPopup();
            FilterBox = new YamuiFilterBox();
        }

        protected override void Dispose(bool disposing) {
            if (YamuiList != null) {
                YamuiList.MouseDown -= YamuiListOnMouseDown;
                YamuiList.EnterPressed -= YamuiListOnEnterPressed;
                YamuiList.RowClicked -= YamuiListOnRowClicked;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DrawContent

        private void DrawContent() {
            // init menu form
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            Controls.Clear();

            // evaluates the width needed to draw the control
            var maxWidth = MenuList.Select(item => item.Level*8 + TextRenderer.MeasureText(item.SubText ?? "", FontManager.GetFont(FontFunction.Small)).Width + TextRenderer.MeasureText(item.DisplayText ?? "", FontManager.GetStandardFont()).Width).Concat(new[] {0}).Max();
            maxWidth += MenuList.Exists(item => item.SubText != null) ? 10 : 0;
            maxWidth += (MenuList.Exists(item => item.ItemImage != null) ? 35 : 8) + 30;
            if (FormMaxSize.Width > 0)
                maxWidth = maxWidth.ClampMax(FormMaxSize.Width);
            if (FormMinSize.Width > 0)
                maxWidth = maxWidth.ClampMin(FormMinSize.Width);
            Width = maxWidth;

            int yPos = BorderWidth;

            // title
            HtmlLabel title = null;
            if (HtmlTitle != null) {
                title = new HtmlLabel {
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Location = new Point(BorderWidth, yPos),
                    AutoSizeHeightOnly = true,
                    Width = Width - BorderWidth*2,
                    BackColor = Color.Transparent,
                    Text = HtmlTitle,
                    IsSelectionEnabled = false,
                    IsContextMenuEnabled = false,
                    Enabled = false
                };
                yPos += title.Height;
            }

            // display filter box?
            if (DisplayFilterBox) {
                FilterBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                FilterBox.Location = new Point(BorderWidth, yPos + 5);
                FilterBox.Size = new Size(Width - BorderWidth*2, 20);
                FilterBox.Padding = new Padding(5, 0, 5, 0);
                if (MenuList.Exists(item => item.CanExpand)) {
                    _isExpanded = MenuList.Exists(item => item.IsExpanded);
                    FilterBox.ExtraButtons = new List<YamuiFilterBox.YamuiFilterBoxButton> {
                        new YamuiFilterBox.YamuiFilterBoxButton {
                            Image = _isExpanded ? Resources.Resources.Collapse : Resources.Resources.Expand,
                            OnClic = buttonExpandRetract_Click,
                            ToolTip = "Toggle <b>Expand/Collapse</b>"
                        }
                    };
                }
                yPos += 30;
            }

            // list
            Padding = new Padding(BorderWidth, yPos, BorderWidth, BorderWidth);
            YamuiList.NoCanExpandItem = !MenuList.Exists(item => item.CanExpand);
            YamuiList.Dock = DockStyle.Fill;
            if (!DisplayNbItems)
                YamuiList.BottomHeight = 0;
            YamuiList.SetItems(MenuList.Cast<ListItem>().ToList());
            YamuiList.MouseDown += YamuiListOnMouseDown;
            YamuiList.EnterPressed += YamuiListOnEnterPressed;
            YamuiList.RowClicked += YamuiListOnRowClicked;
            yPos += YamuiList.Items.Count.ClampMin(1)*YamuiList.RowHeight;

            if (YamuiList.Items.Count > 0) {
                var selectedIdx = YamuiList.Items.Cast<YamuiMenuItem>().ToList().FindIndex(item => item.IsSelectedByDefault);
                if (selectedIdx > 0)
                    YamuiList.SelectedItemIndex = selectedIdx;
            }

            // add controls
            if (title != null)
                Controls.Add(title);
            if (DisplayFilterBox) {
                FilterBox.Initialize(YamuiList);
                Controls.Add(FilterBox);
            }
            Controls.Add(YamuiList);

            // Size the form
            var height = yPos + BorderWidth + (DisplayNbItems ? YamuiList.BottomHeight : 0);
            if (FormMaxSize.Height > 0)
                height = height.ClampMax(FormMaxSize.Height);
            if (FormMinSize.Height > 0)
                height = height.ClampMin(FormMinSize.Height);
            Size = new Size(maxWidth, height);

            // position / size
            Location = ParentWindowRectangle.Width > 0 ? GetBestCenteredPosition(ParentWindowRectangle) : (AutocompletionLineHeight != 0 ? GetBestAutocompPosition(SpawnLocation, AutocompletionLineHeight) : GetBestMenuPosition(SpawnLocation));
            ResizeFormToFitScreen();
            MinimumSize = Size;

            // default focus
            if (DisplayFilterBox)
                FilterBox.ClearAndFocusFilter();
            else
                ActiveControl = YamuiList;

            // Filter?
            if (!string.IsNullOrEmpty(InitialFilterString) && DisplayFilterBox)
                FilterBox.Text = InitialFilterString;

            // So that the OnKeyDown event of this form is executed before the HandleKeyDown event of the control focused
            KeyPreview = true;
        }

        private void YamuiListOnRowClicked(YamuiScrollList yamuiScrollList, MouseEventArgs mouseEventArgs) {
            var item = yamuiScrollList.SelectedItem as YamuiMenuItem;
            if (item != null) {
                if (item.CanExpand) {
                    ClicItemWrapper(item);
                } else {
                    Visible = false;
                    ClicItemWrapper(item);
                    Close();
                    Dispose();
                }
            }
        }

        private void YamuiListOnEnterPressed(YamuiScrollList yamuiScrollList, KeyEventArgs e) {
            var item = yamuiScrollList.SelectedItem as YamuiMenuItem;
            if (item != null) {
                if (item.CanExpand) {
                    ClicItemWrapper(item);
                } else {
                    Visible = false;
                    ClicItemWrapper(item);
                    Close();
                    Dispose();
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Allows the user to move the window from the bottom status of the YamuiList (showing x items)
        /// </summary>
        private void YamuiListOnMouseDown(object sender, MouseEventArgs e) {
            var list = sender as YamuiFilteredTypeList;
            if (list != null && Movable && e.Button == MouseButtons.Left && (new Rectangle(0, list.Height - list.BottomHeight, list.Width, list.BottomHeight)).Contains(e.Location)) {
                // do as if the cursor was on the title bar
                WinApi.ReleaseCapture();
                WinApi.SendMessage(Handle, (uint) WinApi.Messages.WM_NCLBUTTONDOWN, new IntPtr((int) WinApi.HitTest.HTCAPTION), new IntPtr(0));
            }
        }

        private void buttonExpandRetract_Click(YamuiButtonImage sender, EventArgs e) {
            if (_isExpanded)
                YamuiList.ForceAllToCollapse();
            else
                YamuiList.ForceAllToExpand();
            _isExpanded = !_isExpanded;
            FilterBox.ExtraButtonsList[0].BackGrndImage = _isExpanded ? Resources.Resources.Collapse : Resources.Resources.Expand;
        }

        #endregion

        #region Events

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                Close();
                Dispose();
                e.Handled = true;
            }

            if (!e.Handled)
                base.OnKeyDown(e);
        }

        /// <summary>
        /// Close the menu when the user clicked elsewhere
        /// </summary>
        protected override void OnDeactivate(EventArgs e) {
            // ReSharper disable once ObjectCreationAsStatement
            DelayedAction.StartNew(30, () => {
                try {
                    this.SafeInvoke(popup => {
                        popup.Close();
                        popup.Dispose();
                    });
                } catch (Exception) {
                    //ok
                }
            });
            base.OnDeactivate(e);
        }

        /// <summary>
        /// Redirect mouse wheel to the list
        /// </summary>
        protected override void OnMouseWheel(MouseEventArgs e) {
            YamuiList.DoScroll(e.Delta);
            base.OnMouseWheel(e);
        }

        #endregion

        #region Show

        /// <summary>
        /// Call this method to show the notification
        /// </summary>
        public new void Show() {
            DrawContent();
            base.Show();
            Activate();
        }

        public new void Show(IWin32Window owner) {
            DrawContent();
            base.Show(owner);
            Activate();
        }

        #endregion

        #region YamuiFilteredTypeTreeListForMenuPopup

        /// <summary>
        /// Draw the tree differently
        /// </summary>
        public class YamuiFilteredTypeTreeListForMenuPopup : YamuiFilteredTypeTreeList {
            /// <summary>
            /// True if none of the root items can be expanded
            /// </summary>
            public bool NoCanExpandItem { get; set; }

            protected new int _nodeExpandClickMargin = 28;

            /// <summary>
            /// Called by default to paint the row if no OnRowPaint is defined
            /// </summary>
            protected override void RowPaint(ListItem item, YamuiListRow row, PaintEventArgs e) {
                // background
                var backColor = item.IsSeparator ?
                    YamuiThemeManager.Current.MenuBg(false, false, !item.IsDisabled) :
                    YamuiThemeManager.Current.MenuBg(row.IsSelected, row.IsHovered, !item.IsDisabled);
                e.Graphics.Clear(backColor);

                // foreground
                // left line
                if (row.IsSelected && !item.IsDisabled) {
                    using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.AccentColor)) {
                        e.Graphics.FillRectangle(b, new Rectangle(0, 0, 3, ClientRectangle.Height));
                    }
                }

                var curItem = item as FilteredTypeTreeListItem;
                if (curItem != null) {
                    var drawRect = row.ClientRectangle;
                    drawRect.X += 8;
                    drawRect.Width -= 8;
                    drawRect.Height = RowHeight;
                    var shiftedDrawRect = drawRect;

                    // draw the tree structure
                    if (!_isSearching && !NoCanExpandItem)
                        shiftedDrawRect = RowPaintTree(e.Graphics, curItem, drawRect, row);

                    // case of a separator
                    if (item.IsSeparator) {
                        RowPaintSeparator(e.Graphics, curItem.Level == 0 ? drawRect : shiftedDrawRect);
                    } else
                        DrawFilteredTypeRow(e.Graphics, curItem, NoCanExpandItem ? drawRect : shiftedDrawRect, row);
                }
            }
        }

        #endregion
    }

    #region YamuiMenuItem

    public class YamuiMenuItem : FilteredTypeTreeListItem {
        /// <summary>
        /// Action to execute on clic
        /// </summary>
        public Action<YamuiMenuItem> OnClic { get; set; }

        /// <summary>
        /// True if the item should be selected by default in the menu 
        /// (the last item to true is selected, otherwise it's the first in the list)
        /// </summary>
        public bool IsSelectedByDefault { get; set; }

        public override int ItemType {
            get { return -1; }
        }
    }

    #endregion
}