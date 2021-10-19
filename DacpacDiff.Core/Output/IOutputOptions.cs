using CommandLine;

namespace DacpacDiff.Core.Output
{
    public interface IOutputOptions
    {
        [Option(shortName: 'p', longName: "pretty-print", HelpText = "Specify flag to not remove unnecessary whitespace.")]
        bool PrettyPrint { get; init; }

        [Option(shortName: 'd', longName: "disable-dataloss-check", HelpText = "Specify flag to remove failures if change may cause a dataloss.")]
        bool DisableDatalossCheck { get; init; }

        [Option(shortName: 't', longName: "option-to-disable-changes", HelpText = "Specify flag to provide the option to disable each change in the generated script.")]
        bool ChangeDisableOption { get; init; }
    }
}
