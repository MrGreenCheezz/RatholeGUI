using System.Drawing.Drawing2D;

namespace PortsAppGui.UI
{
    public enum DialogKind
    {
        Info,
        Success,
        Warning,
        Error,
        Question
    }

    /// <summary>Dark replacements for MessageBox so no dialog breaks the theme.</summary>
    public static class Dialogs
    {
        public static void Info(IWin32Window? owner, string title, string message)
            => Show(owner, DialogKind.Info, title, message, "OK", null);

        public static void Success(IWin32Window? owner, string title, string message)
            => Show(owner, DialogKind.Success, title, message, "OK", null);

        public static void Error(IWin32Window? owner, string title, string message)
            => Show(owner, DialogKind.Error, title, message, "OK", null);

        public static bool Confirm(IWin32Window? owner, string title, string message, string confirmText = "Yes", string cancelText = "No")
            => Show(owner, DialogKind.Question, title, message, confirmText, cancelText) == DialogResult.OK;

        public static bool ConfirmDanger(IWin32Window? owner, string title, string message, string confirmText = "Delete", string cancelText = "Cancel")
            => Show(owner, DialogKind.Warning, title, message, confirmText, cancelText, danger: true) == DialogResult.OK;

        private static DialogResult Show(IWin32Window? owner, DialogKind kind, string title, string message,
            string confirmText, string? cancelText, bool danger = false)
        {
            using var form = new MessageForm(kind, title, message, confirmText, cancelText, danger);
            return owner == null ? form.ShowDialog() : form.ShowDialog(owner);
        }

        private sealed class MessageForm : Form
        {
            private readonly DialogKind _kind;

            public MessageForm(DialogKind kind, string title, string message, string confirmText, string? cancelText, bool danger)
            {
                _kind = kind;

                Text = title;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                Theme.ApplyTo(this);
                BackColor = Theme.Surface;

                var accent = AccentColor;

                var iconPanel = new Panel
                {
                    Bounds = new Rectangle(24, 26, 36, 36),
                    BackColor = Color.Transparent
                };
                iconPanel.Paint += (_, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using var brush = new SolidBrush(Color.FromArgb(38, accent));
                    e.Graphics.FillEllipse(brush, 0, 0, 35, 35);
                    using var pen = new Pen(accent, 2f);
                    DrawSymbol(e.Graphics, pen, new RectangleF(0, 0, 36, 36));
                };

                var titleLabel = new Label
                {
                    Text = title,
                    Font = Theme.Section,
                    ForeColor = Theme.Text,
                    AutoSize = true,
                    MaximumSize = new Size(430, 0),
                    Location = new Point(76, 26)
                };

                var messageLabel = new Label
                {
                    Text = message,
                    Font = Theme.Body,
                    ForeColor = Theme.TextMuted,
                    AutoSize = true,
                    MaximumSize = new Size(430, 0),
                    Location = new Point(76, 26)
                };

                var scrollHost = new Panel
                {
                    AutoScroll = true,
                    BackColor = Theme.Surface,
                    Location = new Point(76, 0),
                    Width = 450
                };
                messageLabel.Location = Point.Empty;
                scrollHost.Controls.Add(messageLabel);

                Controls.Add(iconPanel);
                Controls.Add(titleLabel);
                Controls.Add(scrollHost);

                // Measure once the labels have laid themselves out.
                var titleHeight = titleLabel.PreferredSize.Height;
                var messageHeight = messageLabel.PreferredSize.Height;
                var maxMessageHeight = Math.Max(120, Screen.PrimaryScreen is { } screen ? screen.WorkingArea.Height / 2 : 320);
                var visibleMessageHeight = Math.Min(messageHeight, maxMessageHeight);

                scrollHost.Top = 26 + titleHeight + 10;
                scrollHost.Height = visibleMessageHeight + (messageHeight > maxMessageHeight ? 4 : 0);

                var buttonsTop = scrollHost.Bottom + 22;

                var confirmButton = new ModernButton
                {
                    Text = confirmText,
                    Variant = danger ? ButtonVariant.Danger : ButtonVariant.Accent,
                    Width = Math.Max(96, TextRenderer.MeasureText(confirmText, Theme.Body).Width + 34),
                    Height = 34,
                    DialogResult = DialogResult.OK
                };

                ModernButton? cancelButton = null;
                if (cancelText != null)
                {
                    cancelButton = new ModernButton
                    {
                        Text = cancelText,
                        Variant = ButtonVariant.Standard,
                        Width = Math.Max(96, TextRenderer.MeasureText(cancelText, Theme.Body).Width + 34),
                        Height = 34,
                        DialogResult = DialogResult.Cancel
                    };
                }

                var contentRight = 76 + 450;
                ClientSize = new Size(contentRight + 24, buttonsTop + 34 + 22);

                confirmButton.Location = new Point(ClientSize.Width - 24 - confirmButton.Width, buttonsTop);
                Controls.Add(confirmButton);
                if (cancelButton != null)
                {
                    cancelButton.Location = new Point(confirmButton.Left - 10 - cancelButton.Width, buttonsTop);
                    Controls.Add(cancelButton);
                }

                AcceptButton = confirmButton;
                CancelButton = (IButtonControl?)cancelButton ?? confirmButton;
                Shown += (_, _) => confirmButton.Focus();
            }

            private Color AccentColor => _kind switch
            {
                DialogKind.Success => Theme.Success,
                DialogKind.Warning => Theme.Warning,
                DialogKind.Error => Theme.Danger,
                DialogKind.Question => Theme.Accent,
                _ => Theme.Accent
            };

            private void DrawSymbol(Graphics graphics, Pen pen, RectangleF bounds)
            {
                var centerX = bounds.Width / 2f;

                switch (_kind)
                {
                    case DialogKind.Success:
                        graphics.DrawLines(pen, new[]
                        {
                            new PointF(bounds.Width * 0.30f, bounds.Height * 0.52f),
                            new PointF(bounds.Width * 0.45f, bounds.Height * 0.67f),
                            new PointF(bounds.Width * 0.71f, bounds.Height * 0.35f)
                        });
                        break;

                    case DialogKind.Error:
                        graphics.DrawLine(pen, bounds.Width * 0.34f, bounds.Height * 0.34f, bounds.Width * 0.66f, bounds.Height * 0.66f);
                        graphics.DrawLine(pen, bounds.Width * 0.66f, bounds.Height * 0.34f, bounds.Width * 0.34f, bounds.Height * 0.66f);
                        break;

                    case DialogKind.Warning:
                        graphics.DrawLine(pen, centerX, bounds.Height * 0.28f, centerX, bounds.Height * 0.58f);
                        graphics.FillEllipse(pen.Brush, centerX - 1.5f, bounds.Height * 0.68f, 3f, 3f);
                        break;

                    case DialogKind.Question:
                        using (var font = new Font("Segoe UI Semibold", 15F))
                            TextRenderer.DrawText(graphics, "?", font, Rectangle.Round(bounds), ((SolidBrush)pen.Brush).Color,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                        break;

                    default:
                        graphics.FillEllipse(pen.Brush, centerX - 1.5f, bounds.Height * 0.28f, 3f, 3f);
                        graphics.DrawLine(pen, centerX, bounds.Height * 0.42f, centerX, bounds.Height * 0.70f);
                        break;
                }
            }
        }
    }
}
