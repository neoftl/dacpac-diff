namespace DacpacDiff.Core.Model
{
    public interface IModuleWithBody : IModel
    {
        string Body { get; set; }
    }
}