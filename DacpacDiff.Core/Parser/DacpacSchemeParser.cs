using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Xml.Linq;

namespace DacpacDiff.Core.Parser;

public class DacpacSchemeParser : ISchemeParser
{
    private static readonly (string type, Action<DatabaseModel, XElement> parser)[] PARSERS =
    [
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
    ];

    [ExcludeFromCodeCoverage(Justification = "Requires file containing zip")]
    public SchemeModel? ParseFile(string filename)
    {
        var name = Path.GetFileNameWithoutExtension(filename);
        if (name == "blank")
        {
            return ParseContent("blank", "<blank />");
        }

        // Extract model.xml from zip
        string? modelData = null;
        using (var zip = ZipFile.OpenRead(filename))
        {
            var modelEntry = zip.Entries.First(e => e.FullName == "model.xml")
                ?? throw new InvalidOperationException("Invalid dacpac provided");

            // Get model element from content
            using var sr = new StreamReader(modelEntry.Open());
            modelData = sr.ReadToEnd();
            modelData = modelData.Replace("xmlns=\"http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02\"", "");
        }

        return modelData != null ? ParseContent(name, modelData) : null;
    }

    internal static SchemeModel ParseContent(string name, string content)
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
        foreach (var el in elsByType.Get("SqlSchema") ?? [])
        {
            var elName = getName(el.Attribute("Name")?.Value ?? throw new MissingMemberException("SqlSchema", "Name"));
            db.Schemas[elName] = new SchemaModel(db, elName);
        }
        elsByType.Remove("SqlSchema");
        foreach (var (type, parser) in PARSERS)
        {
            var els = elsByType.Get(type) ?? [];
            foreach (var el in els)
            {
                parser(db, el);
            }
            elsByType.Remove(type);
        }

        if (elsByType.Keys.Count != 0)
        {
            // TODO: unhandled types
        }

        // Check model validity
        foreach (var idx in db.Schemas.Values.SelectMany(s => s.Modules.Values.OfType<IndexModuleModel>()))
        {
            if (!idx.MapTarget(db))
            {
                // TODO: log bad index
                idx.Schema.Modules.Remove(idx.Name);
            }
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
        if (name.Length > 0 && prefix != null)
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
        return (name.Length > 2 && name[0] == '[' && name[^1] == ']' && name.IndexOf('[', 1) < 0)
            ? name[1..^1]
            : name;
    }

    private static string getSqlType(XElement el, string? collation = null)
    {
        var sqltype = getName(el.Find("Relationship", ("Name", "Type")).First()
            .Element("Entry")?
            .Element("References"));

        var len = el.Find("Property", ("Name", "Length")).FirstOrDefault()?.Attribute("Value")?.Value;
        if (len != null)
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
            if (prec != null)
            {
                var scale = el.Find("Property", ("Name", "Scale")).FirstOrDefault()?.Attribute("Value")?.Value;
                sqltype += $"({prec}, {scale})";
            }
        }
        if (collation != null)
        {
            sqltype += " COLLATION " + collation;
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
        return depEls ?? [];
    }

    private static void withSchema(DatabaseModel db, XElement el, Action<SchemaModel, XElement, string> parser)
    {
        var name = el.Attribute("Name")?.Value ?? throw new InvalidDataException($"Element {el.Name} missing required 'Name' attribute");
        var schemaName = getName(name.Split('.')[0]);
        if (!db.TryGet<SchemaModel>(schemaName, out var schema))
        {
            throw new IndexOutOfRangeException($"Unknown schema: {schemaName}");
        }
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
        if (el.Find("Property", ("Name", "IsAutoGeneratedHistoryTable"), ("Value", "True")).Length != 0
            || el.Parent?.Find(true, "Relationship", ("Name", "TemporalSystemVersioningHistoryTable")).Any(e => e.Find(true, "References", ("Name", table.FullName)).Length != 0) == true)
        {
            return;
        }

        schema.Tables[name] = table;

        // Fields
        var idx = 0;
        var colsXml = el.Find(true, "Element", ("Type", a => a is "SqlSimpleColumn" or "SqlComputedColumn"), ("Name", a => a?.StartsWith(table.FullName, StringComparison.OrdinalIgnoreCase) == true));
        table.Fields = colsXml.Select(e => toField(table, ++idx, e)).ToArray();

        // Temporality
        var temporalXml = el.Find("Relationship", ("Name", "TemporalSystemVersioningHistoryTable")).FirstOrDefault();
        if (temporalXml != null)
        {
            table.Temporality = new TemporalityModel(table)
            {
                HistoryTable = temporalXml.Element("Entry")?.Element("References")?.Attribute("Name")?.Value ?? string.Empty,
                PeriodFieldFrom = getName(colsXml.FirstOrDefault(e => e.Find("Property", ("Name", "GeneratedAlwaysType"), ("Value", "1")).Length != 0), table.FullName),
                PeriodFieldTo = getName(colsXml.FirstOrDefault(e => e.Find("Property", ("Name", "GeneratedAlwaysType"), ("Value", "2")).Length != 0), table.FullName),
            };
        }
    }

