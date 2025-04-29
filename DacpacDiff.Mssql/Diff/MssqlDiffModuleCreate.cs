using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff;

public class MssqlDiffModuleCreate : BaseMssqlDiffBlock<DiffModuleCreate>
{
    public bool UseStub { get; init; }
    public bool DoAsAlter { get; init; }

    public MssqlDiffModuleCreate(DiffModuleCreate diff)
        : base(diff)
    {
        UseStub = diff.Module.StubOnCreate;
        DoAsAlter = diff.DoAsAlter;
    }

    protected override void GetFormat(ISqlFileBuilder sb)
    {
        if (_diff.Module is UniqueConstraintModel uqMod)
        {
            // NOTE: Will never be DoAsAlter; will be a full recreate
            sb.Append("ALTER TABLE ").Append(uqMod.DefiningObjectFullName)
                .Append(" ADD")
                .AppendIf(() => $" CONSTRAINT [{uqMod.Name}]", !uqMod.IsSystemNamed)
                .Append(" UNIQUE ").Append(uqMod.IsClustered ? "CLUSTERED" : "NONCLUSTERED").EnsureLine()
                .AppendLine("(");
            var first = true;
            foreach (var fld in uqMod.Columns)
            {
                sb.AppendIf(() => ",\r\n", !first)
                    .Append($"    [{fld}] ASC"); // TODO
                first = false;
            }
            sb.EnsureLine()
                .Append(") WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]") // TODO
                .EnsureLine();
            return;
        }

        sb.Append(DoAsAlter ? "ALTER " : "CREATE ");

        switch (_diff.Module)
        {
            case FunctionModuleModel funcMod:
                sb.AppendLine($"FUNCTION {funcMod.FullName} (");
                if (funcMod.Parameters.Length > 0)
                {
                    var argSql = funcMod.Parameters.Select(p => $"    {p.Name} {p.Type}"
                        + (p.HasDefault ? $" = {p.DefaultValue}" : "")
                        + (p.IsReadOnly ? " READONLY" : "")
                        + (p.IsOutput ? " OUTPUT" : "")).ToArray();
                    sb.AppendLine(string.Join(",\r\n", argSql));
                }
                sb.Append(") RETURNS ");

                if (funcMod.ReturnTable != null)
                {
                    var tblFields = funcMod.ReturnTable.Fields.Select(f => $"    [{f.Name}] {f.Type}"
                        + (!f.Nullable ? " NOT NULL" : ""));
                    sb.AppendLine($"{funcMod.ReturnType} TABLE (")
                        .AppendLine(string.Join(",\r\n", tblFields))
                        .Append(") AS ");
                }
                else if (funcMod.ReturnType == "TABLE")
                {
                    sb.AppendLine("TABLE")
                        .AppendLine("AS")
                        .Append("    RETURN ");
                }
                else
                {
                    sb.AppendLine(funcMod.ReturnType)
                        .AppendIf(() => "WITH RETURNS NULL ON NULL INPUT", funcMod.ReturnNullForNullInput).EnsureLine()
                        .Append("AS ");
                }

                if (UseStub)
                {
                    appendStub(funcMod, sb);
                }
                else
                {
                    sb.Append(funcMod.Body);
                }
                return;

            case IndexModuleModel idxMod:
                sb.AppendIf(() => "UNIQUE ", idxMod.IsUnique)
                    .AppendIf(() => "CLUSTERED ", idxMod.IsClustered)
                    .Append($"INDEX [{idxMod.Name}] ON {idxMod.IndexedObjectFullName} ([")
                    .Append(string.Join("], [", idxMod.IndexedColumns))
                    .Append("])")
                    .AppendIf(() => " INCLUDE ([" + string.Join("], [", idxMod.IncludedColumns) + "])", idxMod.IncludedColumns.Length > 0)
                    .AppendIf(() => " WHERE " + idxMod.Condition, idxMod.Condition != null)
                    .AppendLine();
                return;

            case ProcedureModuleModel procMod:
                sb.AppendLine($"PROCEDURE {procMod.FullName}");

                if (procMod.Parameters.Length > 0)
                {
                    var argSql = procMod.Parameters.Select(p => $"    {p.Name} {p.Type}"
                        + (p.HasDefault ? $" = {p.DefaultValue}" : "")
                        + (p.IsReadOnly ? " READONLY" : "")
                        + (p.IsOutput ? " OUTPUT" : "")).ToArray();
                    sb.AppendLine(string.Join(",\r\n", argSql));
                }
                if (procMod.ExecuteAs?.Length > 0)
                {
                    sb.AppendLine("WITH EXECUTE AS " + procMod.ExecuteAs);
                }

                sb.Append("AS ");

                if (UseStub)
                {
                    appendStub(procMod, sb);
                }
                else
                {
                    sb.Append(procMod.Body);
                }
                return;

            case TriggerModuleModel trigMod:
                sb.AppendLine($"TRIGGER {trigMod.FullName} ON {trigMod.Parent}")
                    .AppendIf(() => $"    WITH EXECUTE AS {trigMod.ExecuteAs}\r\n", trigMod.ExecuteAs?.Length > 0)
                    .Append(trigMod.Before ? "    AFTER " : "    FOR ")
                    .AppendIf(() => "INSERT, ", trigMod.ForInsert)
                    .AppendIf(() => "UPDATE, ", trigMod.ForUpdate)
                    .AppendIf(() => "DELETE, ", trigMod.ForDelete)
                    .Remove(2)
                    .Append("\r\nAS\r\n")
                    .Append(trigMod.Body)
                    .EnsureLine();
                return;

            case ViewModuleModel viewMod:
                // TODO: SCHEMABINDING
                sb.Append($"VIEW {viewMod.FullName} AS ");

                if (UseStub)
                {
                    appendStub(viewMod, sb);
                }
                else
                {
                    sb.Append(viewMod.Body);
                }
                return;
        }

        throw new NotImplementedException(_diff.Module.GetType().ToString());
    }

    private static void appendStub(FunctionModuleModel funcMod, ISqlFileBuilder sb)
    {
        if (funcMod.ReturnTable != null)
        {
            sb.AppendLine("BEGIN")
                .AppendLine("    RETURN")
                .AppendLine("END");
        }
        else if (funcMod.ReturnType == "TABLE")
        {
            sb.AppendLine("SELECT 1 A");
        }
        else
        {
            sb.AppendLine("BEGIN")
                .AppendLine("    RETURN NULL")
                .AppendLine("END");
        }
    }

#pragma warning disable IDE0060 // Remove unused parameter
    private static void appendStub(ProcedureModuleModel procMod, ISqlFileBuilder sb)
    {
        sb.AppendLine("RETURN 0");
    }

    private static void appendStub(ViewModuleModel viewMod, ISqlFileBuilder sb)
    {
        sb.AppendLine("SELECT 1 A");
    }
#pragma warning restore IDE0060 // Remove unused parameter
}
