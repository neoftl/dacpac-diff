using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    public class DiffObjectDrop : IDifference, IDataLossChange
    {
        public enum ObjectType
        {
            NONE,
            FUNCTION,
            INDEX,
            PROCEDURE,
            SCHEMA,
            SEQUENCE,
            SYNONYM,
            TABLE,
            TRIGGER,
            VIEW,
        }

        public IModel Model { get; }
        public string Title => "Drop " + Type.ToString().ToLower();
        public string Name { get; }
        public ObjectType Type { get; }

        public DiffObjectDrop(ModuleModel module)
        {
            Model = module;
            Name = module.FullName;

            Type = module.Type switch
            {
                ModuleModel.ModuleType.FUNCTION => ObjectType.FUNCTION,
                ModuleModel.ModuleType.INDEX => ObjectType.INDEX,
                ModuleModel.ModuleType.PROCEDURE => ObjectType.PROCEDURE,
                ModuleModel.ModuleType.SEQUENCE => ObjectType.SEQUENCE,
                ModuleModel.ModuleType.TRIGGER => ObjectType.TRIGGER,
                ModuleModel.ModuleType.VIEW => ObjectType.VIEW,
                _ => throw new NotSupportedException(),
            };
        }
        public DiffObjectDrop(SchemaModel schema)
        {
            Model = schema;
            Name = $"[{schema.Name}]";
            Type = ObjectType.SCHEMA;
        }
        public DiffObjectDrop(SynonymModel synonym)
        {
            Model = synonym;
            Name = synonym.FullName;
            Type = ObjectType.SYNONYM;
        }
        public DiffObjectDrop(TableModel table)
        {
            Model = table;
            Name = table.FullName;
            Type = ObjectType.TABLE;
            // TODO: if temporal, will need to do both
        }

        public bool GetDataLossTable(out string tableName)
        {
            tableName = Name;
            return Type == ObjectType.SEQUENCE || Type == ObjectType.TABLE;
        }
    }
}
