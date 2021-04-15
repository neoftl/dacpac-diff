using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;
using System;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffObjectDrop : BaseMssqlDiffBlock<DiffObjectDrop>
    {
        private const string MOD_DEF_PATTERN = @"(?i)ON\s+((?:\[[^\]]+\]\s*\.|\w+\s*\.)?\s*(?:\[[^\]]+\]|\w+))\s*\(";

        public MssqlDiffObjectDrop(DiffObjectDrop diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
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
