using DacpacDiff.Core.Model;
using System.Linq;

namespace DacpacDiff.Core.Diff
{
    public class DiffModuleCreate : IDifference
    {
        public ModuleModel Module { get; }

        public IModel Model => Module;
        public string Title => "Create " + Module.Type.ToString().ToLower() + (NeedsStub ? " stub" : "");
        public string Name => Module.FullName;
        public bool NeedsStub => new[] { ModuleModel.ModuleType.FUNCTION, ModuleModel.ModuleType.PROCEDURE, ModuleModel.ModuleType.VIEW }.Contains(Module.Type);

        public DiffModuleCreate(ModuleModel module)
        {
            Module = module;
        }
    }
}
