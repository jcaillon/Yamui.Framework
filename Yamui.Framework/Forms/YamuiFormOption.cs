using System;

namespace Yamui.Framework.Forms {

    public abstract partial class YamuiForm {

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>example for a tooltip window : YamuiFormOption.WithDropShadow | YamuiFormOption.IsPopup | YamuiFormOption.DontShowInAltTab | YamuiFormOption.DontActivateOnShow | YamuiFormOption.AlwaysOnTop | YamuiFormOption.Unselectable</remarks>
        [Flags]
        public enum YamuiFormOption {
            None = 0,
            WithShadow = 1 << 0,
            WithDropShadow = 1 << 1,
            AlwaysOnTop = 1 << 2,
            DontShowInAltTab = 1 << 3,
            IsPopup = 1 << 4,
            DontActivateOnShow = 1 << 5,
            Unselectable = 1 << 6
        }
    }
}
