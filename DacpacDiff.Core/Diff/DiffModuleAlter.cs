using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff;

public class DiffModuleAlter : IDifference
{
    public ModuleModel Module { get; }
    public bool AsDropCreate { get; }

    public IModel Model => Module;
    public string Title => "Alter " + Module.Type.ToString().ToLower();
    public string Name => Module.FullName;

    public DiffModuleAlter(ModuleModel module, bool asDropCreate = false)
    {
        Module = module;
        AsDropCreate = asDropCreate;
    }
}
