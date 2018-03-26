using System;
using System.Windows.Forms;
using YamuiFramework.Forms;
using YamuiFramework.Themes;

namespace YamuiDemoApp {

    public class ColorScheme {
        public string Name = "Default";
        public int UniqueId = 0;
    }

    static class Program {

        public static YamuiMainAppli MainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            Application.EnableVisualStyles();

            // https://stackoverflow.com/questions/8283631/graphics-drawstring-vs-textrenderer-drawtextwhich-can-deliver-better-quality
            Application.SetCompatibleTextRenderingDefault(false);

            YamuiThemeManager.TabAnimationAllowed = true;
            MainForm = new Form1();

            Application.Run(MainForm);

        }
    }


}
