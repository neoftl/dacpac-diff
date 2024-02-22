using DacpacDiff.Core.Model;
using System.Diagnostics;

namespace DacpacDiff.Core.Diff;

// TODO: should be individual diff per type of alter?
public class DiffFieldAlter : IDifference, IDataLossChange, IChangeProvider
{
    public const string TITLE = "Alter table field";

    public FieldModel LeftField { get; }
    public FieldModel RightField { get; }

    public IModel Model => LeftField;
    public string Name => $"{LeftField.Table.FullName}.[{LeftField.Name}]";
    public string Title => TITLE;

    public DiffFieldAlter(FieldModel lft, FieldModel rgt)
    {
        LeftField = lft ?? throw new ArgumentNullException(nameof(lft));
        RightField = rgt ?? throw new ArgumentNullException(nameof(rgt));
    }

    public bool GetDataLossTable(out string tableName)
    {
        // TODO: More accurate test
        // numeric precision: decimal(x,y) = (x-y).y = if lft > rgt, dataloss
        tableName = RightField.Table.FullName;
        return LeftField.Type != RightField.Type;
    }

    public IEnumerable<IDifference> GetAdditionalChanges()
    {
        var diffs = new List<IDifference>();

        // Will need to drop certain dependencies before can alter
        var rdeps = RightField.Dependents;
        if (rdeps.Length == 0)
        {
            return diffs;
        }
        var ldeps = LeftField.Dependents.ToDictionary(d => d.FullName);
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
