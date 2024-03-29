namespace Lang.Content
{
    /// <summary>
    /// String constants for the language key words.
    /// </summary>
    public static class KeyWords
    {
        public const string If = "if";
        public const string Else = "or";
        public const string While = "as";
        public const string Let = "loc";
        public const string Ref = "ref";
        public const string In = "in";
        public const string Break = "end";
        public const string Continue = "new";
        public const string Return = "out";

        public static readonly string[] All = new string[]
        {
            If, Else, While,
            Let, Ref,
            In,
            Break, Continue, Return,
        };
    }
}