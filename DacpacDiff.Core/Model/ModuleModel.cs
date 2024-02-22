using System;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public abstract class ModuleModel : IModel<ModuleModel, SchemaModel>, IDependentModel, IModelInSchema
    {
        public enum ModuleType
        {
            NONE,
            CONSTRAINT,
            FUNCTION,
            INDEX,
            PROCEDURE,
            SEQUENCE,
            TRIGGER,
            VIEW
        }

        public SchemaModel Schema { get; }
        public string Name { get; }
        public virtual string FullName => $"[{Schema.Name}].[{Name}]";
        public ModuleType Type { get; }
        public virtual string[] Dependencies { get; init; } = Array.Empty<string>();

        /// <summary>
        /// True if this module type should be stubbed on create and altered with actual implementation
        /// TODO: This is to solve a possible dependency issue, which may be better solved through actual dependency checking
        /// </summary>
        public bool StubOnCreate { get; }

        public ModuleModel(SchemaModel schema, string name, ModuleType type)
        {
            Schema = schema;
            Name = name;
            Type = type;
            StubOnCreate = new[] { ModuleType.FUNCTION, ModuleType.PROCEDURE, ModuleType.VIEW }.Contains(Type);
        }

        public abstract bool IsSimilarDefinition(ModuleModel other);
    }
}
