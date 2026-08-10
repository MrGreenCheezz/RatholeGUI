using System.ComponentModel;

namespace PortsAppGui.UI
{
    public enum ButtonVariant
    {
        Standard,
        Accent,
        Danger,
        Ghost
    }

    /// <summary>Flat, rounded, owner-drawn button with an optional Segoe icon glyph.</summary>
    public class ModernButton : Button
    {
        private bool _hovered;
        private bool _pressed;

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            Size = new Size(120, 34);
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
        }

        [DefaultValue(ButtonVariant.Standard)]
        public ButtonVariant Variant { get; set; } = ButtonVariant.Standard;

        /// <summary>Glyph from the icon font, e.g. "". Ignored when the font is missing.</summary>
        [DefaultValue("")]
        public string Glyph { get; set; } = "";

        [DefaultValue(6)]
        public int CornerRadius { get; set; } = 6;

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _pressed = mevent.Button == MouseButtons.Left;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var graphics = pevent.Graphics;
            graphics.TextRenderingHint = Theme.TextHint;
            Theme.ClearWithParentBackground(this, graphics);

            var (fill, border, foreground) = ResolveColors();
            var bounds = new RectangleF(0, 0, Width, Height);

            if (fill.A > 0 || border.HasValue)
                Theme.FillRounded(graphics, bounds, CornerRadius, fill, border);

            if (Focused && Enabled)
            {
                var ring = RectangleF.Inflate(bounds, -1.5f, -1.5f);
                using var path = Theme.RoundedRect(ring, Math.Max(1, CornerRadius - 1));
                using var pen = new Pen(Theme.AccentHover);
                var previous = graphics.SmoothingMode;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.DrawPath(pen, path);
                graphics.SmoothingMode = previous;
            }

            DrawContent(graphics, foreground);
        }

        private (Color fill, Color? border, Color foreground) ResolveColors()
        {
            if (!Enabled)
            {
                return Variant == ButtonVariant.Ghost
                    ? (Color.Transparent, (Color?)null, Theme.TextDisabled)
                    : (Theme.Mix(Theme.Surface, Theme.Card, 0.4), Theme.Border, Theme.TextDisabled);
            }

            return Variant switch
            {
                ButtonVariant.Accent => (
                    _pressed ? Theme.AccentPressed : _hovered ? Theme.AccentHover : Theme.Accent,
                    null,
                    Color.White),

                ButtonVariant.Danger => (
                    _pressed ? Theme.DangerPressed : _hovered ? Theme.DangerHover : Theme.Danger,
                    null,
                    Color.White),

                // Keeps a resting outline: without it a ghost button reads as plain text on dark.
                ButtonVariant.Ghost => (
                    _pressed ? Theme.Card : _hovered ? Theme.CardHover : Color.Transparent,
                    _hovered || _pressed ? Theme.BorderStrong : Theme.Border,
                    _hovered || _pressed ? Theme.Text : Theme.TextMuted),

                _ => (
                    _pressed ? Theme.Surface : _hovered ? Theme.CardHover : Theme.Card,
                    _hovered ? Theme.BorderStrong : Theme.Border,
                    Theme.Text)
            };
        }

        private void DrawContent(Graphics graphics, Color foreground)
        {
            var iconFont = Theme.IconFont;
            var hasGlyph = !string.IsNullOrEmpty(Glyph) && iconFont != null;
            var hasText = !string.IsNullOrEmpty(Text);

            const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine |
                                          TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;

            // Only the widths are measured: the draw rectangles span the full control height so
            // vertical centring can never clip a glyph (MeasureText under-reports height here).
            var unbounded = new Size(int.MaxValue, int.MaxValue);
            var glyphWidth = hasGlyph ? TextRenderer.MeasureText(graphics, Glyph, iconFont!, unbounded, flags).Width : 0;
            var textWidth = hasText ? TextRenderer.MeasureText(graphics, Text, Font, unbounded, flags).Width : 0;
            var gap = hasGlyph && hasText ? 8 : 0;
            var totalWidth = glyphWidth + gap + textWidth;

            var x = TextAlign == ContentAlignment.MiddleLeft
                ? Padding.Left + 4
                : (Width - totalWidth) / 2;

            if (hasGlyph)
            {
                TextRenderer.DrawText(graphics, Glyph, iconFont!, new Rectangle(x, 0, glyphWidth, Height), foreground, flags);
                x += glyphWidth + gap;
            }

            if (hasText)
                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x, 0, textWidth, Height), foreground, flags);
        }
    }
}
