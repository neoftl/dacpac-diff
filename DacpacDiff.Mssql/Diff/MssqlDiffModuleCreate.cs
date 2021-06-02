using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;
using System;
using System.Linq;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffModuleCreate : BaseMssqlDiffBlock<DiffModuleCreate>
    {
        public MssqlDiffModuleCreate(DiffModuleCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            switch (_diff.Module)
            {
                case FunctionModuleModel funcMod: // Stub
                    sb.AppendLine($"CREATE FUNCTION {funcMod.FullName} (");
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
                            .AppendLine(") AS BEGIN")
                            .AppendLine("    RETURN")
                            .AppendLine("END");
                    }
                    else if (funcMod.ReturnType == "TABLE")
                    {
                        sb.AppendLine("TABLE")
                            .AppendLine("AS")
                            .AppendLine("    RETURN SELECT 1 A");
                    }
                    else
                    {
                        sb.AppendLine(funcMod.ReturnType)
                            .AppendIf(() => "WITH RETURNS NULL ON NULL INPUT", funcMod.ReturnNullForNullInput).EnsureLine()
                            .AppendLine("AS BEGIN")
                            .AppendLine("    RETURN NULL")
                            .AppendLine("END");
                    }
                    return;

                case IndexModuleModel idxMod:
                    sb.Append("CREATE ")
                        .AppendIf(() => "UNIQUE ", idxMod.IsUnique)
                        .AppendIf(() => "CLUSTERED ", idxMod.IsClustered)
                        .Append($"INDEX [{idxMod.Name}] ON {idxMod.IndexedObject} ([")
                        .Append(string.Join("], [", idxMod.IndexedColumns))
                        .Append("])")
                        .AppendIf(() => " INCLUDE ([" + string.Join("], [", idxMod.IncludedColumns) + "])", idxMod.IncludedColumns.Length > 0)
                        .AppendIf(() => " WHERE " + idxMod.Condition, idxMod.Condition != null)
                        .AppendLine();
                    return;

                case ProcedureModuleModel procMod: // Stub
                    sb.AppendLine($"CREATE PROCEDURE {procMod.FullName} AS RETURN 0");
                    return;

                case TriggerModuleModel trigMod:
                    sb.Append($"CREATE TRIGGER {trigMod.FullName} ON {trigMod.Parent} ")
                        .Append(trigMod.Before ? "AFTER " : "FOR ")
                        .AppendIf(() => "INSERT", trigMod.ForUpdate)
                        .AppendIf(() => ", ", trigMod.ForUpdate && (trigMod.ForInsert || trigMod.ForDelete))
                        .AppendIf(() => "UPDATE", trigMod.ForInsert)
                        .AppendIf(() => ", ", trigMod.ForInsert && trigMod.ForDelete)
                        .AppendIf(() => "DELETE", trigMod.ForDelete)
                        .Append("\r\nAS\r\n")
                        .Append(trigMod.Body)
                        .EnsureLine();
                    return;

                case ViewModuleModel viewMod: // Stub
                    // TODO: SCHEMABINDING
                    sb.AppendLine($"CREATE VIEW {viewMod.FullName} AS SELECT 1 A");
                    return;
            }

            throw new NotImplementedException(_diff.Module.GetType().ToString());
        }
    }
}
