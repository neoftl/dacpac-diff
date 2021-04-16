using System;

namespace DacpacDiff.Core.Model
{
    public class TableModel : IModel, IDependentModel, IModelInSchema
    {
        public static readonly TableModel Empty = new TableModel();

        public SchemaModel Schema { get; }
        public string Name { get; }
        public string FullName => $"[{Schema.Name}].[{Name}]";

        public TableCheckModel[] Checks { get; set; } = Array.Empty<TableCheckModel>();
        public FieldModel[] Fields { get; set; } = Array.Empty<FieldModel>();
        public string[] PrimaryKey { get; set; } = Array.Empty<string>();
        public bool IsPrimaryKeyUnclustered { get; set; }
        public TemporalityModel Temporality { get; set; } = TemporalityModel.Empty;
        public string[] Dependents { get; set; } = Array.Empty<string>();

        private TableModel()
        {
            Schema = SchemaModel.Empty;
            Name = string.Empty;
        }
        public TableModel(SchemaModel schema, string name)
        {
            Schema = schema;
            Name = name;
        }
    }
}
