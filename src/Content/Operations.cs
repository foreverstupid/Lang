using System.Linq;

namespace Lang.Content
{
    /// <summary>
    /// String constants for the language operations.
    /// </summary>
    public static class Operations
    {
        public const string Negate = "-";
        public const string Not = "!";
        public const string Dereference = "$";

        public const string Assign = "=";
        public const string Insert = "->";

        public const string Plus = "+";
        public const string Minus = "-";
        public const string Divide = "/";
        public const string Multiply = "*";
        public const string Mod = "%";

        public const string And = "&";
        public const string Or = "|";
        public const string In = KeyWords.In;

        public const string Greater = ">";
        public const string Less = "<";
        public const string Equal = "~";

        public const string Cast = ":";
        public const string CanCast = "?";

        public const string RightShift = ">>";
        public const string LeftShift = "<<";

        public static readonly string[] Unary = new string[]
        {
            Negate, Not, Dereference,
        };

        public static readonly string[] Binary = new string[]
        {
            Assign, Plus, Minus, Divide, Multiply,
            And, Or, Greater, Less, Equal,
            Mod, Insert, Cast, CanCast, In,
            RightShift, LeftShift,
        };

        public static readonly string[] All = Unary.Concat(Binary).ToArray();
    }
}