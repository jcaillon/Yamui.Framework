using System;

namespace Yamui.Framework.Forms {

    public abstract partial class YamuiForm {

        [Flags]
        public enum YamuiFormOption {
            WithShadow = 1,
        }
    }
}
