using CommandLine;

namespace Lang
{
    /// <summary>
    /// Contains command line arguments.
    /// </summary>
    class Arguments
    {
        [Value(
            0,
            Required = true,
            HelpText = "The path to the file with the program source")]
        public string FilePath { get; set; }

        [Option(
            'd',
            "debug",
            HelpText = "If specified then the interpreter works in the debug mode")]
        public bool IsDebugMode { get; set; }
    }
}