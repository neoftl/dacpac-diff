using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers
{
    public class ModelComparerFactory : IModelComparerFactory
    {
        private readonly IDictionary<Type, Func<IModelComparer>> _modelComparers = new Dictionary<Type, Func<IModelComparer>>();

        public ModelComparerFactory()
        {
            // Find all comparers
            var comparerTypes = GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(IModelComparer).IsAssignableFrom(t))
                .ToArray();
            _modelComparers.Merge(comparerTypes, getModelType, t =>
                {
                    var constr = t.GetConstructor(new[] { typeof(IModelComparerFactory) });
                    if (constr is not null)
                    {
                        return () => Activator.CreateInstance(t, new object[] { this }) as IModelComparer ?? throw new NullReferenceException();
                    }
                    return () => Activator.CreateInstance(t) as IModelComparer ?? throw new NullReferenceException();
                });

            static Type getModelType(Type t)
            {
                return t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModelComparer<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .Single();
            }
        }

        public IModelComparer<T> GetComparer<T>()
            where T : IModel
        {
            if (_modelComparers.TryGetValue(typeof(T), out var comparer))
            {
                return (IModelComparer<T>)comparer();
            }

            throw new NotImplementedException("Unknown model type to compare: " + typeof(T).FullName);
        }
    }
}
