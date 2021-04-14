namespace DacpacDiff.Core.Diff
{
    /// <summary>
    /// Implemented by changes that may cause data-loss
    /// </summary>
    public interface IDataLossChange : IDifference
    {
        bool GetDataLossTable(out string tableName);
    }
}
