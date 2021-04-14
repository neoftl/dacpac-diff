using CommandLine;

namespace DacpacDiff.Comparer
{
    public class Options
    {
        [Value(index: 0, MetaName = "Start schema", Required = true, HelpText = "The path of the dacpac file to compare against.")]
        public string? StartSchemeFile { get; init; }

        [Value(index: 1, MetaName = "Target schema", Required = true, HelpText = "The path of the dacpac file to compare against.")]
        public string? TargetSchemeFile { get; init; }

        [Option(shortName: 'o', longName: "output", HelpText = "The file to write the result to.")]
        public string? OutputFile { get; init; }

        [Option(shortName: 'p', longName: "pretty-print", HelpText = "Specify flag to not remove unnecessary whitespace.")]
        public bool PrettyPrint { get; init; }

        [Option(shortName: 'd', longName: "disable-dataloss-check", HelpText = "Specify flag to remove failures if change may cause a dataloss.")]
        public bool DisableDatalossCheck { get; init; }
    }
}
