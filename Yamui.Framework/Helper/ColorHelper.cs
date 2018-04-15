using System;
using System.Drawing;

namespace Yamui.Framework.Helper {

    public static class ColorHelper {
        
        /// <devdoc>
        ///      Creates a new color that is a object of the given color.
        /// </devdoc>
        public static Color Dark(this Color baseColor, float percOfDarkDark) {
            return new HlsColor(baseColor).Darker(percOfDarkDark);
        }

        /// <devdoc>
        ///      Creates a new color that is a object of the given color.
        /// </devdoc>
        public static Color Dark(this Color baseColor) {
            return new HlsColor(baseColor).Darker(0.5f);
        }

        /// <devdoc>
        ///      Creates a new color that is a object of the given color.
        /// </devdoc>
        public static Color DarkDark(this Color baseColor) {
            return new HlsColor(baseColor).Darker(1.0f);
        }

        //returns true if the luminosity of c1 is less than c2.
        internal static bool IsDarker(this Color c1, Color c2) {
            HlsColor hc1 = new HlsColor(c1);
            HlsColor hc2 = new HlsColor(c2);
            return (hc1.Luminosity < hc2.Luminosity);
        }

        /// <summary>
        /// Inverts a color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color InvertColor(this Color color) {
            return Color.FromArgb(color.A, (byte)~color.R, (byte)~color.G, (byte)~color.B);
        }

        /// <devdoc>
        ///      Creates a new color that is a object of the given color.
        /// </devdoc>
        public static Color Light(this Color baseColor, float percOfLightLight) {
            return new HlsColor(baseColor).Lighter(percOfLightLight);
        }

        /// <devdoc>
        ///      Creates a new color that is a object of the given color.
        /// </devdoc>
        public static Color Light(this Color baseColor) {
            return new HlsColor(baseColor).Lighter(0.5f);
        }

