using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Text;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlObjectDrop : BaseMssqlDiffBlock<DiffObjectDrop>
    {
        private const string MOD_DEF_PATTERN = @"(?i)ON\s+((?:\[[^\]]+\]\s*\.|\w+\s*\.)?\s*(?:\[[^\]]+\]|\w+))\s*\(";

        public MssqlObjectDrop(DiffObjectDrop diff)
            : base(diff)
        { }

        protected override void GetFormat(StringBuilder sb, bool checkForDataLoss, bool prettyPrint)
        {
            if (_diff.Type == DiffObjectDrop.ObjectType.INDEX && _diff.Model is ModuleModel idx)
            {
                var m = System.Text.RegularExpressions.Regex.Match(idx.Definition, MOD_DEF_PATTERN);
                if (!m.Success)
                {
                    Console.Error.WriteLine($"Cannot drop INDEX {_diff.Name} using this scheme");
                    return;
                }
                sb.Append($"DROP INDEX [{idx.Name}] ON {m.Groups[1].Value}");
                return;
            }

            sb.Append($"DROP {_diff.Type} {_diff.Name}");
        }
    }
}
