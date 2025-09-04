using DacpacDiff.Core.Utility;

namespace DacpacDiff.Core.Model;

public class FunctionModuleModel(SchemaModel schema, string name)
    : ModuleWithBody(schema, name, ModuleType.FUNCTION), IParameterisedModuleModel
{
    public bool ReturnNullForNullInput { get; set; }

    public string? ExecuteAs { get; set; }

    public ParameterModel[] Parameters { get; set; } = [];

    public string ReturnType { get; set; } = string.Empty;

    public TableModel? ReturnTable { get; set; }

    public override bool IsSimilarDefinition(ModuleModel other)
    {
        if (other is not FunctionModuleModel func)
        {
            return false;
        }
        
        return this.IsEqual(func,
            m => m.ReturnNullForNullInput,
            m => m.ExecuteAs,
            m => m.ReturnType,
            m => m.ReturnTable,
            m => m.Body.ScrubSQL());
    }
}
