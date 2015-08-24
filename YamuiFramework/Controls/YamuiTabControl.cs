using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Animations.Animator;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;
using YamuiFramework.Native;

namespace YamuiFramework.Controls {

    #region YamuiTabPageCollection
    [ToolboxItem(false)]
    [Editor("YamuiFramework.Controls.YamuiTabPageCollectionEditor", typeof(UITypeEditor))]
    public class YamuiTabPageCollection : TabControl.TabPageCollection {
        public YamuiTabPageCollection(YamuiTabControl owner)
            : base(owner) { }
    }
    #endregion

    [Designer("YamuiFramework.Controls.YamuiTabControlDesigner")]
    [ToolboxBitmap(typeof(TabControl))]
    public class YamuiTabControl : TabControl {

        #region Fields

        private SubClass _scUpDown;
        private bool _bUpDown;

        private const int TabBottomBorderHeight = 2;

        private ContentAlignment _textAlign = ContentAlignment.TopLeft;
        [DefaultValue(ContentAlignment.TopLeft)]
        [Category("Yamui")]
        public ContentAlignment TextAlign {
            get {
                return _textAlign;
            }
            set {
                _textAlign = value;
            }
        }

        [Editor("YamuiFramework.Controls.YamuiTabPageCollectionEditor", typeof(UITypeEditor))]
        public new TabPageCollection TabPages {
            get {
                return base.TabPages;
            }
        }

        private bool _isMirrored;
        [DefaultValue(false)]
        [Category("Yamui")]
        public new bool IsMirrored {
            get {
                return _isMirrored;
            }
            set {
                if (_isMirrored == value) {
                    return;
                }
                _isMirrored = value;
                UpdateStyles();
            }
        }

        private TabFunction _function = TabFunction.Main;
        [DefaultValue(TabFunction.Main)]
        [Category("Yamui")]
        public TabFunction Function {
            get { return _function; }
            set {
                _function = value;
                SetStuff();
                Font = FontManager.GetTabControlFont(Function);
            }
        }

        // used to remember the position of each tab
        private Dictionary<int, Rectangle> _getRekt = new Dictionary<int, Rectangle>(); 
        private Dictionary<int, bool> _currentlyShown = new Dictionary<int, bool>(); 

        // this is the actual tab index that should be display!
        private int _selectedIndex = 0;
        [Browsable(false)]
        public int SelectIndex {
            get { return _selectedIndex; }
            set {
                if (value < 0) return;
                if (value == _selectedIndex) return;
                _selectedIndex = value;
                _fromSelectIndex = true;
                SelectedIndex = _selectedIndex;
            }
        }

        private bool _fromSelectIndex;
        private int _lastSelectedTab = 0;

        // the index of the current tab hovered by the cursor
        private int _hotTrackTab = -1;
        #endregion

        #region Constructor

        public YamuiTabControl() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint, true);

            MouseMove += (sender, args) => UpdateHotTrack();
            MouseEnter += (sender, args) => UpdateHotTrack();
            MouseLeave += (sender, args) => UpdateHotTrack();
            MouseDown += OnMouseClick;
            
            _animator = new Animator();
            _animator.AnimationType = AnimationType.Custom;
            _animator.Interval = 35;
            _animator.TimeStep = 0.04f;
            _animator.MaxAnimationTime = 500;