        /// <devdoc>
        ///      Creates a new color that is a object of the given color.
        /// </devdoc>
        public static Color LightLight(this Color baseColor) {
            return new HlsColor(baseColor).Lighter(1.0f);
        }
    }

 
    /// <summary>
    /// Logic copied from Win2K sources to copy the lightening and darkening of colors
    /// </summary>
    public struct HlsColor {
        private const int ShadowAdj = -333;
        private const int HilightAdj = 500;

        private const int Range = 240;
        private const int HlsMax = Range;
        private const int RgbMax = 255;
        private const int Undefined = HlsMax * 2 / 3;

        private readonly int _hue;
        private readonly int _saturation;
        private readonly int _luminosity;

        private bool _isSystemColorsControl;

        public HlsColor(Color color) {
            _isSystemColorsControl = (color.ToKnownColor() == SystemColors.Control.ToKnownColor());
            int r = color.R;
            int g = color.G;
            int b = color.B;
            int max, min; /* max and min RGB values */
            int sum, dif;
            int rdelta, gdelta, bdelta; /* intermediate value: % of spread from max */

            /* calculate lightness */
            max = Math.Max(Math.Max(r, g), b);
            min = Math.Min(Math.Min(r, g), b);
            sum = max + min;

            _luminosity = (((sum * HlsMax) + RgbMax) / (2 * RgbMax));

            dif = max - min;
            if (dif == 0) {
                /* r=g=b --> achromatic case */
                _saturation = 0; /* saturation */
                _hue = Undefined; /* hue */
            } else {
                /* chromatic case */
                /* saturation */
                if (_luminosity <= (HlsMax / 2))
                    _saturation = ((dif * HlsMax) + (sum / 2)) / sum;
                else
                    _saturation = ((dif * HlsMax) + (2 * RgbMax - sum) / 2)
                                  / (2 * RgbMax - sum);
                /* hue */
                rdelta = (((max - r) * (HlsMax / 6)) + (dif / 2)) / dif;
                gdelta = (((max - g) * (HlsMax / 6)) + (dif / 2)) / dif;
                bdelta = (((max - b) * (HlsMax / 6)) + (dif / 2)) / dif;

                if (r == max)
                    _hue = bdelta - gdelta;
                else if (g == max)
                    _hue = (HlsMax / 3) + rdelta - bdelta;
                else /* B == cMax */
                    _hue = ((2 * HlsMax) / 3) + gdelta - rdelta;

                if (_hue < 0)
                    _hue += HlsMax;
                if (_hue > HlsMax)
                    _hue -= HlsMax;
            }
        }

        public int Hue => _hue;

        public int Luminosity => _luminosity;

        public int Saturation => _saturation;

        public Color Darker(float percDarker) {
            if (_isSystemColorsControl) {
                // With the usual color scheme, ControlDark/DarkDark is not exactly
                // what we would otherwise calculate
                if (Math.Abs(percDarker) < 0.01) {
                    return SystemColors.ControlDark;
                }

                if (Math.Abs(percDarker - 1.0f) < 0.01) {
                    return SystemColors.ControlDarkDark;
                }

                Color dark = SystemColors.ControlDark;
                Color darkDark = SystemColors.ControlDarkDark;

                int dr = dark.R - darkDark.R;
                int dg = dark.G - darkDark.G;
                int db = dark.B - darkDark.B;

                return Color.FromArgb((byte) (dark.R - (byte) (dr * percDarker)),
                    (byte) (dark.G - (byte) (dg * percDarker)),
                    (byte) (dark.B - (byte) (db * percDarker)));
            }

            int oneLum = 0;
            int zeroLum = NewLuma(ShadowAdj, true);

            /*                                        
                if (luminosity < 40) {
                    zeroLum = NewLuma(120, ShadowAdj, true);
                }
                else {
                    zeroLum = NewLuma(ShadowAdj, true);
                }
                */

            return ColorFromHls(_hue, zeroLum - (int) ((zeroLum - oneLum) * percDarker), _saturation);
        }

        public override bool Equals(object o) {
            if (!(o is HlsColor)) {
                return false;
            }

            HlsColor c = (HlsColor) o;
            return _hue == c._hue &&
                   _saturation == c._saturation &&
                   _luminosity == c._luminosity &&
                   _isSystemColorsControl == c._isSystemColorsControl;
        }

        public static bool operator ==(HlsColor a, HlsColor b) {
            return a.Equals(b);
        }

        public static bool operator !=(HlsColor a, HlsColor b) {
            return !a.Equals(b);
        }

        public override int GetHashCode() {
            return _hue << 6 | _saturation << 2 | _luminosity;
        }

        public Color Lighter(float percLighter) {
            if (_isSystemColorsControl) {
                // With the usual color scheme, ControlLight/LightLight is not exactly
                // what we would otherwise calculate
                if (Math.Abs(percLighter) < 0.01) {
                    return SystemColors.ControlLight;
                }

                if (Math.Abs(percLighter - 1.0f) < 0.01) {
                    return SystemColors.ControlLightLight;
                }

                Color light = SystemColors.ControlLight;
                Color lightLight = SystemColors.ControlLightLight;

                int dr = light.R - lightLight.R;
                int dg = light.G - lightLight.G;
                int db = light.B - lightLight.B;

                return Color.FromArgb((byte) (light.R - (byte) (dr * percLighter)),
                    (byte) (light.G - (byte) (dg * percLighter)),
                    (byte) (light.B - (byte) (db * percLighter)));
            }

            int zeroLum = _luminosity;
            int oneLum = NewLuma(HilightAdj, true);

            /*
                if (luminosity < 40) {
                    zeroLum = 120;
                    oneLum = NewLuma(120, HilightAdj, true);
                }
                else {
                    zeroLum = luminosity;
                    oneLum = NewLuma(HilightAdj, true);
                }
                */

            return ColorFromHls(_hue, zeroLum + (int) ((oneLum - zeroLum) * percLighter), _saturation);
        }

        private int NewLuma(int n, bool scale) {
            return NewLuma(_luminosity, n, scale);
        }

        private int NewLuma(int luminosity, int n, bool scale) {
            if (n == 0)
                return luminosity;

            if (scale) {
                if (n > 0) {
                    return (int) ((luminosity * (1000 - n) + (Range + 1L) * n) / 1000);
                }

                return (luminosity * (n + 1000)) / 1000;
            }

            int newLum = luminosity;
            newLum += (int) ((long) n * Range / 1000);

            if (newLum < 0)
                newLum = 0;
            if (newLum > HlsMax)
                newLum = HlsMax;

            return newLum;
        }

        private Color ColorFromHls(int hue, int luminosity, int saturation) {
            byte r, g, b; /* RGB component values */
            int magic1, magic2; /* calculated magic numbers (really!) */

            if (saturation == 0) {
                /* achromatic case */
                r = g = b = (byte) ((luminosity * RgbMax) / HlsMax);
                if (hue != Undefined) {
                    /* ERROR */
                }
            } else {
                /* chromatic case */
                /* set up magic numbers */
                if (luminosity <= (HlsMax / 2))
                    magic2 = (luminosity * (HlsMax + saturation) + (HlsMax / 2)) / HlsMax;
                else
                    magic2 = luminosity + saturation - ((luminosity * saturation) + HlsMax / 2) / HlsMax;
                magic1 = 2 * luminosity - magic2;

                /* get RGB, change units from HLSMax to RGBMax */
                r = (byte) (((HueToRgb(magic1, magic2, hue + HlsMax / 3) * RgbMax + (HlsMax / 2))) / HlsMax);
                g = (byte) (((HueToRgb(magic1, magic2, hue) * RgbMax + (HlsMax / 2))) / HlsMax);
                b = (byte) (((HueToRgb(magic1, magic2, hue - HlsMax / 3) * RgbMax + (HlsMax / 2))) / HlsMax);
            }

            return Color.FromArgb(r, g, b);
        }

        private int HueToRgb(int n1, int n2, int hue) {
            /* range check: note values passed add/subtract thirds of range */

            /* The following is redundant for WORD (unsigned int) */
            if (hue < 0)
                hue += HlsMax;

            if (hue > HlsMax)
                hue -= HlsMax;

            /* return r,g, or b value from this tridrant */
            if (hue < (HlsMax / 6))
                return (n1 + (((n2 - n1) * hue + (HlsMax / 12)) / (HlsMax / 6)));
            if (hue < (HlsMax / 2))
                return (n2);
            if (hue < ((HlsMax * 2) / 3))
                return (n1 + (((n2 - n1) * (((HlsMax * 2) / 3) - hue) + (HlsMax / 12)) / (HlsMax / 6)));
            return (n1);
        }
    }
}