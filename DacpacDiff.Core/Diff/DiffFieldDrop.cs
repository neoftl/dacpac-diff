using DacpacDiff.Core.Model;
using System.Diagnostics;

namespace DacpacDiff.Core.Diff;

public class DiffFieldDrop : IDifference, IDataLossChange, IChangeProvider
{
    public const string TITLE = "Drop table field";

    public FieldModel Field { get; }

    public IModel Model => Field;
    public string Name => $"{Field.Table.FullName}.[{Field.Name}]";
    public string Title => TITLE;

    public DiffFieldDrop(FieldModel field)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));
    }

    public bool GetDataLossTable(out string tableName)
    {
        tableName = Field.Table.FullName;
        return true;
    }

    public IEnumerable<IDifference> GetAdditionalChanges()
    {
        var diffs = new List<IDifference>();

        // Will need to drop certain dependencies
        foreach (var dep in Field.Dependents)
        {
            switch (dep)
            {
                case IndexModuleModel:
                    diffs.Add(new DiffObjectDrop((ModuleModel)dep));
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
