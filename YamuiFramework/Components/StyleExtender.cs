using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace YamuiFramework.Components {
    [ProvideProperty("ApplyTheme", typeof(Control))]
    public sealed class StyleExtender : Component, IExtenderProvider {
        #region Fields

        private readonly List<Control> extendedControls = new List<Control>();

        #endregion

        #region Constructor

        public StyleExtender() {

        }

        public StyleExtender(IContainer parent)
            : this() {
            if (parent != null) {
                parent.Add(this);
            }
        }

        #endregion

        #region Management Methods

        private void UpdateTheme() {
            Color backColor = ThemeManager.FormColor.BackColor();
            Color foreColor = ThemeManager.FormColor.ForeColor();

            foreach (Control ctrl in extendedControls) {
                if (ctrl != null) {
                    try {
                        ctrl.BackColor = backColor;
                    } catch { }

                    try {
                        ctrl.ForeColor = foreColor;
                    } catch { }
                }
            }
        }

        #endregion

        #region IExtenderProvider

        bool IExtenderProvider.CanExtend(object target) {
            return target is Control;
        }

        [DefaultValue(false)]
        [Category("Yamui")]
        [Description("Apply Theme BackColor and ForeColor.")]
        public bool GetApplyTheme(Control control) {
            return control != null && extendedControls.Contains(control);
        }

        public void SetApplyTheme(Control control, bool value) {
            if (control == null) {
                return;
            }

            if (extendedControls.Contains(control)) {
                if (!value) {
                    extendedControls.Remove(control);
                }
            } else {
                if (value) {
                    extendedControls.Add(control);
                }
            }
        }

        #endregion
    }
}
