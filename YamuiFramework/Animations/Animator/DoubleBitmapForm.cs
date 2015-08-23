using System;
using System.Drawing;
using System.Windows.Forms;

namespace YamuiFramework.Animations.Animator
{
    public partial class DoubleBitmapForm : Form, IFakeControl
    {
        Bitmap bgBmp;
        Bitmap frame;

        public event EventHandler<TransfromNeededEventArg> TransfromNeeded;

        public DoubleBitmapForm()
        {
            InitializeComponent();
            Visible = false;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            //ShowInTaskbar = false;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                unchecked
                {
                    cp.Style = (int)Flags.WindowStyles.WS_POPUP;
                }
                ;// (int)Flags.WindowStyles.WS_CHILD;
                cp.ExStyle |= (int)Flags.WindowStyles.WS_EX_NOACTIVATE | (int)Flags.WindowStyles.WS_EX_TOOLWINDOW;
                cp.X = Location.X;
                cp.Y = Location.Y;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var gr = e.Graphics;

            OnFramePainting(e);

            try
            {
                gr.DrawImage(bgBmp, -Location.X, -Location.Y);
                /*
                if (frame == null)
                {
                    control.Focus();
                    if (control.Focused)
                    {
                        frame = new Bitmap(control.Width, control.Height);
                        //control.DrawToBitmap(frame, new Rectangle(padding.Left, padding.Top, control.Width, control.Height));
                        control.DrawToBitmap(frame, new Rectangle(0, 0, control.Width, control.Height));
                    }
                }*/

                if (frame != null)
                {
                    //var ea = new TransfromNeededEventArg(){ ClientRectangle = new Rectangle(0, 0, this.Width, this.Height) };
                    var ea = new TransfromNeededEventArg();
                    ea.ClientRectangle = ea.ClipRectangle = new Rectangle(control.Bounds.Left - padding.Left, control.Bounds.Top - padding.Top, control.Bounds.Width + padding.Horizontal, control.Bounds.Height + padding.Vertical);
                    OnTransfromNeeded(ea);
                    gr.SetClip(ea.ClipRectangle);
                    gr.Transform = ea.Matrix;
                    //var p = new Point();
                    var p = control.Location;
                    //gr.Transform.Translate(p.X, p.Y);
                    gr.DrawImage(frame, p.X - padding.Left, p.Y - padding.Top);
                }

                OnFramePainted(e);
            }
            catch { }

            //e.Graphics.DrawLine(Pens.Red, Point.Empty, new Point(Width, Height));
        }

        private void OnTransfromNeeded(TransfromNeededEventArg ea)
        {
            if (TransfromNeeded != null)
                TransfromNeeded(this, ea);
        }

        protected virtual void OnFramePainting(PaintEventArgs e)
        {
            if (FramePainting != null)
                FramePainting(this, e);
        }


        protected virtual void OnFramePainted(PaintEventArgs e)
        {
            if (FramePainted != null)
                FramePainted(this, e);
        }

        Padding padding;
        Control control;

        public void InitParent(Control control, Padding padding)
        {
            //Size = new Size(control.Size.Width + padding.Left + padding.Right, control.Size.Height + padding.Top + padding.Bottom);
            //var p = control.Parent == null ? control.Location : control.Parent.PointToScreen(control.Location);
            //Location = new Point(p.X - padding.Left, p.Y - padding.Top);

            this.control = control;
            /*
            if (padding.Left < 10) padding.Left = 15;
            if (padding.Right < 10) padding.Right = 15;
            if (padding.Top < 10) padding.Top = 15;
            if (padding.Bottom < 10) padding.Bottom = 15;*/

            Location = new Point(0, 0);
            Size = Screen.PrimaryScreen.Bounds.Size;
            control.VisibleChanged += control_VisibleChanged;
            this.padding = padding;
        }

        Point controlLocation;

        void control_VisibleChanged(object sender, EventArgs e)
        {
            controlLocation = (sender as Control).Location;
            var s = (sender as Control).Size;

            //this.Location = new Point(p.X - padding.Left, p.Y - padding.Top);
            //this.Location = Point.Empty;
            //this.Size = new Size(s.Width + padding.Left + padding.Right, s.Height + padding.Top + padding.Bottom);
        }

        public Bitmap BgBmp
        {
            get
            {
                return bgBmp;
            }
            set
            {
                bgBmp = value;
            }
        }

        public Bitmap Frame
        {
            get
            {
                return frame;
            }
            set
            {
                frame = value;
            }
        }

        public event EventHandler<PaintEventArgs> FramePainting;

        public event EventHandler<PaintEventArgs> FramePainted;
    }
}
