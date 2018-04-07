using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yamui.Framework.Helper {
    public static class PaintHelper {
        [ThreadStatic] private static Bitmap checkImage;

        [ThreadStatic] private static ImageAttributes disabledImageAttr; // ImageAttributes used to render disabled images

        public static void DrawFlatCheckBox(Graphics graphics, Rectangle rectangle, Color foreground, Color background, ButtonState state) {
            Rectangle offsetRectangle = new Rectangle(rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2);

            //if (checkImage == null || checkImage.Width != rectangle.Width || checkImage.Height != rectangle.Height) {
            //    if (checkImage != null) {
            //        checkImage.Dispose();
            //        checkImage = null;
            //    }

            
            Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
            Color bitmapBackColor;
            using (Graphics g2 = Graphics.FromImage(bitmap)) {
                g2.Clear(Color.Transparent);
                IntPtr dc = g2.GetHdc();
                try {
                    WinApi.RECT rcCheck = WinApi.RECT.FromXYWH(0, 0, rectangle.Width, rectangle.Height);
                    WinApi.DrawFrameControl(new HandleRef(null, dc), ref rcCheck, WinApi.DrawFrameControlTypes.DFC_CAPTION, WinApi.DrawFrameControlStates.DFCS_CAPTIONRESTORE | WinApi.DrawFrameControlStates.DFCS_MONO);
                } finally {
                    g2.ReleaseHdcInternal(dc);
                }
                bitmapBackColor = bitmap.GetPixel(3, 3);
                using (var p = new Pen(bitmapBackColor, 2)) {
                    p.Alignment = PenAlignment.Right;
                    g2.DrawRectangle(p, new Rectangle(1, 1, rectangle.Width -1, rectangle.Height-1));
                }
                ImageAttributes attrs2 = new ImageAttributes();
                attrs2.SetRemapTable(new[] {
                    new ColorMap {
                        OldColor = bitmapBackColor,
                        NewColor = Color.White
                    }
                }, ColorAdjustType.Bitmap);
                g2.DrawImage(bitmap, new Rectangle(0, 0, rectangle.Width, rectangle.Height), 0, 0, rectangle.Width, rectangle.Height, GraphicsUnit.Pixel, attrs2);
            }
            //}

            using (var b = new SolidBrush(Color.Black))
                graphics.FillRectangle(b, rectangle);

            //var imgRect = new Rectangle(rectangle.X + 2, rectangle.Y + 2, checkImage.Width - 4, checkImage.Height - 4);
            var imgRect = rectangle;
            ImageAttributes attrs = new ImageAttributes();
            attrs.SetRemapTable(new[] {
                new ColorMap {
                    OldColor = Color.Black,
                    NewColor = foreground
                },
                new ColorMap {
                    OldColor = Color.White,
                    NewColor = background
                }
            }, ColorAdjustType.Bitmap);
            graphics.DrawImage(bitmap, imgRect, 0, 0, imgRect.Width, imgRect.Height, GraphicsUnit.Pixel, attrs);
            attrs.Dispose();
        }

        // Takes a black and transparent image, turns black pixels into some other color, and leaves transparent pixels alone
        internal static void DrawImageColorized(Graphics graphics, Image image, Rectangle destination, Color replaceBlack) {
            DrawImageColorized(graphics, image, destination, RemapBlackAndWhitePreserveTransparentMatrix(replaceBlack, Color.White));
        }

        // Takes a black and white image, and paints it in color
        private static void DrawImageColorized(Graphics graphics, Image image, Rectangle destination, ColorMatrix matrix) {
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix);
            graphics.DrawImage(image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes, null, IntPtr.Zero);
            attributes.Dispose();
        }

        public static void DrawFrameControl(Graphics graphics, Rectangle rect, WinApi.DrawFrameControlTypes kind, WinApi.DrawFrameControlStates state, Color foreColor, Color backColor) {
            DrawFrameControl(graphics, rect.X, rect.Y, rect.Width, rect.Height, kind, state, foreColor, backColor);
        }

        private static void DrawFrameControl(Graphics graphics, int x, int y, int width, int height, WinApi.DrawFrameControlTypes kind, WinApi.DrawFrameControlStates state, Color foreColor, Color backColor) {
            WinApi.RECT rcFrame = WinApi.RECT.FromXYWH(0, 0, width, height);
            using (Bitmap bitmap = new Bitmap(width, height)) {
                using (Graphics g2 = Graphics.FromImage(bitmap)) {
                    g2.Clear(Color.Transparent);
                    /* using( WindowsGraphics wg = WindowsGraphics.FromGraphics(g2) ){
                        DrawFrameControl(new HandleRef(wg, wg.DeviceContext.Hdc), ref rcFrame, kind, (int) state);  */
                    IntPtr dc = g2.GetHdc();
                    try {
                        WinApi.DrawFrameControl(new HandleRef(null, dc), ref rcFrame, kind, state);
                    } finally {
                        g2.ReleaseHdc();
                    }

                    if (foreColor == Color.Empty || backColor == Color.Empty) {
                        graphics.DrawImage(bitmap, x, y);
                    } else {
                        // Replace black/white with foreColor/backColor.
                        ImageAttributes attrs = new ImageAttributes();
                        ColorMap cm1 = new ColorMap();
                        cm1.OldColor = Color.Black;
                        cm1.NewColor = foreColor;
                        ColorMap cm2 = new ColorMap();
                        cm2.OldColor = Color.White;
                        cm2.NewColor = backColor;
                        attrs.SetRemapTable(new ColorMap[2] {cm1, cm2}, ColorAdjustType.Bitmap);
                        graphics.DrawImage(bitmap, new Rectangle(x, y, width, height), 0, 0, width, height, GraphicsUnit.Pixel, attrs, null, IntPtr.Zero);
                    }
                }
            }
        }

        /// <devdoc>
        ///     Draws an image and makes it look disabled.
        /// </devdoc>
        public static void DrawImageDisabled(Graphics graphics, Image image, int x, int y, Color background) {
            DrawImageDisabled(graphics, image, new Rectangle(x, y, image.Width, image.Height), background, false);
        }

        /// <devdoc>
        ///     Draws an image and makes it look disabled.
        /// </devdoc>
        internal static void DrawImageDisabled(Graphics graphics, Image image, Rectangle imageBounds, Color background, bool unscaledImage) {
            Size imageSize = image.Size;
            if (disabledImageAttr == null) {
                var array = new float[5][];
                array[0] = new[] {0.2125f, 0.2125f, 0.2125f, 0, 0};
                array[1] = new[] {0.2577f, 0.2577f, 0.2577f, 0, 0};
                array[2] = new[] {0.0361f, 0.0361f, 0.0361f, 0, 0};
                array[3] = new[] {0f, 0f, 0f, 1f, 0f};
                array[4] = new[] {0.38f, 0.38f, 0.38f, 0, 1};
                ColorMatrix grayMatrix = new ColorMatrix(array);
                disabledImageAttr = new ImageAttributes();
                disabledImageAttr.ClearColorKey();
                disabledImageAttr.SetColorMatrix(grayMatrix);
            }

            if (unscaledImage) {
                using (Bitmap bmp = new Bitmap(image.Width, image.Height)) {
                    using (Graphics g = Graphics.FromImage(bmp)) {
                        g.DrawImage(image,
                            new Rectangle(0, 0, imageSize.Width, imageSize.Height),
                            0, 0, imageSize.Width, imageSize.Height,
                            GraphicsUnit.Pixel,
                            disabledImageAttr);
                    }

                    graphics.DrawImageUnscaled(bmp, imageBounds);
                }
            } else {
                graphics.DrawImage(image,
                    imageBounds,
                    0, 0, imageSize.Width, imageSize.Height,
                    GraphicsUnit.Pixel,
                    disabledImageAttr);
            }
        }

        // takes an image and replaces all the pixels of oldColor with newColor, drawing the new image into the rectangle on
        // the supplied Graphics object.
        internal static void DrawImageReplaceColor(Graphics g, Image image, Rectangle dest, Color oldColor, Color newColor) {
            ImageAttributes attrs = new ImageAttributes();
            ColorMap cm = new ColorMap {
                OldColor = oldColor,
                NewColor = newColor
            };
            attrs.SetRemapTable(new ColorMap[] {cm}, ColorAdjustType.Bitmap);
            g.DrawImage(image, dest, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attrs, null, IntPtr.Zero);
            attrs.Dispose();
        }

        /// <devdoc>
        ///     Draws a locked selection frame around the given rectangle.
        /// </devdoc>
        public static void DrawLockedFrame(Graphics graphics, Rectangle rectangle, bool primary) {
            Pen pen;

            if (graphics == null) {
                throw new ArgumentNullException("graphics");
            }

            if (primary) {
                pen = Pens.White;
            } else {
                pen = Pens.Black;
            }

            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
            rectangle.Inflate(-1, -1);
            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);

            if (primary) {
                pen = Pens.Black;
            } else {
                pen = Pens.White;
            }

            rectangle.Inflate(-1, -1);
            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
        }

        /// <devdoc>
        ///     Draws a string in the style appropriate for disabled items.
        /// </devdoc>
        public static void DrawStringDisabled(Graphics graphics, string s, Font font,
            Color color, RectangleF layoutRectangle,
            StringFormat format) {
            if (graphics == null) {
                throw new ArgumentNullException("graphics");
            }

            if (SystemInformation.HighContrast) {
                // Ignore the foreground color argument and don't do shading in high contrast, 
                // as colors should match the OS-defined ones.
                graphics.DrawString(s, font, SystemBrushes.GrayText, layoutRectangle, format);
            } else {
                layoutRectangle.Offset(1, 1);
                using (SolidBrush brush = new SolidBrush(ColorHelper.LightLight(color))) {
                    graphics.DrawString(s, font, brush, layoutRectangle, format);

                    layoutRectangle.Offset(-1, -1);
                    color = ColorHelper.Dark(color);
                    brush.Color = color;
                    graphics.DrawString(s, font, brush, layoutRectangle, format);
                }
            }
        }

        // Takes a black and white image, and replaces those colors with the colors of your choice.
        // The replaceBlack and replaceWhite colors must have alpha = 255, because the alpha value
        // of the bitmap is preserved.
        private static ColorMatrix RemapBlackAndWhitePreserveTransparentMatrix(Color replaceBlack, Color replaceWhite) {
            float normBlackRed = replaceBlack.R / (float) 255.0;
            float normBlackGreen = replaceBlack.G / (float) 255.0;
            float normBlackBlue = replaceBlack.B / (float) 255.0;

            float normWhiteRed = replaceWhite.R / (float) 255.0;
            float normWhiteGreen = replaceWhite.G / (float) 255.0;
            float normWhiteBlue = replaceWhite.B / (float) 255.0;

            ColorMatrix matrix = new ColorMatrix {
                Matrix00 = -normBlackRed,
                Matrix01 = -normBlackGreen,
                Matrix02 = -normBlackBlue,
                Matrix10 = normWhiteRed,
                Matrix11 = normWhiteGreen,
                Matrix12 = normWhiteBlue,
                Matrix33 = 1.0f,
                Matrix40 = normBlackRed,
                Matrix41 = normBlackGreen,
                Matrix42 = normBlackBlue,
                Matrix44 = 1.0f
            };

            return matrix;
        }

        internal static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft) {
            if (g == null) {
                throw new ArgumentNullException("g");
            }

            if (backgroundImageLayout == ImageLayout.Tile) {
                // tile

                using (TextureBrush textureBrush = new TextureBrush(backgroundImage, WrapMode.Tile)) {
                    // Make sure the brush origin matches the display rectangle, not the client rectangle,
                    // so the background image scrolls on AutoScroll forms.
                    if (scrollOffset != Point.Empty) {
                        Matrix transform = textureBrush.Transform;
                        transform.Translate(scrollOffset.X, scrollOffset.Y);
                        textureBrush.Transform = transform;
                    }

                    g.FillRectangle(textureBrush, clipRect);
                }
            } else {
                // Center, Stretch, Zoom

                Rectangle imageRectangle = CalculateBackgroundImageRectangle(bounds, backgroundImage, backgroundImageLayout);

                //flip the coordinates only if we don't do any layout, since otherwise the image should be at the center of the
                //displayRectangle anyway.

                if (rightToLeft == RightToLeft.Yes && backgroundImageLayout == ImageLayout.None) {
                    imageRectangle.X += clipRect.Width - imageRectangle.Width;
                }

                // We fill the entire cliprect with the backcolor in case the image is transparent.
                // Also, if gdi+ can't quite fill the rect with the image, they will interpolate the remaining
                // pixels, and make them semi-transparent. This is another reason why we need to fill the entire rect.
                // If we didn't where ever the image was transparent, we would get garbage. VS Whidbey #504388
                using (SolidBrush brush = new SolidBrush(backColor)) {
                    g.FillRectangle(brush, clipRect);
                }

                if (!clipRect.Contains(imageRectangle)) {
                    if (backgroundImageLayout == ImageLayout.Stretch || backgroundImageLayout == ImageLayout.Zoom) {
                        imageRectangle.Intersect(clipRect);
                        g.DrawImage(backgroundImage, imageRectangle);
                    } else if (backgroundImageLayout == ImageLayout.None) {
                        imageRectangle.Offset(clipRect.Location);
                        Rectangle imageRect = imageRectangle;
                        imageRect.Intersect(clipRect);
                        Rectangle partOfImageToDraw = new Rectangle(Point.Empty, imageRect.Size);
                        g.DrawImage(backgroundImage, imageRect, partOfImageToDraw.X, partOfImageToDraw.Y, partOfImageToDraw.Width,
                            partOfImageToDraw.Height, GraphicsUnit.Pixel);
                    } else {
                        Rectangle imageRect = imageRectangle;
                        imageRect.Intersect(clipRect);
                        Rectangle partOfImageToDraw = new Rectangle(new Point(imageRect.X - imageRectangle.X, imageRect.Y - imageRectangle.Y)
                            , imageRect.Size);

                        g.DrawImage(backgroundImage, imageRect, partOfImageToDraw.X, partOfImageToDraw.Y, partOfImageToDraw.Width,
                            partOfImageToDraw.Height, GraphicsUnit.Pixel);
                    }
                } else {
                    ImageAttributes imageAttrib = new ImageAttributes();
                    imageAttrib.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(backgroundImage, imageRectangle, 0, 0, backgroundImage.Width, backgroundImage.Height, GraphicsUnit.Pixel, imageAttrib);
                    imageAttrib.Dispose();
                }
            }
        }

        internal static Rectangle CalculateBackgroundImageRectangle(Rectangle bounds, Image backgroundImage, ImageLayout imageLayout) {
            Rectangle result = bounds;

            if (backgroundImage != null) {
                switch (imageLayout) {
                    case ImageLayout.Stretch:
                        result.Size = bounds.Size;
                        break;

                    case ImageLayout.None:
                        result.Size = backgroundImage.Size;
                        break;

                    case ImageLayout.Center:
                        result.Size = backgroundImage.Size;
                        Size szCtl = bounds.Size;

                        if (szCtl.Width > result.Width) {
                            result.X = (szCtl.Width - result.Width) / 2;
                        }

                        if (szCtl.Height > result.Height) {
                            result.Y = (szCtl.Height - result.Height) / 2;
                        }

                        break;

                    case ImageLayout.Zoom:
                        Size imageSize = backgroundImage.Size;
                        float xRatio = bounds.Width / (float) imageSize.Width;
                        float yRatio = bounds.Height / (float) imageSize.Height;
                        if (xRatio < yRatio) {
                            //width should fill the entire bounds.
                            result.Width = bounds.Width;
                            // preserve the aspect ratio by multiplying the xRatio by the height
                            // adding .5 to round to the nearest pixel
                            result.Height = (int) ((imageSize.Height * xRatio) + .5);
                            if (bounds.Y >= 0) {
                                result.Y = (bounds.Height - result.Height) / 2;
                            }
                        } else {
                            // width should fill the entire bounds
                            result.Height = bounds.Height;
                            // preserve the aspect ratio by multiplying the xRatio by the height
                            // adding .5 to round to the nearest pixel
                            result.Width = (int) ((imageSize.Width * yRatio) + .5);
                            if (bounds.X >= 0) {
                                result.X = (bounds.Width - result.Width) / 2;
                            }
                        }

                        break;
                }
            }

            return result;
        }
    }
}