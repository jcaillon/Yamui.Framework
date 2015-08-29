using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Forms;

namespace YamuiDemoApp.Pages {
    public partial class PageTemplate : YamuiPage {

        #region fields
        private Form _ownerForm;
        #endregion


        #region constructor
        public PageTemplate() {
            InitializeComponent();
            _ownerForm = FindForm();
        }
        #endregion

    }
}
