using System;
using System.Collections.Generic;
using System.IO;
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
        public const string Rand = "_rnd";
        public const string GetFileContent = "_readFile";
        public const string WriteToFile = "_writeFile";
        public const string Length = "_length";

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
                ),
                [Rand] = new BuiltinFunc(
                    0,
                    _ =>
                    {
                        var rnd = new Random();
                        return new RpnFloat(rnd.NextDouble());
                    }
                ),
                [GetFileContent] = new BuiltinFunc(
                    1,
                    ps =>
                    {
                        var path = ps[0].GetString();
                        if (!File.Exists(path))
                        {
                            throw new InterpretationException(
                                $"File {path} doesn't exist"
                            );
                        }

                        using var reader = new StreamReader(path);
                        var content = reader.ReadToEnd();
                        return new RpnString(content);
                    }
                ),
                [WriteToFile] = new BuiltinFunc(
                    2,
                    ps =>
                    {
                        var path = ps[0].GetString();
                        if (!File.Exists(path))
                        {
                            throw new InterpretationException(
                                $"File {path} doesn't exist"
                            );
                        }

                        using var writer = new StreamWriter(path);
                        var content = ps[1].GetString();
                        writer.Write(content);
                        return new RpnString(content);
                    }
                ),
                [Length] = new BuiltinFunc(
                    1,
                    ps => new RpnInteger(ps[0].GetString().Length)
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