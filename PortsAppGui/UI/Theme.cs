using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace PortsAppGui.UI
{
    /// <summary>
    /// Dark palette, fonts and the few native calls that make WinForms look like a modern app.
    /// </summary>
    public static class Theme
    {
        // Surfaces
        public static readonly Color Window = Color.FromArgb(0x16, 0x16, 0x1A);
        public static readonly Color Surface = Color.FromArgb(0x1D, 0x1D, 0x23);
        public static readonly Color Card = Color.FromArgb(0x23, 0x23, 0x2A);
        public static readonly Color CardHover = Color.FromArgb(0x29, 0x29, 0x31);
        public static readonly Color Input = Color.FromArgb(0x18, 0x18, 0x1D);
        // Kept deliberately light against Card/Input: at lower contrast the panel and field edges
        // are practically invisible on a dark background.
        public static readonly Color Border = Color.FromArgb(0x45, 0x45, 0x52);
        public static readonly Color BorderStrong = Color.FromArgb(0x5E, 0x5E, 0x70);

        // Text
        public static readonly Color Text = Color.FromArgb(0xEC, 0xEC, 0xF1);
        public static readonly Color TextMuted = Color.FromArgb(0x99, 0x99, 0xA6);
        public static readonly Color TextDisabled = Color.FromArgb(0x5E, 0x5E, 0x6B);

        // Accents
        public static readonly Color Accent = Color.FromArgb(0x4C, 0x8D, 0xFF);
        public static readonly Color AccentHover = Color.FromArgb(0x62, 0x9B, 0xFF);
        public static readonly Color AccentPressed = Color.FromArgb(0x3B, 0x77, 0xE0);
        public static readonly Color Success = Color.FromArgb(0x3F, 0xC1, 0x67);
        public static readonly Color Danger = Color.FromArgb(0xE5, 0x5B, 0x52);
        public static readonly Color DangerHover = Color.FromArgb(0xF0, 0x6B, 0x62);
        public static readonly Color DangerPressed = Color.FromArgb(0xC7, 0x48, 0x40);
        public static readonly Color Warning = Color.FromArgb(0xE0, 0xA5, 0x3B);

        // Fonts
        public static readonly Font Body = new("Segoe UI", 9F);
        public static readonly Font BodySemibold = new("Segoe UI Semibold", 9F);
        public static readonly Font Caption = new("Segoe UI", 8.25F);
        public static readonly Font Title = new("Segoe UI Semibold", 13.5F);
        public static readonly Font Section = new("Segoe UI Semibold", 10.5F);

        private static Font? _iconFont;
        private static bool _iconFontResolved;

        /// <summary>Segoe MDL2 glyph font, or null when the family is unavailable.</summary>
        public static Font? IconFont
        {
            get
            {
                if (_iconFontResolved)
                    return _iconFont;

                _iconFontResolved = true;
                foreach (var family in new[] { "Segoe Fluent Icons", "Segoe MDL2 Assets" })
                {
                    if (!IsFontInstalled(family))
                        continue;

                    _iconFont = new Font(family, 10F);
                    break;
                }

                return _iconFont;
            }
        }

        private static bool IsFontInstalled(string family)
        {
            try
            {
                using var fontFamily = new FontFamily(family);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static string PickMonoFontFamily()
        {
            foreach (var family in new[] { "Cascadia Mono", "Consolas" })
            {
                if (IsFontInstalled(family))
                    return family;
            }

            return FontFamily.GenericMonospace.Name;
        }

        private static Icon? _appIcon;

        /// <summary>Cached: every dialog asks for it, and Form does not dispose an assigned icon.</summary>
        public static Icon LoadAppIcon() => _appIcon ??= ReadAppIcon();

        private static Icon ReadAppIcon()
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "cheese-32.ico");
            try
            {
                if (File.Exists(iconPath))
                    return new Icon(iconPath);
            }
            catch
            {
                // fall through to the executable icon
            }

            try
            {
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        public static GraphicsPath RoundedRect(RectangleF bounds, float radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            var arc = new RectangleF(bounds.X, bounds.Y, diameter, diameter);

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Fills a rounded rect and optionally strokes a 1px border inside the same bounds.</summary>
        public static void FillRounded(Graphics graphics, RectangleF bounds, float radius, Color fill, Color? border = null)
        {
            var previousMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = RoundedRect(bounds, radius))
            using (var brush = new SolidBrush(fill))
                graphics.FillPath(brush, path);

            if (border.HasValue)
            {
                var strokeBounds = new RectangleF(bounds.X + 0.5f, bounds.Y + 0.5f, bounds.Width - 1, bounds.Height - 1);
                using var strokePath = RoundedRect(strokeBounds, radius);
                using var pen = new Pen(border.Value);
                graphics.DrawPath(pen, strokePath);
            }

            graphics.SmoothingMode = previousMode;
        }

        /// <summary>Paints the parent's background so anti-aliased corners blend instead of showing grey.</summary>
        public static void ClearWithParentBackground(Control control, Graphics graphics)
        {
            graphics.Clear(control.Parent?.BackColor ?? Window);
        }

        public static Color Mix(Color from, Color to, double amount)
        {
            amount = Math.Clamp(amount, 0, 1);
            return Color.FromArgb(
                (int)(from.R + (to.R - from.R) * amount),
                (int)(from.G + (to.G - from.G) * amount),
                (int)(from.B + (to.B - from.B) * amount));
        }

        #region Native dark mode

        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaUseImmersiveDarkModeBefore20h1 = 19;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SetPreferredAppMode(int mode);

        [DllImport("uxtheme.dll", EntryPoint = "#136", SetLastError = true)]
        private static extern void FlushMenuThemes();

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string? subAppName, string? subIdList);

        /// <summary>Asks the OS to render common controls (scrollbars, menus) in dark mode. Best effort.</summary>
        public static void EnableProcessDarkMode()
        {
            try
            {
                SetPreferredAppMode(2); // ForceDark
                FlushMenuThemes();
            }
            catch
            {
                // Undocumented API: ignore on builds where it is missing.
            }
        }

        /// <summary>Paints the native title bar dark (Windows 10 1809+).</summary>
        public static void ApplyDarkTitleBar(Form form)
        {
            try
            {
                var enabled = 1;
                if (DwmSetWindowAttribute(form.Handle, DwmwaUseImmersiveDarkMode, ref enabled, sizeof(int)) != 0)
                    DwmSetWindowAttribute(form.Handle, DwmwaUseImmersiveDarkModeBefore20h1, ref enabled, sizeof(int));
            }
            catch
            {
                // Older Windows: keep the light title bar.
            }
        }

        /// <summary>Switches a control's native scrollbars to the dark explorer theme.</summary>
        public static void ApplyDarkScrollBars(Control control)
        {
            try
            {
                if (control.IsHandleCreated)
                    SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            }
            catch
            {
                // Best effort only.
            }
        }

        #endregion

        /// <summary>Applies window chrome + dark scrollbars once the form has a handle.</summary>
        public static void ApplyTo(Form form)
        {
            form.BackColor = Window;
            form.ForeColor = Text;
            form.Font = Body;

            void Apply(object? sender, EventArgs e)
            {
                ApplyDarkTitleBar(form);
                ApplyDarkScrollBarsRecursive(form);
            }

            if (form.IsHandleCreated)
                Apply(null, EventArgs.Empty);

            form.HandleCreated += Apply;
            form.Shown += Apply;
        }

        public static void ApplyDarkScrollBarsRecursive(Control root)
        {
            ApplyDarkScrollBars(root);
            foreach (Control child in root.Controls)
                ApplyDarkScrollBarsRecursive(child);
        }

        public static readonly TextRenderingHint TextHint = TextRenderingHint.ClearTypeGridFit;
    }
}
