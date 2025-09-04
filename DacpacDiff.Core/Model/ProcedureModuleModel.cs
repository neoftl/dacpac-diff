using DacpacDiff.Core.Utility;

namespace DacpacDiff.Core.Model;

public class ProcedureModuleModel(SchemaModel schema, string name)
    : ModuleWithBody(schema, name, ModuleType.PROCEDURE), IParameterisedModuleModel
{
    public string? ExecuteAs { get; set; }

    public ParameterModel[] Parameters { get; set; } = [];

    public override bool IsSimilarDefinition(ModuleModel other)
    {
        if (other is not ProcedureModuleModel proc)
        {
            return false;
        }

        return this.IsEqual(proc,
            m => m.ExecuteAs,
            m => m.Parameters,
            m => m.Body.ScrubSQL());
    }
}
