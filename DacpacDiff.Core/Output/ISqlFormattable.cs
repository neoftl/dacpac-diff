namespace DacpacDiff.Core.Output
{
    public interface ISqlFormattable
    {
        // TODO: Review these

        /// <summary>
        /// Description of the type of object.
        /// </summary>
        string? Title { get; }

        /// <summary>
        /// Name of this specific instance.
        /// </summary>
        string Name { get; }
    }
}