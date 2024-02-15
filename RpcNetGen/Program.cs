// Copyright by Artur Wolf

namespace RpcNetGen;

using Antlr4.Runtime;
using RpcNetGen.Internal;

internal class Program
{
    private static int Main(string[] args)
    {
        if (!Parse(args, out Arguments arguments))
        {
            ShowHelp();
            return 1;
        }

        if (!Validate(arguments))
        {
            ShowHelp();
            return 1;
        }

        string fileContent = File.ReadAllText(arguments.InputFilePath);
        var stream = new AntlrInputStream(fileContent);

        var lexer = new RpcLexer(stream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new RpcParser(tokens)
        {
            BuildParseTree = true
        };

        RpcParser.RpcSpecificationContext xdrSpecificationContext = parser.rpcSpecification();
        var parsedContent =
            new Content(arguments.Name, arguments.Namespace, arguments.Public ? "public" : "internal", xdrSpecificationContext);

        using var writer = new XdrFileWriter(arguments.OutputFilePath);

        parsedContent.Dump(writer, 0);

        return 0;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("USAGE:");
        Console.WriteLine("  RpcNetGen [-n <namespace>] [-o <output file>] [-p] [-u] <input x file path>");
        Console.WriteLine();
        Console.WriteLine("  -n <namespace>     Namespace in the generated file.");
        Console.WriteLine();
        Console.WriteLine("  -o <output file>   Output file.");
        Console.WriteLine();
        Console.WriteLine("  -p                 Public classes (default is internal).");
        Console.WriteLine();
    }

    private static bool Validate(Arguments arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments.InputFilePath) || !File.Exists(arguments.InputFilePath))
        {
            Console.WriteLine($"Could not find the following input file: {arguments.InputFilePath}.");
            return false;
        }

        arguments.InputFilePath = Path.GetFullPath(arguments.InputFilePath);
        string directoryName = Path.GetDirectoryName(arguments.InputFilePath) ?? throw new InvalidOperationException();

        arguments.Name = Path.GetFileNameWithoutExtension(arguments.InputFilePath);

        if (string.IsNullOrWhiteSpace(arguments.Namespace))
        {
            arguments.Namespace = arguments.Name;
        }

        if (string.IsNullOrWhiteSpace(arguments.OutputFilePath))
        {
            arguments.OutputFilePath = Path.Combine(directoryName, arguments.Name + ".cs");
        }

        return true;
    }

    private static bool Parse(string[] args, out Arguments arguments)
    {
        arguments = new Arguments();

        if ((args.Length == 0) || (args[0] == "--help") || (args[0] == "-h"))
        {
            return false;
        }

        bool CheckNextArgument(int index, string arg)
        {
            if (index >= args.Length)
            {
                Console.WriteLine($"Missing argument after {arg}.");
                return false;
            }

            return true;
        }

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-n")
            {
                i++;
                if (!CheckNextArgument(i, "-n"))
                {
                    return false;
                }

                arguments.Namespace = args[i];
            }
            else if (args[i] == "-o")
            {
                i++;
                if (!CheckNextArgument(i, "-o"))
                {
                    return false;
                }

                arguments.OutputFilePath = args[i];
            }
            else if (args[i] == "-p")
            {
                arguments.Public = true;
            }
            else
            {
                arguments.InputFilePath = args[i];
            }
        }

        return true;
    }

    private class Arguments
    {
        public string InputFilePath;
        public string Name;
        public string Namespace;
        public string OutputFilePath;
        public bool Public;
    }
}
