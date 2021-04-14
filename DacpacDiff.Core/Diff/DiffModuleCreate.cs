using DacpacDiff.Core.Model;
using System.Linq;
using System.Text.RegularExpressions;

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

        public override string ToString()
        {
            switch (Module.Type)
            {
                case ModuleModel.ModuleType.FUNCTION:
                    var returnValue = "BEGIN\r\n    RETURN NULL\r\nEND";
                    var m = Regex.Match(Module.Definition, @"(?is)^(.*?\sAS\s)");
                    var sql = m.Groups[1].Value;
                    m = Regex.Match(sql, @"(?i)RETURNS\s+(@\w+\s+)?TABLE\s");
                    if (m.Success)
                    {
                        returnValue = m.Groups[1].Success ? "BEGIN\r\n    RETURN\r\nEND" : "    RETURN SELECT 1 A";
                    }
                    return $"{sql.Trim()}\r\n{returnValue}";
                case ModuleModel.ModuleType.PROCEDURE:
                    return $"CREATE PROCEDURE {Module.FullName} AS RETURN 0";
                case ModuleModel.ModuleType.VIEW:
                    return $"CREATE VIEW {Module.FullName} AS SELECT 1 A";
            }

            return Module.Definition;
        }
    }
}
