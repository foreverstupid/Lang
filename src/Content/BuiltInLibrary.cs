using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Lang.RpnItems;
using Lang.Exceptions;

namespace Lang.Content
{
    /// <summary>
    /// A library of built-in functions.
    /// </summary>
    public class BuiltInLibrary
    {
        private const string DynamicVarPrefix = "<dyn";
        private static readonly EntityName LibraryObjectName = new EntityName("sys");

        public BuiltInLibrary(IDictionary<EntityName, RpnConst> variables)
        {
            int dynamicVarCounter = 0;
            var funcs = new Dictionary<string[], Func>()
            {
                [new[]{ "write" }] = new Func(
                    ps =>
                    {
                        Console.Write(ps[0].GetString());
                        return ps[0];
                    },
                    RpnConst.MainTypes),
                [new[]{ "read" }] = new Func(_ => new RpnString(Console.ReadLine())),
                [new[]{ "read", "key" }] = new Func(
                    _ =>
                    {
                        var info = Console.ReadKey(intercept: true);
                        return new RpnString(info.KeyChar.ToString());
                    }),
                [new[]{ "read", "key", "visible" }] = new Func(
                    _ =>
                    {
                        var info = Console.ReadKey(intercept: false);
                        return new RpnString(info.KeyChar.ToString());
                    }),
                [new[]{ "rnd" }] = new Func(
                    _ =>
                    {
                        var rnd = new Random();
                        return new RpnFloat(rnd.NextDouble());
                    }),
                [new[]{ "alloc" }] = new Func(
                    _ =>
                    {
                        var name = $"{DynamicVarPrefix}{dynamicVarCounter}>";
                        dynamicVarCounter++;
                        variables.Add(new EntityName(name), new RpnInteger(0));
                        return new RpnVar(name, variables);
                    }),
                [new[]{ "free" }] = new Func(
                    ps =>
                    {
                        var name = ps[0].GetName();
                        if (!name.Value.StartsWith(DynamicVarPrefix))
                        {
                            throw new InterpretationException(
                                "Only dynamic variables can be deallocated");
                        }

                        variables.Remove(name);
                        return RpnConst.True;
                    },
                    RpnConst.Type.Variable),
                [new[]{ "sleep" }] = new Func(
                    ps =>
                    {
                        double ms = ps[0].GetFloat();
                        if (ms < 0)
                        {
                            throw new InterpretationException("Delay should be positive");
                        }

                        Task.Delay(TimeSpan.FromMilliseconds(ms)).Wait();
                        return RpnConst.True;
                    },
                    RpnConst.NumberTypes),
                [new[]{ "exec" }] = new Func(
                    ps =>
                    {
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
                            variables[ps[3].GetName()] = new RpnString(e.Message);
                            return new RpnInteger(e.HResult);
                        }

                        variables[ps[2].GetName()] = new RpnString(process.StandardOutput.ReadToEnd());
                        variables[ps[3].GetName()] = new RpnString(process.StandardError.ReadToEnd());

                        return new RpnInteger(process.ExitCode);
                    },
                    RpnConst.Type.String,
                    RpnConst.Type.String,
                    RpnConst.Type.Variable,
                    RpnConst.Type.Variable
                ),

                [new[]{ "file", "read" }] = new Func(
                    ps =>
                    {
                        var path = ps[0].GetString();
                        if (!File.Exists(path))
                        {
                            throw new InterpretationException(
                                $"File {path} doesn't exist");
                        }

                        var content = File.ReadAllText(path);
                        return new RpnString(content);
                    },
                    RpnConst.Type.String),
                [new[]{ "file", "write" }] = new Func(
                    ps =>
                    {
                        var path = ps[0].GetString();
                        var content = ps[1].GetString();
                        File.AppendAllText(path, content);
                        return new RpnString(content);
                    },
                    RpnConst.Type.String,
                    RpnConst.MainTypes),
                [new[]{ "file", "exists" }] = new Func(
                    ps =>
                    {
                        var path = ps[0].GetString();
                        return RpnConst.Bool(File.Exists(path));
                    },
                    RpnConst.Type.String),
                [new[]{ "file", "delete" }] = new Func(
                    ps =>
                    {
                        var path = ps[0].GetString();
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                            return RpnConst.True;
                        }

                        return RpnConst.False;
                    },
                    RpnConst.Type.String),

                [new[]{ "math", "sqrt" }] = new Func(
                    ps => new RpnFloat(Math.Sqrt(ps[0].GetFloat())),
                    RpnConst.NumberTypes),
                [new[]{ "math", "ln" }] = new Func(
                    ps => new RpnFloat(Math.Log(ps[0].GetFloat())),
                    RpnConst.NumberTypes),
                [new[]{ "math", "exp" }] = new Func(
                    ps => new RpnFloat(Math.Exp(ps[0].GetFloat())),
                    RpnConst.NumberTypes),
                [new[]{ "math", "sin" }] = new Func(
                    ps => new RpnFloat(Math.Sin(ps[0].GetFloat())),
                    RpnConst.NumberTypes),
                [new[]{ "math", "cos" }] = new Func(
                    ps => new RpnFloat(Math.Cos(ps[0].GetFloat())),
                    RpnConst.NumberTypes),
                [new[]{ "math", "tan" }] = new Func(
                    ps => new RpnFloat(Math.Tan(ps[0].GetFloat())),
                    RpnConst.NumberTypes),
            };

            Functions = new Dictionary<EntityName, Func>();
            foreach (var item in funcs)
            {
                var builtInName = ToBuiltInName(item.Key);

                Functions.Add(
                    builtInName,
                    item.Value);

                variables.Add(
                    ToVariableName(item.Key),
                    new RpnBuiltIn(builtInName));
            }
        }

        public Dictionary<EntityName, Func> Functions { get; }

        private static EntityName ToVariableName(params string[] parts)
        {
            var name = RpnIndexator.GetIndexedName(LibraryObjectName, new RpnString(parts[0]));
            for (int i = 1; i < parts.Length; i++)
            {
                name = RpnIndexator.GetIndexedName(name, new RpnString(parts[i]));
            }

            return name;
        }

        private static EntityName ToBuiltInName(params string[] parts)
            => new EntityName(string.Join(".", parts));

        /// <summary>
        /// Built-in function description.
        /// </summary>
        public class Func
        {
            public Func(Algorithm function, params RpnConst.Type[] paramTypes)
            {
                Main = function;
                ParamTypes = paramTypes;
            }

            public delegate RpnConst Algorithm(params RpnConst[] parameters);

            /// <summary>
            /// Gets the fucntion algorithm.
            /// </summary>
            public Algorithm Main { get; }

            /// <summary>
            /// Gets the function parameters count.
            /// </summary>
            public int ParamCount => ParamTypes.Length;

            /// <summary>
            /// Gets allowed types of the function parameters.
            /// </summary>
            public RpnConst.Type[] ParamTypes { get; }
        }
    }
}