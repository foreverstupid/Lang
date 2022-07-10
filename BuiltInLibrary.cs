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
        private const string LibraryObjectName = "sys";

        public BuiltInLibrary(IDictionary<string, RpnConst> variables)
        {
            int dynamicVarCounter = 0;
            Functions = new Dictionary<string, Func>()
            {
                [ToIndexedName("write")] = new Func(
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

                        Console.Write(ps[0].GetString());
                        return ps[0];
                    }
                ),
                [ToIndexedName("read")] = new Func(
                    0,
                    _ => new RpnString(Console.ReadLine())
                ),
                [ToIndexedName("readKey")] = new Func(
                    1,
                    ps =>
                    {
                        var intercept = !ps[0].GetBool();
                        var info = Console.ReadKey(intercept);
                        return new RpnString(info.KeyChar.ToString());
                    }
                ),
                [ToIndexedName("rnd")] = new Func(
                    0,
                    _ =>
                    {
                        var rnd = new Random();
                        return new RpnFloat(rnd.NextDouble());
                    }
                ),
                [ToIndexedName("file", "read")] = new Func(
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

                        var content = File.ReadAllText(path);
                        return new RpnString(content);
                    }
                ),
                [ToIndexedName("file", "write")] = new Func(
                    2,
                    ps =>
                    {
                        var path = ps[0].GetString();
                        var content = ps[1].GetString();
                        File.AppendAllText(path, content);
                        return new RpnString(content);
                    }
                ),
                [ToIndexedName("file", "delete")] = new Func(
                    1,
                    ps =>
                    {
                        var path = ps[0].GetString();
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                            return RpnConst.True;
                        }

                        return RpnConst.False;
                    }
                ),
                [ToIndexedName("file", "exists")] = new Func(
                    1,
                    ps =>
                    {
                        var path = ps[0].GetString();
                        return RpnConst.Bool(File.Exists(path));
                    }
                ),
                [ToIndexedName("length")] = new Func(
                    1,
                    ps => new RpnInteger(ps[0].GetString().Length)
                ),
                [ToIndexedName("alloc")] = new Func(
                    0,
                    _ =>
                    {
                        var name = $"{DynamicVarPrefix}{dynamicVarCounter}";
                        dynamicVarCounter++;
                        variables.Add(name, new RpnInteger(0));
                        return new RpnVar(name, variables);
                    }
                ),
                [ToIndexedName("free")] = new Func(
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
                        return RpnConst.True;
                    }
                ),
                [ToIndexedName("sleep")] = new Func(
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
                        return RpnConst.True;
                    }
                ),
                [ToIndexedName("exec")] = new Func(
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

            foreach (var key in Functions.Keys)
            {
                variables.Add(key, new RpnVar(key, variables));
            }
        }

        public Dictionary<string, Func> Functions { get; }

        private static string ToIndexedName(params string[] parts)
        {
            var name = RpnIndexator.GetIndexedName(LibraryObjectName, new RpnString(parts[0]));
            for (int i = 1; i < parts.Length; i++)
            {
                name = RpnIndexator.GetIndexedName(name, new RpnString(parts[i]));
            }

            return name;
        }

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