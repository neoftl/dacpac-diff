using DacpacDiff.Core.Model;
using System;
using System.Text.RegularExpressions;

namespace DacpacDiff.Core.Diff
{
    public class DiffModuleAlter : IDifference
    {
        public ModuleModel Module { get; }

        public IModel Model => Module;
        public string Title => "Alter " + Module.Type.ToString().ToLower();
        public string Name => Module.FullName;

        public DiffModuleAlter(ModuleModel module)
        {
            Module = module;
        }

        public override string ToString()
        {
            var type = Module.Type switch
            {
                ModuleModel.ModuleType.INDEX => @"(?:UNIQUE\s+)?INDEX",
                ModuleModel.ModuleType.PROCEDURE => "PROC(?:EDURE)?",
                _ => Module.Type.ToString(),
            };
            var m = Regex.Match(Module.Definition, $@"^(?is)(.*?\b)?CREATE\s+{type}\s+(.+)$");
            if (!m.Success)
            {
                throw new InvalidOperationException($"Could not locate 'CREATE {Module.Type}' for [{Module.Schema.Name}].[{Module.Name}]");
            }

            return m.Groups[1].Value + $"ALTER {Module.Type} " + m.Groups[2].Value;
        }
    }
}
