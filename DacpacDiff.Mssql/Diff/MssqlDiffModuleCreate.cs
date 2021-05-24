using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;
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
                case FunctionModuleModel funcMod:
                    // Function SQL
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
                            .AppendIf("WITH RETURNS NULL ON NULL INPUT", funcMod.ReturnNullForNullInput).EnsureLine()
                            .AppendLine("AS BEGIN")
                            .AppendLine("    RETURN NULL")
                            .AppendLine("END");
                    }
                    return;
                case ProcedureModuleModel procMod:
                    sb.AppendLine($"CREATE PROCEDURE {procMod.FullName} AS RETURN 0");
                    return;
                case ViewModuleModel viewMod:
                    sb.AppendLine($"CREATE VIEW {viewMod.FullName} AS SELECT 1 A");
                    return;
            }

            sb.Append(_diff.Module.Definition).EnsureLine();
        }
    }
}
