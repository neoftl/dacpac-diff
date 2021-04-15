using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;
using System.Text.RegularExpressions;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffModuleCreate : BaseMssqlDiffBlock<DiffModuleCreate>
    {
        public MssqlDiffModuleCreate(DiffModuleCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            switch (_diff.Module.Type)
            {
                case ModuleModel.ModuleType.FUNCTION:
                    sb.AppendLine("BEGIN").AppendLine("    RETURN NULL").Append("END");

                    var m = Regex.Match(_diff.Module.Definition, @"(?is)^(.*?\sAS\s)");
                    var sql = m.Groups[1].Value;
                    sb.AppendLine(sql.Trim());

                    if (sql.TryMatch(@"(?i)RETURNS\s+(@\w+\s+)?TABLE\s", out m))
                    {
                        if (m.Groups[1].Success)
                        {
                            sb.AppendLine("BEGIN").AppendLine("    RETURN").AppendLine("END");
                        }
                        else
                        {
                            sb.AppendLine("    RETURN SELECT 1 A");
                        }
                    }
                    return;
                case ModuleModel.ModuleType.PROCEDURE:
                    sb.AppendLine($"CREATE PROCEDURE {_diff.Module.FullName} AS RETURN 0");
                    return;
                case ModuleModel.ModuleType.VIEW:
                    sb.AppendLine($"CREATE VIEW {_diff.Module.FullName} AS SELECT 1 A");
                    return;
            }

            sb.Append(_diff.Module.Definition).EnsureLine();
        }
    }
}
