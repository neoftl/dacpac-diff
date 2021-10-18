namespace DacpacDiff.Core.Model
{
    public interface IModel
    {
        string Name { get; }
        string FullName { get; }
    }

    public interface IModel<TModel, TParent> : IModel
        where TModel : IModel
        where TParent : IModel
    {
    }
}