    private static void parseViewElement(SchemaModel schema, XElement el, string name)
    {
        var view = new ViewModuleModel(
            schema: schema,
            name: name
        )
        {
            Dependencies = resolveDependencies(el, "QueryDependencies")
        };
        schema.Modules[name] = view;

        // TODO: maintain comment
        view.Body = el.Find("Property", ("Name", "QueryScript")).First().Element("Value")?.Value ?? string.Empty;
    }

    private static void parseProcedureElement(SchemaModel schema, XElement el, string name)
    {
        var proc = new ProcedureModuleModel(
            schema: schema,
            name: name
        )
        {
            Dependencies = resolveDependencies(el, "BodyDependencies")
        };
        schema.Modules[name] = proc;

        var idx = 0;
        proc.Parameters = el.Find("Relationship", ("Name", "Parameters")).SingleOrDefault()?
            .Elements("Entry").SelectMany(e => e.Find("Element", ("Type", "SqlSubroutineParameter")))
            .Select(e => toParam(proc, e, ++idx)).ToArray() ?? [];

        proc.ExecuteAs = el.Find("Property", ("Name", "IsCaller"), ("Value", "True"))?.Length > 0
            ? "CALLER"
            : el.Find("Property", ("Name", "IsOwner"), ("Value", "True"))?.Length > 0
            ? "OWNER" : null;

        // TODO: maintain comment
        proc.Body = el.Find("Property", ("Name", "BodyScript")).First().Element("Value")?.Value.Trim() ?? string.Empty;
    }

    private static void parseIndexElement(SchemaModel schema, XElement el, string name)
    {
        var target = el.Find("Relationship", ("Name", "IndexedObject")).Single()
            .Element("Entry")?
            .Element("References")?.Attribute("Name")?.Value ?? string.Empty;
        name = getName(el, target);

        var idx = new IndexModuleModel(
            schema: schema,
            name: name
        )
        {
            // TODO: system named
            IndexedObjectFullName = target,
            Dependencies = resolveDependencies(el, "BodyDependencies"),

            IsUnique = el.Find("Property", ("Name", "IsUnique"), ("Value", "True")).Length != 0,
            IsClustered = el.Find("Property", ("Name", "IsClustered"), ("Value", "True")).Length != 0,
        };
        schema.Modules[name] = idx;

        idx.IndexedColumns = el.Find("Relationship", ("Name", "ColumnSpecifications")).Single()
            .Elements("Entry")
            .SelectMany(e => e.Find("Element", ("Type", "SqlIndexedColumnSpecification")))
            .SelectMany(e => e.Find("Relationship", ("Name", "Column")))
            .Select(e => getName(e.Element("Entry")?.Element("References"), target))
            .ToArray();

        idx.IncludedColumns = el.Find("Relationship", ("Name", "IncludedColumns"))
            .SelectMany(e => e.Elements("Entry").Select(r => getName(r.Element("References"), target)))
            .ToArray();

        var predsXml = el.Find("Property", ("Name", "FilterPredicate")).FirstOrDefault();
        if (predsXml != null)
        {
            var script = predsXml.Element("Value")?.Value ?? string.Empty;
            idx.Condition = $"({script.ReduceBrackets()})"; // TODO: can reference columns
        }
    }

    private static void parseFunctionElement(SchemaModel schema, XElement el, string name)
    {
        var func = new FunctionModuleModel(
            schema: schema,
            name: name
        )
        {
            Dependencies = resolveDependencies(el, "BodyDependencies")
        };
        schema.Modules[name] = func;

        func.ExecuteAs = el.Find("Property", ("Name", "IsCaller"), ("Value", "True"))?.Length > 0
            ? "CALLER"
            : el.Find("Property", ("Name", "IsOwner"), ("Value", "True"))?.Length > 0
            ? "OWNER" : null;
        func.ReturnNullForNullInput = el.Find("Property", ("Name", "DoReturnNullForNullInput"), ("Value", "True"))?.Any() == true;

        var idx = 0;
        func.Parameters = el.Find("Relationship", ("Name", "Parameters")).SingleOrDefault()?
            .Elements("Entry").SelectMany(e => e.Find("Element", ("Type", "SqlSubroutineParameter")))
            .Select(e => toParam(func, e, ++idx)).ToArray() ?? [];

        if (el.Attribute("Type")?.Value == "SqlInlineTableValuedFunction")
        {
            func.ReturnType = "TABLE";
        }
        else if (el.Attribute("Type")?.Value == "SqlMultiStatementTableValuedFunction")
        {
            func.ReturnType = el.Find("Property", ("Name", "ReturnTableVariable")).FirstOrDefault()?.Attribute("Value")?.Value ?? string.Empty;

            idx = 0;
            var colsXml = el.Find("Relationship", ("Name", "Columns")).First().Find(true, "Element", ("Type", a => a is "SqlSimpleColumn" or "SqlComputedColumn"));
            var tbl = new TableModel(schema, func.Name);
            tbl.Fields = colsXml.Select(e => toField(tbl, ++idx, e)).ToArray();

            func.ReturnTable = tbl;
        }
        else
        {
            var typeXml = el.Find("Relationship", ("Name", "Type")).First().Find(true, "Element", ("Type", "SqlTypeSpecifier")).First();
            func.ReturnType = getSqlType(typeXml);
        }

        // TODO: maintain comment
        func.Body = el.Find(true, "Property", ("Name", "BodyScript"))?.First().Element("Value")?.Value.Trim() ?? string.Empty;
    }

