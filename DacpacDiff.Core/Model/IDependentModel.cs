namespace DacpacDiff.Core.Model
{
    public interface IDependentModel : IModel
    {
        /// <summary>
        /// The fully named list of items this object depends on
        /// </summary>
        string[] Dependencies { get; }
    }
}
