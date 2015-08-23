using System.Drawing;

namespace YamuiFramework {

    public enum Themes {
        Default,
        Light,
        Dark
    }

    public static class ThemeManager {

        private static Color _accentColor = Color.FromArgb(162, 0, 37);
        private static Themes _modernTheme = Themes.Dark;

        public static Themes Theme {
            get { return _modernTheme; }
            set { _modernTheme = value; }
        }

        public static Color AccentColor {
            get { return _accentColor; }
            set { _accentColor = value; }
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
        public static class LinksColors {

            public static Color ForeGround(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Disabled.ForeColor();
                else if (isPressed)
                    foreColor = Press.ForeColor();
                else if (isHovered || isFocused)
                    foreColor = AccentColor;
                else
                    foreColor = Normal.ForeColor();

                return foreColor;
            }

            public static Color BackGround(Color controlBackColor, bool useCustomBackColor) {
                return !useCustomBackColor ? FormColor.BackColor() : controlBackColor;;
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
        ///  This class is used for tab controls
        /// </summary>
        public static class TabsColors {

            public static Color ForeGround(bool isHovered, bool isPressed, bool enabled) {
                Color foreColor;

                if (!enabled)
                    foreColor = Disabled.ForeColor();
                else if (isPressed)
                    foreColor = Press.ForeColor();
                else if (isHovered)
                    foreColor = Hover.ForeColor();
                else
                    foreColor = Disabled.ForeColor();

                return foreColor;
            }

            public static class Disabled {
                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(180, 180, 180) : Color.FromArgb(80, 80, 80);
                }
            }

            public static class Hover {
                public static Color ForeColor() {
                    return (Theme == Themes.Light) ? Color.FromArgb(100, 100, 100) : Color.FromArgb(140, 140, 140);
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
                var backColor = controlBackColor;

                if (!enabled)
                    backColor = Disabled.BackColor();
                else if (isPressed)
                    backColor = AccentColor;
                else if (isHovered)
                    backColor = Hover.BackColor();
                else if (isFocused)
                    backColor = Normal.BackColor();
                else if (!useCustomBackColor)
                    backColor = Normal.BackColor();

                return backColor;
            }

            public static Color ForeGround(Color controlForeColor, bool useCustomForeColor, bool isFocused, bool isHovered, bool isPressed, bool enabled) {
                var foreColor = controlForeColor;

                if (!enabled)
                    foreColor = Disabled.ForeColor();
                else if (isPressed)
                    foreColor = Press.ForeColor();
                else if (isHovered)
                    foreColor = Hover.ForeColor();
                else if (isFocused)
                    foreColor = Normal.ForeColor();
                else if (!useCustomForeColor)
                    foreColor = Normal.ForeColor();

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