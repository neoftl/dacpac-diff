using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Changes;

/// <summary>
/// Alter the target object in-place
/// </summary>
public class AlterObject<T> : INoopDifference, IChangeProvider
    where T : IModel
{
    public T Model { get; }
    IModel IDifference.Model => Model;
    public IModel OldModel { get; }

    public virtual string Title => "Alter " + Model.Name;
    public string Name => Model.Name;

    public AlterObject(T tgt, T cur)
    {
        // We know that OldModel is the same object type as Model (C# and SQL)

        Model = tgt ?? throw new ArgumentNullException(nameof(tgt));
        OldModel = cur ?? throw new ArgumentNullException(nameof(cur));
    }

    public virtual IEnumerable<IDifference> GetAdditionalChanges()
    {
        var diffs = new List<IDifference>(5);
        AddChangesToDependents(diffs);

        if (Model is ModuleModel mod)
        {
            // Both are modules
            addModuleChanges(diffs, mod, (ModuleModel)OldModel);
            return diffs.ToArray();
        }

        // TODO: Neither are modules
        throw new NotImplementedException();
    }

    private static void addModuleChanges(IList<IDifference> diffs, ModuleModel module, ModuleModel oldModule)
    {
        // Alter index is always a soft recreate
        if (module.Type == ModuleModel.ModuleType.INDEX)
        {
            diffs.Add(new RecreateObject<ModuleModel>(module, oldModule));
            return;
        }

        // To/from full function return table must be a forced recreate
        if (module is FunctionModuleModel fn && oldModule is FunctionModuleModel ofn
            && (fn.ReturnTable is null) != (ofn.ReturnTable is null))
        {
            diffs.Add(new DiffModuleAlter(module, true));
            return;
        }

        diffs.Add(new DiffModuleAlter(module));
    }

    protected void AddChangesToDependents(IList<IDifference> diffs)
    {
        // Must drop all function dependencies in order to alter function
        if (Model is FunctionModuleModel func)
        {
            // Need all existing dependents that will remain after update
            var deps = func.Schema.Db.FindAllDependents(Model, typeof(FieldDefaultModel), typeof(TableCheckModel), typeof(FunctionModuleModel));

            // TODO: don't like this
            deps = deps.Where(d =>
            {
                var oldDB = ((FunctionModuleModel)OldModel).Schema.Db;

                if (d is TableCheckModel chk)
                {
                    return oldDB.TryGet<TableModel>(chk.Table.FullName, out var tbl) && tbl.Checks.Contains(chk); // If def changing, will already have a drop
                }
                if (d is FieldDefaultModel def)
                {
                    return oldDB.TryGet<TableModel>(def.Field.Table.FullName, out var tbl) && tbl.Fields.Any(f => f.Name == def.Field.Name && f.IsDefaultMatch(def.Field));
                }

                return oldDB.TryGet<IModel>(d.FullName, out _);
            }).ToArray();

            foreach (var dep in deps)
            {
                switch (dep)
                {
                    case FieldDefaultModel def:
                        diffs.Add(new RecreateObject<FieldDefaultModel>(def, def));
                        break;
                    case ModuleModel mod:
                        diffs.Add(new RecreateObject<ModuleModel>(mod, mod));
                        break;
                    case TableCheckModel chk:
                        diffs.Add(new RecreateObject<TableCheckModel>(chk, chk));
                        break;
                    case FieldModel:
                    case TableModel:
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
