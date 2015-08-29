using System.Collections.Generic;
using System.Drawing;
using YamuiFramework.HtmlRenderer.Core.Core;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Themes {

    public enum Themes {
        Default,
        Light,
        Dark
    }

    public static class ThemeManager {

        private static Theme _currentTheme;
        private static List<Theme> _listOfThemes = new List<Theme>();
        private static int _themeToUse = 0;

        public static List<Theme> GetThemesList() {
            if (_listOfThemes.Count == 0) {
                //_listOfThemes.Add(new Theme());
                Class2Xml<Theme>.LoadFromRaw(_listOfThemes, Properties.Resources.themesXml, true);
                //Class2Xml<Theme>.SaveToFile(_listOfThemes, @"C:\Work\YamuiFramework\YamuiDemoApp\bin\Debug\try.xml", true);
            }
            return _listOfThemes;
        } 

        public static Theme Current {
            set { _currentTheme = value; }
            get { return _currentTheme ?? (_currentTheme = GetThemesList().Find(theme => theme.UniqueId == _themeToUse)); }
        }

        private static Color _accentColor = GetAccentColors[13];
        private static Themes _modernTheme = Themes.Dark;

        public static Themes Theme {
            get { return _modernTheme; }
            set { _modernTheme = value; }
        }

        public static Color AccentColor {
            get { return _accentColor; }
            set { _accentColor = value; }
        }

        public static Image ImageTheme { get; set; }

        public static bool AnimationAllowed { get; set; }

        public static Color[] GetAccentColors  {
            get {
                return new[] 
                {
                    Color.FromArgb(164, 196, 0),
                    Color.FromArgb(96, 169, 23),
                    Color.FromArgb(0, 138, 0),
                    Color.FromArgb(0, 171, 169),
                    Color.FromArgb(27, 161, 226),
                    Color.FromArgb(0, 80, 239),
                    Color.FromArgb(106, 0, 255),
                    Color.FromArgb(170, 0, 255),
                    Color.FromArgb(244, 114, 208),
                    Color.FromArgb(216, 0, 115),
                    Color.FromArgb(162, 0, 37),
                    Color.FromArgb(229, 20, 0),
                    Color.FromArgb(250, 104, 0),
                    Color.FromArgb(240, 163, 10),
                    Color.FromArgb(227, 200, 0),
                    Color.FromArgb(130, 90, 44),
                    Color.FromArgb(109, 135, 100),
                    Color.FromArgb(100, 118, 135),
                    Color.FromArgb(118, 96, 138),
                    Color.FromArgb(135, 121, 78)
                };
            }
        }



        public static void UpdateBaseCssData() {
            string baseCss = Properties.Resources.baseCss;
            baseCss = baseCss.Replace("%FormForeGroundColor%", ColorTranslator.ToHtml(Current.LabelsColorsNormalForeColor));
            _baseCssData = HtmlRender.ParseStyleSheet(baseCss);
        }

        private static CssData _baseCssData;
        public static CssData GetBaseCssData() {
            if (_baseCssData == null) {
                UpdateBaseCssData();
            }
            return _baseCssData;
        }

        public static Color[] GetThemeBackColors {
            get { return new[] { Color.FromArgb(230, 230, 230), Color.FromArgb(37, 37, 38)}; }
        }

        /// <summary>
        /// This class is used for trackbars as well as scrollbars
        /// </summary>
        public static class ScrollBarsColors {

            public static Color BackGround(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color backColor;
                if (!enabled)
                    backColor = Current.ScrollBarsColorsDisabledBackColor;
                else if (isPressed)
                    backColor = Current.ScrollBarsColorsNormalBackColor;
                else if (isHovered || isFocused)
                    backColor = Current.ScrollBarsColorsHoverBackColor;
                else
                    backColor = Current.ScrollBarsColorsNormalBackColor;

                return backColor;
            }

            public static Color ForeGround(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Current.ScrollBarsColorsDisabledForeColor;
                else if (isPressed)
                    foreColor = AccentColor;
                else if (isHovered || isFocused)
                    foreColor = Current.ScrollBarsColorsHoverForeColor;
                else
                    foreColor = Current.ScrollBarsColorsNormalForeColor;

                return foreColor;
            }
        }

        /// <summary>
        ///  This class is used for labels as well as links
        /// </summary>
        public static class LabelsColors {

            public static Color ForeGround(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Current.LabelsColorsDisabledForeColor;
                else if (isPressed)
                    foreColor = Current.LabelsColorsPressForeColor;
                else if (isHovered || isFocused)
                    foreColor = AccentColor;
                else
                    foreColor = useCustomForeColor ? controlForeColor : Current.LabelsColorsNormalForeColor;

                return foreColor;
            }

            public static Color BackGround(Color controlBackColor, bool useCustomBackColor) {
                return !useCustomBackColor ? Color.Transparent : controlBackColor; ;
            }
        }

        /// <summary>
        ///  This class is used for tab controls (back color is also used for tab pages)
        /// </summary>
        public static class TabsColors {

            public static Color ForeGround(bool isFocused, bool isHovered, bool isSelected) {
                Color foreColor;

                if (isFocused && isSelected)
                    foreColor = AccentColor;
                else if (isSelected)
                    foreColor = Current.TabsColorsPressForeColor;
                else if (isHovered)
                    foreColor = Current.TabsColorsHoverForeColor;
                else
                    foreColor = Current.TabsColorsNormalForeColor;

                return foreColor;
            }
        }

        /// <summary>
        ///     This class is used for :
        ///     - Buttons
        ///     - CheckBoxes
        ///     - ComboBoxes
        ///     - DatePicker
        ///     - RadioButtons
        /// </summary>
        public static class ButtonColors {
            public static Color BackGround(Color controlBackColor, bool useCustomBackColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color backColor;

                if (!enabled)
                    backColor = Current.ButtonColorsDisabledBackColor;
                else if (isPressed)
                    backColor = AccentColor;
                else if (isHovered)
                    backColor = Current.ButtonColorsHoverBackColor;
                else
                    backColor = useCustomBackColor ? controlBackColor : Current.ButtonColorsNormalBackColor;

                return backColor;
            }

            public static Color ForeGround(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Current.ButtonColorsDisabledForeColor;
                else if (isPressed)
                    foreColor = Current.ButtonColorsPressForeColor;
                else if (isHovered)
                    foreColor = Current.ButtonColorsHoverForeColor;
                else
                    foreColor = useCustomForeColor ? controlForeColor : Current.ButtonColorsNormalForeColor;

                return foreColor;
            }

            public static Color BorderColor(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color borderColor;

                if (!enabled)
                    borderColor = Current.ButtonColorsDisabledBorderColor;
                else if (isPressed)
                    borderColor = AccentColor;
                else if (isFocused)
                    borderColor = AccentColor;
                else if (isHovered)
                    borderColor = Current.ButtonColorsHoverBorderColor;
                else
                    borderColor = Current.ButtonColorsNormalBorderColor;

                return borderColor;
            }
        }
    }
}