using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PortsAppGui.UI
{
    public enum StatusKind
    {
        Unknown,
        Running,
        Stopped,
        Error,
        Busy
    }

    /// <summary>Rounded badge with a coloured dot: the app's headline state indicator.</summary>
    public class StatusPill : Control
    {
        private StatusKind _kind = StatusKind.Unknown;

        public StatusPill()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            ForeColor = Theme.Text;
            Font = Theme.BodySemibold;
            Size = new Size(180, 30);
        }

        [DefaultValue(StatusKind.Unknown)]
        public StatusKind Kind
        {
            get => _kind;
            set
            {
                if (_kind == value)
                    return;

                _kind = value;
                Invalidate();
            }
        }

        [DefaultValue(true)]
        public bool AutoWidth { get; set; } = true;

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            ApplyAutoWidth();
            Invalidate();
        }

        private void ApplyAutoWidth()
        {
            if (!AutoWidth)
                return;

            var textSize = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            Width = 14 + 8 + 8 + textSize.Width + 14;
        }

        public Color DotColor => _kind switch
        {
            StatusKind.Running => Theme.Success,
            StatusKind.Stopped => Theme.TextMuted,
            StatusKind.Error => Theme.Danger,
            StatusKind.Busy => Theme.Warning,
            _ => Theme.TextMuted
        };

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.TextRenderingHint = Theme.TextHint;
            Theme.ClearWithParentBackground(this, graphics);

            var dot = DotColor;
            var bounds = new RectangleF(0, 0, Width, Height);
            Theme.FillRounded(graphics, bounds, Height / 2f, Theme.Mix(Theme.Surface, dot, 0.14), Theme.Mix(Theme.Border, dot, 0.35));

            var previous = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            const int dotSize = 8;
            var dotRect = new RectangleF(14, (Height - dotSize) / 2f, dotSize, dotSize);
            using (var glow = new SolidBrush(Color.FromArgb(70, dot)))
                graphics.FillEllipse(glow, RectangleF.Inflate(dotRect, 3.5f, 3.5f));
            using (var brush = new SolidBrush(dot))
                graphics.FillEllipse(brush, dotRect);

            graphics.SmoothingMode = previous;

            var textRect = new Rectangle(14 + dotSize + 8, 0, Width - (14 + dotSize + 8) - 10, Height);
            TextRenderer.DrawText(graphics, Text, Font, textRect, ForeColor,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}
