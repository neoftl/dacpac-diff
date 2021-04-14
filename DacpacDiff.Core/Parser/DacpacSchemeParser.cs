using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace DacpacDiff.Core.Parser
{
    public class DacpacSchemeParser : ISchemeParser
    {
        public SchemeModel? ParseFile(string filename)
        {
            var modelXml = getModelXml(filename);
            if (modelXml == null)
            {
                return null;
            }

            var db = new DatabaseModel
            {
                Name = "database" // TODO
            };

            // Parse model
            var schemasXml = modelXml.Find("Element", ("Type", "SqlSchema"));
            db.Schemas = schemasXml.Select(e => getSchema(db, modelXml, e.Attribute("Name")?.Value.Trim('[', ']') ?? string.Empty))
                .Union(new[] { getSchema(db, modelXml, "dbo") })
                .ToDictionary(s => s.Name);

            var scheme = new SchemeModel
            {
                Name = Path.GetFileNameWithoutExtension(filename)
            };
            scheme.Databases[db.Name] = db;
            return scheme;
        }

        private static XElement? getModelXml(string filename)
        {
            // Extract model.xml from zip
            using var zip = ZipFile.OpenRead(filename);
            var modelEntry = zip.Entries.First(e => e.FullName == "model.xml");
            if (modelEntry == null)
            {
                throw new InvalidOperationException("Invalid dacpac provided");
            }

            // Get model element from content
            using var sr = new StreamReader(modelEntry.Open());
            var modelData = sr.ReadToEnd();
            modelData = modelData.Replace("xmlns=\"http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02\"", "");
            var rootXml = XDocument.Parse(modelData).Root ?? throw new NullReferenceException();
            return rootXml.Element("Model");
        }

        private static SchemaModel getSchema(DatabaseModel db, XElement rootXml, string name)
        {
            var schema = new SchemaModel(db, name);
            // TODO: UserTypes (SqlTableType, ...)

            // Synonyms
            var els = rootXml.Find("Element", ("Type", a => a == "SqlSynonym"), ("Name", a => a?.StartsWith($"[{schema.Name}]") == true));
            schema.Synonyms = els.Select(e => getSynonym(schema, e)).ToDictionary(t => t.Name);

            // Tables
            els = rootXml.Find("Element", ("Type", a => a == "SqlTable"), ("Name", a => a?.StartsWith($"[{schema.Name}]") == true));
            schema.Tables = els.Select(e => getTable(schema, e))
                .Where(t => t is not null)
                .Cast<TableModel>()
                .ToDictionary(t => t.Name);

            // Index
            els = rootXml.Find("Element", ("Type", a => a == "SqlIndex"), ("Name", a => a?.StartsWith($"[{schema.Name}]") == true));
            var indexes = els.Select(e => getIndex(schema, e)).ToArray();

            // Function
            els = rootXml.Find("Element", ("Type", a => a == "SqlScalarFunction" || a == "SqlInlineTableValuedFunction" || a == "SqlMultiStatementTableValuedFunction"), ("Name", a => a?.StartsWith($"[{schema.Name}]") == true));
            var funcs = els.Select(e => getFunction(schema, e)).ToArray();

            // Sequence: SqlSequence
            var sequences = Array.Empty<ModuleModel>();
            // TODO

            // Stored Procedure
            els = rootXml.Find("Element", ("Type", a => a == "SqlProcedure"), ("Name", a => a?.StartsWith($"[{schema.Name}]") == true));
            var procs = els.Select(e => getProcedure(schema, e)).ToArray();

            // Trigger: SqlDmlTrigger
            els = rootXml.Find("Element", ("Type", a => a == "SqlDmlTrigger"), ("Name", a => a?.StartsWith($"[{schema.Name}]") == true));
            var trigs = els.Select(e => getTrigger(schema, e)).ToArray();

            // View
            els = rootXml.Find("Element", ("Type", a => a == "SqlView"), ("Name", a => a?.StartsWith($"[{schema.Name}]") == true));
            var views = els.Select(e => getView(schema, e)).ToArray();

            schema.Modules.Merge(indexes, m => m.Name)
                .Merge(funcs, m => m.Name)
                .Merge(sequences, m => m.Name)
                .Merge(procs, m => m.Name)
                .Merge(trigs, m => m.Name)
                .Merge(views, m => m.Name);

            return schema;
        }

        private static string getSqlType(XElement el)
        {
            var sqltype = el.Find("Relationship", ("Name", "Type")).First()
                .Element("Entry")?
                .Element("References")?.Attribute("Name")?.Value.Trim('[', ']') ?? string.Empty;

            var len = el.Find("Property", ("Name", "Length")).FirstOrDefault()?.Attribute("Value")?.Value;
            if (len is not null)
            {
                sqltype += $"({len})";
            }
            else if (el.Find("Property", ("Name", "IsMax")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                sqltype += "(MAX)";
            }
            else
            {
                var prec = el.Find("Property", ("Name", "Precision")).FirstOrDefault()?.Attribute("Value")?.Value;
                if (prec is not null)
                {
                    var scale = el.Find("Property", ("Name", "Scale")).FirstOrDefault()?.Attribute("Value")?.Value;
                    sqltype += $"({prec}, {scale})";
                }
            }

            return sqltype;
        }

        private static ModuleModel getFunction(SchemaModel schema, XElement funcXml)
        {
            var func = new ModuleModel
            {
                Type = ModuleModel.ModuleType.FUNCTION,
                Schema = schema,
                Name = funcXml.Attribute("Name")?.Value[(schema.Name.Length + 3)..].Trim('[', ']'),
                //ExecuteAs
                //Dependents?
            };

            var def = $"CREATE FUNCTION {func.FullName} (\r\n";

            var args = funcXml.Find("Relationship", ("Name", "Parameters")).SingleOrDefault()?
                .Elements("Entry").SelectMany(e => e.Find("Element", ("Type", "SqlSubroutineParameter")))
                .Select(toArg).ToArray();
            if (args is not null && args.Length > 0)
            {
                def += string.Join(", ", args);
            }

            if (funcXml.Attribute("Type")?.Value == "SqlInlineTableValuedFunction")
            {
                def += $"\r\n) RETURNS TABLE AS RETURN\r\n";
            }
            else
            {
                var retvar = funcXml.Find("Property", ("Name", "ReturnTableVariable")).FirstOrDefault()?.Attribute("Value")?.Value;
                if (retvar is not null)
                {
                    def += $"\r\n) RETURN {retvar} TABLE (";

                    var colsXml = funcXml.Find("Relationship", ("Name", "Columns")).First().Find(true, "Element", ("Type", a => a == "SqlSimpleColumn" || a == "SqlComputedColumn"));
                    var tbl = new TableModel { Schema = schema, Name = func.Name };
                    var fields = colsXml.Select(e => toField(tbl, 0, e)).ToArray();
                    var colStr = string.Join(", ", fields.Select(f => $"[{f.Name}] {f.Type}" + (!f.Nullable ? " NOT NULL" : "")).ToArray());

                    def += colStr;
                    def += "\r\n) AS\r\n";
                }
                else
                {
                    var typeXml = funcXml.Find("Relationship", ("Name", "Type")).First().Find(true, "Element", ("Type", "SqlTypeSpecifier")).First();
                    def += $"\r\n) RETURNS {getSqlType(typeXml)} AS\r\n";
                }
            }

            def += funcXml.Find(true, "Property", ("Name", "BodyScript"))?.First().Element("Value")?.Value;
            func.Definition = def;

            return func;
        }

        private static ModuleModel getIndex(SchemaModel schema, XElement indexXml)
        {
            var target = indexXml.Find("Relationship", ("Name", "IndexedObject")).Single()
                .Element("Entry")?
                .Element("References")?.Attribute("Name")?.Value ?? string.Empty;

            var idx = new ModuleModel
            {
                Type = ModuleModel.ModuleType.INDEX,
                Schema = schema,
                Name = indexXml.Attribute("Name")?.Value[(target.Length + 1)..].Trim('[', ']'),
                // TODO: system named
                //Dependents?
            };

            var def = "CREATE ";
            if (indexXml.Find("Property", ("Name", "IsUnique"), ("Value", "True")).Any())
            {
                def += "UNIQUE ";
            }
            if (indexXml.Find("Property", ("Name", "IsClustured"), ("Value", "True")).Any())
            {
                def += "CLUSTURED ";
            }
            def += $"INDEX [{idx.Name}] ON {target}";

            var cols = indexXml.Find("Relationship", ("Name", "ColumnSpecifications")).Single()
                .Element("Entry")?
                .Find("Element", ("Type", "SqlIndexedColumnSpecification"))
                .SelectMany(e => e.Find("Relationship", ("Name", "Column")))
                .Select(e => e.Element("Entry")?.Element("References")?.Attribute("Name")?.Value[(target.Length + 1)..])
                .ToArray();
            def += "(" + string.Join(", ", cols ?? Array.Empty<string?>()) + ")";

            var includes = indexXml.Find("Relationship", ("Name", "IncludedColumns"))
                .SelectMany(e => e.Elements("Entry").Select(r => r.Element("References")?.Attribute("Name")?.Value[(target.Length + 1)..]))
                .ToArray();
            if (includes.Length > 0)
            {
                def += " INCLUDE (" + string.Join(", ", includes) + ")";
            }

            var predsXml = indexXml.Find("Property", ("Name", "FilterPredicate")).FirstOrDefault();
            if (predsXml is not null)
            {
                var script = predsXml.Element("Value")?.Value ?? string.Empty;
                if (script.Length < 2 || script[0] != '(' || script[^1] != ')')
                {
                    script = $"({script})";
                }
                def += " WHERE " + script;
            }

            idx.Definition = def;

            return idx;
        }

        private static string toArg(XElement a)
        {
            var name = a.Attribute("Name")?.Value.Split('.')[^1].Trim('[', ']');
            var typeXml = a.Find(true, "Element", ("Type", p => p == "SqlTypeSpecifier" || p == "SqlXmlTypeSpecifier")).First();
            var arg = $"{name} {getSqlType(typeXml)}";

            var def = a.Find("Property", ("Name", "DefaultExpressionScript")).FirstOrDefault()?.Element("Value")?.Value;
            if (def is not null)
            {
                arg += $" = {def}";
            }

            if (a.Find("Property", ("Name", "IsReadOnly")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                arg += " READONLY";
            }

            if (a.Find("Property", ("Name", "IsOutput")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                arg += " OUTPUT";
            }

            return arg;
        }

        private static ModuleModel getProcedure(SchemaModel schema, XElement procXml)
        {
            var proc = new ModuleModel
            {
                Type = ModuleModel.ModuleType.PROCEDURE,
                Schema = schema,
                Name = procXml.Attribute("Name")?.Value[(schema.Name.Length + 3)..].Trim('[', ']'),
                // TODO: ExecuteAs
                //Dependents?
            };

            var def = $"CREATE PROCEDURE {proc.FullName}\r\n";

            var args = procXml.Find("Relationship", ("Name", "Parameters")).SingleOrDefault()?
                .Elements("Entry").SelectMany(e => e.Find("Element", ("Type", "SqlSubroutineParameter")))
                .Select(toArg).ToArray();
            if (args is not null && args.Length > 0)
            {
                def += string.Join(", ", args);
            }

            def += "\r\nAS\r\n";
            def += procXml.Find("Property", ("Name", "BodyScript")).First().Element("Value")?.Value;
            proc.Definition = def;

            return proc;
        }

        private static SynonymModel getSynonym(SchemaModel schema, XElement synXml)
        {
            var syn = new SynonymModel
            {
                Schema = schema,
                Name = synXml.Attribute("Name")?.Value[(schema.Name.Length + 3)..].Trim('[', ']'),
                BaseObject = synXml.Find("Property", ("Name", "ForObjectScript")).First().Element("Value")?.Value,
                //Dependents?
            };

            return syn;
        }

        private static TableModel? getTable(SchemaModel schema, XElement tableXml)
        {
            var table = new TableModel
            {
                Schema = schema,
                Name = tableXml.Attribute("Name")?.Value[(schema.Name.Length + 3)..].Trim('[', ']'),

                // TODO: IsPrimaryKeyUnclustered
                // TODO: PrimaryKey
                // TODO: Checks
                // TODO: Refs

                //Dependents?
            };

            // Ignore history tables (handled by current)
            if (tableXml.Find("Property", ("Name", "IsAutoGeneratedHistoryTable"), ("Value", "True")).Any())
            {
                return null;
            }
            if (tableXml.Parent?.Find(true, "Relationship", ("Name", "TemporalSystemVersioningHistoryTable")).Any(e => e.Find(true, "References", ("Name", table.FullName)).Any()) == true)
            {
                return null;
            }

            // Fields
            var idx = 0;
            var colsXml = tableXml.Find(true, "Element", ("Type", a => a == "SqlSimpleColumn" || a == "SqlComputedColumn"), ("Name", a => a.StartsWith(table.FullName)));
            table.Fields = colsXml.Select(e => toField(table, idx++, e)).ToArray();

            // Temporality
            var temporalXml = tableXml.Find("Relationship", ("Name", "TemporalSystemVersioningHistoryTable")).FirstOrDefault();
            if (temporalXml is not null)
            {
                table.Temporality = new TemporalityModel
                {
                    HistoryTable = temporalXml.Element("Entry")?.Element("References")?.Attribute("Name")?.Value,
                    PeriodFieldFrom = colsXml.FirstOrDefault(e => e.Find("Property", ("Name", "GeneratedAlwaysType"), ("Value", "1")).Any())?.Attribute("Name")?.Value[(table.FullName.Length + 1)..].Trim('[', ']'),
                    PeriodFieldTo = colsXml.FirstOrDefault(e => e.Find("Property", ("Name", "GeneratedAlwaysType"), ("Value", "2")).Any())?.Attribute("Name")?.Value[(table.FullName.Length + 1)..].Trim('[', ']'),
                };
            }

            return table;
        }

        private static ModuleModel getTrigger(SchemaModel schema, XElement trigXml)
        {
            var trig = new ModuleModel
            {
                Type = ModuleModel.ModuleType.INDEX,
                Schema = schema,
                Name = trigXml.Attribute("Name")?.Value[(schema.Name.Length + 3)..].Trim('[', ']'),
                //Dependents?
            };

            var target = trigXml.Find("Relationship", ("Name", "Parent")).Single()
                .Element("Entry")?
                .Element("References")?.Attribute("Name")?.Value;

            var def = $"CREATE TRIGGER {trig.FullName} ON {target} ";

            var trigTypeId = trigXml.Find("Property", ("Name", "SqlTriggerType")).FirstOrDefault()?.Attribute("Value")?.Value;
            var trigType = trigTypeId == "2" ? "AFTER " : "BEFORE ";

            if (trigXml.Find("Property", ("Name", "IsInsertTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                trigType += "INSERT, ";
            }
            if (trigXml.Find("Property", ("Name", "IsUpdateTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                trigType += "UPDATE, ";
            }
            if (trigXml.Find("Property", ("Name", "IsDeleteTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                trigType += "DELETE, ";
            }

            def += trigType.Trim(',', ' ');
            def += "\r\nAS\r\n";
            def += trigXml.Find("Property", ("Name", "BodyScript")).First().Element("Value")?.Value;
            trig.Definition = def;

            return trig;
        }

        private static ModuleModel getView(SchemaModel schema, XElement viewXml)
        {
            var view = new ModuleModel
            {
                Type = ModuleModel.ModuleType.VIEW,
                Schema = schema,
                Name = viewXml.Attribute("Name")?.Value[(schema.Name.Length + 3)..].Trim('[', ']'),
                //Dependents?
            };

            var def = $"CREATE VIEW {view.FullName}\r\nAS\r\n";
            def += viewXml.Find("Property", ("Name", "QueryScript")).First().Element("Value")?.Value;
            view.Definition = def;

            return view;
        }

        private static FieldModel toField(TableModel table, int ord, XElement colXml)
        {
            var field = new FieldModel
            {
                Table = table,
                Identity = colXml.Attribute("IsIdentity")?.Value.ToLower() == "true",
                Name = colXml.Attribute("Name")?.Value[(table.FullName.Length + 1)..].Trim('[', ']'),
                Order = ord,
                Nullable = colXml.Attribute("IsNullable")?.Value.ToLower() != "false",

                //Computation,
                //DefaultConstraint,
                //DefaultName,
                //DefaultValue,
                //Dependents,
                //Ref,
                //RefName,
                //RefTargetField,
                //RefTargetTable,
                //Unique,
                //UniqueName,
                //IsSystemNamedUnique,
            };

            // Type / computed
            if (colXml.Attribute("Type")?.Value == "SqlComputedColumn")
            {
                var expr = colXml.Find("Property", ("Name", "ExpressionScript")).First().Element("Value")?.Value;
                field.Computation = expr;
            }
            else
            {
                var typeXml = colXml.Descendants("Element")
                    .First(e => e.Attribute("Type")?.Value == "SqlTypeSpecifier" || e.Attribute("Type")?.Value == "SqlXmlTypeSpecifier");
                field.Type = getSqlType(typeXml);
            }

            // Default

            // Ref

            // Unique

            return field;
        }
    }
}
