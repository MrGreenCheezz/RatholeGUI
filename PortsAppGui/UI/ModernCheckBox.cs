using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PortsAppGui.UI
{
    /// <summary>Owner-drawn checkbox: rounded box, accent fill, glyph check mark.</summary>
    public class ModernCheckBox : CheckBox
    {
        private const int BoxSize = 18;
        private const int Gap = 8;
        private bool _hovered;

        public ModernCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            BackColor = Color.Transparent;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            Height = 24;
            Cursor = Cursors.Hand;
        }

        [DefaultValue(true)]
        public bool AutoWidth { get; set; } = true;

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            ApplyAutoWidth();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ApplyAutoWidth();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyAutoWidth();
        }

        private void ApplyAutoWidth()
        {
            if (!AutoWidth)
                return;

            var textSize = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            Width = BoxSize + Gap + textSize.Width + 2;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            Invalidate();
            base.OnCheckedChanged(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var graphics = pevent.Graphics;
            graphics.TextRenderingHint = Theme.TextHint;
            Theme.ClearWithParentBackground(this, graphics);

            var top = (Height - BoxSize) / 2f;
            var box = new RectangleF(0, top, BoxSize, BoxSize);

            var fill = !Enabled
                ? Theme.Surface
                : Checked
                    ? (_hovered ? Theme.AccentHover : Theme.Accent)
                    : (_hovered ? Theme.CardHover : Theme.Input);

            var border = !Enabled
                ? Theme.Border
                : Checked
                    ? Color.Transparent
                    : (_hovered ? Theme.BorderStrong : Theme.Border);

            Theme.FillRounded(graphics, box, 4, fill, Checked && Enabled ? null : border);

            if (Checked)
                DrawCheckMark(graphics, box, Enabled ? Color.White : Theme.TextDisabled);

            var textColor = Enabled ? ForeColor : Theme.TextDisabled;
            var textRect = new Rectangle(BoxSize + Gap, 0, Width - BoxSize - Gap, Height);
            TextRenderer.DrawText(graphics, Text, Font, textRect, textColor,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            if (Focused)
            {
                using var pen = new Pen(Theme.AccentHover) { DashStyle = DashStyle.Dot };
                graphics.DrawRectangle(pen, textRect.X - 2, 1, textRect.Width, Height - 3);
            }
        }

        private static void DrawCheckMark(Graphics graphics, RectangleF box, Color color)
        {
            var previous = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(color, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            var points = new[]
            {
                new PointF(box.X + box.Width * 0.26f, box.Y + box.Height * 0.52f),
                new PointF(box.X + box.Width * 0.44f, box.Y + box.Height * 0.70f),
                new PointF(box.X + box.Width * 0.76f, box.Y + box.Height * 0.32f)
            };
            graphics.DrawLines(pen, points);

            graphics.SmoothingMode = previous;
        }
    }
}
