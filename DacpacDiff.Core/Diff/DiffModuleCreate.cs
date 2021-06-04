using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Core.Diff
{
    public class DiffModuleCreate : IDifference, IChangeProvider
    {
        public ModuleModel Module { get; }

        public IModel Model => Module;
        public string Title { get; }
        public string Name => Module.FullName;
        public bool DoAsAlter { get; set; }

        public DiffModuleCreate(ModuleModel module)
        {
            Module = module;
            Title = (Module.StubOnCreate ? "Stub " : "Create ") + Module.Type.ToString().ToLower();
        }

        public IEnumerable<IDifference> GetAdditionalChanges()
        {
            // A stubbed create must have an alter
            if (Module.StubOnCreate)
            {
                return new IDifference[] { new DiffModuleAlter(Module) };
            }

            return Array.Empty<IDifference>();
        }
    }
}
