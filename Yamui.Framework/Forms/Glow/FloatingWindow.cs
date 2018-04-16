using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Yamui.Framework.Forms.Glow
{
/// <summary>
	/// Represents a floating native window with shadow, transparency and the ability to 
	/// move and be resized with the mouse.
	/// </summary>
	public class FloatingWindow: NativeWindow, IDisposable
	{
		#region #  Fields  #

		private bool isMouseIn;
		private bool onMouseMove;
		private bool onMouseDown;
		private bool onMouseUp;
		private bool minimizing;
		private Size sizeBeforeMinimize;
		private bool minimized;
		private int deltaX;
		private int deltaY;
		private Rectangle expandRect;
		private Rectangle closeRect;
		private bool captured;
		private bool resizing;
		private bool disposed;
		private int clientWidth;
		private int clientHeight;
		private bool hasShadow = true;
		private int shadowLength = 4;
		private Point location = new Point(0,0);
		private Size size = new Size(200, 100);
		private int alpha = 200;
		private Control parent;
		private bool supportsLayered;
		private Point lastMouseDown = Point.Empty;
		private Rectangle clientRectangle;
		private Rectangle shadowRectangle;
		private Color borderColor = Color.Navy;
		private Color backColor = Color.Wheat;
		private Color foreColor = Color.Red;
		private string text = "Text";
		private int captionHeight = 20;
		private Rectangle resizeR;

		#endregion

		#region #  Constructors  #

		/// <summary>
		/// Creates a new instance of the <see cref="FloatingWindow"/> class
		/// </summary>
		public FloatingWindow() {
			supportsLayered = OSFeature.Feature.GetVersionPresent(OSFeature.LayeredWindows) != null;
		}

		~FloatingWindow()
		{
			Dispose(false);
		}

		#endregion

		#region #  Methods  #

		#region == Painting ==

		/// <summary>
		/// Paints the shadow for the window.
		/// </summary>
		/// <param name="e">A <see cref="PaintEventArgs"/> containing the event data.</param>
		protected virtual void PaintShadow(PaintEventArgs e)
		{
			if (hasShadow)
			{
				Color color1 = Color.FromArgb(0x30, 0, 0, 0);
				Color color2 = Color.FromArgb(0, 0, 0, 0);
				GraphicsPath path1 = new GraphicsPath();
				GraphicsContainer container1 = e.Graphics.BeginContainer();
				path1.StartFigure();
				path1.AddRectangle(ShadowRectangle);
				path1.CloseFigure();
				using (PathGradientBrush brush2 = new PathGradientBrush(path1))
				{
					brush2.CenterColor = color1;
					brush2.CenterPoint = new Point(ShadowRectangle.X+ShadowRectangle.Width/2,
						ShadowRectangle.Y+ShadowRectangle.Height/2);
					ColorBlend cb = new ColorBlend();
					cb.Colors = new[] {color1, color2};
					cb.Positions = new[] {0.0f,1.0f};
					brush2.InterpolationColors = cb;
					Region region = e.Graphics.Clip;
					//region.Exclude(this.ClientRectangle);
					e.Graphics.Clip = region;
					e.Graphics.FillRectangle(brush2, ShadowRectangle);
				}
				path1.Dispose();
				e.Graphics.EndContainer(container1);
			}
		}
		/// <summary>
		/// Performs the painting of the window.
		/// </summary>
		/// <param name="e">A <see cref="PaintEventArgs"/> containing the event data.</param>
		protected virtual void PerformPaint(PaintEventArgs e)
		{
			PaintShadow(e);
			
			using (SolidBrush brush1 = new SolidBrush(Color.FromArgb(255, backColor)))
			{
				if (!minimized)
				{
					e.Graphics.FillRectangle(brush1, ClientRectangle);
				}
				Rectangle rect = ClientRectangle;
				rect.Height = captionHeight;
				using (SolidBrush brush2 = new SolidBrush(ControlPaint.Dark(backColor)))
				{
					if (minimized)
					{
						e.Graphics.FillRectangle(brush1, rect);
					}
					e.Graphics.FillRectangle(brush2, rect);

					if (text.Length>0)
					{
						using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic))
						{
							sf.LineAlignment = StringAlignment.Center;
							StringFormatFlags sfg = StringFormatFlags.FitBlackBox |
								StringFormatFlags.NoWrap;
							sf.FormatFlags = sfg;
							using (SolidBrush brush3 = new SolidBrush(ControlPaint.Light(foreColor)))
							{
								e.Graphics.DrawString(text, Control.DefaultFont, brush3, new Rectangle(rect.X+3, rect.Y, rect.Width-6, rect.Height), sf);
							}
							using (Pen pen2 = new Pen(SystemColors.Window))
							{
								Rectangle rect3 = rect;
								rect3.X+=(rect.Width-4-11);
								rect3.Y+=4;
								rect3.Width = 11;
								rect3.Height = 11;
								closeRect = rect3;
								e.Graphics.DrawRectangle(pen2, rect3);
								e.Graphics.DrawLine(pen2, rect3.Left, rect3.Top, rect3.Right, rect3.Bottom);
								e.Graphics.DrawLine(pen2, rect3.Left, rect3.Bottom, rect3.Right, rect3.Top);
							}
							using (Pen pen5 = new Pen(SystemColors.Window))
							{
								Rectangle rect3 = rect;
								rect3.X+=(rect.Width-4-11);
								rect3.Y+=4;
								rect3.Width = 11;
								rect3.Height = 11;
								rect3.X -= (11+4);
								expandRect = rect3;
								//GraphicsPath path = new GraphicsPath();
								//path.StartFigure();
								int x = rect3.X+rect3.Width/2;
								int y = rect3.Y+rect3.Height/2;
								if (!minimized)
								{
									e.Graphics.DrawLine(pen5, x-2, y-1, x, y-3);
									e.Graphics.DrawLine(pen5, x-2, y, x, y-2);
									e.Graphics.DrawLine(pen5, x+1, y-3, x+3, y-1);
									e.Graphics.DrawLine(pen5, x+1, y-2, x+3, y);
									e.Graphics.DrawLine(pen5, x-2, y+3, x, y+1);
									e.Graphics.DrawLine(pen5, x-2, y+4, x, y+2);
									e.Graphics.DrawLine(pen5, x+1, y+1, x+3, y+3);
									e.Graphics.DrawLine(pen5, x+1, y+2, x+3, y+4);
								}
								else
								{
									e.Graphics.DrawLine(pen5, x-2, y-2, x, y);
									e.Graphics.DrawLine(pen5, x-2, y-3, x, y-1);
									e.Graphics.DrawLine(pen5, x+1, y, x+3, y-2);
									e.Graphics.DrawLine(pen5, x+1, y-1, x+3, y-3);
									e.Graphics.DrawLine(pen5, x-2, y+2, x, y+4);
									e.Graphics.DrawLine(pen5, x-2, y+1, x, y+3);
									e.Graphics.DrawLine(pen5, x+1, y+4, x+3, y+2);
									e.Graphics.DrawLine(pen5, x+1, y+3, x+3, y+1);								}
							}
						}
					}
				}
				if (minimized)
				{
					resizeR = Rectangle.Empty;
				}
				using (Pen pen1 = new Pen(borderColor))
				{
					Rectangle rect2 = ClientRectangle;
					e.Graphics.DrawRectangle(pen1, rect);
					if (!minimized)
					{
						e.Graphics.DrawRectangle(pen1, rect2);
						pen1.Color = ControlPaint.Light(borderColor);
						e.Graphics.DrawLine(pen1, rect2.Right-9, rect2.Bottom-1, rect2.Right-1, rect2.Bottom-9);
						e.Graphics.DrawLine(pen1, rect2.Right-6, rect2.Bottom-1, rect2.Right-1, rect2.Bottom-6);
						e.Graphics.DrawLine(pen1, rect2.Right-3, rect2.Bottom-1, rect2.Right-1, rect2.Bottom-3);
						resizeR = new Rectangle(rect2.Right-9, rect2.Bottom - 9, 9, 9);
					}
				}
			}
		}
		/// <summary>
		/// Raises the <see cref="Paint"/> event.
		/// </summary>
		/// <param name="e">A <see cref="PaintEventArgs"/> containing the event data.</param>
		public virtual void OnPaint(PaintEventArgs e)
		{
			PerformPaint(e);
			if (Paint!=null)
			{
				Paint(this, e);
			}
		}

		#endregion

		#region == Layout ==
		
		private void Minimize()
		{
			sizeBeforeMinimize = Size;
			minimized = true;
			minimizing = true;
			Size = new Size(Size.Width, captionHeight+shadowLength);
			Invalidate();
			minimizing = false;
		}
		private void Maximize()
		{
			minimized = false;
			Size = sizeBeforeMinimize;
			Invalidate();
		}
		private void PreCalculateLayout()
		{
		}
		private void ComputeLayout()
		{
			RECT rct = new RECT();
			POINT pnt = new POINT();
			User32.GetWindowRect(Handle, ref rct);
			pnt.x = rct.left;
			pnt.y = rct.top;
			User32.ScreenToClient(Handle, ref pnt);
			clientRectangle = new Rectangle(pnt.x, pnt.y, rct.right-rct.left, rct.bottom-rct.top);
			if (hasShadow)
			{
				clientRectangle.Width-=shadowLength;
				clientRectangle.Height-=shadowLength;
				shadowRectangle = clientRectangle;
				shadowRectangle.X+=shadowLength;
				shadowRectangle.Y+=shadowLength;
			}
			else
			{
				shadowRectangle = Rectangle.Empty;
			}
		}


		#endregion

		#region == Updating ==

		protected internal void Invalidate()
		{
			UpdateLayeredWindow();
		}
		private void UpdateLayeredWindow()
		{
			UpdateLayeredWindow(location, size, (byte)alpha);
		}
		private void UpdateLayeredWindowAnimate()
		{
			UpdateLayeredWindow(true);
		}
		
		private void UpdateLayeredWindow(bool animate)
		{
			if (animate)
			{
				for (int num1 = 0; num1 < 0xff; num1 += 3)
				{
					if (num1 == 0xfe)
					{
						num1 = 0xff;
					}
					UpdateLayeredWindow(location, size, (byte) num1);
				}
			}
			else
			{
				UpdateLayeredWindow(location, size, 0xff);
			}
		}
		private void UpdateLayeredWindow(byte alpha)
		{
			UpdateLayeredWindow(location, size, alpha);
		}
		private void UpdateLayeredWindow(Point point, Size size, byte alpha)
		{
			Bitmap bitmap1 = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
			using (Graphics graphics1 = Graphics.FromImage(bitmap1))
			{
				Rectangle rectangle1;
				SIZE size1;
				POINT point1;
				POINT point2;
				BLENDFUNCTION blendfunction1;
				rectangle1 = new Rectangle(0, 0, size.Width, size.Height);
				OnPaint(new PaintEventArgs(graphics1, rectangle1));
				IntPtr ptr1 = User32.GetDC(IntPtr.Zero);
				IntPtr ptr2 = Gdi32.CreateCompatibleDC(ptr1);
				IntPtr ptr3 = bitmap1.GetHbitmap(Color.FromArgb(0));
				IntPtr ptr4 = Gdi32.SelectObject(ptr2, ptr3);
				size1.cx = size.Width;
				size1.cy = size.Height;
				point1.x = point.X;
				point1.y = point.Y;
				point2.x = 0;
				point2.y = 0;
				blendfunction1 = new BLENDFUNCTION();
				blendfunction1.BlendOp = 0;
				blendfunction1.BlendFlags = 0;
				blendfunction1.SourceConstantAlpha = alpha;
				blendfunction1.AlphaFormat = 1;
				User32.UpdateLayeredWindow(Handle, ptr1, ref point1, ref size1, ptr2, ref point2, 0, ref blendfunction1, 2);
				Gdi32.SelectObject(ptr2, ptr4);
				User32.ReleaseDC(IntPtr.Zero, ptr1);
				Gdi32.DeleteObject(ptr3);
				Gdi32.DeleteDC(ptr2);
			}
		}

		
		#endregion

		#region == Show / Hide ==
		
		/// <summary>
		/// Shows the window.
		/// </summary>
		/// <remarks>
		/// Showing is done with animation.
		/// </remarks>
		public virtual void Show()
		{
			Show(X, Y);
		}
		/// <summary>
		/// Shows the window at the specified location.
		/// </summary>
		/// <param name="x">The horizontal coordinate.</param>
		/// <param name="y">The vertical coordinate.</param>
		/// <remarks>
		/// Showing is done with animation.
		/// </remarks>
		public virtual void Show(int x, int y)
		{
			Show(x, y, true);
		}
		/// <summary>
		/// Shows the window at the specified location.
		/// </summary>
		/// <param name="x">The horizontal coordinate.</param>
		/// <param name="y">The vertical coordinate.</param>
		/// <param name="animate"><b>true</b> if the showing should be done with animation; otherwise, <b>false</b>.</param>
		public virtual void Show(int x, int y, bool animate)
		{
			location = new Point(x, y);
			PreCalculateLayout();			
			CreateParams params1 = CreateParams;
			if (Handle == IntPtr.Zero)
			{
				CreateHandle(params1);
			}
			ComputeLayout();
			if (supportsLayered)
			{
				if (animate)
				{
					User32.ShowWindow(Handle, 4);
					Thread thread1 = new Thread(UpdateLayeredWindowAnimate);
					thread1.IsBackground = true;
					thread1.Start();
				}
				else
				{
					UpdateLayeredWindow();
				}
			}
			User32.ShowWindow(Handle, 4);
		}
		private void ShowInvisible(int x, int y)
		{
			CreateParams params1 = new CreateParams();
			params1.Caption = "FloatingNativeWindow";
			int nX = x;
			int nY = y;
			Screen screen1 = Screen.FromHandle(Handle);
			if ((nX + size.Width) > screen1.Bounds.Width)
			{
				nX = screen1.Bounds.Width - size.Width;
			}
			if ((nY + size.Height) > screen1.Bounds.Height)
			{
				nY = screen1.Bounds.Height - size.Height;
			}
			location = new Point(nX, nY);
			Size size1 = size;
			Point point1 = location;
			params1.X = nX;
			params1.Y = nY;
			params1.Height = size1.Height;
			params1.Width = size1.Width;
			params1.Parent = IntPtr.Zero;
			params1.Style = -2147483648;
			params1.ExStyle = 0x88;
			if (supportsLayered)
			{
				params1.ExStyle += 0x80000;
			}
			if (Handle == IntPtr.Zero)
			{
				CreateHandle(params1);
			}
			size = size1;
			location = point1;
			ComputeLayout();
		}
		/// <summary>
		/// Shows the window with a specific animation.
		/// </summary>
		/// <param name="x">The horizontal coordinate.</param>
		/// <param name="y">The vertical coordinate.</param>
		/// <param name="mode">An <see cref="AnimateMode"/> parameter.</param>
		public virtual void ShowAnimate(int x, int y, AnimateMode mode)
		{
			uint flag = (uint)AnimateWindow.AW_CENTER;
			switch (mode)
			{
				case AnimateMode.Blend:
					Show(x, y, true);
					return;
				case AnimateMode.ExpandCollapse:
					flag = (uint)AnimateWindow.AW_CENTER;
					break;
				case AnimateMode.SlideLeftToRight:
					flag = (uint)(AnimateWindow.AW_HOR_POSITIVE | AnimateWindow.AW_SLIDE);
					break;
				case AnimateMode.SlideRightToLeft:
					flag = (uint)(AnimateWindow.AW_HOR_NEGATIVE | AnimateWindow.AW_SLIDE);
					break;
				case AnimateMode.SlideBottmToTop:
					flag = (uint)(AnimateWindow.AW_VER_POSITIVE | AnimateWindow.AW_SLIDE);
					break;
				case AnimateMode.SlideTopToBottom:
					flag = (uint)(AnimateWindow.AW_VER_NEGATIVE | AnimateWindow.AW_SLIDE);
					break;
				case AnimateMode.RollLeftToRight:
					flag = (uint)(AnimateWindow.AW_HOR_POSITIVE);
					break;
				case AnimateMode.RollRightToLeft:
					flag = (uint)(AnimateWindow.AW_HOR_NEGATIVE);
					break;
				case AnimateMode.RollBottmToTop:
					flag = (uint)(AnimateWindow.AW_VER_POSITIVE);
					break;
				case AnimateMode.RollTopToBottom:
					flag = (uint)(AnimateWindow.AW_VER_NEGATIVE);
					break;
			}
			if (supportsLayered)
			{
				ShowInvisible(x, y);
				UpdateLayeredWindow();
				User32.AnimateWindow(Handle, 200, flag);
			}
			else
			{
				Show(x, y);
			}
		}
		/// <summary>
		/// Hides the window with a specific animation.
		/// </summary>
		/// <param name="mode">An <see cref="AnimateMode"/> parameter.</param>
		public virtual void HideAnimate(AnimateMode mode)
		{
			uint flag = (uint)AnimateWindow.AW_CENTER;
			switch (mode)
			{
				case AnimateMode.Blend:
					HideWindowWithAnimation();
					return;
				case AnimateMode.ExpandCollapse:
					flag = (uint)AnimateWindow.AW_CENTER;
					break;
				case AnimateMode.SlideLeftToRight:
					flag = (uint)(AnimateWindow.AW_HOR_POSITIVE | AnimateWindow.AW_SLIDE);
					break;
				case AnimateMode.SlideRightToLeft:
					flag = (uint)(AnimateWindow.AW_HOR_NEGATIVE | AnimateWindow.AW_SLIDE);
					break;
				case AnimateMode.SlideBottmToTop:
					flag = (uint)(AnimateWindow.AW_VER_POSITIVE | AnimateWindow.AW_SLIDE);
					break;
				case AnimateMode.SlideTopToBottom:
					flag = (uint)(AnimateWindow.AW_VER_NEGATIVE | AnimateWindow.AW_SLIDE);
					break;
				case AnimateMode.RollLeftToRight:
					flag = (uint)(AnimateWindow.AW_HOR_POSITIVE);
					break;
				case AnimateMode.RollRightToLeft:
					flag = (uint)(AnimateWindow.AW_HOR_NEGATIVE);
					break;
				case AnimateMode.RollBottmToTop:
					flag = (uint)(AnimateWindow.AW_VER_POSITIVE);
					break;
				case AnimateMode.RollTopToBottom:
					flag = (uint)(AnimateWindow.AW_VER_NEGATIVE);
					break;
			}
			flag = flag | (uint)AnimateWindow.AW_HIDE;
			if (supportsLayered)
			{
				UpdateLayeredWindow();
				User32.AnimateWindow(Handle, 200, flag);
			}
			Hide();
		}
		/// <summary>
		/// Hides the window.
		/// </summary>
		public virtual void Hide()
		{
			if (captured)
			{
				User32.ReleaseCapture();
			}
			User32.ShowWindow(Handle, 0);
			ReleaseHandle();
		}
		private void HideWindowWithAnimation()
		{
			if (supportsLayered)
			{
				for (int num1 = 0xff; num1 > 0; num1 -= 3)
				{
					if (num1 < 0)
					{
						num1 = 0;
					}
					UpdateLayeredWindow(location, size, (byte) num1);
				}
			}
			Hide();
		}


		#endregion

		#region == Mouse ==

		private POINT MousePositionToClient(POINT point)
		{
			POINT point1;
			point1.x = point.x;
			point1.y = point.y;
			User32.ScreenToClient(Handle, ref point1);
			return point1;
		}
		private POINT MousePositionToScreen(MSG msg)
		{
			POINT point1;
			point1.x = (short) (((int) msg.lParam) & 0xffff);
			point1.y = (short) ((((int) msg.lParam) & -65536) >> 0x10);
			if ((((msg.message != 0xa2) && (msg.message != 0xa8)) && ((msg.message != 0xa5) && (msg.message != 0xac))) && (((msg.message != 0xa1) && (msg.message != 0xa7)) && ((msg.message != 0xa4) && (msg.message != 0xab))))
			{
				User32.ClientToScreen(msg.hwnd, ref point1);
			}
			return point1;
		}
		private POINT MousePositionToScreen(POINT point)
		{
			POINT point1;
			point1.x = point.x;
			point1.y = point.y;
			User32.ClientToScreen(Handle, ref point1);
			return point1;
		}
		private POINT MousePositionToScreen(Message msg)
		{
			POINT point1;
			point1.x = (short) (((int) msg.LParam) & 0xffff);
			point1.y = (short) ((((int) msg.LParam) & -65536) >> 0x10);
			if ((((msg.Msg != 0xa2) && (msg.Msg != 0xa8)) && ((msg.Msg != 0xa5) && (msg.Msg != 0xac))) && (((msg.Msg != 0xa1) && (msg.Msg != 0xa7)) && ((msg.Msg != 0xa4) && (msg.Msg != 0xab))))
			{
				User32.ClientToScreen(msg.HWnd, ref point1);
			}
			return point1;
		}

		private void PerformWmMouseDown(ref Message m)
		{
			if (!closeRect.Contains(lastMouseDown) && 
				!expandRect.Contains(lastMouseDown))
			{
				if (resizeR.Contains(lastMouseDown))
				{
					resizing = true;
				}
				captured = true;
				User32.SetCapture(Handle);
			}
		}
		private void PerformWmMouseMove(ref Message m)
		{
			Point p = Control.MousePosition;
			POINT point1 = new POINT();
			point1.x = p.X;
			point1.y = p.Y;
			point1 = MousePositionToClient(point1);

			if (resizing || resizeR.Contains(point1.x, point1.y))
			{
				Cursor.Current = Cursors.SizeNWSE;
			}
			else
				Cursor.Current = Cursors.Arrow;
			if (captured)
			{
				
				if (resizing)
				{
					int w = Math.Max(50, (p.X+deltaX)-Location.X);
					int h = Math.Max(50, (p.Y+deltaY)-Location.Y);
					Size = new Size(w, h);
				}
				else
				{
					Location = new Point(p.X-deltaX, p.Y-deltaY);
				}
			}
		}
		private void PerformWmMouseUp(ref Message m)
		{
			resizing = false;
			if (captured)
			{
				captured = false;
				User32.ReleaseCapture();
			}
		}
		private void PerformWmMouseActivate(ref Message m)
		{
			m.Result = (IntPtr) 3;
		}


		protected virtual void OnMouseMove(MouseEventArgs e)
		{
			if (MouseMove!=null)
			{
				MouseMove(this, e);
			}
			onMouseMove = true;
		}
		protected virtual void OnMouseDown(MouseEventArgs e)
		{
			if (MouseDown!=null)
			{
				MouseDown(this, e);
			}
			onMouseDown = true;
		}
		protected virtual void OnMouseUp(MouseEventArgs e)
		{
			if (MouseUp!=null)
			{
				MouseUp(this, e);
			}
			onMouseUp = true;
		}

		protected virtual void OnMouseEnter()
		{
			if (MouseEnter!=null)
			{
				MouseEnter(this, EventArgs.Empty);
			}
		}
		protected virtual void OnMouseLeave()
		{
			if (MouseLeave!=null)
			{
				MouseLeave(this, EventArgs.Empty);
			}
		}

		#endregion

		#region == Other messages ==

		private bool PerformWmNcHitTest(ref Message m)
		{
			POINT point1;
			Point p = Control.MousePosition;
			point1.x = p.X;
			point1.y = p.Y;
			point1 = MousePositionToClient(point1);

			Rectangle rect = new Rectangle(0, 0, Width, captionHeight);

			if (resizeR.Contains(point1.x, point1.y))
			{
				return false;
			}
			if (expandRect.Contains(point1.x, point1.y))
			{
				return false;
			}
			if (closeRect.Contains(point1.x, point1.y))
			{
				return false;
			}
			if (rect.Contains(point1.x, point1.y))
			{
				return false;
			}

			m.Result = (IntPtr) (-1);
			return true;
		}
		
		private void PerformWmSetCursor(ref Message m)
		{
		}
		private void PerformWmPaint(ref Message m)
		{
			PAINTSTRUCT paintstruct1;
			RECT rect1;
			Rectangle rectangle1;
			paintstruct1 = new PAINTSTRUCT();
			IntPtr ptr1 = User32.BeginPaint(m.HWnd, ref paintstruct1);
			rect1 = new RECT();
			User32.GetWindowRect(Handle, ref rect1);
			rectangle1 = new Rectangle(0, 0, rect1.right - rect1.left, rect1.bottom - rect1.top);
			using (Graphics graphics1 = Graphics.FromHdc(ptr1))
			{
				Bitmap bitmap1 = new Bitmap(rectangle1.Width, rectangle1.Height);
				using (Graphics graphics2 = Graphics.FromImage(bitmap1))
				{
					OnPaint(new PaintEventArgs(graphics2, rectangle1));
				}
				graphics1.DrawImageUnscaled(bitmap1, 0, 0);
			}
			User32.EndPaint(m.HWnd, ref paintstruct1);
		}

		protected override void WndProc(ref Message m)
		{
			int num1 = m.Msg;
			if (num1 <= 0x1c) // WM_PAINT
			{
				if (num1 == 15)
				{
					PerformWmPaint(ref m);
					return;
				}
			}
			else
			{
				switch (num1)
				{
					case 0x20: // WM_SETCURSOR
					{
						PerformWmSetCursor(ref m);
						return;
					}
					case 0x21: // WM_MOUSEACTIVATE
					{
						PerformWmMouseActivate(ref m);
						return;
					}
					case 0x84: // WM_NCHITTEST
					{
						if (!PerformWmNcHitTest(ref m))
						{
							base.WndProc(ref m);
						}
						return;
					}
					case 0x200: // WM_MOUSEMOVE
						if (!isMouseIn)
						{
							OnMouseEnter();
							isMouseIn = true;
						}
						Point p6 = new Point(m.LParam.ToInt32());
						OnMouseMove(new MouseEventArgs(Control.MouseButtons, 1, p6.X, p6.X, 0));
						if (onMouseMove)
						{
							PerformWmMouseMove(ref m);
							onMouseMove = false;
						}
						break;
					case 0x201: // WM_MOUSEDOWN
					{
						POINT point1;
						lastMouseDown = new Point(m.LParam.ToInt32());
						point1 = new POINT();
						point1.x = lastMouseDown.X;
						point1.y = lastMouseDown.Y;
						point1 = MousePositionToScreen(point1);
						deltaX = point1.x - Location.X;
						deltaY = point1.y - Location.Y;
						OnMouseDown(new MouseEventArgs(Control.MouseButtons, 1, lastMouseDown.X, lastMouseDown.Y, 0));
						if (onMouseDown)
						{
							PerformWmMouseDown(ref m);
							onMouseDown = false;
						}
						if (resizing)
						{
							deltaX = Bounds.Right - point1.x;
							deltaY = Bounds.Bottom - point1.y;
						}

						return;
					}
					case 0x202: // WM_LBUTTONUP
					{
						Point p = new Point(m.LParam.ToInt32());
						OnMouseUp(new MouseEventArgs(Control.MouseButtons, 1, p.X, p.Y, 0));
						if (onMouseUp)
						{
							if (closeRect.Contains(p))
							{
								Hide();
								return;
							}
							if (expandRect.Contains(p))
							{
								if (minimized)
									Maximize();
								else
									Minimize();
								return;
							}
							PerformWmMouseUp(ref m);
							onMouseUp = false;
						}
						return;
					}
					case 0x02A3: // WM_MOUSELEAVE
					{
						if (isMouseIn)
						{
							OnMouseLeave();
							isMouseIn = false;
						}
						break;
					}
				}
			}
			base.WndProc(ref m);
		}


		#endregion

		#region == Event Methods ==

		protected virtual void OnLocationChanged(EventArgs e)
		{
			OnMove(EventArgs.Empty);
			if (LocationChanged!=null)
			{
				LocationChanged(this, e);
			}
		}
		protected virtual void OnSizeChanged(EventArgs e)
		{
			OnResize(EventArgs.Empty);
			if (SizeChanged!=null)
			{
				SizeChanged(this, e);
			}
		}
		protected virtual void OnMove(EventArgs e)
		{
			if (Move!=null)
			{
				Move(this, e);
			}
		}
		protected virtual void OnResize(EventArgs e)
		{
			if (Resize!=null)
			{
				Resize(this, e);
			}
		}


		#endregion

		#region == Size and Location ==

		protected virtual void SetBoundsCore(int x, int y, int width, int height)
		{
			if (width < (11+11+4+4)) width = 11+11+4+4;
			if (height < captionHeight*2 && !minimizing) height = captionHeight*2;

			if (((X != x) || (Y != y)) || ((Width != width) || (Height != height)))
			{
				if (Handle != IntPtr.Zero)
				{
					int num1 = 20;
					if ((X == x) && (Y == y))
					{
						num1 |= 2;
					}
					if ((Width == width) && (Height == height))
					{
						num1 |= 1;
					}
					User32.SetWindowPos(Handle, IntPtr.Zero, x, y, width, height, (uint)num1);
				}
				else
				{
					UpdateBounds(x, y, width, height);
				}
			}
		}
		private void UpdateBounds()
		{
			RECT rect1;
			rect1 = new RECT();
			User32.GetClientRect(Handle, ref rect1);
			int num1 = rect1.right;
			int num2 = rect1.bottom;
			User32.GetWindowRect(Handle, ref rect1);
			if (User32.GetParent(Handle)!=IntPtr.Zero)
			{
				User32.MapWindowPoints(IntPtr.Zero, User32.GetParent(Handle), ref rect1, 2);
			}
			UpdateBounds(rect1.left, rect1.top, rect1.right - rect1.left, rect1.bottom - rect1.top, num1, num2);
		}
		private void UpdateBounds(int x, int y, int width, int height)
		{
			RECT rect1;
			int num3;
			rect1 = new RECT();
			rect1.bottom = num3 = 0;
			rect1.top = num3 = num3;
			rect1.right = num3 = num3;
			rect1.left = num3;
			CreateParams params1 = CreateParams;
			User32.AdjustWindowRectEx(ref rect1, params1.Style, false, params1.ExStyle);
			int num1 = width - (rect1.right - rect1.left);
			int num2 = height - (rect1.bottom - rect1.top);
			UpdateBounds(x, y, width, height, num1, num2);
		}
		private void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight)
		{
			bool flag1 = (X != x) || (Y != y);
			bool flag2 = (((Width != width) || (Height != height)) || (this.clientWidth != clientWidth)) || (this.clientHeight != clientHeight);
			size = new Size(width, height);
			location = new Point(x, y);
			this.clientWidth = clientWidth;
			this.clientHeight = clientHeight;
			if (flag1)
			{
				OnLocationChanged(EventArgs.Empty);
			}
			if (flag2)
			{
				OnSizeChanged(EventArgs.Empty);
			}
		}
 

		#endregion

		#region == Various ==

		public void RecreateHandle()
		{
			CreateParams params1 = CreateParams;
			CreateHandle(params1);
			Invalidate();
		}
 
		private void value_HandleDestroyed(object sender, EventArgs e)
		{
			parent.HandleDestroyed-=value_HandleDestroyed;
			Hide();
		}

		public void Destroy()
		{
			Hide();
			Dispose();
		}

		
		#endregion

		#endregion

		#region #  Properties  #

		public bool Minimized
		{
			get {return minimized;}
			set
			{
				if (value)
					Minimize();
				else
					Maximize();
			}
		}
		protected virtual CreateParams CreateParams
		{
			get
			{
				CreateParams params1 = new CreateParams();
				params1.Caption = "FloatingNativeWindow";
				int nX = location.X;
				int nY = location.Y;
				Screen screen1 = Screen.FromHandle(Handle);
				if ((nX + size.Width) > screen1.Bounds.Width)
				{
					nX = screen1.Bounds.Width - size.Width;
				}
				if ((nY + size.Height) > screen1.Bounds.Height)
				{
					nY = screen1.Bounds.Height - size.Height;
				}
				location = new Point(nX, nY);
				Size size1 = size;
				Point point1 = location;
				params1.X = nX;
				params1.Y = nY;
				params1.Height = size1.Height;
				params1.Width = size1.Width;
				params1.Parent = IntPtr.Zero;
				params1.Style = -2147483648;
				params1.ExStyle = 0x88;
				if (supportsLayered)
				{
					params1.ExStyle += 0x80000;
				}
				size = size1;
				location = point1;
				return params1;
			}
		}
		public Control Parent
		{
			get {return parent;}
			set
			{
				if (value == parent) return;
				if (parent!=null)
				{
					value.HandleDestroyed-=value_HandleDestroyed;
				}
				parent = value;
				if (value != null)
				{
					value.HandleDestroyed+=value_HandleDestroyed;
				}
			}
		}

		public virtual Size Size
		{
			get {return size;}
			set
			{
				if (Handle!=IntPtr.Zero)
				{
					SetBoundsCore(location.X, location.Y, value.Width, value.Height);
					RECT rect = new RECT();
					User32.GetWindowRect(Handle, ref rect);
					size = new Size(rect.right-rect.left, rect.bottom-rect.top);
					UpdateLayeredWindow();
				}
				else
				{
					size = value;
				}
			}
		}
		public virtual Point Location
		{
			get {return location;}
			set
			{
				if (Handle!=IntPtr.Zero)
				{
					SetBoundsCore(value.X, value.Y, size.Width, size.Height);
					RECT rect = new RECT();
					User32.GetWindowRect(Handle, ref rect);
					location = new Point(rect.left, rect.top);
					UpdateLayeredWindow();
				}
				else
				{
					location = value;
				}
			}
		}
		public int Height
		{
			get {return size.Height;}
			set
			{
				Size = new Size(size.Width, value);
			}
		}
		public int Width
		{
			get {return size.Width;}
			set
			{
				Size = new Size(value, size.Height);
			}
		}
		public int X
		{
			get {return location.X;}
			set
			{
				Location = new Point(value, location.Y);
			}
		}
		public int Y
		{
			get {return location.Y;}
			set
			{
				Location = new Point(location.X, value);
			}
		}
		public Rectangle ClientRectangle
		{
			get
			{
				RECT rct = new RECT();
				POINT pnt = new POINT();
				User32.GetWindowRect(Handle, ref rct);
				pnt.x = rct.left;
				pnt.y = rct.top;
				User32.ScreenToClient(Handle, ref pnt);
				Rectangle rect = new Rectangle(pnt.x, pnt.y, rct.right-rct.left, rct.bottom-rct.top);
				if (hasShadow)
				{
					rect.Width -= shadowLength;
					rect.Height -= shadowLength;
				}
				return rect;
			}
		}
		protected Rectangle ShadowRectangle
		{
			get 
			{
				Rectangle rect = ClientRectangle;
				rect.X += shadowLength;
				rect.Y += shadowLength;
				return rect;
			}
		}
		public Rectangle Bounds
		{
			get
			{
				return new Rectangle(location, size);
			}
			set
			{
				location = value.Location;
				size = value.Size;
				SetBoundsCore(location.X, location.Y, size.Width, size.Height);
			}
		}
		public bool HasShadow
		{
			get {return hasShadow;}
			set
			{
				hasShadow = value;
				ComputeLayout();
				Invalidate();
			}
		}
		public string Text
		{
			get {return text;}
			set
			{
				text = value;
				ComputeLayout();
				Invalidate();
			}
		}
		public int CaptionHeight
		{
			get {return captionHeight;}
			set
			{
				captionHeight = value;
				ComputeLayout();
				Invalidate();
			}
		}

		public int Alpha
		{
			get {return alpha;}
			set
			{
				if (alpha == value) return;
				if (value < 0 || value > 255)
				{
					throw new ArgumentException("Alpha must be between 0 and 255");
				}
				alpha = value;
				UpdateLayeredWindow((byte)alpha);
			}
		}

		#endregion

		#region #  Events  #

		public event PaintEventHandler Paint;
		public event EventHandler SizeChanged;
		public event EventHandler LocationChanged;
		public event EventHandler Move;
		public event EventHandler Resize;
		public event MouseEventHandler MouseDown;
		public event MouseEventHandler MouseUp;
		public event MouseEventHandler MouseMove;
		public event EventHandler MouseEnter;
		public event EventHandler MouseLeave;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (parent!=null)
					{
						parent.HandleDestroyed-=value_HandleDestroyed;
					}
				}
				DestroyHandle();
				disposed = true;
			}
		}

		#endregion
	}

	#region #  Win32  #

	internal struct PAINTSTRUCT
	{
		public IntPtr hdc;
		public int fErase;
		public Rectangle rcPaint;
		public int fRestore;
		public int fIncUpdate;
		public int Reserved1;
		public int Reserved2;
		public int Reserved3;
		public int Reserved4;
		public int Reserved5;
		public int Reserved6;
		public int Reserved7;
		public int Reserved8;
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct POINT
	{
		public int x;
		public int y;
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct SIZE
	{
		public int cx;
		public int cy;
	}
	[StructLayout(LayoutKind.Sequential)][CLSCompliant(false)]
	internal struct TRACKMOUSEEVENTS
	{
		public uint cbSize;
		public uint dwFlags;
		public IntPtr hWnd;
		public uint dwHoverTime;
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct MSG
	{
		public IntPtr hwnd;
		public int message;
		public IntPtr wParam;
		public IntPtr lParam;
		public int time;
		public int pt_x;
		public int pt_y;
	}
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct BLENDFUNCTION
	{
		public byte BlendOp;
		public byte BlendFlags;
		public byte SourceConstantAlpha;
		public byte AlphaFormat;
	}
	internal class AnimateWindow
	{
		private AnimateWindow() 
		{
		}
		public static int AW_HOR_POSITIVE = 0x1;
		public static int AW_HOR_NEGATIVE = 0x2;
		public static int AW_VER_POSITIVE = 0x4;
		public static int AW_VER_NEGATIVE = 0x8;
		public static int AW_CENTER = 0x10;
		public static int AW_HIDE = 0x10000;
		public static int AW_ACTIVATE = 0x20000;
		public static int AW_SLIDE = 0x40000;
		public static int AW_BLEND = 0x80000;
	}
	public enum AnimateMode
	{
		SlideRightToLeft,
		SlideLeftToRight,
		SlideTopToBottom,
		SlideBottmToTop,
		RollRightToLeft,
		RollLeftToRight,
		RollTopToBottom,
		RollBottmToTop,
		Blend,
		ExpandCollapse
	}
	internal class User32
	{
		// Methods
		private User32()
		{
		}
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool AnimateWindow(IntPtr hWnd, uint dwTime, uint dwFlags);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT pt);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool DispatchMessage(ref MSG msg);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool DrawFocusRect(IntPtr hWnd, ref RECT rect);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetDC(IntPtr hWnd);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetFocus();
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern ushort GetKeyState(int virtKey);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool GetMessage(ref MSG msg, int hWnd, uint wFilterMin, uint wFilterMax);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetParent(IntPtr hWnd);
		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
		public static extern bool GetClientRect(IntPtr hWnd, [In, Out] ref RECT rect);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetWindow(IntPtr hWnd, int cmd);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool HideCaret(IntPtr hWnd);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool InvalidateRect(IntPtr hWnd, ref RECT rect, bool erase);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr LoadCursor(IntPtr hInstance, uint cursor);
		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
		public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref RECT rect, int cPoints);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool PeekMessage(ref MSG msg, int hWnd, uint wFilterMin, uint wFilterMax, uint wFlag);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool ReleaseCapture();
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern uint SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SetCursor(IntPtr hCursor);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SetFocus(IntPtr hWnd);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int newLong);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int Width, int Height, uint flags);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool ShowCaret(IntPtr hWnd);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool SetCapture(IntPtr hWnd);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int ShowWindow(IntPtr hWnd, short cmdShow);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref int bRetValue, uint fWinINI);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool TrackMouseEvent(ref TRACKMOUSEEVENTS tme);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool TranslateMessage(ref MSG msg);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool UpdateWindow(IntPtr hwnd);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool WaitMessage();
		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
		public static extern bool AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);
	}
	internal class Gdi32
	{
		// Methods
		private Gdi32()
		{
		}
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern int CombineRgn(IntPtr dest, IntPtr src1, IntPtr src2, int flags);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr CreateBrushIndirect(ref LOGBRUSH brush);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr CreateRectRgnIndirect(ref RECT rect);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern bool DeleteDC(IntPtr hDC);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr DeleteObject(IntPtr hObject);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern int GetClipBox(IntPtr hDC, ref RECT rectBox);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern bool PatBlt(IntPtr hDC, int x, int y, int width, int height, uint flags);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern int SelectClipRgn(IntPtr hDC, IntPtr hRgn);
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
	}
	[StructLayout(LayoutKind.Sequential)][CLSCompliant(false)]
	public struct LOGBRUSH
	{
		public uint lbStyle;
		public uint lbColor;
		public uint lbHatch;
	}

	#endregion
}
