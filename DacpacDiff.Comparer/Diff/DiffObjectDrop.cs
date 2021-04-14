using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Comparer.Diff
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

        public IModel Model { get; set; }
        public string Title => "Drop " + Type.ToString().ToLower();
        public string Name { get; set; }
        public ObjectType Type { get; set; }

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

        public override string ToString()
        {
            if (Type == ObjectType.INDEX)
            {
                // TODO: Need "DROP INDEX [x] ON [y]"
                var m = System.Text.RegularExpressions.Regex.Match(((ModuleModel)Model)?.Definition, @"(?i)ON\s+((?:\[[^\]]+\]\s*\.|\w+\s*\.)?\s*(?:\[[^\]]+\]|\w+))\s*\(");
                if (!m.Success)
                {
                    System.Console.Error.WriteLine($"Cannot drop INDEX {Name} using this schema version");
                    return null;
                }
                return $"DROP INDEX [{((ModuleModel)Model).Name}] ON {m.Groups[1].Value}";
            }

            return $"DROP {Type} {Name}";
        }

        public bool GetDataLossTable(out string tableName)
        {
            tableName = Name;
            return Type == ObjectType.SEQUENCE || Type == ObjectType.TABLE;
        }
    }
}
