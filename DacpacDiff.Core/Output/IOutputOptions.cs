using CommandLine;

namespace DacpacDiff.Core.Output
{
    public interface IOutputOptions
    {
        [Option(shortName: 'p', longName: "pretty-print", HelpText = "Specify flag to not remove unnecessary whitespace.")]
        bool PrettyPrint { get; init; }

        [Option(shortName: 'd', longName: "disable-dataloss-check", HelpText = "Specify flag to remove failures if change may cause a dataloss.")]
        bool DisableDatalossCheck { get; init; }
    }
}
