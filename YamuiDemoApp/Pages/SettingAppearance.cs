using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework;
using YamuiFramework.Animations.Animator;
using YamuiFramework.Controls;
using YamuiFramework.Themes;

namespace YamuiDemoApp.Pages {
    public partial class SettingAppearance : YamuiPage {
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

            comboTheme.DataSource = ThemeManager.GetThemesList().Select(theme => theme.ThemeName).ToList();
        }

        private void NewColorPickerOnCheckedChanged(object sender, EventArgs eventArgs) {
            YamuiColorRadioButton rb = sender as YamuiColorRadioButton;
            if (rb != null && rb.Checked) {
                ThemeManager.AccentColor = rb.BackColor;
                RefreshForm();
            }
        }

        private void RefreshForm() {
            var form = FindForm();
            if (form != null) {
                // anim?
                form.Refresh();
            }
        }

        private void SettingAppearance_Load(object sender, EventArgs e) {

            //System.Drawing.ColorTranslator.FromHtml(hex);
        }

        private void yamuiComboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            ThemeManager.Current = ThemeManager.GetThemesList().Find(theme => theme.ThemeName == (string)comboTheme.SelectedItem);
            RefreshForm();
        }

        
    }
}
