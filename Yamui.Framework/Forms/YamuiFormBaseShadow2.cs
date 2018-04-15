#region header

// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormBaseShadow.cs) is part of YamuiFramework.
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WpfGlowWindow.Glow;
using Yamui.Framework.Helper;

namespace Yamui.Framework.Forms {
    /// <summary>
    /// Form class that implements interesting utilities + shadow + onpaint + movable borderless
    /// </summary>
    public class YamuiFormBaseShadow2 : YamuiFormBase
    {
        private Controls.YamuiButton yamuiButton1;
        private GlowDecorator _glowDecorator;
        private SideGlow _rightGlow;

        public YamuiFormBaseShadow2() {
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            this.yamuiButton1 = new Yamui.Framework.Controls.YamuiButton();
            this.SuspendLayout();
            // 
            // yamuiButton1
            // 
            this.yamuiButton1.BackGrndImage = null;
            this.yamuiButton1.GreyScaleBackGrndImage = null;
            this.yamuiButton1.IsFocused = false;
            this.yamuiButton1.IsHovered = false;
            this.yamuiButton1.IsPressed = false;
            this.yamuiButton1.Location = new System.Drawing.Point(35, 37);
            this.yamuiButton1.Name = "yamuiButton1";
            this.yamuiButton1.SetImgSize = new System.Drawing.Size(0, 0);
            this.yamuiButton1.Size = new System.Drawing.Size(116, 39);
            this.yamuiButton1.TabIndex = 0;
            this.yamuiButton1.Text = "yamuiButton1";
            this.yamuiButton1.ButtonPressed += new System.EventHandler<System.EventArgs>(this.yamuiButton1_ButtonPressed);
            // 
            // YamuiFormBaseShadow2
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.yamuiButton1);
            this.Name = "YamuiFormBaseShadow2";
            this.ResumeLayout(false);

        }

        protected override void WndProc(ref Message m) {
            bool handled = false;
            if (_glowDecorator != null)
                _glowDecorator.WndProc(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);
            if (!handled)
                base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if (_rightGlow != null) {

                e.Graphics.DrawImage(_rightGlow.GetBitmap(9, 100), new Rectangle(10, 10, 9, 100), 0, 0, 9, 100, GraphicsUnit.Pixel);
            }
        }

        private void yamuiButton1_ButtonPressed(object sender, EventArgs e)
        {
            _glowDecorator = new GlowDecorator();
            _glowDecorator.Attach(this);
            _glowDecorator.ActiveColor = Color.Cyan;
            _glowDecorator.InactiveColor = Color.Gray;
            _glowDecorator.Activate(true);
            _glowDecorator.EnableResize(true);

            //_rightGlow = new SideGlow(DockStyle.Right, Handle);
            //var pos = new WinApi.WINDOWPOS();
            //pos.x = Left;
            //pos.y = Top;
            //pos.cx = Width;
            //pos.cy = Height;
            //_rightGlow.SetLocation(pos);
            //_rightGlow.SetSize(Width, Height);
            //_rightGlow.Show(true);
            //WinApi.ShowWindow(new HandleRef(_rightGlow, _rightGlow.Handle), WinApi.ShowWindowStyle.SW_SHOWNOACTIVATE);
        }
    }

}