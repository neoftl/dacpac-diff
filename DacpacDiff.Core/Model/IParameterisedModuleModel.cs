namespace DacpacDiff.Core.Model
{
    public interface IParameterisedModuleModel : IModel
    {
        ParameterModel[] Parameters { get; set; }
    }
}