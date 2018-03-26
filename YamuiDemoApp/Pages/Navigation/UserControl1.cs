using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Controls;

namespace YamuiDemoApp.Pages.Navigation
{
    public partial class UserControl1 : YamuiPage
    {
        public UserControl1()
        {
            InitializeComponent();

            var sb = new StringBuilder();
            for (int i = 0; i < 3333; i++) {
                sb.Append(i + "<br>");
            }

            htmlLabel1.Text = sb.ToString();
            

           scrollPanelTest1.VerticalScroll.UpdateLength(100, 10, Height, Width);
           scrollPanelTest1.VerticalScroll.SmallChange = 1;
           scrollPanelTest1.VerticalScroll.LargeChange = 5;
           scrollPanelTest1.VerticalScroll.ExtraEndPadding = 15;
           scrollPanelTest1.VerticalScroll.OnValueChange += (simple, i, j) => { yamuiLabel1.Text = j.ToString(); };
           scrollPanelTest1.VerticalScroll.OnRedrawScrollBars += () => scrollPanelTest1.Invalidate();
           
           scrollPanelTest1.HorizontalScroll.UpdateLength(100, 10, Width, Height);
           scrollPanelTest1.HorizontalScroll.SmallChange = 1;
           scrollPanelTest1.HorizontalScroll.LargeChange = 5;
           scrollPanelTest1.HorizontalScroll.ExtraEndPadding = 15;
           scrollPanelTest1.HorizontalScroll.OnValueChange += (simple, i, j) => { yamuiLabel1.Text = j.ToString(); };
           scrollPanelTest1.HorizontalScroll.OnRedrawScrollBars += () => scrollPanelTest1.Invalidate();
            
        }
    }
}
