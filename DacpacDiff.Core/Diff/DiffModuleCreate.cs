using DacpacDiff.Core.Model;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Diff
{
    public class DiffModuleCreate : IDifference
    {
        public ModuleModel Module { get; }

        public IModel Model => Module;
        public string Title => "Create " + Module.Type.ToString().ToLower() + (Module.StubOnCreate ? " stub" : "");
        public string Name => Module.FullName;

        public DiffModuleCreate(ModuleModel module)
        {
            Module = module;
        }
    }
}
