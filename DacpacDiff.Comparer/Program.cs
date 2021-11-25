using CommandLine;
using DacpacDiff.Comparer;
using DacpacDiff.Comparer.Comparers;
using DacpacDiff.Core;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Parser;
using DacpacDiff.Core.Utility;
using System.Text.RegularExpressions;

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
        var formatProvider = new FormatProviderFactory().GetFormat("mssql");

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

        // Exclude unwanted schemas
        if (o.ExcludeSchemas?.Any() == true)
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

        // Exclude unwanted objects
        if (o.ExcludeObjects?.Any() == true)
        {
            void stripExcludedObjects(Regex re, DatabaseModel db)
            {
                foreach (var sch in db.Schemas.Values)
                {
                    foreach (var mod in sch.Modules)
                    {
                        if (re.IsMatch(mod.Value.FullName))
                        {
                            sch.Modules.Remove(mod);
                        }
                    }
                    foreach (var syn in sch.Synonyms)
                    {
                        if (re.IsMatch(syn.Value.FullName))
                        {
                            sch.Synonyms.Remove(syn);
                        }
                    }
                    foreach (var tbl in sch.Tables)
                    {
                        if (re.IsMatch(tbl.Value.FullName))
                        {
                            sch.Tables.Remove(tbl);
                        }
                    }
                    foreach (var ut in sch.UserTypes)
                    {
                        if (re.IsMatch(ut.Value.FullName))
                        {
                            sch.UserTypes.Remove(ut);
                        }
                    }
                }
            }
            foreach (var objectPattern in o.ExcludeObjects)
            {
                var re = new Regex("^" + Regex.Escape(objectPattern).Replace("\\*", ".*?") + "$");
                foreach (var db in leftScheme.Databases.Values)
                {
                    stripExcludedObjects(re, db);
                }
                foreach (var db in rightScheme.Databases.Values)
                {
                    stripExcludedObjects(re, db);
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