            SetStuff();
        }

        private void SetStuff() {
            ItemSize = new Size(5, (Function == TabFunction.Main) ? 30 : 18);
            Padding = new Point((Function == TabFunction.Main) ? 8 : 6, 0);
        }
        #endregion

        #region User function
        /// <summary>
        /// Called this function to refresh the tab if you changed it's "hidden" attribute
        /// </summary>
        public void ApplyHideThisSettings() {
            for (int i = 0; i < TabPages.Count; i++) {
                var tPage = (YamuiTabPage) TabPages[i];
                if (!_currentlyShown.ContainsKey(i)) break;
                if (_currentlyShown[i] != tPage.HiddenPage)
                    Invalidate(GetRektOf(i));
            }
        }

        public int GetIndexOf(YamuiTabPage page) {
            for (int i = 0; i < TabPages.Count; i++) {
                var tPage = (YamuiTabPage)TabPages[i];
                if (tPage == page)
                    return i;
            }
            return -1;
        }
        #endregion

        #region "tab animator"
        Animator _animator;

        protected override void OnSelecting(TabControlCancelEventArgs e) {
            base.OnSelecting(e);
            // cancel the tab selecting if it was not set through SelectIndex
            if (!_fromSelectIndex) {
                e.Cancel = true;
                return;
            }
            _fromSelectIndex = false;
            Application.DoEvents();
            Invalidate(GetRektOf(e.TabPageIndex));
            Invalidate(GetRektOf(_lastSelectedTab));
            _lastSelectedTab = e.TabPageIndex;
            if (!ThemeManager.AnimationAllowed) return;
            try {
                Animation anim = new Animation();
                anim.AnimateOnlyDifferences = true;
                //anim.SlideCoeff = new PointF(0.03f, 0f);
                anim.TimeCoeff = 4F;
                anim.TransparencyCoeff = 1F;
                _animator.ClearQueue();
                _animator.BeginUpdate(this, false, anim, new Rectangle(0, ItemSize.Height, Width, Height - ItemSize.Height));
                BeginInvoke(new MethodInvoker(() => _animator.EndUpdate(this)));
            } catch (Exception) {
                // ignored
            }
        }
        #endregion

        #region tabhover
        // returns the index of the tab under the cursor, or -1 if no tab is under
        private int GetTabUnderCursor() {
            Point cursor = PointToClient(Cursor.Position);
            for (int i = 0; i < TabPages.Count; i++) {
                if (GetRektOf(i).Contains(cursor))
                    return i;
            }
            return -1;
        }

        // updates hot tracking based on the current cursor position
        private void UpdateHotTrack() {
            int hot = GetTabUnderCursor();
            if (hot != _hotTrackTab) {
                // invalidate the old hot-track item to remove hot-track effects
                if (_hotTrackTab != -1)
                    Invalidate(GetRektOf(_hotTrackTab));

                _hotTrackTab = hot;

                // invalidate the new hot-track item to add hot-track effects
                if (_hotTrackTab != -1)
                    Invalidate(GetRektOf(_hotTrackTab));

                // force the tab to redraw invalidated regions
                Update();
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs mouseEventArgs) {
            SelectIndex = GetTabUnderCursor();
        }
        #endregion

        #region Paint Methods
        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            graphics.Clear(Color.Transparent);
            if ((Parent != null)) {
                clipRect.Offset(Location);
                PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                GraphicsState state = graphics.Save();
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                try {
                    graphics.TranslateTransform(-Location.X, -Location.Y);
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                } finally {
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.TabsColors.Normal.BackColor();
                if (backColor != Color.Transparent)
                    e.Graphics.Clear(backColor);
                else
                    PaintTransparentBackground(e.Graphics, DisplayRectangle);
            } catch {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                OnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {
            for (var index = 0; index < TabPages.Count; index++) {
                DrawTab(index, e.Graphics);
            }
        }

        private void DrawTab(int index, Graphics graphics) {
            YamuiTabPage tabPage = (YamuiTabPage)TabPages[index];
            Font usedFont = FontManager.GetTabControlFont((Function == TabFunction.Secondary && index != SelectIndex) ? TabFunction.SecondaryNotSelected : Function);
            Rectangle thisTabRekt;

            if (!_getRekt.ContainsKey(index)) {
                var textWidth = TextRenderer.MeasureText(graphics, tabPage.Text, usedFont, new Size(int.MaxValue, int.MaxValue), FontManager.GetTextFormatFlags(TextAlign)).Width + ((Function == TabFunction.Secondary) ? 10 : 0);
                if (_getRekt.Count == 0)
                    thisTabRekt = new Rectangle(GetTabRect(index).X, GetTabRect(index).Y, textWidth, GetTabRect(index).Height);
                else
                    thisTabRekt = new Rectangle(_getRekt.Last().Value.X + _getRekt.Last().Value.Width, _getRekt.Last().Value.Y, textWidth, _getRekt.Last().Value.Height);
                _getRekt.Add(index, thisTabRekt);
            } else {
                thisTabRekt = GetRektOf(index);
            }

            // redraw the back just in case
            Color backColor = ThemeManager.TabsColors.Normal.BackColor();
            if (backColor != Color.Transparent) {
                using (SolidBrush b = new SolidBrush(backColor))
                    graphics.FillRectangle(b, thisTabRekt);
            } else
                PaintTransparentBackground(graphics, thisTabRekt);

            Color foreColor = ThemeManager.TabsColors.ForeGround(Focused, index == _hotTrackTab, index == SelectIndex, Enabled);
            if (!tabPage.HiddenPage || DesignMode)
                TextRenderer.DrawText(graphics, tabPage.Text, usedFont, thisTabRekt, foreColor, FontManager.GetTextFormatFlags(TextAlign));

            if (!_currentlyShown.ContainsKey(index)) _currentlyShown.Add(index, tabPage.HiddenPage);
            else _currentlyShown[index] = tabPage.HiddenPage;
        }

        // Draw < > symbol to switch tabs
        [SecuritySafeCritical]
        private void DrawUpDown(Graphics graphics) {
            Color backColor = ThemeManager.FormColor.BackColor();
            Rectangle borderRect = new Rectangle();
            WinApi.GetClientRect(_scUpDown.Handle, ref borderRect);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(backColor);
            using (Brush b = new SolidBrush(ThemeManager.TabsColors.Press.ForeColor())) {
                GraphicsPath gp = new GraphicsPath(FillMode.Winding);
                PointF[] pts = { new PointF(6, (float)borderRect.Height / 2), new PointF(16, (float)borderRect.Height / 2 - 6), new PointF(16, (float)borderRect.Height / 2 + 6) };
                gp.AddLines(pts);
                graphics.FillPath(b, gp);
                gp.Reset();
                PointF[] pts2 = { new PointF(borderRect.Width - 15, (float)borderRect.Height / 2 - 6), new PointF(borderRect.Width - 5, (float)borderRect.Height / 2), new PointF(borderRect.Width - 15, (float)borderRect.Height / 2 + 6) };
                gp.AddLines(pts2);
                graphics.FillPath(b, gp);
                gp.Dispose();
            }
        }
        #endregion
        
        #region Overridden Methods
        protected override void OnSelectedIndexChanged(EventArgs e) {
            UpdateUpDown();
            try {
                YamuiForm ownerForm = (YamuiForm)FindForm();
                if (ownerForm != null) ownerForm.GoToPageByUserSelection();
            } catch (Exception) {
                throw new Exception("derp");
            }
            base.OnSelectedIndexChanged(e);
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            Invalidate();
        }

        [SecuritySafeCritical]
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            //if (!DesignMode)
                //WinApi.ShowScrollBar(Handle, (int)WinApi.ScrollBar.SB_BOTH, 0);
        }

        protected override CreateParams CreateParams {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get {
                const int wsExLayoutrtl = 0x400000;
                const int wsExNoinheritlayout = 0x100000;
                var cp = base.CreateParams;
                if (_isMirrored) {
                    cp.ExStyle = cp.ExStyle | wsExLayoutrtl | wsExNoinheritlayout;
                }
                return cp;
            }
        }

        private new Rectangle GetTabRect(int index) {
            if (index < 0)
                return new Rectangle();
            Rectangle baseRect = base.GetTabRect(index);
            return baseRect;
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();
            OnFontChanged(EventArgs.Empty);
            FindUpDown();
        }

        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);
            FindUpDown();
            UpdateUpDown();
        }

        protected override void OnControlRemoved(ControlEventArgs e) {
            base.OnControlRemoved(e);
            FindUpDown();
            UpdateUpDown();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WmSetfont = 0x30;
        private const int WmFontchange = 0x1d;

        [SecuritySafeCritical]
        protected override void OnFontChanged(EventArgs e) {
            base.OnFontChanged(e);
            IntPtr hFont = FontManager.GetTabControlFont(Function).ToHfont();
            SendMessage(Handle, WmSetfont, hFont, (IntPtr)(-1));
            SendMessage(Handle, WmFontchange, IntPtr.Zero, IntPtr.Zero);
            UpdateStyles();
        }
        #endregion

        #region Helper Methods
        private Rectangle GetRektOf(int index) {
            return _getRekt.ContainsKey(index) ? _getRekt[index] : new Rectangle();
        }

        [SecuritySafeCritical]
        private void FindUpDown() {
            if (!DesignMode) {
                bool bFound = false;

                IntPtr pWnd = WinApi.GetWindow(Handle, WinApi.GW_CHILD);

                while (pWnd != IntPtr.Zero) {
                    char[] className = new char[33];

                    int length = WinApi.GetClassName(pWnd, className, 32);

                    string s = new string(className, 0, length);

                    if (s == "msctls_updown32") {
                        bFound = true;

                        if (!_bUpDown) {
                            _scUpDown = new SubClass(pWnd, true);
                            _scUpDown.SubClassedWndProc += scUpDown_SubClassedWndProc;

                            _bUpDown = true;
                        }
                        break;
                    }

                    pWnd = WinApi.GetWindow(pWnd, WinApi.GW_HWNDNEXT);
                }

                if ((!bFound) && (_bUpDown))
                    _bUpDown = false;
            }
        }

        [SecuritySafeCritical]
        private void UpdateUpDown() {
            if (_bUpDown && !DesignMode) {
                if (WinApi.IsWindowVisible(_scUpDown.Handle)) {
                    Rectangle rect = new Rectangle();
                    WinApi.GetClientRect(_scUpDown.Handle, ref rect);
                    WinApi.InvalidateRect(_scUpDown.Handle, ref rect, true);
                }
            }
        }

        [SecuritySafeCritical]
        private int scUpDown_SubClassedWndProc(ref Message m) {
            switch (m.Msg) {
                case (int)WinApi.Messages.WM_PAINT:
                    IntPtr hDc = WinApi.GetWindowDC(_scUpDown.Handle);
                    Graphics g = Graphics.FromHdc(hDc);
                    DrawUpDown(g);
                    g.Dispose();
                    WinApi.ReleaseDC(_scUpDown.Handle, hDc);
                    m.Result = IntPtr.Zero;
                    Rectangle rect = new Rectangle();
                    WinApi.GetClientRect(_scUpDown.Handle, ref rect);
                    WinApi.ValidateRect(_scUpDown.Handle, ref rect);
                    return 1;
            }

            return 0;
        }

        #endregion

    }

    #region YamuiTabControlDesigner
    internal class YamuiTabControlDesigner : ParentControlDesigner {
        #region Fields

        private readonly DesignerVerbCollection _designerVerbs = new DesignerVerbCollection();

        private IDesignerHost _designerHost;

        private ISelectionService _selectionService;

        public override SelectionRules SelectionRules {
            get { return Control.Dock == DockStyle.Fill ? SelectionRules.Visible : base.SelectionRules; }
        }

        public override DesignerVerbCollection Verbs {
            get {
                if (_designerVerbs.Count == 2) {
                    var myControl = (YamuiTabControl) Control;
                    _designerVerbs[1].Enabled = myControl.TabCount != 0;
                }
                return _designerVerbs;
            }
        }

        public IDesignerHost DesignerHost {
            get { return _designerHost ?? (_designerHost = (IDesignerHost) (GetService(typeof (IDesignerHost)))); }
        }

        public ISelectionService SelectionService {
            get { return _selectionService ?? (_selectionService = (ISelectionService) (GetService(typeof (ISelectionService)))); }
        }

        #endregion

        #region Constructor

        public YamuiTabControlDesigner() {
            var verb1 = new DesignerVerb("Add Tab", OnAddPage);
            var verb2 = new DesignerVerb("Remove Tab", OnRemovePage);
            _designerVerbs.AddRange(new[] {verb1, verb2});
        }

        #endregion

        #region Private Methods

        private void OnAddPage(Object sender, EventArgs e) {
            var parentControl = (YamuiTabControl) Control;
            var oldTabs = parentControl.Controls;

            RaiseComponentChanging(TypeDescriptor.GetProperties(parentControl)["TabPages"]);

            var p = (YamuiTabPage) (DesignerHost.CreateComponent(typeof (YamuiTabPage)));
            p.Text = p.Name;
            parentControl.TabPages.Add(p);

            RaiseComponentChanged(TypeDescriptor.GetProperties(parentControl)["TabPages"],
                oldTabs, parentControl.TabPages);
            parentControl.SelectedTab = p;

            SetVerbs();
        }

        private void OnRemovePage(Object sender, EventArgs e) {
            var parentControl = (YamuiTabControl) Control;
            var oldTabs = parentControl.Controls;

            if (parentControl.SelectedIndex < 0) {
                return;
            }

            RaiseComponentChanging(TypeDescriptor.GetProperties(parentControl)["TabPages"]);

            DesignerHost.DestroyComponent(parentControl.TabPages[parentControl.SelectedIndex]);

            RaiseComponentChanged(TypeDescriptor.GetProperties(parentControl)["TabPages"],
                oldTabs, parentControl.TabPages);

            SelectionService.SetSelectedComponents(new IComponent[] {
                parentControl
            }, SelectionTypes.Auto);

            SetVerbs();
        }

        private void SetVerbs() {
            var parentControl = (YamuiTabControl) Control;

            switch (parentControl.TabPages.Count) {
                case 0:
                    Verbs[1].Enabled = false;
                    break;
                default:
                    Verbs[1].Enabled = true;
                    break;
            }
        }

        #endregion

        #region Overrides
        
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            switch (m.Msg) {
                case (int) WinApi.Messages.WM_NCHITTEST:
                    if (m.Result.ToInt32() == (int) WinApi.HitTest.HTTRANSPARENT) {
                        m.Result = (IntPtr) WinApi.HitTest.HTCLIENT;
                    }
                    break;
            }
        }
        
        protected override bool GetHitTest(Point point) {
            if (SelectionService.PrimarySelection == Control) {
                var hti = new WinApi.TCHITTESTINFO {
                    pt = Control.PointToClient(point),
                    flags = 0
                };

                var m = new Message {
                    HWnd = Control.Handle,
                    Msg = WinApi.TCM_HITTEST
                };

                var lparam =
                    Marshal.AllocHGlobal(Marshal.SizeOf(hti));
                Marshal.StructureToPtr(hti,
                    lparam, false);
                m.LParam = lparam;

                base.WndProc(ref m);
                Marshal.FreeHGlobal(lparam);

                if (m.Result.ToInt32() != -1) {
                    return hti.flags != (int) WinApi.TabControlHitTest.TCHT_NOWHERE;
                }
            }

            return false;
        }
        
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");

            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");

            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("UseVisualStyleBackColor");

            //properties.Remove("ItemSize");
            properties.Remove("Font");
            properties.Remove("RightToLeft");

            base.PreFilterProperties(properties);
        }

        #endregion
    }

    #endregion

    #region YamuiTabPageCollectionEditor

    internal class YamuiTabPageCollectionEditor : CollectionEditor {
        protected override CollectionForm CreateCollectionForm() {
            var baseForm = base.CreateCollectionForm();
            baseForm.Text = "YamuiTabPage Collection Editor";
            return baseForm;
        }

        public YamuiTabPageCollectionEditor(Type type)
            : base(type) {}

        protected override Type CreateCollectionItemType() {
            return typeof (YamuiTabPage);
        }

        protected override Type[] CreateNewItemTypes() {
            return new[] {typeof (YamuiTabPage)};
        }
    }

    #endregion

}
