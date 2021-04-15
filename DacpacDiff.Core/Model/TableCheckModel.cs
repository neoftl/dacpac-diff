namespace DacpacDiff.Core.Model
{
    public class TableCheckModel : IModel<TableCheckModel, TableModel>
    {
        public TableModel Table { get; }
        public string Name { get; set; }
        public bool IsSystemNamed { get; set; }
        public string Definition { get; set; }

        private TableCheckModel()
        {
            Table = TableModel.Empty;
        }
        public TableCheckModel(TableModel table)
        {
            Table = table;
        }
    }
}
