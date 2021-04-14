using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public class TableModel : IModel<TableModel, SchemaModel>, IDependentModel, IModelInSchema
    {
        public static readonly TableModel Empty = new TableModel();

        public SchemaModel Schema { get; set; } = SchemaModel.Empty;
        public string Name { get; set; }
        public string FullName => $"[{Schema.Name}].[{Name}]";

        public TableCheckModel[] Checks { get; set; }
        public FieldModel[] Fields { get; set; }
        public string[] PrimaryKey { get; set; }
        public bool IsPrimaryKeyUnclustered { get; set; }
        public IDictionary<string, RefModel> Refs { get; } = new Dictionary<string, RefModel>();
        public TemporalityModel Temporality { get; set; }
        public string[] Dependents { get; set; }

        public TableModel SetState(SchemaModel schema, string name)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Checks = Checks?.Select(c => c.SetState(this, null)).ToArray();
            return this;
        }
    }
}
