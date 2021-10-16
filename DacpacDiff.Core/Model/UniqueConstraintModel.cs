using DacpacDiff.Core.Utility;
using System;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public class UniqueConstraintModel : ModuleModel
    {
        public bool IsSystemNamed { get; }

        public bool IsClustered { get; set; }

        public string DefiningObjectFullName { get; init; } = string.Empty;
        public IModel? DefiningObject { get; private set; }

        public string[] Columns { get; set; } = Array.Empty<string>();

        public UniqueConstraintModel(SchemaModel schema, string? name)
            : base(schema, name ?? "Unnamed", ModuleType.CONSTRAINT)
        {
            IsSystemNamed = !(name?.Length > 0);
        }

        public override bool IsSimilarDefinition(ModuleModel other)
        {
            if (other is not UniqueConstraintModel idx)
            {
                return false;
            }

            return this.IsEqual(idx,
                m => m.IsClustered,
                m => m.DefiningObjectFullName,
                m => m.Columns);
        }

        public bool MapTarget(DatabaseModel db)
        {
            DefiningObject = db.Get(DefiningObjectFullName);
            if (DefiningObject == null)
            {
                return false;
            }

            if (DefiningObject is TableModel tbl)
            {
                return Columns.All(c => tbl.Fields.Any(f => f.Name == c));
            }

            throw new NotImplementedException("Constraint defined against " + DefiningObject.GetType().FullName);
        }
    }
}
