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
            using var reader = GetReader(args);

            try
            {
                var parser = new LexicalParser();
                var tokens = Parse(reader, parser);

                if (args.IsDebugMode)
                {
                    LogTokens(tokens);
                }

                var syntaxer = new SyntaxAnalyzer(new ConsoleLogger());
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
        /// Gets the source code reader.
        /// </summary>
        /// <param name="args">CMD arguments.</param>
        static private StreamReader GetReader(Arguments args)
        {
            if (!File.Exists(args.FilePath))
            {
                throw new ArgumentException($"File \"{args.FilePath}\" doesn't exist!");
            }

            return new StreamReader(args.FilePath);
        }

        /// <summary>
        /// Transforms the source code into the token list.
        /// </summary>
        static private IEnumerable<Token> Parse(StreamReader reader, LexicalParser parser)
        {
            var tokens = new List<Token>();
            int nextChar;

            do
            {
                nextChar = reader.Read();   // returns -1 on the stream end
                NextToken(nextChar);
            }
            while (nextChar >= 0);

            return tokens;

            /// <summary>
            /// Appends the token to the token list if the token is constructed.
            /// </summary>
            void NextToken(int character)
            {
                var token = parser.GetNextToken(character);
                if (!(token is null))
                {
                    tokens.Add(token);
                }
            }
        }
    }
}
