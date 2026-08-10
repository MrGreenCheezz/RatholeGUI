using System.ComponentModel;

namespace PortsAppGui.UI
{
    /// <summary>Rounded surface with an optional coloured status stripe on the left edge.</summary>
    public class CardPanel : Panel
    {
        public CardPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Card;
            ForeColor = Theme.Text;
            Font = Theme.Body;
        }

        [DefaultValue(8)]
        public int CornerRadius { get; set; } = 8;

        public Color FillColor { get; set; } = Theme.Card;

        public Color BorderColor { get; set; } = Theme.Border;

        /// <summary>Width of the left stripe; 0 hides it.</summary>
        [DefaultValue(0)]
        public int StripeWidth { get; set; }

        public Color StripeColor { get; set; } = Theme.Accent;

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            Theme.ClearWithParentBackground(this, graphics);

            var bounds = new RectangleF(0, 0, Width, Height);
            Theme.FillRounded(graphics, bounds, CornerRadius, FillColor, BorderColor);

            if (StripeWidth > 0)
            {
                // Clip to the card outline so the stripe keeps the rounded left corners.
                using var outline = Theme.RoundedRect(bounds, CornerRadius);
                var previousClip = graphics.Clip;
                graphics.SetClip(outline);
                using (var brush = new SolidBrush(StripeColor))
                    graphics.FillRectangle(brush, 0, 0, StripeWidth, Height);
                graphics.Clip = previousClip;
            }

            base.OnPaint(e);
        }
    }
}
