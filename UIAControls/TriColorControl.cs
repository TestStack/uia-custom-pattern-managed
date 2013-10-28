using System;
using System.Drawing;
using System.Windows.Forms;


namespace UIAControls
{
    /// <summary>
    /// A simple color picking control.
    /// 
    /// This isn't a complete custom control implementation,
    /// but it is good enough for our purposes.
    /// </summary>
    public partial class TriColorControl : UserControl
    {
        #region Public methods

        public TriColorControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Retrieve the controls's current color value.
        /// </summary>
        public TriColorValue Value
        {
            get
            {
                return value;
            }
            set
            {
                if (this.value != value)
                {
                    var oldValue = this.value;
                    this.value = value;
                    Invalidate();

                    Provider.RaiseEventsForNewValue(oldValue, value);
                }
            }
        }

        /// <summary>
        /// Find the value for a given point on the control
        /// </summary>
        /// <param name="ptClient">Point in client coordinates</param>
        /// <param name="value">Value for point</param>
        /// <returns>true if value was found</returns>
        public bool ValueFromPoint(Point ptClient, out TriColorValue value)
        {
            value = TriColorValue.Red;
            var found = false;
            if (RedRect.Contains(ptClient))
            {
                value = TriColorValue.Red;
                found = true;
            }
            else if (YellowRect.Contains(ptClient))
            {
                value = TriColorValue.Yellow;
                found = true;
            }
            else if (GreenRect.Contains(ptClient))
            {
                value = TriColorValue.Green;
                found = true;
            }
            return found;
        }

        /// <summary>
        /// Find the rectangle (in client coordinates) for a given value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Rectangle RectFromValue(TriColorValue value)
        {
            switch (value)
            {
                case TriColorValue.Red: return RedRect;
                case TriColorValue.Yellow: return YellowRect;
                case TriColorValue.Green: return GreenRect;
            }
            return Rectangle.Empty;
        }

        #endregion

        #region Message handlers

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Bars            
            e.Graphics.FillRectangle(Brushes.Red, RedRect);
            e.Graphics.FillRectangle(Brushes.Yellow, YellowRect);
            e.Graphics.FillRectangle(Brushes.Green, GreenRect);

            // Labels
            e.Graphics.DrawString("Red", Font, Brushes.White, 
                RectFFromRect(Rectangle.Inflate(RedRect, -4, -4)));
            e.Graphics.DrawString("Yellow", Font, Brushes.Black,
                RectFFromRect(Rectangle.Inflate(YellowRect, -4, -4)));
            e.Graphics.DrawString("Green", Font, Brushes.White,
                RectFFromRect(Rectangle.Inflate(GreenRect, -4, -4)));

            // Selection: draw a small circle in the selected value area
            var contentBrush = value == TriColorValue.Yellow ? Brushes.Black : Brushes.White;
            var selRect = RectForValue(value);
            var selCenterPoint = new Point(selRect.Left + selRect.Width / 2,
                                             selRect.Top + selRect.Height / 2);
            var rectMarker = new Rectangle(selCenterPoint, new Size(0, 0));
            rectMarker.Inflate(10, 10);
            e.Graphics.FillEllipse(contentBrush, rectMarker);

            // Border
            var border = ClientRectangle;
            border.Height -= 1;
            border.Width -= 1;
            e.Graphics.DrawRectangle(Pens.Black, border);

            // Focus rect
            if (Focused)
            {
                var focusRect = ClientRectangle;
                focusRect.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(e.Graphics, focusRect);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            TriColorValue newValue; ;
            if (ValueFromPoint(e.Location, out newValue))
            {
                Value = newValue;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            var newValue = value;
            if (!(e.Alt || e.Control || e.Shift))
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (!TriColorValueHelper.IsFirst(newValue))
                    {
                        newValue = TriColorValueHelper.PreviousValue(newValue);
                        e.Handled = true;
                    }
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (!TriColorValueHelper.IsLast(newValue))
                    {
                        newValue = TriColorValueHelper.NextValue(newValue);
                        e.Handled = true;                      
                    }
                }
            }
            Value = newValue;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            // Handle WM_GETOBJECT
            if (m.Msg == 0x3D /* WM_GETOBJECT */)
            {
                m.Result = NativeMethods.UiaReturnRawElementProvider(
                    m.HWnd, m.WParam, m.LParam, Provider);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        #endregion

        #region Helper methods

        private RectangleF RectFFromRect(Rectangle rect)
        {           
            return new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Find the rectangle corresponding to a given value
        /// </summary>
        private Rectangle RectForValue(TriColorValue value)
        {
            Rectangle rect;
            switch (value)
            {
                case TriColorValue.Red: rect = RedRect; break;
                case TriColorValue.Yellow: rect = YellowRect; break;
                case TriColorValue.Green: rect = GreenRect; break;
                default: rect = new Rectangle(); break;
            }
            return rect;
        }

        private Rectangle RedRect
        {
            get
            {
                return new Rectangle(
                    0,
                    0,
                    Width,
                    Height / 3);
            }
        }

        private Rectangle YellowRect
        {
            get
            {
                return new Rectangle(
                    0,
                    RedRect.Bottom,
                    Width,
                    Height / 3);
            }
        }

        private Rectangle GreenRect
        {
            get
            {
                return new Rectangle(
                    0,
                    YellowRect.Bottom,
                    Width,
                    Height - YellowRect.Bottom);
            }
        }

        private TriColorProvider Provider
        {
            get
            {
                if (provider == null)
                {
                    provider = new TriColorProvider(this);
                }
                return provider;
            }
        }

        #endregion
        
        #region Private fields

        private TriColorValue value;
        private TriColorProvider provider;

        #endregion
    }
}
