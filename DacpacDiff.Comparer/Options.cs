using CommandLine;
using DacpacDiff.Core.Output;
using DacpacDiff.Core.Parser;
using System.Collections.Generic;

namespace DacpacDiff.Comparer
{
    public class Options : IOutputOptions, IParserOptions
    {
        [Value(index: 0, MetaName = "Start schema", Required = true, HelpText = "The path of the dacpac file for the current scheme.")]
        public string? StartSchemeFile { get; init; }

        [Value(index: 1, MetaName = "Target schema", Required = true, HelpText = "The path of the dacpac file for the desired scheme.")]
        public string? TargetSchemeFile { get; init; }

        [Option(shortName: 'o', longName: "output", HelpText = "The file to write the result to.")]
        public string? OutputFile { get; init; }

        [Option(shortName: 'n', longName: "new", HelpText = "No existing scheme to compare to.")]
        public bool New { get; init; }

        [Option(shortName: 'l', longName: "standard-eols", HelpText = "Standardise line endings to CRLF.")]
        public bool StandardiseLineEndings { get; init; }

        [Option(shortName: 'x', longName: "exclude-schemas", HelpText = "List of schemas to not compare.")]
        public IEnumerable<string>? ExcludeSchemas { get; init; }

        // TODO: option to only compare certain types

        public bool PrettyPrint { get; init; }
        public bool DisableDatalossCheck { get; init; }
    }
}
