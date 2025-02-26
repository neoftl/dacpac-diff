﻿using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff;

public class MssqlDiffFieldDrop(DiffFieldDrop diff)
    : BaseMssqlDiffBlock<DiffFieldDrop>(diff)
{
    protected override void GetFormat(ISqlFileBuilder sb)
    {
        // TODO: ref

        // Drop unnamed uniqueness
        if (_diff.Field.IsUnique)
        {
            sb.DROP_UNNAMED_UNIQUE(_diff.Field);
        }

        // Drop unnamed default
        if (_diff.Field.HasDefault && _diff.Field.DefaultName?.Length is null or 0)
        {
            sb.Append("EXEC #usp_DropUnnamedDefault ")
                .Append($"'{_diff.Field.Table.FullName}', ")
                .Append($"'{_diff.Field.Name}'")
                .AppendGo();
        }

        var fld = _diff.Field;
        sb.AppendLine($"ALTER TABLE {fld.Table.FullName} DROP COLUMN [{fld.Name}]");
    }
}
