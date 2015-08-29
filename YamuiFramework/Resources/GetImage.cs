using System.Drawing;

namespace YamuiFramework.Resources {
    class GetImage {

        private static GetImage _instance;

        public static GetImage GetInstance() {
            return _instance ?? (_instance = new GetImage());
        }

        public Image Get(string filename) {
            return (Image)Resources.ResourceManager.GetObject(filename);
        }
    }
}
