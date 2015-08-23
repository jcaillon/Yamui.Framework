using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Native;

namespace YamuiFramework.Controls {
    #region Enums

    public enum LabelMode {
        Default,
        Selectable
    }

    #endregion

    [Designer("YamuiFramework.Controls.YamuiLabelDesigner")]
    [ToolboxBitmap(typeof(Label))]
    public class YamuiLabel : Label {

        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        private DoubleBufferedTextBox _baseTextBox;

        private LabelMode _selectionMode = LabelMode.Default;
        [DefaultValue(LabelMode.Default)]
        [Category("Yamui")]
        public LabelMode SelectionMode {
            get { return _selectionMode; }
            set { _selectionMode = value; }
        }

        private LabelFunction _function = LabelFunction.Normal;
        [DefaultValue(LabelFunction.Normal)]
        [Category("Yamui")]
        public LabelFunction Function {
            get { return _function; }
            set { _function = value; }
        }

        private bool _wrapToLine;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool WrapToLine {
            get { return _wrapToLine; }
            set { _wrapToLine = value; Refresh(); }
        }

        #endregion

        #region Constructor

        public YamuiLabel() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            _baseTextBox = new DoubleBufferedTextBox();
            _baseTextBox.Visible = false;
            Controls.Add(_baseTextBox);
        }

        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.LinksColors.BackGround(BackColor, UseCustomBackColor);
                if (backColor != Color.Transparent)
                    e.Graphics.Clear(backColor);
            } catch {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                if (GetStyle(ControlStyles.AllPaintingInWmPaint))
                    OnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {

            Color foreColor = ThemeManager.LinksColors.ForeGround(ForeColor, UseCustomForeColor, false, false, false, Enabled);

            if (SelectionMode == LabelMode.Selectable) {
                CreateBaseTextBox();
                UpdateBaseTextBox();

                if (!_baseTextBox.Visible) {
                    TextRenderer.DrawText(e.Graphics, Text, FontManager.GetLabelFont(Function), ClientRectangle, foreColor, FontManager.GetTextFormatFlags(TextAlign));
                }
            } else {
                DestroyBaseTextbox();
                TextRenderer.DrawText(e.Graphics, Text, FontManager.GetLabelFont(Function), ClientRectangle, foreColor, FontManager.GetTextFormatFlags(TextAlign, _wrapToLine));
            }
        }

        #endregion

        #region Overridden Methods

        public override void Refresh() {
            if (SelectionMode == LabelMode.Selectable) {
                UpdateBaseTextBox();
            }
            base.Refresh();
        }

        public override Size GetPreferredSize(Size proposedSize) {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics()) {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, Text, FontManager.GetLabelFont(Function), proposedSize, FontManager.GetTextFormatFlags(TextAlign));
            }

            return preferredSize;
        }

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e) {
            if (SelectionMode == LabelMode.Selectable) {
                HideBaseTextBox();
            }

            base.OnResize(e);
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);

            if (SelectionMode == LabelMode.Selectable) {
                ShowBaseTextBox();
            }
        }

        #endregion

        #region Label Selection Mode

        private class DoubleBufferedTextBox : TextBox {
            public DoubleBufferedTextBox() {
                SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);
            }
        }

        private bool _firstInitialization = true;

        private void CreateBaseTextBox() {
            if (_baseTextBox.Visible && !_firstInitialization) return;
            if (!_firstInitialization) return;

            _firstInitialization = false;

            if (!DesignMode) {
                Form parentForm = FindForm();
                if (parentForm != null) {
                    parentForm.ResizeBegin += parentForm_ResizeBegin;
                    parentForm.ResizeEnd += parentForm_ResizeEnd;
                }
            }

            _baseTextBox.BackColor = Color.Transparent;
            _baseTextBox.Visible = true;
            _baseTextBox.BorderStyle = BorderStyle.None;
            _baseTextBox.Font = FontManager.GetLabelFont(Function);
            _baseTextBox.Location = new Point(1, 0);
            _baseTextBox.Text = Text;
            _baseTextBox.ReadOnly = true;

            _baseTextBox.Size = GetPreferredSize(Size.Empty);
            _baseTextBox.Multiline = true;

            _baseTextBox.DoubleClick += BaseTextBoxOnDoubleClick;
            _baseTextBox.Click += BaseTextBoxOnClick;

            Controls.Add(_baseTextBox);
        }

        private void parentForm_ResizeEnd(object sender, EventArgs e) {
            if (SelectionMode == LabelMode.Selectable) {
                ShowBaseTextBox();
            }
        }

        private void parentForm_ResizeBegin(object sender, EventArgs e) {
            if (SelectionMode == LabelMode.Selectable) {
                HideBaseTextBox();
            }
        }

        private void DestroyBaseTextbox() {
            if (!_baseTextBox.Visible) return;

            _baseTextBox.DoubleClick -= BaseTextBoxOnDoubleClick;
            _baseTextBox.Click -= BaseTextBoxOnClick;
            _baseTextBox.Visible = false;
        }

        private void UpdateBaseTextBox() {
            if (!_baseTextBox.Visible) return;

            SuspendLayout();
            _baseTextBox.SuspendLayout();

            _baseTextBox.BackColor = ThemeManager.LinksColors.BackGround(BackColor, UseCustomBackColor);
            _baseTextBox.ForeColor = ThemeManager.LinksColors.ForeGround(ForeColor, UseCustomForeColor, false, false, false, Enabled);

            _baseTextBox.Font = FontManager.GetLabelFont(Function);
            _baseTextBox.Text = Text;
            _baseTextBox.BorderStyle = BorderStyle.None;

            Size = GetPreferredSize(Size.Empty);

            _baseTextBox.ResumeLayout();
            ResumeLayout();
        }

        private void HideBaseTextBox() {
            _baseTextBox.Visible = false;
        }

        private void ShowBaseTextBox() {
            _baseTextBox.Visible = true;
        }

        [SecuritySafeCritical]
        private void BaseTextBoxOnClick(object sender, EventArgs eventArgs) {
            WinCaret.HideCaret(_baseTextBox.Handle);
        }

        [SecuritySafeCritical]
        private void BaseTextBoxOnDoubleClick(object sender, EventArgs eventArgs) {
            _baseTextBox.SelectAll();
            WinCaret.HideCaret(_baseTextBox.Handle);
        }

        #endregion
    }

    #region designer

    internal class YamuiLabelDesigner : ControlDesigner {

        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");

            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");

            properties.Remove("UseVisualStyleBackColor");

            properties.Remove("Font");
            properties.Remove("RightToLeft");

            base.PreFilterProperties(properties);
        }
    }

    #endregion

}
