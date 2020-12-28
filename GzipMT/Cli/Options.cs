namespace GzipMT.Cli
{
    public abstract class Options
    {}

    public class HelpOptions : Options
    {
        public string HelpText { get; set; }
    }

    public class VersionOptions : Options
    {
        public string VersionText { get; set; }
    }


    public abstract class ProcessingOptions : Options
    {
        public string InputFile { get; set; }

        public string OutputFile { get; set; }
    }

    public class CompressingOptions : ProcessingOptions
    {}

    public class DecompressingOptions : ProcessingOptions
    {}
}
