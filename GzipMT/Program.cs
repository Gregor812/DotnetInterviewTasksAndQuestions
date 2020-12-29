using GzipMT.Abstractions;
using GzipMT.Application;
using GzipMT.Cli;
using System;
using System.Threading;

namespace GzipMT
{
    class Program
    {
        private const int BufferSize = 8 * 1024 * 1024;

        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();
        private static IDataProcessor _dataProcessor;

        static int Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            Console.WriteLine("Press Ctrl+C to cancel");
            try
            {
                var options = OptionsParser.ParseArgs(args);
                switch (options)
                {
                    case HelpOptions o:
                        Console.WriteLine(o.HelpText);
                        return 0;
                    case VersionOptions o:
                        Console.WriteLine(o.VersionText);
                        return 0;
                    case ProcessingOptions o:
                        _dataProcessor = DataProcessorFactory.GetInstance(o, BufferSize);
                        return _dataProcessor.Run(Cts.Token);
                }
            }
            catch (ParsingException e)
            {
                Console.WriteLine("Options parsing failed:");
                Console.WriteLine(e.Message);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            return 0;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Cts.Cancel();
            Console.WriteLine("Operation cancelled by user");
            _dataProcessor.WaitForExit();
            Environment.Exit(1);
        }
    }
}
