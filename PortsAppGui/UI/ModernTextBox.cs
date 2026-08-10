using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace PortsAppGui.UI
{
    /// <summary>Borderless TextBox wrapped in an owner-drawn rounded frame with focus and error states.</summary>
    public class ModernTextBox : Control
    {
        private readonly TextBox _inner;
        private bool _hovered;
        private bool _hasError;

        public ModernTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                Font = Theme.Body
            };
            _inner.TextChanged += (_, _) => OnTextChanged(EventArgs.Empty);
            _inner.GotFocus += (_, _) => Invalidate();
            _inner.LostFocus += (_, _) => Invalidate();
            _inner.MouseEnter += (_, _) => SetHovered(true);
            _inner.MouseLeave += (_, _) => SetHovered(false);
            _inner.KeyDown += (_, e) => OnKeyDown(e);

            Controls.Add(_inner);
            BackColor = Theme.Input;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            Padding = new Padding(10, 6, 10, 6);
            Size = new Size(180, 32);
            Cursor = Cursors.IBeam;
        }

        [Browsable(false)]
        public TextBox Inner => _inner;

        [AllowNull]
        public override string Text
        {
            get => _inner.Text;
            set => _inner.Text = value ?? "";
        }

        [DefaultValue("")]
        public string PlaceholderText
        {
            get => _inner.PlaceholderText;
            set => _inner.PlaceholderText = value;
        }

        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => _inner.ReadOnly;
            set => _inner.ReadOnly = value;
        }

        [DefaultValue(false)]
        public bool UseSystemPasswordChar
        {
            get => _inner.UseSystemPasswordChar;
            set => _inner.UseSystemPasswordChar = value;
        }

        [DefaultValue(32767)]
        public int MaxLength
        {
            get => _inner.MaxLength;
            set => _inner.MaxLength = value;
        }

        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign
        {
            get => _inner.TextAlign;
            set => _inner.TextAlign = value;
        }

        [DefaultValue(6)]
        public int CornerRadius { get; set; } = 6;

        /// <summary>Draws a red frame; used to point at invalid fields.</summary>
        [DefaultValue(false)]
        public bool HasError
        {
            get => _hasError;
            set
            {
                if (_hasError == value)
                    return;

                _hasError = value;
                Invalidate();
            }
        }

        public new void Focus() => _inner.Focus();

        public void SelectAll() => _inner.SelectAll();

        protected override void OnMouseEnter(EventArgs e)
        {
            SetHovered(true);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            SetHovered(false);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _inner.Focus();
            base.OnMouseDown(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            _inner.Enabled = Enabled;
            _inner.BackColor = Enabled ? Theme.Input : Theme.Surface;
            _inner.ForeColor = Enabled ? Theme.Text : Theme.TextDisabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            _inner.Font = Font;
            LayoutInner();
            base.OnFontChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            LayoutInner();
            base.OnSizeChanged(e);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            LayoutInner();
            base.OnPaddingChanged(e);
        }

        private void LayoutInner()
        {
            var width = Math.Max(0, Width - Padding.Horizontal);
            var top = Math.Max(Padding.Top, (Height - _inner.Height) / 2);
            _inner.SetBounds(Padding.Left, top, width, _inner.Height);
        }

        private void SetHovered(bool hovered)
        {
            if (_hovered == hovered)
                return;

            _hovered = hovered;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            Theme.ClearWithParentBackground(this, graphics);

            var fill = Enabled ? Theme.Input : Theme.Surface;
            var border = !Enabled ? Theme.Border
                : _hasError ? Theme.Danger
                : _inner.Focused ? Theme.Accent
                : _hovered ? Theme.BorderStrong
                : Theme.Border;

            Theme.FillRounded(graphics, new RectangleF(0, 0, Width, Height), CornerRadius, fill, border);
        }
    }
}
