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

        //private static readonly Dictionary<Type, Func<IModel, DiffObjectDrop>> _constructors = new();
        //public static DiffObjectDrop Create(IModel model)
        //{
        //    if (_constructors.Count == 0)
        //    {
        //        foreach (var c in typeof(DiffObjectDrop).GetConstructors().Where(c => c.GetParameters().All(p => typeof(IModel).IsInstanceOfType( p.ParameterType))))
        //        {
        //            _constructors[c.GetParameters().First().ParameterType] = (IModel o) => (DiffObjectDrop)c.Invoke(new object[] { o });
        //        }
        //    }

        //    if (!_constructors.TryGetValue(model.GetType(), out var constr))
        //    {
        //        throw new NotImplementedException("No DiffObjectDrop constructor to handle type " + model.GetType());
        //    }

        //    return constr(model);
        //}

        public bool GetDataLossTable(out string tableName)
        {
            tableName = Name;
            return Type == ObjectType.SEQUENCE || Type == ObjectType.TABLE;
        }
    }
}
