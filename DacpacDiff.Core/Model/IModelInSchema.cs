namespace DacpacDiff.Core.Model
{
    public interface IModelInSchema : IModel
    {
        SchemaModel Schema { get; }
    }
}
