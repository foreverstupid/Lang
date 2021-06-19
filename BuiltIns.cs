using System;
using System.Collections.Generic;
using System.IO;
using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// A library of built-in functions.
    /// </summary>
    public class BuiltInLibrary
    {
        private const string DynamicVarPrefix = "#dyn#";

        public const string Write = "_write";
        public const string Read = "_read";
        public const string Rand = "_rnd";
        public const string GetFileContent = "_readFile";
        public const string WriteToFile = "_writeFile";
        public const string Length = "_length";
        public const string New = "_alloc";
        public const string Free = "_free";

        public BuiltInLibrary(IDictionary<string, RpnConst> variables)
        {
            int counter = 0;
            Functions = new Dictionary<string, Func>()
            {
                [Write] = new Func(
                    1,
                    ps =>
                    {
                        if (ps[0].ValueType == RpnConst.Type.Func)
                        {
                            throw new InterpretationException("Cannot write a lambda");
                        }

                        if (ps[0].ValueType == RpnConst.Type.None)
                        {
                            throw new InterpretationException("Cannot write the None value");
                        }

                        if (ps[0].ValueType == RpnConst.Type.BuiltIn)
                        {
                            throw new InterpretationException("Cannot write a built-in function");
                        }

                        Console.Write(ps[0].GetString());
                        return ps[0];
                    }
                ),
                [Read] = new Func(
                    0,
                    _ => new RpnString(Console.ReadLine())
                ),
                [Rand] = new Func(
                    0,
                    _ =>
                    {
                        var rnd = new Random();
                        return new RpnFloat(rnd.NextDouble());
                    }
                ),
                [GetFileContent] = new Func(
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
                [WriteToFile] = new Func(
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
                [Length] = new Func(
                    1,
                    ps => new RpnInteger(ps[0].GetString().Length)
                ),
                [New] = new Func(
                    0,
                    _ =>
                    {
                        var name = $"{DynamicVarPrefix}{counter}";
                        counter++;
                        variables.Add(name, new RpnInteger(0));
                        return new RpnVar(name);
                    }
                ),
                [Free] = new Func(
                    1,
                    ps =>
                    {
                        if (ps[0].ValueType != RpnConst.Type.Variable)
                        {
                            throw new InterpretationException(
                                "Expected variable as a parameter"
                            );
                        }

                        var name = ps[0].GetString();
                        if (!name.StartsWith(DynamicVarPrefix))
                        {
                            throw new InterpretationException(
                                "Only dynamic variables can be deallocated"
                            );
                        }

                        variables.Remove(name);
                        return new RpnNone();
                    }
                )
            };
        }

        public Dictionary<string, Func> Functions { get; }

        /// <summary>
        /// Built-in function description.
        /// </summary>
        public class Func
        {
            public Func(int paramCount, Algorithm function)
            {
                Main = function;
                ParamCount = paramCount;
            }

            public delegate RpnConst Algorithm(params RpnConst[] parameters);

            /// <summary>
            /// Gets the fucntion algorithm.
            /// </summary>
            public Algorithm Main { get; }

            /// <summary>
            /// Gets the function parameter count.
            /// </summary>
            public int ParamCount { get; }
        }
    }
}