using DacpacDiff.Core.Utility;
using System;

namespace DacpacDiff.Core.Model
{
    public class ModuleModel : IModel<ModuleModel, SchemaModel>, IDependentModel, IModelInSchema
    {
        public enum ModuleType
        {
            NONE,
            FUNCTION,
            INDEX,
            PROCEDURE,
            SEQUENCE,
            TRIGGER,
            VIEW
        }

        public SchemaModel Schema { get; set; } = SchemaModel.Empty;
        public string Name { get; set; }
        public string FullName => $"[{Schema.Name}].[{Name}]";
        public ModuleType Type { get; set; }
        public string Definition { get; set; } = string.Empty;
        public string? ExecuteAs { get; set; }
        public string[] Dependencies { get; set; } = Array.Empty<string>();

        public bool IsSimilarDefinition(ModuleModel alt)
        {
            var def1 = Definition.ScrubSQL();
            var def2 = alt.Definition.ScrubSQL();
            return def1 == def2;
        }
    }
}
