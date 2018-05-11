using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Yamui.Framework.Helper {

    /// <summary>
    /// Put bitmap in a cache to avoid having to recreate them each time
    /// </summary>
    public class ImageStorer {

        private static ImageStorer _instance;

        public static ImageStorer Instance => _instance ?? (_instance = new ImageStorer());

        private Dictionary<string, Bitmap> _storedBitmaps = new Dictionary<string, Bitmap>();

        /// <summary>
        /// Get an image from the drawer
        /// </summary>
        public Bitmap WithDraw(ImageDrawerType type, Size size, Color color) {
            var key = GetKey(type, size, color);
            if (_storedBitmaps.ContainsKey(key)) {
                return _storedBitmaps[key];
            }

            // create the new bitmap
            Bitmap newImage = GetImage(type, size, color);
            _storedBitmaps.Add(key, newImage);
            return newImage;
        }

        /// <summary>
        /// Get an image from the drawer. If it doesn't exist, it uses the given create function and stores the bitmap for later usage
        /// </summary>
        public Bitmap WithDraw(string type, Size size, Color color, Func<Size, Color, Bitmap> createBitmapFunc) {
            var key = GetKey(type, size, color);
            if (_storedBitmaps.ContainsKey(key)) {
                return _storedBitmaps[key];
            }

            // create the new bitmap
            Bitmap newImage = createBitmapFunc(size, color);
            _storedBitmaps.Add(key, newImage);
            return newImage;
        }
        
        /// <summary>
        /// Clear all the stored images
        /// </summary>
        public void ClearCache() {
            foreach (var storedBitmapsValue in _storedBitmaps.Values) {
                storedBitmapsValue.Dispose();
            }
            _storedBitmaps.Clear();
        }

        /// <summary>
        /// get the key to use in the <see cref="_storedBitmaps"/> dictionnary
        /// </summary>
        private string GetKey(ImageDrawerType type, Size size, Color color) {
            return $"{(int) type}:{size.Width}:{size.Height}:{color.A}:{color.R}:{color.G}:{color.B}";
        }

        /// <summary>
        /// get the key to use in the <see cref="_storedBitmaps"/> dictionnary
        /// </summary>
        private string GetKey(string type, Size size, Color color) {
            return $"c:{type}:{size.Width}:{size.Height}:{color.A}:{color.R}:{color.G}:{color.B}";
        }

        private Bitmap GetImage(ImageDrawerType type, Size size, Color color) {
            Bitmap bitmap;
            int middlePixel = size.Width / 2;
            int sizeIncrease;
            switch (type) {
                case ImageDrawerType.Close:
                    bitmap = new Bitmap(size.Width, size.Height);
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        using (Pen p = new Pen(Color.FromArgb(70, color), 1)) {
                            p.Alignment = PenAlignment.Right;
                            g2.DrawLine(p, 0, 1, size.Width - 2, size.Width - 1);
                            g2.DrawLine(p, 1, 0, size.Width - 1, size.Width - 2);
                            g2.DrawLine(p, 0, size.Width - 2, size.Width - 2, 0);
                            g2.DrawLine(p, 1, size.Width - 1, size.Width - 1, 1);
                        }
                        using (Pen p = new Pen(color, 1)) {
                            p.Alignment = PenAlignment.Right;
                            g2.DrawLine(p, 0, 0, middlePixel - 1, middlePixel - 1);
                            g2.DrawLine(p, 0, size.Width - 1, middlePixel - 1, middlePixel);
                            g2.DrawLine(p, size.Width - 1, 0, middlePixel, middlePixel - 1);
                            g2.DrawLine(p, size.Width - 1, size.Width - 1, middlePixel, middlePixel);
                        }
                    }
                    break;
                case ImageDrawerType.Minimize:
                    bitmap = new Bitmap(size.Width, size.Height);
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        using (Pen p = new Pen(color, 1)) {
                            p.Alignment = PenAlignment.Right;
                            g2.DrawLine(p, 0, middlePixel, size.Width - 1, middlePixel);
                        }
                    }
                    break;
                case ImageDrawerType.Maximize:
                    bitmap = new Bitmap(size.Width, size.Height);
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        g2.PaintBorder(0, 0, size.Width, size.Width, 1, color);
                    }
                    break;
                case ImageDrawerType.Restore:
                    bitmap = new Bitmap(size.Width, size.Height);
                    var offset = (int) Math.Round((double)2 / 10 * size.Width);
                    var rectangleSize = size.Width - offset;
                    var rectangle = new Rectangle(offset, 0, rectangleSize, rectangleSize);
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        g2.PaintBorder(rectangle, 1, color);
                        rectangle.X = 0;
                        rectangle.Y = offset;
                        g2.PaintBorder(rectangle, 1, color);
                        rectangle.Inflate(-1, -1);
                        g2.CompositingMode = CompositingMode.SourceCopy;
                        g2.PaintRectangle(rectangle, Color.Transparent);
                    }
                    break;
                case ImageDrawerType.ArrowDown:
                    bitmap = new Bitmap(size.Width, size.Height);
                    sizeIncrease = (size.Width - 12) / 3;
                    var oneIfOdd = sizeIncrease < 1 ? size.Width % 2 : 0;
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        using (Pen p = new Pen(color, 1)) {
                            p.Alignment = PenAlignment.Right;
                            g2.DrawLine(p, middlePixel - 4 - sizeIncrease, middlePixel - 2 - sizeIncrease, middlePixel - 2, middlePixel);
                            g2.DrawLine(p, middlePixel - 1, middlePixel + 1, middlePixel + oneIfOdd, middlePixel + 1);
                            g2.DrawLine(p, middlePixel + 1 + oneIfOdd, middlePixel, middlePixel + 3 + sizeIncrease + oneIfOdd, middlePixel - 2 - sizeIncrease);
                        }
                        using (Pen p = new Pen(Color.FromArgb(175, color), 1)) {
                            p.Alignment = PenAlignment.Center;
                            g2.DrawLine(p, middlePixel - 3 - sizeIncrease, middlePixel - 2 - sizeIncrease, middlePixel - 2, middlePixel - 1);
                            g2.DrawLine(p, middlePixel - 1, middlePixel, middlePixel + oneIfOdd, middlePixel);
                            g2.DrawLine(p, middlePixel + 1 + oneIfOdd, middlePixel - 1, middlePixel + 2 + sizeIncrease + oneIfOdd, middlePixel - 2 - sizeIncrease);
                        }
                    }
                    break;
                case ImageDrawerType.AlternateArrowDown:
                    bitmap = new Bitmap(size.Width, size.Height);
                    sizeIncrease = (size.Width - 12) / 3;
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        using (Pen p = new Pen(color, 1)) {
                            p.Alignment = PenAlignment.Right;
                            for (int i = -1; i < 2; i++) {
                                g2.DrawLine(p, middlePixel - 4 - sizeIncrease, middlePixel - 2 - sizeIncrease + i, middlePixel - 1, middlePixel + 1 + i);
                                g2.DrawLine(p, middlePixel, middlePixel + i, middlePixel + 2 + sizeIncrease, middlePixel - 2 - sizeIncrease + i);
                            }
                        }
                    }
                    break;
                case ImageDrawerType.ArrowUp:
                    bitmap = GetImage(ImageDrawerType.ArrowDown, size, color);
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case ImageDrawerType.ArrowRight:
                    bitmap = GetImage(ImageDrawerType.ArrowDown, size, color);
                    bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case ImageDrawerType.ArrowLeft:
                    bitmap = GetImage(ImageDrawerType.ArrowDown, size, color);
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case ImageDrawerType.Checkbox:
                    bitmap = new Bitmap(size.Width, size.Height);
                    sizeIncrease = (size.Width - 12) / 3;
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        using (Pen p = new Pen(color, 1)) {
                            p.Alignment = PenAlignment.Right;
                            for (int i = 0; i < 3; i++) {
                                g2.DrawLine(p, middlePixel - 5 - sizeIncrease, middlePixel - 2 - sizeIncrease + i, middlePixel - 2, middlePixel + 1 + i);
                                g2.DrawLine(p, middlePixel - 1, middlePixel + i, middlePixel + 3 + sizeIncrease, middlePixel - 4 - sizeIncrease + i);
                            }
                        }
                        using (Pen p = new Pen(Color.FromArgb(125, color), 1)) {
                            p.Alignment = PenAlignment.Right;
                            var i = -1;
                            g2.DrawLine(p, middlePixel - 5 - sizeIncrease, middlePixel - 2 - sizeIncrease + i, middlePixel - 2, middlePixel + 1 + i);
                            g2.DrawLine(p, middlePixel - 1, middlePixel + i, middlePixel + 3 + sizeIncrease, middlePixel - 4 - sizeIncrease + i);
                        }
                    }
                    break;
                case ImageDrawerType.WindowResizeIcon:
                    bitmap = new Bitmap(size.Width, size.Height);
                    sizeIncrease = size.Width / 4;
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        using (var b = new SolidBrush(color)) {
                            var resizeHandleSize = new Size(sizeIncrease, sizeIncrease);
                            g2.FillRectangles(b, new[] {
                                new Rectangle(new Point(size.Width - sizeIncrease, size.Height - sizeIncrease), resizeHandleSize),
                                new Rectangle(new Point(size.Width - sizeIncrease * 2 - 1, size.Height - sizeIncrease * 2 - 1), resizeHandleSize),
                                new Rectangle(new Point(size.Width - sizeIncrease * 2 - 1, size.Height - sizeIncrease), resizeHandleSize),
                                new Rectangle(new Point(size.Width - sizeIncrease, size.Height - sizeIncrease * 2 - 1), resizeHandleSize),
                                new Rectangle(new Point(size.Width - sizeIncrease * 3 - 2, size.Height - sizeIncrease), resizeHandleSize),
                                new Rectangle(new Point(size.Width - sizeIncrease, size.Height - sizeIncrease * 3 - 2), resizeHandleSize)
                            });
                        }
                    }
                    break;
                case ImageDrawerType.ArrowDownFull:
                    bitmap = new Bitmap(size.Width, size.Height);
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        using (var b = new SolidBrush(color)) {
                            var arrowHeight = (int) Math.Round((double) 6 / 10 * size.Height / 2);
                            var arrowBaseWidth = (int) Math.Round((double) size.Width / 2);
                            g2.PixelOffsetMode = PixelOffsetMode.HighQuality; // needed or some pixels are missing
                            g2.FillPolygon(b, new[] {new Point(middlePixel - arrowBaseWidth, middlePixel - arrowHeight), new Point(middlePixel + arrowBaseWidth, middlePixel - arrowHeight), new Point(middlePixel, middlePixel + arrowHeight)});
                        }
                    }
                    break;
                case ImageDrawerType.LightningStrike:
                    bitmap = new Bitmap(size.Width, size.Height);
                    var arrowBase = size.Height / 3;
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        g2.Clear(Color.Transparent);
                        using (var b = new SolidBrush(color)) {
                            g2.PixelOffsetMode = PixelOffsetMode.HighQuality; // needed or some pixels are missing
                            g2.SmoothingMode = SmoothingMode.AntiAlias; // needed or some pixels are missing
                            g2.FillPolygon(b, new[] {new Point(0, size.Height), new Point(middlePixel, middlePixel - arrowBase), new Point(middlePixel, middlePixel)});
                            g2.FillPolygon(b, new[] {new Point(size.Width, 0), new Point(middlePixel, middlePixel + arrowBase), new Point(middlePixel, middlePixel)});
                        }
                    }
                    break;
                case ImageDrawerType.CloseAll:
                    bitmap = GetImage(ImageDrawerType.Close, size, color);
                    var boxOffset = (int) Math.Round((double)2 / 10 * size.Width);
                    using (Graphics g2 = Graphics.FromImage(bitmap)) {
                        //g2.PaintBorder(boxOffset, boxOffset, size.Width - 2*boxOffset, size.Width - 2*boxOffset, 1, color);
                        using (Pen p = new Pen(Color.FromArgb(125, color), 1)) {
                            p.Alignment = PenAlignment.Right;
                            g2.DrawLine(p, 0, 0, 0, size.Height - 1);
                            g2.DrawLine(p, size.Width - 1, 0, size.Width - 1, size.Height - 1);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return bitmap;
        }
        
    }

    public enum ImageDrawerType {
        Close,
        Minimize,
        Maximize,
        Restore,
        ArrowDown,
        ArrowUp,
        ArrowRight,
        ArrowLeft,
        Checkbox,
        AlternateArrowDown,
        ArrowDownFull,
        WindowResizeIcon,
        LightningStrike,
        CloseAll
    }
}