    private static void parseTriggerElement(SchemaModel schema, XElement el, string name)
    {
        var trig = new TriggerModuleModel(
            schema: schema,
            name: name
        )
        {
            Dependencies = resolveDependencies(el, "BodyDependencies")
        };
        schema.Modules[name] = trig;

        trig.Parent = el.Find("Relationship", ("Name", "Parent")).Single()
            .Element("Entry")?
            .Element("References")?
            .Attribute("Name")?.Value ?? string.Empty;

        trig.ExecuteAs = el.Find("Property", ("Name", "IsCaller"), ("Value", "True"))?.Length > 0
            ? "CALLER"
            : el.Find("Property", ("Name", "IsOwner"), ("Value", "True"))?.Length > 0
            ? "OWNER" : null;

        trig.Before = el.Find("Property", ("Name", "SqlTriggerType")).FirstOrDefault()?.Attribute("Value")?.Value != "2";
        trig.ForDelete = el.Find("Property", ("Name", "IsDeleteTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True";
        trig.ForInsert = el.Find("Property", ("Name", "IsInsertTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True";
        trig.ForUpdate = el.Find("Property", ("Name", "IsUpdateTrigger")).FirstOrDefault()?.Attribute("Value")?.Value == "True";

        // TODO: maintain comment
        trig.Body = el.Find("Property", ("Name", "BodyScript")).First().Element("Value")?.Value.TrimStart() ?? string.Empty;
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
        var dtblName = refFieldPath?[..refFieldPath.LastIndexOf('.')] ?? string.Empty;
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
        var ftblName = refFieldPath?[..refFieldPath.LastIndexOf('.')] ?? string.Empty;
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
            .NotNull().ToArray() ?? [];
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
            .Elements("Entry")?
            .Find("Element", ("Type", "SqlIndexedColumnSpecification"))
            .Find("Relationship", ("Name", "Column"))
            .Elements("Entry")?
            .Elements("References")?.Attributes("Name")?
            .Select(a => tbl?.Fields.SingleOrDefault(f => f.FullName == a.Value))
            .NotNull().ToArray() ?? [];
        if (tbl is null || flds.Length == 0)
        {
            // TODO: log bad unique
            return;
        }

        if (flds.Length == 1)
        {
            flds.Single().IsUnique = true;
        }
        else
        {
            var schema = tbl.Schema;

            // TODO: naming
            //var tblName = el.Find("Relationship", ("Name", "DefiningTable")).Single()
            //    .Element("Entry")?.Element("References")?.Attribute("Name")?.Value;
            var uq = new UniqueConstraintModel(schema, null, tbl.FullName, flds.Select(f => f.Name).ToArray());
            schema.Modules[uq.Name] = uq;
        }
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

        var defName = el.Attribute("Name")?.Value;
        defName = defName != null ? getName(defName, tbl.FullName) : null;
        fld.Default = new FieldDefaultModel(fld, defName, defValue)
        {
            Dependencies = resolveDependencies(el, "ExpressionDependencies")
        };
    }

    private static ParameterModel toParam(IParameterisedModuleModel parent, XElement a, int order)
    {
        var typeXml = a.Find(true, "Element", ("Type", p => p is "SqlTypeSpecifier" or "SqlXmlTypeSpecifier")).First();

        var param = new ParameterModel(
            parent: parent,
            name: getName(a).Split('.')[^1][1..^1]
        )
        {
            Order = order,
            Type = getSqlType(typeXml),
            DefaultValue = a.Find("Property", ("Name", "DefaultExpressionScript")).FirstOrDefault()?.Element("Value")?.Value,
            IsReadOnly = a.Find("Property", ("Name", "IsReadOnly")).FirstOrDefault()?.Attribute("Value")?.Value == "True",
            IsOutput = a.Find("Property", ("Name", "IsOutput")).FirstOrDefault()?.Attribute("Value")?.Value == "True"
        };

        return param;
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
            var typeXml = colXml.Find(true, "Element", ("Type", a => a is "SqlTypeSpecifier" or "SqlXmlTypeSpecifier")).First();
            field.Type = getSqlType(typeXml);
            field.Collation = colXml.Find("Property", ("Name", "Collation")).FirstOrDefault()?.Attribute("Value")?.Value;
        }

        return field;
    }
}
