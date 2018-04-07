﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Yamui.Framework.Themes;

namespace Yamui.Framework.Controls {

    [Designer(typeof(YamuiControlDesigner))]
    public class YamuiControl : Control, IScrollableControl {

        #region IYamuiControl

        public void UpdateBoundsPublic() {
            UpdateBounds();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Paint transparent background
        /// </summary>
        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            if (Parent != null) {
                clipRect.Offset(Location);
                PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                GraphicsState state = graphics.Save();
                //graphics.SmoothingMode = SmoothingMode.HighSpeed;
                try {
                    graphics.TranslateTransform(-Location.X, -Location.Y);
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                } finally {
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            } else {
                graphics.Clear(YamuiThemeManager.Current.FormBack);
            }
        }

        #endregion

        #region MeasureTextCache

        protected override void OnTextChanged(EventArgs e) {
            MeasureTextCache.InvalidateCache();
            base.OnTextChanged(e);
        }

        internal MeasureTextCache2 MeasureTextCache {
            get {
                if (_textMeasurementCache == null) {
                    _textMeasurementCache = new MeasureTextCache2();
                }

                return _textMeasurementCache;
            }
        }

        public static readonly Size MaxSize = new Size(Int32.MaxValue, Int32.MaxValue);
        public static readonly Size InvalidSize = new Size(Int32.MinValue, Int32.MinValue);
        private MeasureTextCache2 _textMeasurementCache;

        /// MeasureTextCache
        /// Cache mechanism added for VSWhidbey 500516
        /// 3000 character strings take 9 seconds to load the form
        internal sealed class MeasureTextCache2 {

            private Size _unconstrainedPreferredSize = InvalidSize;
            private const int MaxCacheSize = 6; // the number of preferred sizes to store
            private int _nextCacheEntry = -1; // the next place in the ring buffer to store a preferred size

            private PreferredSizeCache[] _sizeCacheList; // MRU of size MaxCacheSize

            /// InvalidateCache
            /// Clears out the cached values, should be called whenever Text, Font or a TextFormatFlag has changed
            public void InvalidateCache() {
                _unconstrainedPreferredSize = InvalidSize;
                _sizeCacheList = null;
            }

            /// GetTextSize
            /// Given constraints, format flags a font and text, determine the size of the string
            /// employs an MRU of the last several constraints passed in via a ring-buffer of size MaxCacheSize.
            /// Assumes Text and TextFormatFlags are the same, if either were to change, a call to 
            /// InvalidateCache should be made
            public Size GetTextSize(string text, Font font, Size proposedConstraints, TextFormatFlags flags) {
                if (!TextRequiresWordBreak(text, font, proposedConstraints, flags)) {
                    // Text fits within proposed width

                    // IF we're here, this means we've got text that can fit into the proposedConstraints
                    // without wrapping.  We've determined this because our 

                    // as a side effect of calling TextRequiresWordBreak, 
                    // unconstrainedPreferredSize is set.
                    return _unconstrainedPreferredSize;
                } else {
                    // Text does NOT fit within proposed width - requires WordBreak

                    // IF we're here, this means that the wrapping width is smaller 
                    // than our max width.  For example: we measure the text with infinite
                    // bounding box and we determine the width to fit all the characters 
                    // to be 200 px wide.  We would come here only for proposed widths less
                    // than 200 px.

                    // Create our ring buffer if we dont have one
                    if (_sizeCacheList == null) {
                        _sizeCacheList = new PreferredSizeCache[MaxCacheSize];
                    }

                    // check the existing constraints from previous calls
                    foreach (PreferredSizeCache sizeCache in _sizeCacheList) {
                        if (sizeCache.ConstrainingSize == proposedConstraints) {
                            return sizeCache.PreferredSize;
                        }

                        if ((sizeCache.ConstrainingSize.Width == proposedConstraints.Width)
                            && (sizeCache.PreferredSize.Height <= proposedConstraints.Height)) {
                            // Caching a common case where the width matches perfectly, and the stored preferred height 
                            // is smaller or equal to the constraining size.                             
                            //        prefSize = GetPreferredSize(w,Int32.MaxValue);
                            //        prefSize = GetPreferredSize(w,prefSize.Height);

                            return sizeCache.PreferredSize;
                        }

                        // 
                    }

                    // if we've gotten here, it means we dont have a cache entry, therefore
                    // we should add a new one in the next available slot.
                    Size prefSize = TextRenderer.MeasureText(text, font, proposedConstraints, flags);
                    _nextCacheEntry = (_nextCacheEntry + 1) % MaxCacheSize;
                    _sizeCacheList[_nextCacheEntry] = new PreferredSizeCache(proposedConstraints, prefSize);

                    return prefSize;
                }
            }

            /// GetUnconstrainedSize
            /// Gets the unconstrained (Int32.MaxValue, Int32.MaxValue) size for a piece of text
            private Size GetUnconstrainedSize(string text, Font font, TextFormatFlags flags) {
                if (_unconstrainedPreferredSize == InvalidSize) {
                    // we also investigated setting the SingleLine flag, however this did not yield as much benefit as the word break
                    // and had possibility of causing internationalization issues.

                    flags = (flags & ~TextFormatFlags.WordBreak); // rip out the wordbreak flag
                    _unconstrainedPreferredSize = TextRenderer.MeasureText(text, font, MaxSize, flags);
                }

                return _unconstrainedPreferredSize;
            }

            /// TextRequiresWordBreak
            /// If you give the text all the space in the world it wants, then there should be no reason
            /// for it to break on a word.  So we find out what the unconstrained size is (Int32.MaxValue, Int32.MaxValue)
            /// for a string - eg. 35, 13.  If the size passed in has a larger width than 35, then we know that
            /// the WordBreak flag is not necessary.
            public bool TextRequiresWordBreak(string text, Font font, Size size, TextFormatFlags flags) {
                // if the unconstrained size of the string is larger than the proposed width
                // we need the word break flag, otherwise we dont, its a perf hit to use it.
                return GetUnconstrainedSize(text, font, flags).Width > size.Width;
            }

            private struct PreferredSizeCache {
                public PreferredSizeCache(Size constrainingSize, Size preferredSize) {
                    ConstrainingSize = constrainingSize;
                    PreferredSize = preferredSize;
                }

                public Size ConstrainingSize;
                public Size PreferredSize;
            }
        }

        #endregion
    }

    #region designer

    internal class YamuiControlDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {

            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");

            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");

            //properties.Remove("BackColor");
            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("UseVisualStyleBackColor");

            properties.Remove("Font");
            //properties.Remove("ForeColor");
            properties.Remove("RightToLeft");
            properties.Remove("Text");

            base.PreFilterProperties(properties);
        }
    }

    #endregion
}