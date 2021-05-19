namespace DacpacDiff.Core.Model
{
    public interface IParameterisedModuleModel : IModel
    {
        string FullName { get; }
        ParameterModel[] Parameters { get; set; }
    }
}