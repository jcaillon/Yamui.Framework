using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Yamui.Framework.Helper {

    /// <summary>
    /// TODO : Use something like this when we have all the painting done at the same location (ie in the OnPaint of the form)
    /// The idea is that the form asks each child to draw itself providing an abstraction to draw (children dont immediatly use
    /// GDI/GDI+ methods but the methods of an interface that we provide)
    /// We cache the GDI objects (like the brushes, pens etc...) at the form level (same for the ImageStorer, we can put it at a form level)
    /// so they are disposed when the form closes (for brushes, pen, we dispose them right after painting the whole form, it is better not to cache them!)
    /// </summary>
    public class GraphicsPalette : IDisposable 
    {
        [ThreadStatic]
        private static GraphicsPalette _current = null;
        private readonly Dictionary<Color, SolidBrush> _solidBrushes = new Dictionary<Color, SolidBrush>();

        public GraphicsPalette() 
        {
            if (_current == null)
                _current = this;
        }

        public void Dispose() 
        {
            if (_current == this)
                _current = null;

            foreach (var solidBrush in _solidBrushes.Values)
                solidBrush.Dispose();            
        }

        public static SolidBrush GetSolidBrush(Color color) 
        {
            if (!_current._solidBrushes.ContainsKey(color))
                _current._solidBrushes[color] = new SolidBrush(color);

            return _current._solidBrushes[color];
        }
    }

    public static class DrawHelper {
       
        /// <summary>
        /// Paint a border (an empty rectangle with the desired border width)
        /// </summary>
        public static void PaintBorder(this Graphics g, Rectangle borderRectangle, int thickness, Color color) {
            g.PaintBorder(borderRectangle.X, borderRectangle.Y, borderRectangle.Width, borderRectangle.Height, thickness, color);
        }
        
        /// <summary>
        /// Paint a border (an empty rectangle with the desired border width)
        /// </summary>
        public static void PaintBorder(this Graphics g, int x, int y, int width, int height, int thickness, Color color) {
            if (thickness == 1) {
                // historical issue of dotnet that will never be fixed
                width -= 1;
                height -= 1;
            }
            using (var p = new Pen(color, thickness) {
                Alignment = PenAlignment.Inset
            }) {
                g.DrawRectangle(p, x, y, width, height);
            }
        }

        /// <summary>
        /// Fill a rectangle with the given color
        /// </summary>
        public static void PaintRectangle(this Graphics g, Rectangle rectangle, Color color) {
            using (var b = new SolidBrush(color)) {
                g.FillRectangle(b, rectangle);
            }
        }

        /// <summary>
        /// Fill a rectangle with the given color
        /// </summary>
        public static void PaintClipRegion(this Graphics g, Color color) {
            using (var b = new SolidBrush(color)) {
                g.FillRegion(b, g.Clip);
            }
        }

        public static void PaintCachedImage(this Graphics g, Rectangle destinationRectangle, ImageDrawerType type, Size size, Color color) {
            g.DrawImage(ImageStorer.Instance.WithDraw(type, size, color), destinationRectangle, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel);
        }

        public static void HandleWmPaint(ref Message m, Control control, Action<PaintEventArgs> paint) {
            var ps = new WinApi.PAINTSTRUCT();
            bool needDisposeDc = false;
            try {
                Rectangle clip;
                IntPtr dc;
                if (m.WParam == IntPtr.Zero) {
                    dc = WinApi.BeginPaint(new HandleRef(control, control.Handle), ref ps);
                    if (dc == IntPtr.Zero) {
                        return;
                    }
                    needDisposeDc = true;
                    clip = new Rectangle(ps.rcPaint_left, ps.rcPaint_top, ps.rcPaint_right - ps.rcPaint_left, ps.rcPaint_bottom - ps.rcPaint_top);
                } else {
                    dc = m.WParam;
                    clip = control.ClientRectangle;
                }

                if (clip.Width > 0 && clip.Height > 0) {
                    try {
                        using (var bufferedGraphics = BufferedGraphicsManager.Current.Allocate(dc, control.ClientRectangle)) {
                            bufferedGraphics.Graphics.SetClip(clip);
                            using (var pevent = new PaintEventArgs(bufferedGraphics.Graphics, clip)) {
                                paint?.Invoke(pevent);
                            }
                            bufferedGraphics.Render();
                        }
                    } catch (Exception ex) {
                        // BufferContext.Allocate will throw out of memory exceptions
                        // when it fails to create a device dependent bitmap while trying to 
                        // get information about the device we are painting on.
                        // That is not the same as a system running out of memory and there is a 
                        // very good chance that we can continue to paint successfully. We cannot
                        // check whether double buffering is supported in this case, and we will disable it.
                        // We could set a specific string when throwing the exception and check for it here
                        // to distinguish between that case and real out of memory exceptions but we
                        // see no reasons justifying the additional complexity.
                        if (!(ex is OutOfMemoryException)) {
                            throw;
                        }
                    }
                }
            } finally {
                if (needDisposeDc) {
                    WinApi.EndPaint(new HandleRef(control, control.Handle), ref ps);
                }
            }
        }

        public static void DrawImage(Graphics g) {
            // improve performances
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low; // or NearestNeighbour
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
        }

        #region Resume/Suspend drawing

        /// <summary>Allows suspending and resuming redrawing a Windows Forms window via the <b>WM_SETREDRAW</b> 
        /// Windows message.</summary>
        /// <remarks>Usage: The window for which drawing will be suspended and resumed needs to instantiate this type, 
        /// passing a reference to itself to the constructor, then call either of the public methods. For each call to 
        /// <b>SuspendDrawing</b>, a corresponding <b>ResumeDrawing</b> call must be made. Calls may be nested, but
        /// should not be made from any other than the GUI thread. (This code tries to work around such an error, but 
        /// is not guaranteed to succeed.)</remarks>
        public class DrawingSuspender {
                
            private int _suspendCounter;
            
            private IWin32Window _owner;

            private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;


            public DrawingSuspender(IWin32Window owner) {
                this._owner = owner;
            }
            
            /// <summary>This overload allows you to specify whether the optimal flags for a container 
            /// or child control should be used. To specify custom flags, use the overload that accepts 
            /// a <see cref="WinApi.RedrawWindowFlags"/> parameter.</summary>
            /// <param name="isContainer">When <b>true</b>, the optimal flags for redrawing a container 
            /// control are used; otherwise the optimal flags for a child control are used.</param>
            public void ResumeDrawing(bool isContainer = false) {
                ResumeDrawing(isContainer ? WinApi.RedrawWindowFlags.Erase | WinApi.RedrawWindowFlags.Frame | WinApi.RedrawWindowFlags.Invalidate | WinApi.RedrawWindowFlags.AllChildren : WinApi.RedrawWindowFlags.NoErase | WinApi.RedrawWindowFlags.Invalidate | WinApi.RedrawWindowFlags.InternalPaint);
            }

            public void ResumeDrawing(WinApi.RedrawWindowFlags flags) {
                Interlocked.Decrement(ref _suspendCounter);

                if (_suspendCounter == 0) {
                    Action resume = new Action(() => {
                        WinApi.SendMessage(new HandleRef(_owner, _owner.Handle), (int) Window.Msg.WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                        WinApi.RedrawWindow(new HandleRef(_owner, _owner.Handle), IntPtr.Zero, IntPtr.Zero, flags);
                    });
                    try {
                        resume();
                    } catch (InvalidOperationException) {
                        _synchronizationContext.Post(s => ((Action) s)(), resume);
                    }
                }
            }

            public void SuspendDrawing() {
                try {
                    if (_suspendCounter == 0) {
                        Action suspend = new Action(() => WinApi.SendMessage(new HandleRef(_owner, _owner.Handle), (int) Window.Msg.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero));
                        try {
                            suspend();
                        } catch (InvalidOperationException) {
                            _synchronizationContext.Post(s => ((Action) s)(), suspend);
                        }
                    }
                } finally {
                    Interlocked.Increment(ref _suspendCounter);
                }
            }
        } 

        #endregion
        
    }
}