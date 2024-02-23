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
    public class RecreateObject<T> : AlterObject<T>
        where T : IModel
    {
        public override string Title => "Recreate " + Model.Name;

        public RecreateObject(T tgt, T cur)
            : base(tgt, cur)
        {
        }

        public override IEnumerable<IDifference> GetAdditionalChanges()
        {
            var diffs = new List<IDifference>();
            AddChangesToDependents(diffs);

            // We know that OldModel is the same type as Model
            switch (Model)
            {
                case FieldDefaultModel def:
                    // TODO: Specific changes for defaults?
                    var fieldWithoutDefault = new FieldModel(def.Field)
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
