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

        public SchemaModel Schema { get; }
        public string Name { get; }
        public string FullName => $"[{Schema.Name}].[{Name}]";
        public ModuleType Type { get; }
        public string[] Dependencies { get; init; } = Array.Empty<string>();
        public string Definition { get; set; } = string.Empty;

        public ModuleModel(SchemaModel schema, string name, ModuleType type)
        {
            Schema = schema;
            Name = name;
            Type = type;
        }

        public bool IsSimilarDefinition(ModuleModel alt)
        {
            var def1 = Definition.ScrubSQL();
            var def2 = alt.Definition.ScrubSQL();
            return def1 == def2;
        }
    }
}
