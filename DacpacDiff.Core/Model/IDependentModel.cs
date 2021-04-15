namespace DacpacDiff.Core.Model
{
    public interface IDependentModel : IModel
    {
        string[] Dependents { get; set; }
    }
}
