﻿using System;
using System.Drawing;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.WinForms;
using YamuiFramework.Forms;

namespace YamuiFramework {

    public enum Themes {
        Default,
        Light,
        Dark
    }

    public static class ThemeManager {

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
            string baseCss = Properties.Resources.theme;
            baseCss = baseCss.Replace("%FormForeGroundColor%", ColorTranslator.ToHtml(LabelsColors.Normal.ForeColor()));
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
        /// ForeColor is for the title of the form (and resize pixels)
        /// </summary>
        public static class FormColor {
            public static Color BackColor() {
                return (Theme == Themes.Light) ? Color.FromArgb(230, 230, 230) : Color.FromArgb(37, 37, 38);
            }
            public static Color ForeColor() {
                return (Theme == Themes.Light) ? Color.FromArgb(30, 30, 30) : Color.FromArgb(210, 210, 210);
            }
        }

        /// <summary>
        /// This class is used for trackbars as well as scrollbars
        /// </summary>
        public static class ScrollBarsColors {

            public static Color BackGround(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color backColor;
                if (!enabled)
                    backColor = Disabled.BackColor();
                else if (isPressed)
                    backColor = Normal.BackColor();
                else if (isHovered || isFocused)
                    backColor = Hover.BackColor();
                else
                    backColor = Normal.BackColor();

                return backColor;
            }

            public static Color ForeGround(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Disabled.ForeColor();
                else if (isPressed)
                    foreColor = AccentColor;
                else if (isHovered || isFocused)
                    foreColor = Hover.ForeColor();
                else
                    foreColor = Normal.ForeColor();

                return foreColor;
            }

            public static class Normal {
                public static Color BackColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(204, 204, 204) : Color.FromArgb(51, 51, 51);
                }

                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(102, 102, 102) : Color.FromArgb(153, 153, 153);
                }
            }

            public static class Hover {
                public static Color BackColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(204, 204, 204) : Color.FromArgb(51, 51, 51);
                }

                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(37, 37, 38) : Color.FromArgb(204, 204, 204);
                }
            }

            public static class Disabled {
                public static Color BackColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(230, 230, 230) : Color.FromArgb(34, 34, 34);
                }

                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(179, 179, 179) : Color.FromArgb(85, 85, 85);
                }
            }
        }

        /// <summary>
        ///  This class is used for labels as well as links
        /// </summary>
        public static class LabelsColors {

            public static Color ForeGround(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Disabled.ForeColor();
                else if (isPressed)
                    foreColor = Press.ForeColor();
                else if (isHovered || isFocused)
                    foreColor = AccentColor;
                else
                    foreColor = useCustomForeColor ? controlForeColor : Normal.ForeColor();

                return foreColor;
            }

            public static Color BackGround(Color controlBackColor, bool useCustomBackColor) {
                return !useCustomBackColor ? Color.Transparent : controlBackColor; ;
            }

            public static class Normal {
                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(30, 30, 30) : Color.FromArgb(180, 180, 181);
                }
            }

            public static class Press {

                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(0, 0, 0) : Color.FromArgb(93, 93, 93);
                }
            }

            public static class Disabled {

                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(150, 150, 150) : Color.FromArgb(80, 80, 80);
                }
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
                    foreColor = Press.ForeColor();
                else if (isHovered)
                    foreColor = Hover.ForeColor();
                else
                    foreColor = Normal.ForeColor();

                return foreColor;
            }

            public static class Normal {
                public static Color BackColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(230, 230, 230) : Color.FromArgb(37, 37, 38);
                }
                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(110, 110, 110) : Color.FromArgb(80, 80, 80);
                }
            }

            public static class Hover {
                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(60, 60, 60) : Color.FromArgb(140, 140, 140);
                }
            }

            public static class Press {
                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(30, 30, 30) : Color.FromArgb(193, 193, 194);
                }
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
                    backColor = Disabled.BackColor();
                else if (isPressed)
                    backColor = AccentColor;
                else if (isHovered)
                    backColor = Hover.BackColor();
                else
                    backColor = useCustomBackColor ? controlBackColor : Normal.BackColor();

                return backColor;
            }

            public static Color ForeGround(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Disabled.ForeColor();
                else if (isPressed)
                    foreColor = Press.ForeColor();
                else if (isHovered)
                    foreColor = Hover.ForeColor();
                else
                    foreColor = useCustomForeColor ? controlForeColor : Normal.ForeColor();

                return foreColor;
            }

            public static Color BorderColor(bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color borderColor;

                if (!enabled)
                    borderColor = Disabled.BorderColor();
                else if (isPressed)
                    borderColor = AccentColor;
                else if (isFocused)
                    borderColor = AccentColor;
                else if (isHovered)
                    borderColor = Hover.BorderColor();
                else
                    borderColor = Normal.BorderColor();

                return borderColor;
            }

            public static class Normal {
                public static Color BackColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(230, 230, 230) : Color.FromArgb(51, 51, 51);
                }

                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(30, 30, 30) : Color.FromArgb(209, 209, 209);
                }

                public static Color BorderColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(190, 190, 190) : Color.FromArgb(51, 51, 51);
                }
            }

            public static class Hover {
                public static Color BackColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(210, 210, 210) : Color.FromArgb(62, 62, 66);
                }

                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(30, 30, 30) : Color.FromArgb(209, 209, 209);
                }

                public static Color BorderColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(190, 190, 190) : Color.FromArgb(62, 62, 66);
                }
            }

            public static class Press {
                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(30, 30, 30) : Color.FromArgb(209, 209, 209);
                }
            }

            public static class Disabled {
                public static Color BackColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(230, 230, 230) : Color.FromArgb(51, 51, 51);
                }

                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(100, 100, 100) : Color.FromArgb(84, 84, 84);
                }

                public static Color BorderColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(190, 190, 190) : Color.FromArgb(51, 51, 51);
                }
            }
        }
    }
}