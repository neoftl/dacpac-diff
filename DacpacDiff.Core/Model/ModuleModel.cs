using System;

namespace DacpacDiff.Core.Model
{
    public abstract class ModuleModel : IModel<ModuleModel, SchemaModel>, IDependentModel, IModelInSchema
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

        public ModuleModel(SchemaModel schema, string name, ModuleType type)
        {
            Schema = schema;
            Name = name;
            Type = type;
        }

        public abstract bool IsSimilarDefinition(ModuleModel other);
    }
}
