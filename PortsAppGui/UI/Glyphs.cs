namespace PortsAppGui.UI
{
    /// <summary>
    /// Segoe MDL2 Assets / Segoe Fluent Icons code points.
    /// Built from numeric code points so the source files stay pure ASCII and cannot be
    /// mangled by an editor that guesses the wrong encoding.
    /// </summary>
    public static class Glyphs
    {
        private static string From(int codePoint) => char.ConvertFromUtf32(codePoint);

        public static readonly string Play = From(0xE768);
        public static readonly string Stop = From(0xE71A);
        public static readonly string Settings = From(0xE713);
        public static readonly string Network = From(0xE839);
        public static readonly string Page = From(0xE7C3);
        public static readonly string Document = From(0xE8A5);
        public static readonly string Search = From(0xE721);
        public static readonly string Add = From(0xE710);
        public static readonly string Save = From(0xE74E);
        public static readonly string Refresh = From(0xE72C);
        public static readonly string Delete = From(0xE74D);
        public static readonly string Copy = From(0xE8C8);
    }
}
