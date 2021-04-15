using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Comparers
{
    public interface IModelComparerFactory
    {
        IModelComparer<T> GetComparer<T>() where T : IModel;
    }
}