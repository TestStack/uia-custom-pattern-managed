using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using Interop.UIAutomationCore;


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
                return this.value;
            }
            set
            {
                if (this.value != value)
                {
                    TriColorValue oldValue = this.value;
                    this.value = value;
                    this.Invalidate();

                    this.Provider.RaiseEventsForNewValue(oldValue, value);
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
            bool found = false;
            if (this.RedRect.Contains(ptClient))
            {
                value = TriColorValue.Red;
                found = true;
            }
            else if (this.YellowRect.Contains(ptClient))
            {
                value = TriColorValue.Yellow;
                found = true;
            }
            else if (this.GreenRect.Contains(ptClient))
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
                case TriColorValue.Red: return this.RedRect;
                case TriColorValue.Yellow: return this.YellowRect;
                case TriColorValue.Green: return this.GreenRect;
            }
            return Rectangle.Empty;
        }

        #endregion

        #region Message handlers

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Bars            
            e.Graphics.FillRectangle(Brushes.Red, this.RedRect);
            e.Graphics.FillRectangle(Brushes.Yellow, this.YellowRect);
            e.Graphics.FillRectangle(Brushes.Green, this.GreenRect);

            // Labels
            e.Graphics.DrawString("Red", this.Font, Brushes.White, 
                RectFFromRect(Rectangle.Inflate(this.RedRect, -4, -4)));
            e.Graphics.DrawString("Yellow", this.Font, Brushes.Black,
                RectFFromRect(Rectangle.Inflate(this.YellowRect, -4, -4)));
            e.Graphics.DrawString("Green", this.Font, Brushes.White,
                RectFFromRect(Rectangle.Inflate(this.GreenRect, -4, -4)));

            // Selection: draw a small circle in the selected value area
            Brush contentBrush = this.value == TriColorValue.Yellow ? Brushes.Black : Brushes.White;
            Rectangle selRect = RectForValue(this.value);
            Point selCenterPoint = new Point(selRect.Left + selRect.Width / 2,
                                             selRect.Top + selRect.Height / 2);
            Rectangle rectMarker = new Rectangle(selCenterPoint, new Size(0, 0));
            rectMarker.Inflate(10, 10);
            e.Graphics.FillEllipse(contentBrush, rectMarker);

            // Border
            Rectangle border = this.ClientRectangle;
            border.Height -= 1;
            border.Width -= 1;
            e.Graphics.DrawRectangle(Pens.Black, border);

            // Focus rect
            if (this.Focused)
            {
                Rectangle focusRect = this.ClientRectangle;
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
                this.Value = newValue;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            TriColorValue newValue = this.value;
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
            this.Value = newValue;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            this.Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            // Handle WM_GETOBJECT
            if (m.Msg == 0x3D /* WM_GETOBJECT */)
            {
                m.Result = NativeMethods.UiaReturnRawElementProvider(
                    m.HWnd, m.WParam, m.LParam, this.Provider);
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
                case TriColorValue.Red: rect = this.RedRect; break;
                case TriColorValue.Yellow: rect = this.YellowRect; break;
                case TriColorValue.Green: rect = this.GreenRect; break;
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
                    this.Width,
                    this.Height / 3);
            }
        }

        private Rectangle YellowRect
        {
            get
            {
                return new Rectangle(
                    0,
                    this.RedRect.Bottom,
                    this.Width,
                    this.Height / 3);
            }
        }

        private Rectangle GreenRect
        {
            get
            {
                return new Rectangle(
                    0,
                    this.YellowRect.Bottom,
                    this.Width,
                    this.Height - this.YellowRect.Bottom);
            }
        }

        private TriColorProvider Provider
        {
            get
            {
                if (this.provider == null)
                {
                    this.provider = new TriColorProvider(this);
                }
                return this.provider;
            }
        }

        #endregion
        
        #region Private fields

        private TriColorValue value;
        private TriColorProvider provider;

        #endregion
    }
}
