using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages {
    public partial class SettingAppearance : YamuiUserControl {
        public SettingAppearance() {
            InitializeComponent();

            // AccentColors
            int x = 0;
            int y = 0;
            foreach (var accentColor in ThemeManager.GetAccentColors) {
                var newColorPicker = new YamuiColorRadioButton();
                PanelAccentColor.Controls.Add(newColorPicker);
                newColorPicker.CheckedChanged += NewColorPickerOnCheckedChanged;
                newColorPicker.BackColor = accentColor;
                newColorPicker.Bounds = new Rectangle(x, y, 50, 50);
                if (y + 2*newColorPicker.Height > PanelAccentColor.Height) {
                    x += newColorPicker.Width;
                    y = 0;
                } else
                    y += newColorPicker.Height;
                if (ThemeManager.AccentColor == accentColor)
                    newColorPicker.Checked = true;
            }

            // themes
            x = 0;
            foreach (var themeColor in ThemeManager.GetThemeBackColors) {
                var newColorPicker = new YamuiColorRadioButton();
                PanelTheme.Controls.Add(newColorPicker);
                newColorPicker.CheckedChanged += NewColorPickerOnCheckedChanged;
                newColorPicker.BackColor = themeColor;
                newColorPicker.UseBorder = true;
                newColorPicker.Bounds = new Rectangle(x, y, 50, 50);
                x += newColorPicker.Width;
                if (ThemeManager.FormColor.BackColor() == themeColor)
                    newColorPicker.Checked = true;
            }
        }

        private void NewColorPickerOnCheckedChanged(object sender, EventArgs eventArgs) {
            YamuiColorRadioButton rb = sender as YamuiColorRadioButton;
            if (rb != null && rb.Checked) {
                if (rb.Parent == PanelAccentColor) {
                    ThemeManager.AccentColor = rb.BackColor;
                } else {
                    ThemeManager.Theme = (rb.BackColor == ThemeManager.GetThemeBackColors[0]) ? Themes.Light : Themes.Dark;
                }
                var form = rb.FindForm();
                if (form != null) form.Refresh();
            }
        }

        private void SettingAppearance_Load(object sender, EventArgs e) {

            //System.Drawing.ColorTranslator.FromHtml(hex);
        }
    }
}
