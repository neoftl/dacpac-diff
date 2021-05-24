using CommandLine;
using DacpacDiff.Comparer;
using DacpacDiff.Comparer.Comparers;
using DacpacDiff.Core;
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
        if (!FileUtilities.TryParsePath(o.StartSchemeFile, out var rightSchemeFile) || rightSchemeFile?.Exists != true)
        {
            Console.Error.WriteLine("Unable to find source scheme: " + o.StartSchemeFile);
            return;
        }
        if (!FileUtilities.TryParsePath(o.TargetSchemeFile, out var leftSchemeFile) || leftSchemeFile?.Exists != true)
        {
            if (o.New)
            {
                leftSchemeFile = rightSchemeFile;
            }
            else
            {
                Console.Error.WriteLine("Unable to find target scheme: " + o.TargetSchemeFile);
                return;
            }
        }
        if (o.New)
        {
            rightSchemeFile = new FileInfo("blank" + leftSchemeFile.Extension); // TODO: blank file
        }

        if ((o.OutputFile?.Length ?? 0) > 0 & !FileUtilities.TryParsePath(o.OutputFile, out var outputFile))
        {
            Console.Error.WriteLine("Unable to use output file: " + o.OutputFile);
            return;
        }

        var schemeParserFactory = new SchemeParserFactory();
        var comparerFactory = new ModelComparerFactory();
        var formatProvider = FormatProviderFactory.GetFormat("mssql");

        // Source scheme
        var rightFmt = schemeParserFactory.GetFileFormat(rightSchemeFile);
        var rightScheme = rightFmt.ParseFile(rightSchemeFile.FullName);
        if (rightScheme?.Databases.Any() != true)
        {
            Console.Error.WriteLine("Unable to find a database specified in the source scheme");
            return;
        }

        // Target scheme
        var leftFmt = schemeParserFactory.GetFileFormat(leftSchemeFile);
        var leftScheme = leftFmt.ParseFile(leftSchemeFile.FullName);
        if (leftScheme?.Databases.Any() != true)
        {
            Console.Error.WriteLine("Unable to find a database specified in the target scheme");
            return;
        }

        if (o.ExcludeSchemas != null)
        {
            foreach (var schemaName in o.ExcludeSchemas)
            {
                foreach (var db in leftScheme.Databases.Values)
                {
                    db.Schemas.Remove(schemaName);
                }
                foreach (var db in rightScheme.Databases.Values)
                {
                    db.Schemas.Remove(schemaName);
                }
            }
        }

        // Compare schemes (left scheme to replace right)
        var comparer = new SchemeComparer(comparerFactory);
        var diffs = comparer.Compare(leftScheme, rightScheme);

        var targetVer = rightScheme.GetDatabaseVersion();

        // Output
        var outputFormat = formatProvider.GetSqlFileBuilder();
        outputFormat.Options = o;
        var result = outputFormat.Generate(leftScheme.Name, rightScheme.Name, targetVer, diffs);

        if (o.StandardiseLineEndings)
        {
            result = result.StandardiseLineEndings("\r\n");
        }

        if (outputFile != null)
        {
            File.WriteAllText(outputFile.FullName, result);
        }
        else
        {
            Console.WriteLine(result);
        }
    });