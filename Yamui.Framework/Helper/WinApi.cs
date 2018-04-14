#region header

// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (WinApi.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace Yamui.Framework.Helper {
    [SuppressUnmanagedCodeSecurity]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static partial class WinApi {
        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public WindowStyles dwStyle;
            public WindowStylesEx dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler) : this() {
                // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
                cbSize = (UInt32) (Marshal.SizeOf(typeof(WINDOWINFO)));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;

            public MARGINS(int Left, int Right, int Top, int Bottom) {
                cxLeftWidth = Left;
                cxRightWidth = Right;
                cyTopHeight = Top;
                cyBottomHeight = Bottom;
            }

            public MARGINS(Padding nonClientAreaPadding) {
                cxLeftWidth = nonClientAreaPadding.Left;
                cxRightWidth = nonClientAreaPadding.Right;
                cyTopHeight = nonClientAreaPadding.Top;
                cyBottomHeight = nonClientAreaPadding.Bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int x;
            public int y;

            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE {
            public int cx;
            public int cy;

            public SIZE(int cx, int cy) {
                this.cx = cx;
                this.cy = cy;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TCHITTESTINFO {
            public Point pt;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class COMRECT {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public COMRECT() { }

            public COMRECT(Rectangle r) {
                left = r.X;
                top = r.Y;
                right = r.Right;
                bottom = r.Bottom;
            }

            public COMRECT(int left, int top, int right, int bottom) {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public static COMRECT FromXYWH(int x, int y, int width, int height) {
                return new COMRECT(x, y, x + width, y + height);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom) {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(Rectangle r) {
                left = r.Left;
                top = r.Top;
                right = r.Right;
                bottom = r.Bottom;
            }

            public void Pad(Padding pad) {
                left += pad.Left;
                top += pad.Top;
                right -= pad.Right;
                bottom -= pad.Bottom;
            }

            public static RECT FromXYWH(int x, int y, int width, int height) {
                return new RECT(x, y, x + width, y + height);
            }

            public Size Size {
                get { return new Size(right - left, bottom - top); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO {
            public POINT ptReserved;

            /// <summary>
            /// The maximized width (x member) and the maximized height (y member) of the window. For top-level windows, this value is based on the width of the primary monitor.
            /// </summary>
            public POINT ptMaxSize;

            /// <summary>
            /// The position of the left side of the maximized window (x member) and the position of the top of the maximized window (y member). For top-level windows, this value is based on the position of the primary monitor.
            /// </summary>
            public POINT ptMaxPosition;

            /// <summary>
            /// The minimum tracking size is the smallest window size that can be produced by using the borders to size the window
            /// </summary>
            public POINT ptMinTrackSize;

            /// <summary>
            /// The maximum tracking size is the largest window size that can be produced by using the borders to size the window
            /// </summary>
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR {
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NCCALCSIZE_PARAMS {
            /// <summary>
            /// Contains the new coordinates of a window that has been moved or resized, that is, it is the proposed new window coordinates.
            /// </summary>
            public RECT rectProposed;

            /// <summary>
            /// Contains the coordinates of the window before it was moved or resized.
            /// </summary>
            public RECT rectBeforeMove;

            /// <summary>
            /// Contains the coordinates of the window's client area before the window was moved or resized.
            /// </summary>
            public RECT rectClientBeforeMove;

            /// <summary>
            /// Pointer to a WINDOWPOS structure that contains the size and position values specified in the operation that moved or resized the window.
            /// </summary>
            public WINDOWPOS lpPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS {
            public IntPtr hwnd;
            public IntPtr hWndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;

            /// <summary>
            /// see SetWindowPosFlags
            /// </summary>
            public SetWindowPosFlags flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT {
            public IntPtr hdc;

            public bool fErase;

            // rcPaint was a by-value RECT structure
            public int rcPaint_left;
            public int rcPaint_top;
            public int rcPaint_right;
            public int rcPaint_bottom;
            public bool fRestore;
            public bool fIncUpdate;
            public int reserved1;
            public int reserved2;
            public int reserved3;
            public int reserved4;
            public int reserved5;
            public int reserved6;
            public int reserved7;
            public int reserved8;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TRACKMOUSEEVENT {
            public uint cbSize;
            public uint dwFlags;
            public IntPtr hwndTrack;
            public uint dwHoverTime;
        }

        #endregion

        #region Enums

        public enum WindowLongParam {
            /// <summary>Sets a new address for the window procedure.</summary>
            /// <remarks>You cannot change this attribute if the window does not belong to the same process as the calling thread.</remarks>
            GWL_WNDPROC = -4,

            /// <summary>Sets a new application instance handle.</summary>
            GWLP_HINSTANCE = -6,

            GWLP_HWNDPARENT = -8,

            /// <summary>Sets a new identifier of the child window.</summary>
            /// <remarks>The window cannot be a top-level window.</remarks>
            GWL_ID = -12,

            /// <summary>Sets a new window style.</summary>
            GWL_STYLE = -16,

            /// <summary>Sets a new extended window style.</summary>
            /// <remarks>See <see cref="ExWindowStyles"/>.</remarks>
            GWL_EXSTYLE = -20,

            /// <summary>Sets the user data associated with the window.</summary>
            /// <remarks>This data is intended for use by the application that created the window. Its value is initially zero.</remarks>
            GWL_USERDATA = -21,

            /// <summary>Sets the return value of a message processed in the dialog box procedure.</summary>
            /// <remarks>Only applies to dialog boxes.</remarks>
            DWLP_MSGRESULT = 0,

            /// <summary>Sets new extra information that is private to the application, such as handles or pointers.</summary>
            /// <remarks>Only applies to dialog boxes.</remarks>
            DWLP_USER = 8,

            /// <summary>Sets the new address of the dialog box procedure.</summary>
            /// <remarks>Only applies to dialog boxes.</remarks>
            DWLP_DLGPROC = 4
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa969530(v=vs.85).aspx?f=255&MSPPError=-2147217396
        /// </summary>
        [Flags]
        public enum DWMWINDOWATTRIBUTE : uint {
            NCRenderingEnabled = 1,
            NCRenderingPolicy = 2,
            TransitionsForceDisabled = 3,
            AllowNCPaint = 4,
            CaptionButtonBounds = 5,
            NonClientRtlLayout = 6,
            ForceIconicRepresentation = 7,
            Flip3DPolicy = 8,
            ExtendedFrameBounds = 9,
            HasIconicBitmap = 10,
            DisallowPeek = 11,
            ExcludedFromPeek = 12,
            Cloak = 13,
            Cloaked = 14,
            FreezeRepresentation = 15
        }

        [Flags]
        public enum DWMNCRenderingPolicy : uint {
            UseWindowStyle = 0,
            Disabled = 1,
            Enabled = 2,
            Last = 3
        }

        /// <summary>
        ///     Special window handles
        /// </summary>
        public enum SpecialWindowHandles {
            // ReSharper disable InconsistentNaming
            /// <summary>
            ///     Places the window at the top of the Z order.
            /// </summary>
            HWND_TOP = 0,

            /// <summary>
            ///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
            /// </summary>
            HWND_BOTTOM = 1,

            /// <summary>
            ///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
            /// </summary>
            HWND_TOPMOST = -1,

            /// <summary>
            ///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
            /// </summary>
            HWND_NOTOPMOST = -2
        }

        [Flags]
        public enum SetWindowPosFlags : uint {
            /// <summary>
            ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
            /// </summary>
            SWP_ASYNCWINDOWPOS = 0x4000,

            /// <summary>
            ///     Prevents generation of the WM_SYNCPAINT message.
            /// </summary>
            SWP_DEFERERASE = 0x2000,

            /// <summary>
            /// Draws a frame (defined in the window's class description) around the window.
            ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
            /// </summary>
            SWP_FRAMECHANGED = 0x0020,

            /// <summary>
            ///     Hides the window.
            /// </summary>
            SWP_HIDEWINDOW = 0x0080,

            /// <summary>
            ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOACTIVATE = 0x0010,

            /// <summary>
            ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
            /// </summary>
            SWP_NOCOPYBITS = 0x0100,

            /// <summary>
            ///     Retains the current position (ignores X and Y parameters).
            /// </summary>
            SWP_NOMOVE = 0x0002,

            /// <summary>
            ///     Does not change the owner window's position in the Z order.
            /// </summary>
            SWP_NOOWNERZORDER = 0x0200,

            /// <summary>
            ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            /// </summary>
            SWP_NOREDRAW = 0x0008,

            /// <summary>
            ///     Same as the SWP_NOOWNERZORDER flag.
            /// </summary>
            SWP_NOREPOSITION = 0x0200,

            /// <summary>
            ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            /// </summary>
            SWP_NOSENDCHANGING = 0x0400,

            /// <summary>
            ///     Retains the current size (ignores the cx and cy parameters).
            /// </summary>
            SWP_NOSIZE = 0x0001,

            /// <summary>
            ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOZORDER = 0x0004,

            /// <summary>
            ///     Displays the window.
            /// </summary>
            SWP_SHOWWINDOW = 0x0040,
        }

        /// <summary>
        /// The initial state of the frame control. 
        /// </summary>
        [Flags()]
        public enum DrawFrameControlStates {
            // =====================================================================================
            // If uType is DFC_BUTTON, uState can be one of the following values.
            // =====================================================================================
            /// <summary>
            /// Check box
            /// </summary>
            DFCS_BUTTONCHECK = 0,

            /// <summary>
            /// Image for radio button (nonsquare needs image)
            /// </summary>
            DFCS_BUTTONRADIOIMAGE = 1,

            /// <summary>
            /// Mask for radio button (nonsquare needs mask)
            /// </summary>
            DFCS_BUTTONRADIOMASK = 2,

            /// <summary>
            /// Radio button
            /// </summary>
            DFCS_BUTTONRADIO = 4,

            /// <summary>
            /// Three-state button
            /// </summary>
            DFCS_BUTTON3STATE = 8,

            /// <summary>
            /// Push button
            /// </summary>
            DFCS_BUTTONPUSH = 0x10,

            // =====================================================================================
            // If uType is DFC_CAPTION, uState can be one of the following values.
            // =====================================================================================
            /// <summary>
            /// Close button
            /// </summary>
            DFCS_CAPTIONCLOSE = 0,

            /// <summary>
            /// Minimize button
            /// </summary>
            DFCS_CAPTIONMIN = 1,

            /// <summary>
            /// Maximize button
            /// </summary>
            DFCS_CAPTIONMAX = 2,

            /// <summary>
            /// Restore button
            /// </summary>
            DFCS_CAPTIONRESTORE = 3,

            /// <summary>
            /// Help button
            /// </summary>
            DFCS_CAPTIONHELP = 4,

            // =====================================================================================
            // If uType is DFC_MENU, uState can be one of the following values.
            // =====================================================================================
            /// <summary>
            /// Submenu arrow
            /// </summary>
            DFCS_MENUARROW = 0,

            /// <summary>
            /// Check mark
            /// </summary>
            DFCS_MENUCHECK = 1,

            /// <summary>
            /// Bullet
            /// </summary>
            DFCS_MENUBULLET = 2,

            /// <summary>
            /// Submenu arrow pointing left. This is used for the right-to-left cascading menus used with right-to-left languages such as Arabic or Hebrew.
            /// </summary>
            DFCS_MENUARROWRIGHT = 4,

            // =====================================================================================
            // If uType is DFC_SCROLL, uState can be one of the following values.
            // =====================================================================================
            /// <summary>
            /// Up arrow of scroll bar
            /// </summary>
            DFCS_SCROLLUP = 0,

            /// <summary>
            /// Down arrow of scroll bar
            /// </summary>
            DFCS_SCROLLDOWN = 1,

            /// <summary>
            /// Left arrow of scroll bar
            /// </summary>
            DFCS_SCROLLLEFT = 2,

            /// <summary>
            /// Right arrow of scroll bar
            /// </summary>
            DFCS_SCROLLRIGHT = 3,

            /// <summary>
            /// Combo box scroll bar
            /// </summary>
            DFCS_SCROLLCOMBOBOX = 5,

            /// <summary>
            /// Size grip in lower-right corner of window
            /// </summary>
            DFCS_SCROLLSIZEGRIP = 8,

            /// <summary>
            /// Size grip in lower-left corner of window. This is used with right-to-left languages such as Arabic or Hebrew.
            /// </summary>
            DFCS_SCROLLSIZEGRIPRIGHT = 0x10,

            // =====================================================================================
            // The following style can be used to adjust the bounding rectangle of the push button.
            // =====================================================================================
            /// <summary>
            /// Bounding rectangle is adjusted to exclude the surrounding edge of the push button.
            /// </summary>
            DFCS_ADJUSTRECT = 0x2000,

            // =====================================================================================
            // One or more of the following values can be used to set the state of the control to be drawn.
            // =====================================================================================
            /// <summary>
            /// Button is inactive (grayed).
            /// </summary>
            DFCS_INACTIVE = 0x100,

            /// <summary>
            /// Button is pushed.
            /// </summary>
            DFCS_PUSHED = 0x200,

            /// <summary>
            /// Button is checked.
            /// </summary>
            DFCS_CHECKED = 0x400,

            /// <summary>
            /// The background remains untouched. This flag can only be combined with DFCS_MENUARROWUP or DFCS_MENUARROWDOWN.
            /// </summary>
            DFCS_TRANSPARENT = 0x800,

            /// <summary>
            /// Button is hot-tracked.
            /// </summary>
            DFCS_HOT = 0x1000,

            /// <summary>
            /// Button has a flat border.
            /// </summary>
            DFCS_FLAT = 0x4000,

            /// <summary>
            /// Button has a monochrome border.
            /// </summary>
            DFCS_MONO = 0x8000
        }

        /// <summary>
        /// The type of frame control to draw. This parameter can be one of the following values.
        /// </summary>
        [Flags]
        public enum DrawFrameControlTypes {
            /// <summary>
            /// Standard button
            /// </summary>
            DFC_BUTTON = 4,

            /// <summary>
            /// Title bar
            /// </summary>
            DFC_CAPTION = 1,

            /// <summary>
            /// Menu bar
            /// </summary>
            DFC_MENU = 2,

            /// <summary>
            /// Popup menu item.
            /// </summary>
            DFC_POPUPMENU = 5,

            /// <summary>
            /// Scroll bar
            /// </summary>
            DFC_SCROLL = 3
        }

        public enum HitTest {
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTGROWBOX = 4,
            HTSIZE = HTGROWBOX,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTREDUCE = HTMINBUTTON,
            HTZOOM = HTMAXBUTTON,
            HTSIZEFIRST = HTLEFT,
            HTSIZELAST = HTBOTTOMRIGHT,

            /// <summary>
            /// 
            /// </summary>
            HTTRANSPARENT = -1
        }

        public enum SysCommands {
            SC_SIZE = 0xF000,
            SC_MOVE = 0xF010,
            SC_MINIMIZE = 0xF020,
            SC_MAXIMIZE = 0xF030,
            /// <summary>
            /// sent instead of SC_MAXIMIZE when double clicking the caption bar
            /// </summary>
            SC_MAXIMIZEDBLCLICK = 0xF032,
            SC_NEXTWINDOW = 0xF040,
            SC_PREVWINDOW = 0xF050,
            SC_CLOSE = 0xF060,
            SC_VSCROLL = 0xF070,
            SC_HSCROLL = 0xF080,
            SC_MOUSEMENU = 0xF090,
            SC_KEYMENU = 0xF100,
            SC_ARRANGE = 0xF110,
            SC_RESTORE = 0xF120,
            /// <summary>
            /// sent instead of SC_RESTORE when double clicking the caption bar
            /// </summary>
            SC_RESTOREDBLCLICK = 0xF122,
            SC_TASKLIST = 0xF130,
            SC_SCREENSAVE = 0xF140,
            SC_HOTKEY = 0xF150,

            //#if(WINVER >= 0x0400) //Win95
            SC_DEFAULT = 0xF160,
            SC_MONITORPOWER = 0xF170,
            SC_CONTEXTHELP = 0xF180,
            SC_SEPARATOR = 0xF00F,
            //#endif /* WINVER >= 0x0400 */

            //#if(WINVER >= 0x0600) //Vista
            SCF_ISSECURE = 0x00000001,
            //#endif /* WINVER >= 0x0600 */

            /*
              * Obsolete names
              */
            SC_ICON = SC_MINIMIZE,
            SC_ZOOM = SC_MAXIMIZE
        }

        [Flags]
        public enum WindowStylesEx : uint {
            /// <summary>Specifies a window that accepts drag-drop files.</summary>
            WS_EX_ACCEPTFILES = 0x00000010,

            /// <summary>Forces a top-level window onto the taskbar when the window is visible.</summary>
            WS_EX_APPWINDOW = 0x00040000,

            /// <summary>Specifies a window that has a border with a sunken edge.</summary>
            WS_EX_CLIENTEDGE = 0x00000200,

            /// <summary>
            /// Specifies a window that paints all descendants in bottom-to-top painting order using double-buffering.
            /// This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. This style is not supported in Windows 2000.
            /// </summary>
            /// <remarks>
            /// With WS_EX_COMPOSITED set, all descendants of a window get bottom-to-top painting order using double-buffering.
            /// Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects,
            /// but only if the descendent window also has the WS_EX_TRANSPARENT bit set.
            /// Double-buffering allows the window and its descendents to be painted without flicker.
            /// </remarks>
            WS_EX_COMPOSITED = 0x02000000,

            /// <summary>
            /// Specifies a window that includes a question mark in the title bar. When the user clicks the question mark,
            /// the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message.
            /// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command.
            /// The Help application displays a pop-up window that typically contains help for the child window.
            /// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
            /// </summary>
            WS_EX_CONTEXTHELP = 0x00000400,

            /// <summary>
            /// Specifies a window which contains child windows that should take part in dialog box navigation.
            /// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations
            /// such as handling the TAB key, an arrow key, or a keyboard mnemonic.
            /// </summary>
            WS_EX_CONTROLPARENT = 0x00010000,

            /// <summary>Specifies a window that has a double border.</summary>
            WS_EX_DLGMODALFRAME = 0x00000001,

            /// <summary>
            /// Specifies a window that is a layered window.
            /// This cannot be used for child windows or if the window has a class style of either CS_OWNDC or CS_CLASSDC.
            /// </summary>
            WS_EX_LAYERED = 0x00080000,

            /// <summary>
            /// Specifies a window with the horizontal origin on the right edge. Increasing horizontal values advance to the left.
            /// The shell language must support reading-order alignment for this to take effect.
            /// </summary>
            WS_EX_LAYOUTRTL = 0x00400000,

            /// <summary>Specifies a window that has generic left-aligned properties. This is the default.</summary>
            WS_EX_LEFT = 0x00000000,

            /// <summary>
            /// Specifies a window with the vertical scroll bar (if present) to the left of the client area.
            /// The shell language must support reading-order alignment for this to take effect.
            /// </summary>
            WS_EX_LEFTSCROLLBAR = 0x00004000,

            /// <summary>
            /// Specifies a window that displays text using left-to-right reading-order properties. This is the default.
            /// </summary>
            WS_EX_LTRREADING = 0x00000000,

            /// <summary>
            /// Specifies a multiple-document interface (MDI) child window.
            /// </summary>
            WS_EX_MDICHILD = 0x00000040,

            /// <summary>
            /// Specifies a top-level window created with this style does not become the foreground window when the user clicks it.
            /// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
            /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
            /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
            /// </summary>
            WS_EX_NOACTIVATE = 0x08000000,

            /// <summary>
            /// Specifies a window which does not pass its window layout to its child windows.
            /// </summary>
            WS_EX_NOINHERITLAYOUT = 0x00100000,

            /// <summary>
            /// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
            /// </summary>
            WS_EX_NOPARENTNOTIFY = 0x00000004,

            /// <summary>Specifies an overlapped window.</summary>
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

            /// <summary>Specifies a palette window, which is a modeless dialog box that presents an array of commands.</summary>
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

            /// <summary>
            /// Specifies a window that has generic "right-aligned" properties. This depends on the window class.
            /// The shell language must support reading-order alignment for this to take effect.
            /// Using the WS_EX_RIGHT style has the same effect as using the SS_RIGHT (static), ES_RIGHT (edit), and BS_RIGHT/BS_RIGHTBUTTON (button) control styles.
            /// </summary>
            WS_EX_RIGHT = 0x00001000,

            /// <summary>Specifies a window with the vertical scroll bar (if present) to the right of the client area. This is the default.</summary>
            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            /// <summary>
            /// Specifies a window that displays text using right-to-left reading-order properties.
            /// The shell language must support reading-order alignment for this to take effect.
            /// </summary>
            WS_EX_RTLREADING = 0x00002000,

            /// <summary>Specifies a window with a three-dimensional border style intended to be used for items that do not accept user input.</summary>
            WS_EX_STATICEDGE = 0x00020000,

            /// <summary>
            /// Specifies a window that is intended to be used as a floating toolbar.
            /// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
            /// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
            /// If a tool window has a system menu, its icon is not displayed on the title bar.
            /// However, you can display the system menu by right-clicking or by typing ALT+SPACE. 
            /// </summary>
            WS_EX_TOOLWINDOW = 0x00000080,

            /// <summary>
            /// Specifies a window that should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
            /// To add or remove this style, use the SetWindowPos function.
            /// </summary>
            WS_EX_TOPMOST = 0x00000008,

            /// <summary>
            /// Specifies a window that should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
            /// The window appears transparent because the bits of underlying sibling windows have already been painted.
            /// To achieve transparency without these restrictions, use the SetWindowRgn function.
            /// </summary>
            WS_EX_TRANSPARENT = 0x00000020,

            /// <summary>Specifies a window that has a border with a raised edge.</summary>
            WS_EX_WINDOWEDGE = 0x00000100
        }

        /// <summary>
        /// Window Styles.
        /// The following styles can be specified wherever a window style is required. After the control has been created, these styles cannot be modified, except as noted.
        /// </summary>
        [Flags]
        public enum WindowStyles : int {
            /// <summary>The window has a thin-line border.</summary>
            WS_BORDER = 0x800000,

            /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
            WS_CAPTION = 0xc00000,

            /// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
            WS_CHILD = 0x40000000,

            /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
            WS_CLIPCHILDREN = 0x2000000,

            /// <summary>
            /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
            /// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
            /// </summary>
            WS_CLIPSIBLINGS = 0x4000000,

            /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
            WS_DISABLED = 0x8000000,

            /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
            WS_DLGFRAME = 0x400000,

            /// <summary>
            /// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
            /// The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
            /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
            /// </summary>
            WS_GROUP = 0x20000,

            /// <summary>The window has a horizontal scroll bar.</summary>
            WS_HSCROLL = 0x100000,

            /// <summary>The window is initially maximized.</summary> 
            WS_MAXIMIZE = 0x1000000,

            /// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary> 
            WS_MAXIMIZEBOX = 0x10000,

            /// <summary>The window is initially minimized.</summary>
            WS_MINIMIZE = 0x20000000,

            /// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
            WS_MINIMIZEBOX = 0x20000,

            /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
            WS_OVERLAPPED = 0x0,

            /// <summary>The window is an overlapped window.</summary>
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

            /// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
            WS_POPUP = unchecked((int) 0x80000000),

            /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

            /// <summary>The window has a sizing border.</summary>
            WS_SIZEFRAME = 0x40000,

            /// <summary>The window has a sizing border. Same as the WS_SIZEBOX style.</summary>
            WS_THICKFRAME = 0x00040000,

            /// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
            WS_SYSMENU = 0x80000,

            /// <summary>
            /// The window is a control that can receive the keyboard focus when the user presses the TAB key.
            /// Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.  
            /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
            /// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
            /// </summary>
            WS_TABSTOP = 0x10000,

            /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
            WS_VISIBLE = 0x10000000,

            /// <summary>The window has a vertical scroll bar.</summary>
            WS_VSCROLL = 0x200000
        }

        [Flags]
        public enum WindowClassStyles : uint {
            /// <summary>Aligns the window's client area on a byte boundary (in the x direction). This style affects the width of the window and its horizontal placement on the display.</summary>
            ByteAlignClient = 0x1000,

            /// <summary>Aligns the window on a byte boundary (in the x direction). This style affects the width of the window and its horizontal placement on the display.</summary>
            ByteAlignWindow = 0x2000,

            /// <summary>
            /// Allocates one device context to be shared by all windows in the class.
            /// Because window classes are process specific, it is possible for multiple threads of an application to create a window of the same class.
            /// It is also possible for the threads to attempt to use the device context simultaneously. When this happens, the system allows only one thread to successfully finish its drawing operation.
            /// </summary>
            ClassDC = 0x40,

            /// <summary>Sends a double-click message to the window procedure when the user double-clicks the mouse while the cursor is within a window belonging to the class.</summary>
            DoubleClicks = 0x8,

            /// <summary>
            /// Enables the drop shadow effect on a window. The effect is turned on and off through SPI_SETDROPSHADOW.
            /// Typically, this is enabled for small, short-lived windows such as menus to emphasize their Z order relationship to other windows.
            /// </summary>
            DropShadow = 0x20000,

            /// <summary>Indicates that the window class is an application global class. For more information, see the "Application Global Classes" section of About Window Classes.</summary>
            GlobalClass = 0x4000,

            /// <summary>Redraws the entire window if a movement or size adjustment changes the width of the client area.</summary>
            HorizontalRedraw = 0x2,

            /// <summary>Disables Close on the window menu.</summary>
            NoClose = 0x200,

            /// <summary>Allocates a unique device context for each window in the class.</summary>
            OwnDC = 0x20,

            /// <summary>
            /// Sets the clipping rectangle of the child window to that of the parent window so that the child can draw on the parent.
            /// A window with the CS_PARENTDC style bit receives a regular device context from the system's cache of device contexts.
            /// It does not give the child the parent's device context or device context settings. Specifying CS_PARENTDC enhances an application's performance.
            /// </summary>
            ParentDC = 0x80,

            /// <summary>
            /// Saves, as a bitmap, the portion of the screen image obscured by a window of this class.
            /// When the window is removed, the system uses the saved bitmap to restore the screen image, including other windows that were obscured.
            /// Therefore, the system does not send WM_PAINT messages to windows that were obscured if the memory used by the bitmap has not been discarded and if other screen actions have not invalidated the stored image.
            /// This style is useful for small windows (for example, menus or dialog boxes) that are displayed briefly and then removed before other screen activity takes place.
            /// This style increases the time required to display the window, because the system must first allocate memory to store the bitmap.
            /// </summary>
            SaveBits = 0x800,

            /// <summary>Redraws the entire window if a movement or size adjustment changes the height of the client area.</summary>
            VerticalRedraw = 0x1
        }

        [Flags]
        public enum RedrawWindowFlags : uint {
            ///<summary>Invalidates lprcUpdate or hrgnUpdate (only one may be non-NULL). 
            ///If both are NULL, the entire window is invalidated.</summary>
            Invalidate = 0x1,

            ///<summary>Causes a WM_PAINT message to be posted to the window regardless of 
            ///whether any portion of the window is invalid.</summary>
            InternalPaint = 0x2,

            ///<summary>Causes the window to receive a WM_ERASEBKGND message when the window 
            ///is repainted. The <b>Invalidate</b> flag must also be specified; otherwise, 
            ///<b>Erase</b> has no effect.</summary>
            Erase = 0x4,

            ///<summary>Validates lprcUpdate or hrgnUpdate (only one may be non-NULL). If both 
            ///are NULL, the entire window is validated. This flag does not affect internal 
            ///WM_PAINT messages.</summary>
            Validate = 0x8,

            ///<summary>Suppresses any pending internal WM_PAINT messages. This flag does not 
            ///affect WM_PAINT messages resulting from a non-NULL update area.</summary>
            NoInternalPaint = 0x10,

            ///<summary>Suppresses any pending WM_ERASEBKGND messages.</summary>
            NoErase = 0x20,

            ///<summary>Excludes child windows, if any, from the repainting operation.</summary>
            NoChildren = 0x40,

            ///<summary>Includes child windows, if any, in the repainting operation.</summary>
            AllChildren = 0x80,

            ///<summary>Causes the affected windows (as specified by the <b>AllChildren</b> and <b>NoChildren</b> flags) to 
            ///receive WM_NCPAINT, WM_ERASEBKGND, and WM_PAINT messages, if necessary, before the function returns.</summary>
            UpdateNow = 0x100,

            ///<summary>Causes the affected windows (as specified by the <b>AllChildren</b> and <b>NoChildren</b> flags) 
            ///to receive WM_NCPAINT and WM_ERASEBKGND messages, if necessary, before the function returns. 
            ///WM_PAINT messages are received at the ordinary time.</summary>
            EraseNow = 0x200,

            ///<summary>Causes any part of the nonclient area of the window that intersects the update region 
            ///to receive a WM_NCPAINT message. The <b>Invalidate</b> flag must also be specified; otherwise, 
            ///<b>Frame</b> has no effect. The WM_NCPAINT message is typically not sent during the execution of 
            ///RedrawWindow unless either <b>UpdateNow</b> or <b>EraseNow</b> is specified.</summary>
            Frame = 0x400,

            ///<summary>Suppresses any pending WM_NCPAINT messages. This flag must be used with <b>Validate</b> and 
            ///is typically used with <b>NoChildren</b>. <b>NoFrame</b> should be used with care, as it could cause parts 
            ///of a window to be painted improperly.</summary>
            NoFrame = 0x800
        }

        [Flags]
        public enum TMEFlags : uint {
            /// <summary>
            /// The caller wants to cancel a prior tracking request. The caller should also specify the type of tracking that it wants to cancel. For example, to cancel hover tracking, the caller must pass the TME_CANCEL and TME_HOVER flags.
            /// </summary>
            TME_CANCEL = 0x80000000,

            /// <summary>
            /// The caller wants hover notification. Notification is delivered as a WM_MOUSEHOVER message.
            /// If the caller requests hover tracking while hover tracking is already active, the hover timer will be reset.
            /// This flag is ignored if the mouse pointer is not over the specified window or area.
            /// </summary>
            TME_HOVER = 0x00000001,

            /// <summary>
            /// The caller wants leave notification. Notification is delivered as a WM_MOUSELEAVE message. If the mouse is not over the specified window or area, a leave notification is generated immediately and no further tracking is performed.
            /// </summary>
            TME_LEAVE = 0x00000002,

            /// <summary>
            /// The caller wants hover and leave notification for the nonclient areas. Notification is delivered as WM_NCMOUSEHOVER and WM_NCMOUSELEAVE messages.
            /// </summary>
            TME_NONCLIENT = 0x00000010,

            /// <summary>
            /// The function fills in the structure instead of treating it as a tracking request. The structure is filled such that had that structure been passed to TrackMouseEvent, it would generate the current tracking. The only anomaly is that the hover time-out returned is always the actual time-out and not HOVER_DEFAULT, if HOVER_DEFAULT was specified during the original TrackMouseEvent request. 
            /// </summary>
            TME_QUERY = 0x40000000,
        }

        public enum ShowWindowCommands {
            /// <summary>
            ///        Hides the window and activates another window.
            /// </summary>
            SW_HIDE = 0,

            /// <summary>
            ///        Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
            /// </summary>
            SW_SHOWNORMAL = 1,

            /// <summary>
            ///        Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
            /// </summary>
            SW_NORMAL = 1,

            /// <summary>
            ///        Activates the window and displays it as a minimized window.
            /// </summary>
            SW_SHOWMINIMIZED = 2,

            /// <summary>
            ///        Activates the window and displays it as a maximized window.
            /// </summary>
            SW_SHOWMAXIMIZED = 3,

            /// <summary>
            ///        Maximizes the specified window.
            /// </summary>
            SW_MAXIMIZE = 3,

            /// <summary>
            ///        Displays a window in its most recent size and position. This value is similar to <see cref="ShowWindowCommands.SW_SHOWNORMAL"/>, except the window is not activated.
            /// </summary>
            SW_SHOWNOACTIVATE = 4,

            /// <summary>
            ///        Activates the window and displays it in its current size and position.
            /// </summary>
            SW_SHOW = 5,

            /// <summary>
            ///        Minimizes the specified window and activates the next top-level window in the z-order.
            /// </summary>
            SW_MINIMIZE = 6,

            /// <summary>
            ///        Displays the window as a minimized window. This value is similar to <see cref="ShowWindowCommands.SW_SHOWMINIMIZED"/>, except the window is not activated.
            /// </summary>
            SW_SHOWMINNOACTIVE = 7,

            /// <summary>
            ///        Displays the window in its current size and position. This value is similar to <see cref="ShowWindowCommands.SW_SHOW"/>, except the window is not activated.
            /// </summary>
            SW_SHOWNA = 8,

            /// <summary>
            ///        Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
            /// </summary>
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11
        }

        [Flags]
        public enum ScrollWindowExFlags : uint {

            /// <summary>
            /// Scrolls all child windows that intersect the rectangle pointed to by the prcScroll parameter. The child windows are scrolled by the number of pixels specified by the dx and dy parameters. The system sends a WM_MOVE message to all child windows that intersect the prcScroll rectangle, even if they do not move
            /// </summary>
            SW_SCROLLCHILDREN = 0x0001,

            /// <summary>
            /// Invalidates the region identified by the hrgnUpdate parameter after scrolling
            /// </summary>
            SW_INVALIDATE = 0x0002,

            /// <summary>
            /// Erases the newly invalidated region by sending a WM_ERASEBKGND message to the window when specified with the SW_INVALIDATE flag
            /// </summary>
            SW_ERASE = 0x0004,

            /// <summary>
            /// Scrolls using smooth scrolling. Use the HIWORD portion of the flags parameter to indicate how much time, in milliseconds, the smooth-scrolling operation should take
            /// </summary>
            SW_SMOOTHSCROLL = 0x0010
        }

        public enum WmSizeEnum {
            SIZE_RESTORED = 0,
            SIZE_MINIMIZED = 1,
            SIZE_MAXIMIZED = 2,
            SIZE_MAXSHOW = 3,
            SIZE_MAXHIDE = 4,
        }

        #endregion

        #region Fields

        public static readonly IntPtr TRUE = new IntPtr(1);
        public static readonly IntPtr FALSE = IntPtr.Zero;
        
        // Changes the client size of a control
        public const int EM_SETRECT = 0xB3;

        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        public const int SW_SCROLLCHILDREN = 0x0001;

        public const int SW_INVALIDATE = 0x0002;

        public const int SW_ERASE = 0x0004;

        public const int SW_SMOOTHSCROLL = 0x0010;

        #endregion

        #region API Calls

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(HandleRef hWnd, ShowWindowCommands nCmdShow);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool DrawFrameControl(HandleRef hDC, ref RECT rect, DrawFrameControlTypes type, DrawFrameControlStates state);

        [DllImport("user32.dll")]
        public static extern bool TrackMouseEvent([In, Out] TRACKMOUSEEVENT lpEventTrack);

        [DllImport("user32.dll")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int ScrollWindowEx(HandleRef hWnd, int nXAmount, int nYAmount, COMRECT rectScrollRegion, ref RECT rectClip, HandleRef hrgnUpdate, ref RECT prcUpdate, ScrollWindowExFlags flags);

        [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "GetDC", CharSet = CharSet.Auto)]
        private static extern IntPtr IntGetDC(HandleRef hWnd);

        /// <summary>
        /// Device context for the client area
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static IntPtr GetDC(HandleRef hWnd) {
            return IntGetDC(hWnd);
        }

        [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "GetWindowDC", CharSet = CharSet.Auto)]
        private static extern IntPtr IntGetWindowDC(HandleRef hWnd);

        /// <summary>
        /// Device context for the non client area
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static IntPtr GetWindowDC(HandleRef hWnd) {
            return IntGetWindowDC(hWnd);
        }

        [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "ReleaseDC", CharSet = CharSet.Auto)]
        private static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);

        public static int ReleaseDC(HandleRef hWnd, HandleRef hDC) {
            return IntReleaseDC(hWnd, hDC);
        }

        [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "EndPaint", CharSet = CharSet.Auto)]
        private static extern bool IntEndPaint(HandleRef hWnd, ref PAINTSTRUCT lpPaint);

        public static bool EndPaint(HandleRef hWnd, [In, MarshalAs(UnmanagedType.LPStruct)]
            ref PAINTSTRUCT lpPaint) {
            return IntEndPaint(hWnd, ref lpPaint);
        }

        [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "BeginPaint", CharSet = CharSet.Auto)]
        private static extern IntPtr IntBeginPaint(HandleRef hWnd, [In, Out] ref PAINTSTRUCT lpPaint);

        public static IntPtr BeginPaint(HandleRef hWnd, [In, Out, MarshalAs(UnmanagedType.LPStruct)]
            ref PAINTSTRUCT lpPaint) {
            return IntBeginPaint(hWnd, ref lpPaint);
        }

        /// <summary>
        /// Returns a rectangle representing the topleft / bottomright corners of the window
        /// </summary>
        [DllImport("user32.dll")]
        public static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        /// <summary>
        /// Returns a rectangle reprsenting the location + width of the given window
        /// </summary>
        public static Rectangle GetWindowRect(IntPtr hWnd) {
            Rectangle output = new Rectangle();
            GetWindowRect(hWnd, ref output);
            output.Width -= output.X;
            output.Height -= output.Y;
            return output;
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition() {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return new Point(lpPoint.x, lpPoint.y);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        // Changes the client size of a control
        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessageRefRect(IntPtr hWnd, uint msg, int wParam, ref RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        /// <summary>
        /// Gets the handle of the window that currently has focus.
        /// </summary>
        /// <returns>
        /// The handle of the window that currently has focus.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Activates the specified window.
        /// </summary>
        /// <param name="hWnd">
        /// The handle of the window to be focused.
        /// </param>
        /// <returns>
        /// True if the window was focused; False otherwise.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);
        
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetCapture(HandleRef hwnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        #endregion

        #region unsafe

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        //GetWindowLong won't work correctly for 64-bit: we should use GetWindowLongPtr instead.  On
        //32-bit, GetWindowLongPtr is just #defined as GetWindowLong.  GetWindowLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr GetWindowLong(HandleRef hWnd, WindowLongParam nIndex) {
            if (IntPtr.Size == 4) {
                return GetWindowLong32(hWnd, (int) nIndex);
            }

            return GetWindowLongPtr64(hWnd, (int) nIndex);
        }

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLong")]
        public static extern IntPtr GetWindowLong32(HandleRef hWnd, int nIndex);

        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLongPtr64(HandleRef hWnd, int nIndex);

        //SetWindowLong won't work correctly for 64-bit: we should use SetWindowLongPtr instead.  On
        //32-bit, SetWindowLongPtr is just #defined as SetWindowLong.  SetWindowLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr SetWindowLong(HandleRef hWnd, WindowLongParam nIndex, HandleRef dwNewLong) {
            if (IntPtr.Size == 4) {
                return SetWindowLongPtr32(hWnd, (int) nIndex, dwNewLong);
            }

            return SetWindowLongPtr64(hWnd, (int) nIndex, dwNewLong);
        }

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, HandleRef dwNewLong);

        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, HandleRef dwNewLong);

        #endregion

        #region GetCharFromKey

        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff, int cchBuff, uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        public enum MapType : uint {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3
        }

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        /// <summary>
        /// Returns null or the char associated to the keyValue pressed
        /// </summary>
        public static char? GetCharFromKey(int keyValue) {
            return GetCharFromKey(KeyInterop.KeyFromVirtualKey(keyValue));
        }

        /// <summary>
        /// Returns null or the char associated to the key pressed
        /// </summary>
        public static char? GetCharFromKey(Key key) {
            char? ch = null;

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint) virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint) virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result) {
                case -1:
                    break;
                case 0:
                    break;
                case 1: {
                    ch = stringBuilder[0];
                    break;
                }
                default: {
                    ch = stringBuilder[0];
                    break;
                }
            }

            return ch;
        }

        public static string GetCharsFromKeys(Keys key, bool shift, bool altGr) {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift) {
                keyboardState[(int) Keys.ShiftKey] = 0xff;
            }

            if (altGr) {
                keyboardState[(int) Keys.ControlKey] = 0xff;
                keyboardState[(int) Keys.Menu] = 0xff;
            }

            ToUnicode((uint) key, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }

        #endregion

        #region DwmApi API calls

        [DllImport("dwmapi.dll")]
        public static extern int DwmDefWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, out IntPtr result);

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hdc, ref MARGINS marInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int pvAttribute, uint cbAttribute);

        #endregion
    }
}