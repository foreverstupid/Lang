using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Lang.RpnItems;

namespace Lang
{
    class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<Arguments>(args)
                .WithParsed<Arguments>(
                    arguments => MainCore(arguments)
                );
        }

        private static void MainCore(Arguments args)
        {
            string src = GetSourceCode(args);

            try
            {
                var parser = new LexicalParser();
                var tokens = parser.Parse(src);

                if (args.IsDebugMode)
                {
                    LogTokens(tokens);
                }

                var syntaxer = new SyntaxAnalyzer(new ConsoleLogger(omitAdditionalInfo: true));
                var program = syntaxer.Analyse(tokens, args.IsDebugMode);

                if (args.IsDebugMode)
                {
                    LogRpns(program);
                }

                var interpreter = new Interpreter();
                var exitValue = interpreter.Run(program, args.IsDebugMode);
                Console.WriteLine(
                    "\n=============================================\n" +
                    "Program finished with exit value: " + exitValue
                );
            }
            catch (LexicalAnalysisException le)
            {
                Console.WriteLine(le.Message);
            }
            catch (SyntaxException se)
            {
                Console.WriteLine(se.Message);
            }
            catch (InterpretationException ie)
            {
                Console.WriteLine(ie.Message);
            }
        }

        private static void LogTokens(IEnumerable<Token> tokens)
        {
            Console.WriteLine($"{"Token",20} {"Type",20} {"Position"}");
            Console.WriteLine("-------------------------------------------------------------------------------");
            foreach (var token in tokens)
            {
                Console.WriteLine($"{token.Value, 20} {token.TokenType, 20} {token.Line}:{token.StartPosition}");
            }
        }

        private static void LogRpns(IEnumerable<Rpn> rpns)
        {
            foreach (var rpn in rpns)
            {
                Console.WriteLine(rpn);
            }
        }

        /// <summary>
        /// Gets the source code.
        /// </summary>
        /// <param name="args">CMD arguments.</param>
        static private string GetSourceCode(Arguments args)
        {
            if (!File.Exists(args.FilePath))
            {
                throw new ArgumentException($"File \"{args.FilePath}\" doesn't exist!");
            }

            return File.ReadAllText(args.FilePath);
        }
    }
}
