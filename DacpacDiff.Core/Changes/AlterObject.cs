using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Changes
{
    /// <summary>
    /// Alter the target object in-place
    /// </summary>
    public class AlterObject<T> : INoopDifference, IChangeProvider
        where T : IModel
    {
        public IModel Model { get; }
        public IModel OldModel { get; }

        public virtual string Title => "Alter " + Model.Name;
        public string Name => Model.Name;

        public AlterObject(T lft, T rgt)
        {
            Model = lft ?? throw new ArgumentNullException(nameof(lft));
            OldModel = rgt ?? throw new ArgumentNullException(nameof(rgt));
        }

        public virtual IEnumerable<IDifference> GetAdditionalChanges()
        {
            // We know that OldModel is the same type as Model

            // Alter index is a recreate
            if (Model is IndexModuleModel idx)
            {
                return new IDifference[] { new RecreateObject<ModuleModel>(idx, (IndexModuleModel)OldModel) };
            }

            var diffs = new List<IDifference>(5);
            AddChangesToDependents(diffs);

            switch (Model)
            {
                case ModuleModel mod:
                    diffs.Add(new DiffModuleAlter(mod));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return diffs.ToArray();
        }

        protected void AddChangesToDependents(IList<IDifference> diffs)
        {
            // Must drop all function dependencies in order to alter function
            if (Model is FunctionModuleModel func)
            {
                // Need all existing dependents that will remain after update
                var deps = func.Schema.Db.FindAllDependents(Model);
                deps = deps.Where(d => ((FunctionModuleModel)OldModel).Schema.Db.TryGet<IModel>(d.FullName, out _)).ToArray();

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
}
