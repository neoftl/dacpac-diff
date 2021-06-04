using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Core.Changes
{
    /// <summary>
    /// Fully recreate the target object
    /// Often this is a drop-create
    /// </summary>
    public class RecreateObject<T> : INoopDifference, IChangeProvider
        where T : IModel
    {
        public IModel Model { get; }
        public IModel OldModel { get; }

        public string Title => "Recreate " + Model.Name;
        public string Name => Model.Name;

        public RecreateObject(T lft, T rgt)
        {
            Model = lft ?? throw new ArgumentNullException(nameof(lft));
            OldModel = rgt ?? throw new ArgumentNullException(nameof(rgt));
        }

        public IEnumerable<IDifference> GetAdditionalChanges()
        {
            var diffs = new List<IDifference>();

            // Must drop all function dependencies in order to alter function
            if (Model is FunctionModuleModel func)
            {
                var deps = func.Schema.Db.FindAllDependents(func);
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
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            // We know that OldModel is the same type as Model
            switch (Model)
            {
                case FieldDefaultModel def:
                    // TODO: Specific changes for defaults?
                    var fieldWithoutDefault = new FieldModel(def.Field.Table, def.Field.Name)
                    {
                        Default = null
                    };
                    diffs.Add(new DiffFieldAlter(fieldWithoutDefault, ((FieldDefaultModel)OldModel).Field));
                    diffs.Add(new DiffFieldAlter(def.Field, fieldWithoutDefault)); // TODO: prevent removal as duplicate; enforce order (or specific default changes)
                    break;
                case ModuleModel mod:
                    // Stubbing will remove dependency, so only drop if no stub
                    if (!mod.StubOnCreate)
                    {
                        diffs.Add(new DiffObjectDrop((ModuleModel)OldModel));
                    }
                    diffs.Add(new DiffModuleCreate(mod)
                    {
                        DoAsAlter = mod.StubOnCreate
                    });
                    break;
                case TableCheckModel chk:
                    diffs.Add(new DiffTableCheckDrop((TableCheckModel)OldModel));
                    diffs.Add(new DiffTableCheckCreate(chk));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return diffs.ToArray();
        }
    }
}
