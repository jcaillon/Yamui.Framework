﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiInput.cs) is part of YamuiFramework.
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
using System.Reflection;
using System.Windows.Forms;
using Yamui.Framework.Controls;
using Yamui.Framework.Fonts;
using Yamui.Framework.Helper;
using Yamui.Framework.HtmlRenderer.Core.Core.Entities;
using Yamui.Framework.HtmlRenderer.WinForms;

namespace Yamui.Framework.Forms {
    public sealed partial class YamuiInput : YamuiFormButtons {

        #region Fields

        public object DataObject;

        /// <summary>
        /// Out object bind?
        /// </summary>
        public bool Bound;

        private List<MemberInfo> _items = new List<MemberInfo>();

        /// <summary>
        /// True if the form contains data that need user's input
        /// </summary>
        private bool HasData {
            get { return DataObject != null; }
        }

        /// <summary>
        /// true if the message needed scrolls
        /// </summary>
        private bool _hasScrollMessage;

        public int DialogIntResult = -1;

        private int _dataLabelWidth;

        private const int MinButtonWidth = 80;
        private const int ButtonPadding = 10;
        private const int InputDefaultWidth = 200;
        private const int InputPadding = 10;

        private HtmlToolTip _tooltip;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor, you should the method ShwDlg instead
        /// </summary>
        private YamuiInput(string htmlTitle, string htmlMessage, List<string> buttonsList, object dataObject, int formMaxWidth, int formMaxHeight, int formMinWidth, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked) {
            InitializeComponent();

            _tooltip = new HtmlToolTip();

            var maxWidthInPanel = formMaxWidth - (Padding.Left + Padding.Right);
            contentPanel.DisableBackgroundImage = true;

            // if there was an object data passed on, need to check the max width needed to draw the inputs
            DataObject = dataObject;
            if (HasData) {
                // we make a list MemberInfo for each field in the data passed
                if (DataObject.GetType().IsSimpleType())
                    _items.Add(null);
                else {
                    foreach (var mi in DataObject.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public)) {
                        if (GetAttr(mi) != null && GetAttr(mi).Hidden)
                            continue;
                        var fi = mi as FieldInfo;
                        var pi = mi as PropertyInfo;
                        if (fi != null && Utilities.IsSupportedType(fi.FieldType)) {
                            _items.Add(fi);
                        } else if (pi != null && Utilities.IsSupportedType(pi.PropertyType) && pi.GetIndexParameters().Length == 0 && pi.CanWrite) {
                            _items.Add(pi);
                        }
                    }
                    _items.Sort((x, y) => (GetAttr(x) != null ? GetAttr(x).Order : int.MaxValue) - (GetAttr(y) != null ? GetAttr(y).Order : int.MaxValue));
                }

                // redefine the minimum Form Width for each input we will need to display
                var widthSpace = InputDefaultWidth + InputPadding*3 + Padding.Left + Padding.Right;
                for (int i = 0; i < _items.Count; i++) {
                    var item = _items[i];
                    if (item != null)
                        _dataLabelWidth = _dataLabelWidth.ClampMin(Utilities.MeasureHtmlPrefWidth((GetAttr(item) != null ? GetAttr(item).Label : item.Name), 0, maxWidthInPanel - widthSpace));
                }
                formMinWidth = formMinWidth.ClampMin(_dataLabelWidth + widthSpace);
            }

            // Set title, it will define a new minimum width for the message box
            var space = FormButtonWidth*2 + BorderWidth*2 + titleLabel.Padding.Left + 5;
            titleLabel.SetNeededSize(htmlTitle, formMinWidth - space, formMaxWidth - space, true);
            var newPadding = Padding;
            newPadding.Top = titleLabel.Height + 10;
            Padding = newPadding;
            titleLabel.Location = new Point(5, 5);

            // Set buttons
            int cumButtonWidth = 0;
            for (int i = buttonsList.Count - 1; i >= 0; i--) {
                Controls.Add(InsertButton(i, buttonsList[i], ref cumButtonWidth));
            }

            // set content label
            space = Padding.Left + Padding.Right;
            contentLabel.SetNeededSize(htmlMessage ?? string.Empty, (cumButtonWidth + ButtonPadding + BorderWidth*2 + 20).ClampMin(formMinWidth - space), maxWidthInPanel);
            contentLabel.Width = (formMinWidth - space).ClampMin(contentLabel.Width);
            if (onLinkClicked != null)
                contentLabel.LinkClicked += onLinkClicked;
            contentLabel.Anchor = contentLabel.Anchor | AnchorStyles.Right;
            var yPos = contentLabel.Location.Y + contentLabel.Height;

