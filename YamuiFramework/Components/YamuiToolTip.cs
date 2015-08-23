using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace YamuiFramework.Components
{
    [ToolboxBitmap(typeof(ToolTip))]
    public class YamuiToolTip : ToolTip
    {
        #region Fields

        [DefaultValue(true)]
        [Browsable(false)]
        public new bool ShowAlways
        {
            get { return base.ShowAlways; }
            set { base.ShowAlways = true; }
        }

        [DefaultValue(true)]
        [Browsable(false)]
        public new bool OwnerDraw
        {
            get { return base.OwnerDraw; }
            set { base.OwnerDraw = true; }
        }

        [Browsable(false)]
        public new bool IsBalloon
        {
            get { return base.IsBalloon; }
            set { base.IsBalloon = false; }
        }

        [Browsable(false)]
        public new Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        [Browsable(false)]
        public new Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        [Browsable(false)]
        public new string ToolTipTitle
        {
            get { return base.ToolTipTitle; }
            set { base.ToolTipTitle = ""; }
        }

        [Browsable(false)]
        public new ToolTipIcon ToolTipIcon
        {
            get { return base.ToolTipIcon; }
            set { base.ToolTipIcon = ToolTipIcon.None; }
        }

        #endregion

        #region Constructor

        public YamuiToolTip()
        {
            OwnerDraw = true;
            ShowAlways = true;

            Draw += YamuiToolTip_Draw;
            Popup += YamuiToolTip_Popup;
        }

        #endregion

        #region Management Methods

        public new void SetToolTip(Control control, string caption)
        {
            base.SetToolTip(control, caption);

            foreach (Control c in control.Controls)
            {
                SetToolTip(c, caption);
            }
        }

        private void YamuiToolTip_Popup(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = new Size(e.ToolTipSize.Width + 24, e.ToolTipSize.Height + 9);
        }

        private void YamuiToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            Color backColor = ThemeManager.ButtonColors.Normal.BackColor();
            Color borderColor = ThemeManager.ButtonColors.Normal.BorderColor();
            Color foreColor = ThemeManager.ButtonColors.Normal.ForeColor();

            using (SolidBrush b = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(b, e.Bounds);
            }
            using (Pen p = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(p, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));
            }

            TextRenderer.DrawText(e.Graphics, e.ToolTipText, FontManager.GetStandardFont(), e.Bounds, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        #endregion
    }
}
