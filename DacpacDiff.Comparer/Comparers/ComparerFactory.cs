using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Comparer.Comparers
{
    public class ComparerFactory : IComparerFactory
    {
        public IComparer<T> GetComparer<T>()
            where T : IModel
        {
            var type = typeof(T);
            if (type == typeof(DatabaseModel))
            {
                return (IComparer<T>)new DatabaseComparer(this);
            }
            if (type == typeof(FieldModel))
            {
                return (IComparer<T>)new FieldComparer();
            }
            if (type == typeof(ModuleModel))
            {
                return (IComparer<T>)new ModuleComparer();
            }
            if (type == typeof(SchemaModel))
            {
                return (IComparer<T>)new SchemaComparer(this);
            }
            if (type == typeof(SynonymModel))
            {
                return (IComparer<T>)new SynonymComparer();
            }
            if (type == typeof(TableModel))
            {
                return (IComparer<T>)new TableComparer(this);
            }
            if (type == typeof(TableCheckModel))
            {
                return (IComparer<T>)new TableCheckComparer();
            }
            if (type == typeof(UserTypeModel))
            {
                return (IComparer<T>)new UserTypeComparer();
            }

            throw new NotImplementedException("Unknown model type to compare: " + type.FullName);
        }
    }
}
