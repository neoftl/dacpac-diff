﻿using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace DacpacDiff.Core.Parser
{
    public class DacpacSchemeParser : ISchemeParser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
        private static readonly (string type, Action<DatabaseModel, XElement> parser)[] PARSERS = new (string, Action<DatabaseModel, XElement>)[]
        {
            ("SqlTable", (db, el) => withSchema(db, el, parseTableElement)),
            ("SqlView", (db, el) => withSchema(db, el, parseViewElement)),
            ("SqlProcedure", (db, el) => withSchema(db, el, parseProcedureElement)),
            ("SqlIndex", (db, el) => withSchema(db, el, parseIndexElement)),
            ("SqlScalarFunction", (db, el) => withSchema(db, el, parseFunctionElement)),
            ("SqlInlineTableValuedFunction", (db, el) => withSchema(db, el, parseFunctionElement)),
            ("SqlMultiStatementTableValuedFunction", (db, el) => withSchema(db, el, parseFunctionElement)),
            ("SqlDmlTrigger", (db, el) => withSchema(db, el, parseTriggerElement)),
            ("SqlSynonym", (db, el) => withSchema(db, el, parseSynonymElement)),
            // SqlSequence

            ("SqlForeignKeyConstraint", parseForeignKeyElement),
            ("SqlPrimaryKeyConstraint", parsePrimaryKeyElement),
            ("SqlUniqueConstraint", parseUniqueElement),
            ("SqlCheckConstraint", parseCheckElement),
            ("SqlDefaultConstraint", parseDefaultElement),

            // SqlPermissionStatement
            // SqlRole
            // SqlLogin
            // SqlUser
        };

        [ExcludeFromCodeCoverage(Justification = "Requires file containing zip")]
        public SchemeModel? ParseFile(string filename)
        {
            var name = Path.GetFileNameWithoutExtension(filename);
            if (filename == "blank.dacpac")
            {
                return ParseContent("blank", "<blank />");
            }

            // Extract model.xml from zip
            string? modelData = null;
            using (var zip = ZipFile.OpenRead(filename))
            {
                var modelEntry = zip.Entries.First(e => e.FullName == "model.xml");
                if (modelEntry == null)
                {
                    throw new InvalidOperationException("Invalid dacpac provided");
                }

                // Get model element from content
                using var sr = new StreamReader(modelEntry.Open());
                modelData = sr.ReadToEnd();
                modelData = modelData.Replace("xmlns=\"http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02\"", "");
            }

            return modelData != null ? ParseContent(name, modelData) : null;
        }

        internal static SchemeModel? ParseContent(string name, string content)
        {
            var scheme = new SchemeModel(name);
            
            var db = new DatabaseModel("database"); // DB name not in dacpac
            db.Schemas["dbo"] = new SchemaModel(db, "dbo"); // Always has "dbo" schema
            
            scheme.Databases[db.Name] = db;

            var rootXml = XDocument.Parse(content).Root;
            var modelXml = rootXml?.Element("Model");
            if (modelXml is null)
            {
                return scheme;
            }

            // Build model hierarchy
            var elsByType = modelXml.Elements("Element")
                .GroupBy(e => e.Attribute("Type")?.Value ?? string.Empty)
                .ToDictionary(e => e.Key, e => e.ToArray());
            foreach (var el in elsByType.Get("SqlSchema") ?? Array.Empty<XElement>())
            {
                var elName = getName(el.Attribute("Name")?.Value ?? throw new MissingMemberException("SqlSchema", "Name"));
                db.Schemas[elName] = new SchemaModel(db, elName);
            }
            elsByType.Remove("SqlSchema");
            foreach (var (type, parser) in PARSERS)
            {
                var els = elsByType.Get(type) ?? Array.Empty<XElement>();
                foreach (var el in els)
                {
                    parser(db, el);
                }
                elsByType.Remove(type);
            }

            if (elsByType.Keys.Any())
            {
            }

            return scheme;
        }

        private static string getName(XElement? el, string? prefix = null)
        {
            var name = el?.Attribute("Name")?.Value ?? string.Empty;
            return getName(name, prefix);
        }
        private static string getName(string name, string? prefix = null)
        {
            if (name.Length > 0 && prefix is not null)
            {
                if (name.StartsWith($"[{prefix}].", StringComparison.OrdinalIgnoreCase))
                {
                    name = name[(prefix.Length + 3)..];
                }
                else if (name.StartsWith($"{prefix}.", StringComparison.OrdinalIgnoreCase))
                {
                    name = name[(prefix.Length + 1)..];
                }
            }
            return (name.Length > 2 && name[0] == '[' && name[^1] == ']')
                ? name[1..^1]
                : name;
        }

        private static string getSqlType(XElement el)
        {
            var sqltype = getName(el.Find("Relationship", ("Name", "Type")).First()
                .Element("Entry")?
                .Element("References"));

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

        private static string[] resolveDependencies(XElement el, string name)
        {
            var depEls = el.Find("Relationship", ("Name", name)).FirstOrDefault()?
                .Elements("Entry").Select(e => e.Element("References"))
                .Where(e => e != null && e.Attribute("ExternalSource") == null)
                .Select(e => e?.Attribute("Name")?.Value)
                .Where(e => e != null).Cast<string>().ToArray();
            return depEls?.Select(d => d.Split('.'))
                .Where(d => d.Length > 1)
                .Select(d => $"{d[0]}.{d[1]}")
                .Distinct().ToArray() ?? Array.Empty<string>();
        }

        private static void withSchema(DatabaseModel db, XElement el, Action<SchemaModel, XElement, string> parser)
        {
            var name = el.Attribute("Name")?.Value ?? throw new InvalidDataException($"Element {el.Name} missing required 'Name' attribute");
            var schemaName = getName(name.Split('.')[0]);
            var schema = db.Get<SchemaModel>(schemaName) ?? throw new IndexOutOfRangeException($"Unknown schema: {schemaName}");
            name = getName(name, schema.Name);
            parser(schema, el, name);
        }

        private static void parseTableElement(SchemaModel schema, XElement el, string name)
        {
            var table = new TableModel(
                schema: schema,
                name: name
            );

            // Ignore history tables (handled by current)
            if (el.Find("Property", ("Name", "IsAutoGeneratedHistoryTable"), ("Value", "True")).Any()
                || el.Parent?.Find(true, "Relationship", ("Name", "TemporalSystemVersioningHistoryTable")).Any(e => e.Find(true, "References", ("Name", table.FullName)).Any()) == true)
            {
                return;
            }

            schema.Tables[name] = table;

            // Fields
            var idx = 0;
            var colsXml = el.Find(true, "Element", ("Type", a => a == "SqlSimpleColumn" || a == "SqlComputedColumn"), ("Name", a => a?.StartsWith(table.FullName, StringComparison.OrdinalIgnoreCase) == true));
            table.Fields = colsXml.Select(e => toField(table, ++idx, e)).ToArray();

            // Temporality
            var temporalXml = el.Find("Relationship", ("Name", "TemporalSystemVersioningHistoryTable")).FirstOrDefault();
            if (temporalXml is not null)
            {
                table.Temporality = new TemporalityModel
                {
                    HistoryTable = temporalXml.Element("Entry")?.Element("References")?.Attribute("Name")?.Value ?? string.Empty,
                    PeriodFieldFrom = getName(colsXml.FirstOrDefault(e => e.Find("Property", ("Name", "GeneratedAlwaysType"), ("Value", "1")).Any()), table.FullName),
                    PeriodFieldTo = getName(colsXml.FirstOrDefault(e => e.Find("Property", ("Name", "GeneratedAlwaysType"), ("Value", "2")).Any()), table.FullName),
                };
            }
        }

        private static void parseViewElement(SchemaModel schema, XElement el, string name)
        {
            var view = new ModuleModel
            {
                Type = ModuleModel.ModuleType.VIEW,
                Schema = schema,
                Name = name,
                Dependencies = resolveDependencies(el, "QueryDependencies")
            };
            schema.Modules[name] = view;

            var def = $"CREATE VIEW {view.FullName}\r\nAS\r\n";
            def += el.Find("Property", ("Name", "QueryScript")).First().Element("Value")?.Value;
            view.Definition = def;
        }

        private static void parseProcedureElement(SchemaModel schema, XElement el, string name)
        {
            var proc = new ModuleModel
            {
                Type = ModuleModel.ModuleType.PROCEDURE,
                Schema = schema,
                Name = name,
                Dependencies = resolveDependencies(el, "BodyDependencies")
            };
            schema.Modules[name] = proc;

            // TODO: don't build SQL; store as pieces
            var def = $"CREATE PROCEDURE {proc.FullName}\r\n";

            var args = el.Find("Relationship", ("Name", "Parameters")).SingleOrDefault()?
                .Elements("Entry").SelectMany(e => e.Find("Element", ("Type", "SqlSubroutineParameter")))
                .Select(toArg).ToArray();
            if (args is not null && args.Length > 0)
            {
                def += "    " + string.Join(",\r\n    ", args);
            }

            if (el.Find("Property", ("Name", "IsCaller"), ("Value", "True"))?.Any() == true)
            {
                proc.ExecuteAs = "CALLER";
                def += "\r\nWITH EXECUTE AS CALLER";
            }
            else if (el.Find("Property", ("Name", "IsOwner"), ("Value", "True"))?.Any() == true)
            {
                proc.ExecuteAs = "OWNER";
                def += "\r\nWITH EXECUTE AS OWNER";
            }

            def += "\r\nAS\r\n";
            def += el.Find("Property", ("Name", "BodyScript")).First().Element("Value")?.Value.Trim();
            proc.Definition = def;
        }

        private static void parseIndexElement(SchemaModel schema, XElement el, string name)
        {
            var target = el.Find("Relationship", ("Name", "IndexedObject")).Single()
                .Element("Entry")?
                .Element("References")?.Attribute("Name")?.Value;
            name = getName(el, target);

            var idx = new ModuleModel
            {
                Type = ModuleModel.ModuleType.INDEX,
                Schema = schema,
                Name = name,
                Dependencies = resolveDependencies(el, "BodyDependencies")
                // TODO: system named
            };
            schema.Modules[name] = idx;

            // TODO: don't build SQL; store as pieces
            var def = "CREATE ";
            if (el.Find("Property", ("Name", "IsUnique"), ("Value", "True")).Any())
            {
                def += "UNIQUE ";
            }
            if (el.Find("Property", ("Name", "IsClustured"), ("Value", "True")).Any())
            {
                def += "CLUSTURED ";
            }
            def += $"INDEX [{name}] ON {target}";

            var cols = el.Find("Relationship", ("Name", "ColumnSpecifications")).Single()
                .Elements("Entry")?
                .SelectMany(e => e.Find("Element", ("Type", "SqlIndexedColumnSpecification")))
                .SelectMany(e => e.Find("Relationship", ("Name", "Column")))
                .Select(e => getName(e.Element("Entry")?.Element("References"), target))
                .ToArray();
            def += "([" + string.Join("], [", cols ?? Array.Empty<string?>()) + "])";

            var includes = el.Find("Relationship", ("Name", "IncludedColumns"))
                .SelectMany(e => e.Elements("Entry").Select(r => getName(r.Element("References"), target)))
                .ToArray();
            if (includes.Length > 0)
            {
                def += " INCLUDE ([" + string.Join("], [", includes) + "])";
            }

            var predsXml = el.Find("Property", ("Name", "FilterPredicate")).FirstOrDefault();
            if (predsXml is not null)
            {
                var script = predsXml.Element("Value")?.Value ?? string.Empty;
                script = $"({script.ReduceBrackets()})";
                def += " WHERE " + script;
            }

            idx.Definition = def;
        }

        private static void parseFunctionElement(SchemaModel schema, XElement el, string name)
        {
            var func = new ModuleModel
            {
                Type = ModuleModel.ModuleType.FUNCTION,
                Schema = schema,
                Name = name,
                Dependencies = resolveDependencies(el, "BodyDependencies")
                //ExecuteAs
            };
            schema.Modules[name] = func;

            // TODO: don't build SQL; store as pieces
            var def = $"CREATE FUNCTION {func.FullName} (\r\n";

            var args = el.Find("Relationship", ("Name", "Parameters")).SingleOrDefault()?
                .Elements("Entry").SelectMany(e => e.Find("Element", ("Type", "SqlSubroutineParameter")))
                .Select(toArg).ToArray();
            if (args is not null && args.Length > 0)
            {
                def += string.Join(", ", args);
            }

            if (el.Attribute("Type")?.Value == "SqlInlineTableValuedFunction")
            {
                def += $"\r\n) RETURNS TABLE AS RETURN\r\n";
            }
            else
            {
                var retvar = el.Find("Property", ("Name", "ReturnTableVariable")).FirstOrDefault()?.Attribute("Value")?.Value;
                if (retvar is not null)
                {
                    def += $"\r\n) RETURN {retvar} TABLE (";

                    var idx = 0;
                    var colsXml = el.Find("Relationship", ("Name", "Columns")).First().Find(true, "Element", ("Type", a => a == "SqlSimpleColumn" || a == "SqlComputedColumn"));
                    var tbl = new TableModel(schema, func.Name);
                    var fields = colsXml.Select(e => toField(tbl, ++idx, e)).ToArray();
                    var colStr = string.Join(", ", fields.Select(f => $"[{f.Name}] {f.Type}" + (!f.Nullable ? " NOT NULL" : "")).ToArray());

                    def += colStr;
                    def += "\r\n) AS\r\n";
                }
                else
                {
                    var typeXml = el.Find("Relationship", ("Name", "Type")).First().Find(true, "Element", ("Type", "SqlTypeSpecifier")).First();
                    def += $"\r\n) RETURNS {getSqlType(typeXml)} AS\r\n";
                }
            }

            def += el.Find(true, "Property", ("Name", "BodyScript"))?.First().Element("Value")?.Value;
            func.Definition = def;
        }

        private static void parseTriggerElement(SchemaModel schema, XElement el, string name)
        {
            var trig = new ModuleModel
            {
                Type = ModuleModel.ModuleType.TRIGGER,
                Schema = schema,
                Name = name,
                Dependencies = resolveDependencies(el, "BodyDependencies")
            };
            schema.Modules[name] = trig;

            var target = el.Find("Relationship", ("Name", "Parent")).Single()
                .Element("Entry")?
                .Element("References")?
                .Attribute("Name")?.Value;

            // TODO: don't build SQL; store as pieces
            var def = $"CREATE TRIGGER {trig.FullName} ON {target} ";

            var trigTypeId = el.Find("Property", ("Name", "SqlTriggerType")).FirstOrDefault()?.Attribute("Value")?.Value;
            var trigType = trigTypeId == "2" ? "AFTER " : "BEFORE ";

            if (el.Find("Property", ("Name", "IsInsertTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                trigType += "INSERT, ";
            }
            if (el.Find("Property", ("Name", "IsUpdateTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                trigType += "UPDATE, ";
            }
            if (el.Find("Property", ("Name", "IsDeleteTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True")
            {
                trigType += "DELETE, ";
            }

            def += trigType.Trim(',', ' ').TrimEnd()
                + "\r\nAS\r\n"
                + el.Find("Property", ("Name", "BodyScript")).First().Element("Value")?.Value.TrimStart();
            trig.Definition = def;
        }

        private static void parseSynonymElement(SchemaModel schema, XElement el, string name)
        {
            var syn = new SynonymModel(
                schema: schema,
                name: name,
                baseObject: el.Find("Property", ("Name", "ForObjectScript")).First().Element("Value")?.Value ?? string.Empty
            );
            schema.Synonyms[name] = syn;
        }

        private static void parseForeignKeyElement(DatabaseModel db, XElement el)
        {
            var refFieldPath = el.Find("Relationship", ("Name", "Columns"))
                .Elements("Entry")
                .Elements("References")?
                .Single().Attribute("Name")?.Value;
            if (refFieldPath is null)
            {
                // TODO: log bad ref
                return;
            }
            var dtblName = refFieldPath[..refFieldPath.LastIndexOf('.')];
            if (!db.TryGet<TableModel>(dtblName, out var dtbl)
                || !dtbl.Fields.TryGetValue(v => v.FullName == refFieldPath, out var dfld))
            {
                // TODO: log unknown def table/field
                return;
            }

            refFieldPath = el.Find("Relationship", ("Name", "ForeignColumns"))
                .Elements("Entry")
                .Elements("References")?
                .Single().Attribute("Name")?.Value;
            if (refFieldPath is null)
            {
                // TODO: log bad ref
                return;
            }
            var ftblName = refFieldPath[..refFieldPath.LastIndexOf('.')];
            if (!db.TryGet<TableModel>(ftblName, out var ftbl)
                || !ftbl.Fields.TryGetValue(v => v.FullName == refFieldPath, out var ffld))
            {
                // TODO: log unknown foreign table
                return;
            }

            if (dfld.HasReference)
            {
                // TODO: log duplicate reference
                return;
            }

            dfld.Ref = new FieldRefModel(dfld, ffld);
            // TODO: naming
        }

        private static void parsePrimaryKeyElement(DatabaseModel db, XElement el)
        {
            var tbl = el.Find("Relationship", ("Name", "DefiningTable")).Single()
                .Elements("Entry")?
                .Elements("References")?.Attributes("Name")?
                .Select(a => db.Get(a.Value) as TableModel)
                .SingleOrDefault();
            var flds = el.Find("Relationship", ("Name", "ColumnSpecifications")).Single()
                .Element("Entry")?
                .Find("Element", ("Type", "SqlIndexedColumnSpecification"))
                .Find("Relationship", ("Name", "Column"))
                .Elements("Entry")?
                .Elements("References")?.Attributes("Name")?
                .Select(a => tbl?.Fields.SingleOrDefault(f => f.FullName == a.Value))
                .NotNull().ToArray() ?? Array.Empty<FieldModel>();
            if (tbl is null || flds.Length == 0)
            {
                // TODO: log bad pkey
                return;
            }

            foreach (var fld in flds)
            {
                fld.IsPrimaryKey = true;
                // TODO: IsPrimaryKeyUnclustered
            }
        }

        private static void parseUniqueElement(DatabaseModel db, XElement el)
        {
            var tbl = el.Find("Relationship", ("Name", "DefiningTable")).Single()
                .Elements("Entry")?
                .Elements("References")?.Attributes("Name")?
                .Select(a => db.Get(a.Value) as TableModel)
                .SingleOrDefault();
            var flds = el.Find("Relationship", ("Name", "ColumnSpecifications")).Single()
                .Element("Entry")?
                .Find("Element", ("Type", "SqlIndexedColumnSpecification"))
                .Find("Relationship", ("Name", "Column"))
                .Elements("Entry")?
                .Elements("References")?.Attributes("Name")?
                .Select(a => tbl?.Fields.SingleOrDefault(f => f.FullName == a.Value))
                .NotNull().ToArray() ?? Array.Empty<FieldModel>();
            if (tbl is null || flds.Length != 1)
            {
                // TODO: log bad unique
                return;
            }

            flds.Single().IsUnique = true;
        }

        private static void parseCheckElement(DatabaseModel db, XElement el)
        {
            var tblName = el.Find("Relationship", ("Name", "DefiningTable")).Single()
                .Element("Entry")?.Element("References")?.Attribute("Name")?.Value;
            var checkExpr = el.Find("Property", ("Name", "CheckExpressionScript")).Single().Element("Value")?.Value;
            if (tblName is null || checkExpr is null
                || !db.TryGet<TableModel>(tblName, out var tbl))
            {
                // TODO: log bad check
                return;
            }

            var chkName = el.Attribute("Name")?.Value;
            chkName = chkName != null ? getName(chkName, tbl.Schema.Name) : null;
            var chk = new TableCheckModel(tbl, chkName, checkExpr)
            {
                Dependencies = resolveDependencies(el, "CheckExpressionDependencies")
            };
            tbl.Checks.Add(chk);
        }

        private static void parseDefaultElement(DatabaseModel db, XElement el)
        {
            var tblName = el.Find("Relationship", ("Name", "DefiningTable")).Single()
                .Element("Entry")?.Element("References")?.Attribute("Name")?.Value;
            var fldName = el.Find("Relationship", ("Name", "ForColumn")).Single()
                .Element("Entry")?.Element("References")?.Attribute("Name")?.Value;
            var defValue = el.Find("Property", ("Name", "DefaultExpressionScript")).Single().Element("Value")?.Value;
            if (tblName is null || fldName is null || defValue is null
                || !db.TryGet<TableModel>(tblName, out var tbl)
                || !tbl.Fields.TryGetValue(f => f.FullName == fldName, out var fld))
            {
                // TODO: log bad ref
                return;
            }

            fld.Default = new FieldDefaultModel(fld, null, defValue)
            {
                Dependencies = resolveDependencies(el, "ExpressionDependencies")
            };
        }

        private static string toArg(XElement a)
        {
            var name = getName(a).Split('.')[^1].Trim('[', ']');
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

        private static FieldModel toField(TableModel table, int ord, XElement colXml)
        {
            var field = new FieldModel(
                table: table,
                name: getName(colXml, table.FullName)
            )
            {
                Identity = colXml.Find("Property", ("Name", "IsIdentity")).FirstOrDefault()?.Attribute("Value")?.Value.ToLower() == "true",
                Order = ord,
                Nullable = colXml.Find("Property", ("Name", "IsNullable")).FirstOrDefault()?.Attribute("Value")?.Value.ToLower() != "false",
                // Ref done later
            };

            // Type / computed
            if (colXml.Attribute("Type")?.Value == "SqlComputedColumn")
            {
                var expr = colXml.Find("Property", ("Name", "ExpressionScript")).First().Element("Value")?.Value;
                field.Computation = expr;
                // TODO: may need to add to dependencies
            }
            else
            {
                var typeXml = colXml.Descendants("Element")
                    .First(e => e.Attribute("Type")?.Value == "SqlTypeSpecifier" || e.Attribute("Type")?.Value == "SqlXmlTypeSpecifier");
                field.Type = getSqlType(typeXml);
            }

            return field;
        }
    }
}
