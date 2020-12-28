using System;
using System.Reflection;

namespace GzipMT.Cli
{
    public static class OptionsParser
    {
        private const string UsageText = @"Usage:
GzipMT <verb> [<input file> <output file>]

verb    compress|decompress|help|version

If the application is called with 'help' or 'version' verb, other args won't be processed.

If called with 'compress' or 'decompress' verb, 'input file' and 'output file' args should be specified.";

        public static Options ParseArgs(string[] args)
        {
            args = args ?? new string[] { };

            if (args.Length < 1)
                throw new ParsingException(UsageText);

            var verb = args[0].ToLowerInvariant();

            switch (verb)
            {
                case "help":
                    return new HelpOptions { HelpText = UsageText };
                case "version":
                    return new VersionOptions { VersionText = $"Version: {Assembly.GetExecutingAssembly().GetName().Version}" };
                case "compress":
                case "decompress":
                    return ParseProcessingArgs(args);
                default:
                    throw new ParsingException($"Unknown verb: {verb}{Environment.NewLine}{Environment.NewLine}{UsageText}");
            }
        }

        private static ProcessingOptions ParseProcessingArgs(string[] args)
        {
            if (args.Length < 3)
                throw new ParsingException($"Not enough args{Environment.NewLine}{Environment.NewLine}{UsageText}");

            var verb = args[0].ToLowerInvariant();

            switch (verb)
            {
                case "compress":
                    return new CompressingOptions
                    {
                        InputFile = args[1],
                        OutputFile = args[2]
                    };
                case "decompress":
                    return new DecompressingOptions
                    {
                        InputFile = args[1],
                        OutputFile = args[2]
                    };
                default:
                    throw new ParsingException($"Unknown action type: {args[0]}{Environment.NewLine}{Environment.NewLine}{UsageText}");
            }
        }
    }
}
