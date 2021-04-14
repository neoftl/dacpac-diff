using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Comparers
{
	public interface IComparerFactory
	{
		IComparer<T> GetComparer<T>() where T : IModel;
	}
}