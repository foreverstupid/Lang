namespace Lang.Content
{
    /// <summary>
    /// String constants for the language syntax constructions.
    /// </summary>
    public static class Syntax
    {
        public const string ParamsStart = "[";
        public const string ParamsSeparator = ",";
        public const string ParamsEnd = "]";
        public const string ParamsApply = "=>";

        public const string EvalArgsStart = "(";
        public const string EvalArgsSeparator = ",";
        public const string EvalArgsEnd = ")";

        public const string ConditionStart = "(";
        public const string ConditionEnd = ")";

        public const string GroupStart = "{";
        public const string GroupDelimiter = ";";
        public const string GroupEnd = "}";

        public const string ExpressionBracketOpen = "(";
        public const string ExpressionBracketClose = ")";

        public const string IndexatorStart = "[";
        public const string IndexatorEnd = "]";
        public const string FieldSeparator = ".";

        public const string InitializerStart = "{";
        public const string InitializerSeparator = ",";
        public const string InitializerEnd = "}";
    }
}