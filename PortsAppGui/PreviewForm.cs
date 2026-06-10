namespace PortsAppGui
{
    public class PreviewForm : Form
    {
        public PreviewForm(string clientConfig, string serverConfig)
        {
            Text = "Generated TOML preview";
            Size = new Size(900, 650);
            StartPosition = FormStartPosition.CenterParent;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CreatePage("Client config", clientConfig));
            tabs.TabPages.Add(CreatePage("Server config", serverConfig));
            Controls.Add(tabs);
        }

        private static TabPage CreatePage(string title, string content)
        {
            var page = new TabPage(title);
            var textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                WordWrap = false,
                Font = new Font(FontFamily.GenericMonospace, 10),
                Text = content
            };
            page.Controls.Add(textBox);
            return page;
        }
    }
}
