using DacpacDiff.Core.Utility;
using System;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public class UniqueConstraintModel : ModuleModel
    {
        public bool IsSystemNamed { get; }
        public override string FullName => !IsSystemNamed ? Name : $"(unnamed) {DefiningObjectFullName} ([{string.Join("], [", Columns)}])";

        public bool IsClustered { get; set; }

        public string DefiningObjectFullName { get; }
        public IModel? DefiningObject { get; private set; }

        public string[] Columns { get; }

        private static string getComparableName(string definingObjectFullName, string[] columns)
        {
            return $"UQ::{definingObjectFullName}({string.Join(",", columns)})";
        }

        public UniqueConstraintModel(SchemaModel schema, string? name, string definingObjectFullName, string[] columns)
            : base(schema, name ?? getComparableName(definingObjectFullName, columns), ModuleType.CONSTRAINT)
        {
            IsSystemNamed = !(name?.Length > 0);
            DefiningObjectFullName = definingObjectFullName;
            Columns = columns;
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
