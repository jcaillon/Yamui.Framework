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
            

        }
    }
}
