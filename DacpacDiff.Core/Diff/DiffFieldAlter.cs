using DacpacDiff.Core.Model;
using System.Diagnostics;

namespace DacpacDiff.Core.Diff;

// TODO: should be individual diff per type of alter?
public class DiffFieldAlter : IDifference, IDataLossChange, IChangeProvider
{
    public const string TITLE = "Alter table field";

    public FieldModel TargetField { get; }
    public FieldModel CurrentField { get; }

    public IModel Model => TargetField;
    public string Name => $"{TargetField.Table.FullName}.[{TargetField.Name}]";
    public string Title => TITLE;

    public DiffFieldAlter(FieldModel tgt, FieldModel cur)
    {
        TargetField = tgt ?? throw new ArgumentNullException(nameof(tgt));
        CurrentField = cur ?? throw new ArgumentNullException(nameof(cur));
    }

    public bool GetDataLossTable(out string tableName)
    {
        // TODO: More accurate test
        // numeric precision: decimal(x,y) = (x-y).y = if tgt > cur, dataloss
        tableName = CurrentField.Table.FullName;
        return TargetField.Type != CurrentField.Type;
    }

    public IEnumerable<IDifference> GetAdditionalChanges()
    {
        var diffs = new List<IDifference>();

        // Will need to drop certain dependencies before can alter
        var rdeps = CurrentField.Dependents;
        if (rdeps.Length == 0)
        {
            return diffs;
        }
        var ldeps = TargetField.Dependents.ToDictionary(d => d.FullName);
        foreach (var rdep in rdeps)
        {
            if (!ldeps.TryGetValue(rdep.FullName, out var ldep))
            {
                continue;
            }

            switch (rdep)
            {
                case IndexModuleModel:
                    diffs.Add(new DiffObjectDrop((ModuleModel)rdep));
                    diffs.Add(new DiffModuleCreate((ModuleModel)ldep));
                    break;
                case FunctionModuleModel:
                    // NOOP
                    break;
                default:
                    Debugger.Break(); // Review
                    break;
            }
        }

        return diffs;
    }
}
