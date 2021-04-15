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
        public string Definition { get; set; }
        public string ExecuteAs { get; set; }
        public string[] Dependents { get; set; } = Array.Empty<string>();
        
        public static string ScrubDefinition(string def)
        {
            return def.Replace(" ", "")
                .Replace("(", "")
                .Replace(")", "")
                .ToLower();
        }
        public bool IsSimilarDefinition(ModuleModel alt)
        {
            var def1 = ScrubDefinition(Definition);
            var def2 = ScrubDefinition(alt.Definition);
            return def1 == def2;
        }
    }
}
