using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace YamuiFramework.Controls
{
    public class YamuiUserControl : UserControl
    {
        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }
        #endregion

        #region Overridden Methods

        protected override void OnPaintBackground(PaintEventArgs e) {
            try {
                e.Graphics.Clear(UseCustomBackColor ? BackColor : ThemeManager.FormColor.BackColor());
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

        protected virtual void OnPaintForeground(PaintEventArgs e) {}

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        #endregion
    }
}
