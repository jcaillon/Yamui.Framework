using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace YamuiFramework.Native {
    public class ControlHelper {

        public static IEnumerable<Control> GetAll(Control control, Type type) {
            var controls = control.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
        }

        public static Control GetFirst(Control control, Type type) {
            foreach (var control1 in control.Controls) {
                if (control1.GetType() == type) return (Control)control1;
            }
            return null;
        }
    }
}
