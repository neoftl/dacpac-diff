using System;

namespace DacpacDiff.Core.Model
{
    public class IndexModuleModel : ModuleModel
    {
        public bool IsClustered { get; set; }
        public bool IsUnique { get; set; }

        public string IndexedObject { get; init; } = string.Empty;

        public string[] IndexedColumns { get; set; } = Array.Empty<string>();
        public string[] IncludedColumns { get; set; } = Array.Empty<string>();
        public string? Condition { get; set; }

        public IndexModuleModel(SchemaModel schema, string name)
            : base(schema, name, ModuleType.INDEX)
        {
        }
    }
}
