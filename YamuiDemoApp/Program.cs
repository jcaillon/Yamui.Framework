using System;
using System.Windows.Forms;
using YamuiFramework.Forms;

namespace YamuiDemoApp {
    static class Program {

        public static YamuiForm MainForm;
        public static YamuiSmokeScreen MainSmokeScreen;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new Form1();
            Application.Run(MainForm);
        }
    }
}
