using System;

namespace DacpacDiff.Core.Model
{
    public class TableCheckModel : IModel<TableCheckModel, TableModel>
    {
        public TableModel Table { get; set; } = TableModel.Empty;
        public string Name { get; set; }
        public bool IsSystemNamed { get; set; }
        public string Definition { get; set; }

        public TableCheckModel SetState(TableModel table, string name)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            return this;
        }
    }
}
