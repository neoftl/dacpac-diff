using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffTableCheckDrop : BaseMssqlDiffBlock<DiffTableCheckDrop>
    {
        public MssqlDiffTableCheckDrop(DiffTableCheckDrop diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            if (_diff.TableCheck.IsSystemNamed)
            {
                var cleanSql = _diff.TableCheck.Definition.ScrubSQL().Replace("'", "''");
                var md5 = MD5.HashData(Encoding.UTF8.GetBytes(cleanSql));
                var md5Str = string.Join("", md5.Select(b => b.ToString("x2")));

                sb.AppendLine($"EXEC #usp_DropUnnamedCheckConstraint '{_diff.TableCheck.Table.FullName}', 0x{md5Str}");
            }
            else
            {
                sb.AppendLine($"ALTER TABLE {_diff.TableCheck.Table.FullName} DROP CONSTRAINT [{_diff.TableCheck.Name}]");
            }
        }
    }
}
