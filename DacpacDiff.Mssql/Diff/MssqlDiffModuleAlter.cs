using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;
using System;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffModuleAlter : BaseMssqlDiffBlock<DiffModuleAlter>
    {
        public MssqlDiffModuleAlter(DiffModuleAlter diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            var type = _diff.Module.Type switch
            {
                ModuleModel.ModuleType.INDEX => @"(?:UNIQUE\s+)?INDEX",
                ModuleModel.ModuleType.PROCEDURE => "PROC(?:EDURE)?",
                _ => _diff.Module.Type.ToString(),
            };
            if (!_diff.Module.Definition.TryMatch($@"^(?is)(.*?\b)?CREATE\s+{type}\s+(.+)$", out var m))
            {
                throw new InvalidOperationException($"Could not locate 'CREATE {_diff.Module.Type}' for [{_diff.Module.Schema.Name}].[{_diff.Module.Name}]");
            }

            sb.EnsureLine().Append(m.Groups[1].Value.Trim()).EnsureLine()
                .AppendLine($"ALTER {_diff.Module.Type} " + m.Groups[2].Value);
        }
    }
}
