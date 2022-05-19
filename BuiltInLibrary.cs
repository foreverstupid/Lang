using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
        public const string ReadKey = "_readKey";
        public const string Rand = "_rnd";
        public const string GetFileContent = "_readFile";
        public const string WriteToFile = "_writeFile";
        public const string Length = "_length";
        public const string New = "_alloc";
        public const string Free = "_free";
        public const string Sleep = "_sleep";
        public const string Exec = "_exec";

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
                [ReadKey] = new Func(
                    1,
                    ps =>
                    {
                        var intercept = !ps[0].GetBool();
                        var info = Console.ReadKey(intercept);
                        return new RpnString(info.KeyChar.ToString());
                    }
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
                ),
                [Sleep] = new Func(
                    1,
                    ps =>
                    {
                        if (ps[0].ValueType != RpnConst.Type.Float &&
                            ps[0].ValueType != RpnConst.Type.Integer)
                        {
                            throw new InterpretationException(
                                "Expected a number as a delay in milliseconds");
                        }

                        double ms = ps[0].GetFloat();
                        if (ms < 0)
                        {
                            throw new InterpretationException("Delay should be positive");
                        }

                        Task.Delay(TimeSpan.FromMilliseconds(ms)).Wait();
                        return new RpnInteger(1);
                    }
                ),
                [Exec] = new Func(
                    4,
                    ps =>
                    {
                        if (ps[0].ValueType != RpnConst.Type.String)
                        {
                            throw new InterpretationException(
                                "Expected a string as a program path");
                        }

                        if (ps[1].ValueType != RpnConst.Type.String)
                        {
                            throw new InterpretationException(
                                "Expected a string as a program arguments");
                        }

                        if (ps[2].ValueType != RpnConst.Type.Variable)
                        {
                            throw new InterpretationException(
                                "Expected a variable as an output parameter for" +
                                "the execution output");
                        }

                        if (!variables.ContainsKey(ps[2].GetString()))
                        {
                            throw new InterpretationException("Given output variable doesn't exist");
                        }

                        if (ps[3].ValueType != RpnConst.Type.Variable)
                        {
                            throw new InterpretationException(
                                "Expected a variable as an error output parameter for" +
                                "the execution output");
                        }

                        if (!variables.ContainsKey(ps[3].GetString()))
                        {
                            throw new InterpretationException("Given error output variable doesn't exist");
                        }

                        var processStartInfo = new ProcessStartInfo()
                        {
                            FileName = ps[0].GetString(),
                            Arguments = ps[1].GetString(),
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        };

                        Process process;
                        try
                        {
                            process = Process.Start(processStartInfo);
                            process.WaitForExit();
                        }
                        catch (Exception e)
                        {
                            variables[ps[3].GetString()] = new RpnString(e.Message);
                            return new RpnInteger(e.HResult);
                        }

                        variables[ps[2].GetString()] = new RpnString(process.StandardOutput.ReadToEnd());
                        variables[ps[3].GetString()] = new RpnString(process.StandardError.ReadToEnd());

                        return new RpnInteger(process.ExitCode);
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