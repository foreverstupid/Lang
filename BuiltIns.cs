using System;
using System.Collections.Generic;
using System.Linq;
using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// All built-in function.
    /// </summary>
    public static class BuiltIns
    {
        public const string Write = "_write";
        public const string Read = "_read";

        public delegate RpnConst Function(params RpnConst[] parameters);

        public static Dictionary<string, BuiltinFunc> Funcs { get; } =
            new Dictionary<string, BuiltinFunc>()
            {
                [Write] = new BuiltinFunc(
                    1,
                    ps =>
                    {
                        Console.Write(ps[0].GetString());
                        return ps[0];
                    }
                ),
                [Read] = new BuiltinFunc(
                    0,
                    _ => new RpnString(Console.ReadLine())
                )
            };

        /// <summary>
        /// Built-in function description.
        /// </summary>
        public class BuiltinFunc
        {
            public BuiltinFunc(int paramCount, Function function)
            {
                Func = function;
                ParamCount = paramCount;
            }

            public delegate RpnConst Function(params RpnConst[] parameters);

            /// <summary>
            /// Gets the fucntion.
            /// </summary>
            public Function Func { get; }

            /// <summary>
            /// Gets the function parameter count.
            /// </summary>
            public int ParamCount { get; }
        }
    }
}