using CommandLine;
using DacpacDiff.Comparer;
using DacpacDiff.Comparer.Comparers;
using DacpacDiff.Core.Parser;
using DacpacDiff.Core.Utility;
using System;
using System.IO;
using System.Linq;

Parser.Default.ParseArguments<Options>(args)
    .WithNotParsed(e =>
    {
        e.Output();
    })
    .WithParsed(o =>
    {
        if (!FileUtilities.TryParsePath(o.StartSchemeFile, out var leftSchemeFile) || leftSchemeFile?.Exists != true)
        {
            Console.Error.WriteLine("Unable to find source scheme: " + o.StartSchemeFile);
            leftSchemeFile = null;
        }
        if (!FileUtilities.TryParsePath(o.TargetSchemeFile, out var rightSchemeFile) || rightSchemeFile?.Exists != true)
        {
            Console.Error.WriteLine("Unable to find target scheme: " + o.TargetSchemeFile);
            rightSchemeFile = null;
        }

        if ((o.OutputFile?.Length ?? 0) > 0 & !FileUtilities.TryParsePath(o.OutputFile, out var outputFile))
        {
            Console.Error.WriteLine("Unable to use output file: " + o.OutputFile);
            leftSchemeFile = null;
        }

        if (leftSchemeFile is null || rightSchemeFile is null)
        {
            return;
        }

        var schemeParserFactory = new SchemeParserFactory();
        var comparerFactory = new ComparerFactory();
        var formatProvider = FormatProviderFactory.GetFormat("mssql");

        // Source scheme
        var leftFmt = schemeParserFactory.GetFileFormat(leftSchemeFile);
        var leftScheme = leftFmt.ParseFile(leftSchemeFile.FullName);

        if (leftScheme?.Databases.Any() != true)
        {
            Console.Error.WriteLine("Unable to find a database specified in the source scheme");
            return;
        }

        // Target scheme
        var rightFmt = schemeParserFactory.GetFileFormat(rightSchemeFile);
        var rightScheme = rightFmt.ParseFile(rightSchemeFile.FullName);

        if (rightScheme?.Databases.Any() != true)
        {
            Console.Error.WriteLine("Unable to find a database specified in the target scheme");
            return;
        }

        // Compare schemes
        var comparer = new SchemeComparer(comparerFactory);
        var diffs = comparer.Compare(leftScheme, rightScheme);

        var rightVer = rightScheme.GetDatabaseVersion();

        // Output
        var outputFormat = formatProvider.GetOutputGenerator();
        var result = outputFormat.Generate(leftScheme.Name, rightScheme.Name, rightVer, diffs, !o.DisableDatalossCheck, !o.PrettyPrint);

        if (outputFile != null)
        {
            File.WriteAllText(outputFile.FullName, result);
        }
        else
        {
            Console.WriteLine(result);
        }
    });