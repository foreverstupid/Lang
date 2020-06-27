using System;
using System.Collections.Generic;
using System.IO;

namespace Lang
{
    class Program
    {
        public static void Main(string[] args)
        {
            using var reader = GetReader(args);
            var parser = new LexicalParser();

            var tokens = Parse(reader, parser);

            // Console.WriteLine($"{"Token",20} {"Type",20} {"Position"}");
            // Console.WriteLine("-------------------------------------------------------------------------------");
            // foreach (var token in tokens)
            // {
            //     Console.WriteLine($"{token.Value, 20} {token.TokenType, 20} {token.Line}:{token.StartPosition}");
            // }

            var syntaxer = new SyntaxAnalyzer(new NullLogger());
            var program = syntaxer.Analyse(tokens);

            // foreach (var rpn in program.Program)
            // {
            //     Console.WriteLine(rpn);
            // }

            var interpreter = new Interpreter();
            var exitValue = interpreter.Run(program);
            Console.WriteLine("\nProgram finished with exit value: " + exitValue);
        }

        /// <summary>
        /// Gets the source code reader.
        /// </summary>
        /// <param name="args">CMD arguments.</param>
        static private StreamReader GetReader(string[] args)
        {
            var filePath = args[0];
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"File \"{filePath}\" doesn't exist!");
            }

            return new StreamReader(filePath);
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
