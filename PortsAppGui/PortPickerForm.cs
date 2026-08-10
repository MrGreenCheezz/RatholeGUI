using PortsAppGui.UI;

namespace PortsAppGui
{
    /// <summary>Lists applications currently listening on this machine and lets the user pick one.</summary>
    public class PortPickerForm : Form
    {
        private static readonly HashSet<string> SystemProcessNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "System", "System Idle", "svchost", "lsass", "services", "wininit", "smss", "csrss", "spoolsv"
        };

        private readonly ListView _list;
        private readonly ModernTextBox _search;
        private readonly ModernCheckBox _showTcp;
        private readonly ModernCheckBox _showUdp;
        private readonly ModernCheckBox _hideSystem;
        private readonly Label _summary;
        private readonly ModernButton _selectButton;
        private readonly ModernButton _refreshButton;

        private List<ListeningEndpoint> _endpoints = new();
        private int _sortColumn = -1;
        private bool _sortAscending = true;
        private bool _isScanning;
        private bool _isStretchingColumns;

        public ListeningEndpoint? SelectedEndpoint { get; private set; }

        public PortPickerForm()
        {
            Text = "Choose a listening application";
            ClientSize = new Size(840, 560);
            MinimumSize = new Size(720, 460);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Icon = Theme.LoadAppIcon();
            Theme.ApplyTo(this);

            var header = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Theme.Surface, Padding = new Padding(20, 16, 20, 16) };

            var caption = new Label
            {
                Text = "Applications listening on this PC",
                Font = Theme.Section,
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(20, 14)
            };

            _search = new ModernTextBox
            {
                PlaceholderText = "Filter by application, port, PID…",
                Location = new Point(20, 46),
                Width = 320
            };
            _search.TextChanged += (_, _) => ApplyFilter();

            _showTcp = new ModernCheckBox { Text = "TCP", Checked = true, Location = new Point(356, 50) };
            // UDP is off by default: browsers and system services open dozens of short-lived UDP
            // sockets that would bury the handful of applications worth tunnelling.
            _showUdp = new ModernCheckBox { Text = "UDP", Checked = false, Location = new Point(424, 50) };
            _hideSystem = new ModernCheckBox { Text = "Hide system processes", Checked = true, Location = new Point(496, 50) };

            _showTcp.CheckedChanged += (_, _) => ApplyFilter();
            _showUdp.CheckedChanged += (_, _) => ApplyFilter();
            _hideSystem.CheckedChanged += (_, _) => ApplyFilter();

            _refreshButton = new ModernButton
            {
                Text = "Rescan",
                Glyph = Glyphs.Refresh,
                Variant = ButtonVariant.Standard,
                Width = 110,
                Height = 32,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Top = 44
            };
            _refreshButton.Click += async (_, _) => await LoadAsync();
            header.Resize += (_, _) => _refreshButton.Left = header.ClientSize.Width - 20 - _refreshButton.Width;

            header.Controls.Add(caption);
            header.Controls.Add(_search);
            header.Controls.Add(_showTcp);
            header.Controls.Add(_showUdp);
            header.Controls.Add(_hideSystem);
            header.Controls.Add(_refreshButton);