            // ensure a minimum width if there is no message
            contentLabel.Width = (formMinWidth - space).ClampMin(contentLabel.Width);

            // if there was an object data passed on, need to set up inputs for the user to fill in
            if (HasData) {
                // Build rows for each item
                yPos += 10;
                for (int i = 0; i < _items.Count; i++) {
                    contentPanel.Controls.Add(InsertInputForItem(i, ref yPos));
                    contentPanel.Controls.Add(InsertLabelForItem(i, ref yPos));
                }
            }

            // set form size
            Size = new Size(contentLabel.Width + space, (Padding.Top + Padding.Bottom + yPos).ClampMax(formMaxHeight));
            if (contentPanel.VerticalScroll.HasScroll) {
                _hasScrollMessage = true;
                Width += 10;
            }
            MinimumSize = Size;

            // quickly correct the tab order.. (we created the button from right to left and i'm too lazy to think atm)
            var tabOrderManager = new TabOrderManager(this);
            tabOrderManager.SetTabOrder(TabOrderManager.TabScheme.AcrossFirst);

            KeyPreview = true;
        }

        #endregion

        #region override

        // allow the user to use the scroll directly when the messagebox shows, instead of having to click on the scroller
        protected override void OnShown(EventArgs e) {
            base.OnShown(e);

            Focus();
            WinApi.SetForegroundWindow(Handle);

            if (_hasScrollMessage)
                ActiveControl = contentLabel;
            else if (HasData)
                ActiveControl = contentPanel.Controls.Find("input0", false).FirstOrDefault();
            else
                ActiveControl = Controls.Find("yamuiButton0", false).FirstOrDefault();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Escape:
                    // close the form
                    Close();
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    // click the first button
                    var firstButton = Controls.Find("yamuiButton0", false).FirstOrDefault() as YamuiButton;
                    if (firstButton != null) {
                        e.Handled = true;
                        firstButton.PerformClick();
                    }
                    break;
            }
            if (!e.Handled)
                base.OnKeyDown(e);
        }

        #endregion

        #region private

        private YamuiButton InsertButton(int i, string buttonText, ref int cumButtonWidth) {
            var size = TextRenderer.MeasureText(buttonText, FontManager.GetStandardFont());
            var newButton = new YamuiButton {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(Math.Max(size.Width + 10, MinButtonWidth), 25),
                Name = "yamuiButton" + i,
                Tag = i,
                Text = buttonText
            };
            newButton.Location = new Point(Width - BorderWidth - ButtonPadding - newButton.Width - cumButtonWidth, Height - BorderWidth - ButtonPadding - newButton.Height);
            cumButtonWidth += newButton.Width + 5;

            newButton.ButtonPressed += (sender, args) => {
                DialogIntResult = (int) ((YamuiButton) sender).Tag;

                // the first button triggers the validation
                if (DialogIntResult == 0) {
                    if (ValidateChildren()) {
                        BindToData();
                        Close();
                    } else {
                        DialogIntResult = -1;
                    }
                } else {
                    Close();
                }
            };
            return newButton;
        }

        private HtmlLabel InsertLabelForItem(int i, ref int yPos) {
            var item = _items[i];
            var lbl = new HtmlLabel {
                AutoSizeHeightOnly = true,
                BackColor = Color.Transparent,
                Location = new Point(InputPadding, yPos),
                Size = new Size(_dataLabelWidth + InputPadding - 1, 20),
                IsSelectionEnabled = false,
                Text = item != null ? (GetAttr(item) != null ? GetAttr(item).Label : item.Name) : ""
            };
            yPos += Math.Max(30, lbl.Height + 5);
            return lbl;
        }

        private Control InsertInputForItem(int i, ref int yPos) {
            var item = _items[i];
            var itemType = GetItemType(item);

            // Get default text value
            object val;
            if (item == null)
                val = DataObject;
            else if (item is PropertyInfo)
                val = ((PropertyInfo) item).GetValue(DataObject, null);
            else
                val = ((FieldInfo) item).GetValue(DataObject);

            string strValue = val.ConvertToStr();
            var inputWidth = contentPanel.Width - _dataLabelWidth - InputPadding*3;

            // Build control type
            Control retVal;

            if (itemType == typeof(bool)) {
                retVal = new YamuiButtonToggle {
                    Location = new Point(_dataLabelWidth + InputPadding*2, yPos),
                    Size = new Size(40, 16),
                    Text = null,
                    Checked = (bool) val
                };

                // for enum or list of strings
            } else if (itemType.IsEnum || (itemType == typeof(string) && GetAttr(item) != null && GetAttr(item).AllowListedValuesOnly)) {
                var cb = new YamuiComboBox {
                    Location = new Point(_dataLabelWidth + InputPadding*2, yPos),
                    Size = new Size(inputWidth, 20),
                    Anchor = Anchor | AnchorStyles.Right
                };
                var dataSource = new List<string>();
                if (itemType.IsEnum) {
                    foreach (var name in Enum.GetNames(itemType)) {
                        var attribute = Attribute.GetCustomAttribute(itemType.GetField(name), typeof(DescriptionAttribute), true) as DescriptionAttribute;
                        dataSource.Add(attribute != null ? attribute.Description : name);
                    }
                } else {
                    dataSource = strValue.Split('|').ToList();
                    strValue = dataSource[0];
                }
                cb.DataSource = dataSource;
                cb.Text = strValue;
                retVal = cb;

                // for everything else
            } else {
                var tb = new YamuiTextBox {
                    Location = new Point(_dataLabelWidth + InputPadding*2, yPos),
                    Size = new Size(inputWidth, 20),
                    Text = strValue,
                    Anchor = Anchor | AnchorStyles.Right,
                    Multiline = false
                };
                tb.AcceptsTab = false;
                tb.CausesValidation = true;
                tb.Enter += (s, e) => tb.SelectAll();

                if (itemType == typeof(char))
                    tb.KeyPress += (s, e) => e.Handled = !char.IsControl(e.KeyChar) && tb.TextLength > 0;
                else
                    tb.KeyPress += (s, e) => e.Handled = Utilities.IsInvalidKey(e.KeyChar, itemType);

                tb.Validating += (s, e) => {
                    bool invalid = IsTextInvalid(tb, itemType);
                    e.Cancel = invalid;
                    _errorProvider.SetError(tb, invalid ? "The value has an invalid format for <" + itemType.Name + ">." : "");
                };

                tb.Validated += (s, e) => _errorProvider.SetError(tb, "");

                _errorProvider.SetIconPadding(tb, -18);
                _errorProvider.Icon = Resources.Resources.IcoError;
                retVal = tb;
            }

            // Set standard props
            retVal.Name = "input" + i;

            // add tooltip on the control
            if (_items[i] != null && !string.IsNullOrEmpty(GetAttr(_items[i]).Tooltip))
                _tooltip.SetToolTip(retVal, GetAttr(_items[i]).Tooltip);

            return retVal;
        }

        /// <summary>
        /// Binds input text values back to the Data object.
        /// </summary>
        public void BindToData() {
            for (int i = 0; i < _items.Count; i++) {
                var item = _items[i];
                var itemType = GetItemType(item);

                // Get value from control
                Control c = contentPanel.Controls["input" + i];
                object val;
                if (c is YamuiButtonToggle)
                    val = ((YamuiButtonToggle) c).Checked;
                else if (c is YamuiComboBox && itemType.IsEnum) {
                    val = c.Text.ConvertFromStr(itemType);
                    foreach (var name in Enum.GetNames(itemType)) {
                        var attribute = Attribute.GetCustomAttribute(itemType.GetField(name), typeof(DescriptionAttribute), true) as DescriptionAttribute;
                        if (name.Equals(c.Text) || attribute != null && attribute.Description.Equals(c.Text)) {
                            val = name.ConvertFromStr(itemType);
                        }
                    }
                } else
                    val = c.Text.ConvertFromStr(itemType);

                // Apply value to dataObject
                if (item == null)
                    DataObject = val;
                else if (item is PropertyInfo)
                    ((PropertyInfo) item).SetValue(DataObject, val, null);
                else
                    ((FieldInfo) item).SetValue(DataObject, val);
            }
            Bound = true;
        }

        private bool IsTextInvalid(YamuiTextBox tb, Type itemType) {
            if (string.IsNullOrEmpty(tb.Text))
                return false;
            return !tb.Text.CanConvertToType(itemType);
        }

        private YamuiInputAttribute GetAttr(MemberInfo mi) {
            if (mi == null)
                return null;
            return (YamuiInputAttribute) Attribute.GetCustomAttribute(mi, typeof(YamuiInputAttribute), true);
        }

        private Type GetItemType(MemberInfo mi) {
            return mi == null ? DataObject.GetType() : (mi is PropertyInfo ? ((PropertyInfo) mi).PropertyType : ((FieldInfo) mi).FieldType);
        }

        #endregion

        #region Show/ShowDialog

        /// <summary>
        /// Show a message box dialog, wait for user input
        /// </summary>
        public static int ShowDialog(IntPtr ownerHandle, string caption, string htmlTitle, string htmlMessage, List<string> buttonsList, ref object data, int maxFormWidth = 0, int maxFormHeight = 0, int minFormWidth = 0, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null) {
            YamuiInput form;
            return Show(ownerHandle, caption, htmlTitle, htmlMessage, buttonsList, ref data, out form, maxFormWidth, maxFormHeight, minFormWidth, true, onLinkClicked);
        }

        /// <summary>
        /// Show a message box dialog
        /// </summary>
        public static int Show(IntPtr ownerHandle, string caption, string htmlTitle, string htmlMessage, List<string> buttonsList, ref object data, out YamuiInput msgBox, int maxFormWidth = 0, int maxFormHeight = 0, int minFormWidth = 0, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null) {
            return Show(ownerHandle, caption, htmlTitle, htmlMessage, buttonsList, ref data, out msgBox, maxFormWidth, maxFormHeight, minFormWidth, false, onLinkClicked);
        }

        private static int Show(IntPtr ownerHandle, string caption, string htmlTitle, string htmlMessage, List<string> buttonsList, ref object data, out YamuiInput msgBox, int maxFormWidth = 0, int maxFormHeight = 0, int minFormWidth = 0, bool waitResponse = true, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null) {
            var ownerRect = WinApi.GetWindowRect(ownerHandle);
            var ownerLocation = ownerRect.Location;
            ownerLocation.Offset(ownerRect.Width/2, ownerRect.Height/2);
            var screen = Screen.FromPoint(ownerLocation);

            // correct input if needed
            if (maxFormWidth == 0)
                maxFormWidth = screen.WorkingArea.Width - 20;
            if (maxFormHeight == 0)
                maxFormHeight = screen.WorkingArea.Height - 20;
            if (minFormWidth == 0)
                minFormWidth = 300;
            if (data != null)
                waitResponse = true;

            // new message box
            msgBox = new YamuiInput(htmlTitle, htmlMessage, buttonsList, data, maxFormWidth, maxFormHeight, minFormWidth, onLinkClicked) {
                ShowInTaskbar = !waitResponse,
                Text = caption
            };

            // center parent
            msgBox.Location = new Point((ownerRect.Width - msgBox.Width)/2 + ownerRect.X, (ownerRect.Height - msgBox.Height)/2 + ownerRect.Y);

            // get yamui form
            var yamuiForm = FromHandle(ownerHandle) as YamuiMainAppli;

            // we either display a modal or a normal messagebox
            if (waitResponse) {
                if (yamuiForm != null)
                    yamuiForm.HasModalOpened = true;

                msgBox.ShowDialog(new WindowWrapper(ownerHandle));

                if (msgBox.Bound)
                    data = msgBox.DataObject;

                if (yamuiForm != null)
                    yamuiForm.HasModalOpened = false;

                msgBox.Dispose();

                // get focus back to owner
                WinApi.SetForegroundWindow(ownerHandle);
            } else {
                msgBox.Show(new WindowWrapper(ownerHandle));
            }

            var res = msgBox.DialogIntResult;

            return res;
        }

        #endregion
    }

    #region YamuiInputDialogItemAttribute

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YamuiInputAttribute : Attribute {
        public YamuiInputAttribute() {}

        public YamuiInputAttribute(string label) {
            Label = label;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item is hidden and not displayed
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the label to use as the label for this field or property
        /// </summary>
        public string Label { get; set; }
        
        /// <summary>
        /// Text to show in the tooltip
        /// </summary>
        public string Tooltip { get; set; }
        
        /// <summary>
        /// Gets or sets the order in which to display the input for this field
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// For strings, you can constrain the values to a list of string delimited by |, the input will then be a combo box,
        /// set this to true and set the default value value for the field with the list
        /// </summary>
        public bool AllowListedValuesOnly { get; set; }
    }

    #endregion
}