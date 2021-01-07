using GzipMT.Abstractions;
using GzipMT.Application;
using GzipMT.Cli;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;

namespace GzipMT
{
    class Program
    {
        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();
        private static IDataProcessor _dataProcessor;

        static int Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            var settings = GetSettings();

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
                        return RunApplication(o, settings.BufferSizeBytes.Value, settings.WorkerThreadsNumber.Value);
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

        private static AppSettings GetSettings()
        {
            var settings = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build()
                .GetSection("CommonSettings")
                .Get<AppSettings>();

            if (settings == null)
            {
                Console.WriteLine("No settings found. Default settings will be applied");
                settings = AppSettings.Default;
            }
            else
            {
                if (settings.BufferSizeBytes == null)
                {
                    Console.WriteLine($"Default buffer size will be applied: {AppSettings.DefaultBufferSizeBytes}");
                    settings.BufferSizeBytes = AppSettings.DefaultBufferSizeBytes;
                }

                if (settings.WorkerThreadsNumber == null)
                {
                    Console.WriteLine($"Default worker threads number will be applied: {AppSettings.MaxWorkerThreadsNumber}");
                    settings.WorkerThreadsNumber = AppSettings.MaxWorkerThreadsNumber;
                }
                else if (settings.WorkerThreadsNumber > AppSettings.MaxWorkerThreadsNumber)
                {
                    Console.WriteLine(
                        $"Worker threads count will be reduced. Max value is {AppSettings.MaxWorkerThreadsNumber}");
                    settings.WorkerThreadsNumber = AppSettings.MaxWorkerThreadsNumber;
                }
            }

            return settings;
        }

        private static int RunApplication(ProcessingOptions o, int bufferSizeBytes, int workerThreadsNumber)
        {
            using (_dataProcessor = DataProcessorFactory.GetInstance(o, bufferSizeBytes, workerThreadsNumber))
            {
                return _dataProcessor.Run(Cts.Token);
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Cts.Cancel();
            Console.WriteLine("Operation cancelled by user");
            _dataProcessor?.WaitForExit();
            Environment.Exit(1);
        }
    }
}