            _list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false,
                OwnerDraw = true,
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                Font = Theme.Body,
                HeaderStyle = ColumnHeaderStyle.Clickable
            };
            _list.Columns.Add("Application", 210);
            _list.Columns.Add("PID", 60, HorizontalAlignment.Right);
            _list.Columns.Add("Proto", 60);
            _list.Columns.Add("Listening on", 160);
            _list.Columns.Add("Port", 70, HorizontalAlignment.Right);
            _list.Columns.Add("What it usually is", 180);

            _list.DrawColumnHeader += DrawColumnHeader;
            _list.DrawItem += DrawItem;
            _list.DrawSubItem += DrawSubItem;
            _list.Resize += (_, _) => StretchLastColumn();
            _list.ColumnWidthChanged += (_, _) => StretchLastColumn();
            _list.SelectedIndexChanged += (_, _) => UpdateSelection();
            _list.DoubleClick += (_, _) => Accept();
            _list.ColumnClick += OnColumnClick;
            _list.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    Accept();
            };

            // The list sits inside a rounded frame so the table has a visible edge against the window.
            var listFrame = new CardPanel
            {
                Dock = DockStyle.Fill,
                CornerRadius = 8,
                FillColor = Theme.Input,
                BackColor = Theme.Input,
                BorderColor = Theme.BorderStrong,
                Padding = new Padding(8)
            };
            listFrame.Controls.Add(_list);

            var listHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 12, 20, 12), BackColor = Theme.Window };
            listHost.Controls.Add(listFrame);

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Theme.Surface, Padding = new Padding(20, 15, 20, 15) };

            _summary = new Label
            {
                Text = "Scanning…",
                Font = Theme.Body,
                ForeColor = Theme.TextMuted,
                AutoSize = false,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _selectButton = new ModernButton
            {
                Text = "Use this port",
                Variant = ButtonVariant.Accent,
                Width = 130,
                Height = 34,
                Dock = DockStyle.Right,
                Enabled = false
            };
            _selectButton.Click += (_, _) => Accept();

            var cancelButton = new ModernButton
            {
                Text = "Cancel",
                Variant = ButtonVariant.Standard,
                Width = 100,
                Height = 34,
                Dock = DockStyle.Right,
                Margin = new Padding(0, 0, 10, 0),
                DialogResult = DialogResult.Cancel
            };

            var spacer = new Panel { Dock = DockStyle.Right, Width = 10, BackColor = Theme.Surface };

            // Docked children are laid out back-to-front, so the fill must be added first
            // and the right-most button last.
            footer.Controls.Add(_summary);
            footer.Controls.Add(cancelButton);
            footer.Controls.Add(spacer);
            footer.Controls.Add(_selectButton);

            Controls.Add(listHost);
            Controls.Add(footer);
            Controls.Add(header);

            CancelButton = cancelButton;
            Shown += async (_, _) =>
            {
                Theme.ApplyDarkScrollBars(_list);
                StretchLastColumn();
                await LoadAsync();
            };
        }

        private async Task LoadAsync()
        {
            if (_isScanning)
                return;

            _isScanning = true;
            _refreshButton.Enabled = false;
            _summary.Text = "Scanning…";

            try
            {
                _endpoints = await Task.Run(() => PortScanner.Scan());
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _endpoints = new List<ListeningEndpoint>();
                _list.Items.Clear();
                _summary.Text = $"Scan failed: {ex.Message}";
            }
            finally
            {
                _isScanning = false;
                _refreshButton.Enabled = true;
            }
        }

        private void ApplyFilter()
        {
            var query = _search.Text.Trim();
            var filtered = _endpoints.Where(Matches).ToList();

            if (_sortColumn >= 0)
                filtered = Sort(filtered);

            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (var endpoint in filtered)
            {
                var item = new ListViewItem(new[]
                {
                    endpoint.ProcessName,
                    endpoint.Pid.ToString(),
                    endpoint.Protocol,
                    endpoint.AddressDisplay,
                    endpoint.Port.ToString(),
                    string.IsNullOrEmpty(endpoint.Hint) ? endpoint.Description : endpoint.Hint
                })
                {
                    Tag = endpoint
                };
                _list.Items.Add(item);
            }
            _list.EndUpdate();

            _summary.Text = filtered.Count == 0
                ? _endpoints.Count == 0 ? "Nothing is listening (or the scan was blocked)." : "No matches for the current filter."
                : $"{filtered.Count} listening endpoint(s){(string.IsNullOrEmpty(query) ? "" : $" matching \"{query}\"")}.";

            UpdateSelection();

            bool Matches(ListeningEndpoint endpoint)
            {
                if (endpoint.Protocol == "TCP" && !_showTcp.Checked)
                    return false;
                if (endpoint.Protocol == "UDP" && !_showUdp.Checked)
                    return false;
                if (_hideSystem.Checked && (endpoint.Pid <= 4 || SystemProcessNames.Contains(endpoint.ProcessName)))
                    return false;
                if (query.Length == 0)
                    return true;

                return endpoint.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase)
                       || endpoint.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
                       || endpoint.Port.ToString().Contains(query, StringComparison.Ordinal)
                       || endpoint.Pid.ToString().Contains(query, StringComparison.Ordinal)
                       || endpoint.Hint.Contains(query, StringComparison.OrdinalIgnoreCase);
            }
        }

        private List<ListeningEndpoint> Sort(List<ListeningEndpoint> source)
        {
            Func<ListeningEndpoint, object> key = _sortColumn switch
            {
                1 => endpoint => endpoint.Pid,
                2 => endpoint => endpoint.Protocol,
                3 => endpoint => endpoint.AddressDisplay,
                4 => endpoint => endpoint.Port,
                5 => endpoint => endpoint.Hint,
                _ => endpoint => endpoint.ProcessName.ToLowerInvariant()
            };

            var ordered = source.OrderBy(key, Comparer<object>.Create(CompareValues));
            return (_sortAscending ? ordered : ordered.Reverse()).ToList();

            static int CompareValues(object? left, object? right)
            {
                if (left is int leftInt && right is int rightInt)
                    return leftInt.CompareTo(rightInt);

                return string.Compare(left?.ToString(), right?.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private void OnColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (_sortColumn == e.Column)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn = e.Column;
                _sortAscending = true;
            }

            ApplyFilter();
        }

        private void UpdateSelection()
        {
            var endpoint = _list.SelectedItems.Count > 0 ? _list.SelectedItems[0].Tag as ListeningEndpoint : null;
            _selectButton.Enabled = endpoint != null;

            if (endpoint == null)
                return;

            var summary = $"{endpoint.ProcessName} (pid {endpoint.Pid})  →  {endpoint.SuggestedLocalAddress}:{endpoint.Port} / {endpoint.Protocol}";
            if (endpoint.ExecutablePath.Length > 0)
                summary += $"   ·   {endpoint.ExecutablePath}";

            _summary.Text = summary;
        }

        private void Accept()
        {
            if (_list.SelectedItems.Count == 0)
                return;

            SelectedEndpoint = _list.SelectedItems[0].Tag as ListeningEndpoint;
            if (SelectedEndpoint == null)
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        #region Dark owner drawing

        private static void DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var background = new SolidBrush(Theme.Surface))
                e.Graphics.FillRectangle(background, e.Bounds);

            using (var line = new Pen(Theme.Border))
                e.Graphics.DrawLine(line, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

            var textBounds = Rectangle.Inflate(e.Bounds, -10, 0);
            var alignment = e.Header?.TextAlign == HorizontalAlignment.Right
                ? TextFormatFlags.Right
                : TextFormatFlags.Left;

            TextRenderer.DrawText(e.Graphics, e.Header?.Text ?? "", Theme.Caption, textBounds, Theme.TextMuted,
                alignment | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>
        /// Deliberately empty. If the row background were filled here, a partial repaint - which is
        /// what hovering triggers - would clear the whole row while only some cells get redrawn,
        /// and the rest of the row would be left blank. Every cell paints its own background in
        /// DrawSubItem instead.
        /// </summary>
        private static void DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = false;

            var item = e.Item;
            if (item == null || e.ColumnIndex < 0 || e.ColumnIndex >= _list.Columns.Count)
                return;

            var text = e.SubItem?.Text ?? "";
            var rowIndex = e.ItemIndex >= 0 ? e.ItemIndex : item.Index;
            var selected = item.Selected;

            var background = selected
                ? Theme.Mix(Theme.Input, Theme.Accent, 0.32)
                : rowIndex % 2 == 0
                    ? Theme.Input
                    : Theme.Mix(Theme.Input, Theme.Card, 0.85);

            using (var brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, e.Bounds);

            using (var separator = new Pen(Theme.Mix(Theme.Input, Theme.Border, 0.75)))
                e.Graphics.DrawLine(separator, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

            if (selected && e.ColumnIndex == 0)
            {
                using var accent = new SolidBrush(Theme.Accent);
                e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Top, 3, e.Bounds.Height - 1);
            }

            var isPrimary = e.ColumnIndex == 0;
            var color = selected || isPrimary
                ? Theme.Text
                : e.ColumnIndex == _list.Columns.Count - 1
                    ? Theme.TextMuted
                    : Theme.Mix(Theme.TextMuted, Theme.Text, 0.55);

            var font = isPrimary ? Theme.BodySemibold : Theme.Body;
            var textBounds = Rectangle.Inflate(e.Bounds, -10, 0);
            var alignment = _list.Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Right
                ? TextFormatFlags.Right
                : TextFormatFlags.Left;

            TextRenderer.DrawText(e.Graphics, text, font, textBounds, color,
                alignment | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>
        /// Widens the last column to the edge. Anything past the final column in the header strip is
        /// drawn by the OS in its light theme, which shows up as a white block on the right.
        /// </summary>
        private void StretchLastColumn()
        {
            if (_isStretchingColumns || _list.Columns.Count == 0)
                return;

            _isStretchingColumns = true;
            try
            {
                var used = 0;
                for (var i = 0; i < _list.Columns.Count - 1; i++)
                    used += _list.Columns[i].Width;

                var last = _list.Columns[^1];
                var remaining = Math.Max(120, _list.ClientSize.Width - used);
                if (last.Width != remaining)
                    last.Width = remaining;
            }
            finally
            {
                _isStretchingColumns = false;
            }
        }

        #endregion
    }
}
