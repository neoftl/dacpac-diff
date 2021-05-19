using System;

namespace DacpacDiff.Core.Model
{
    public class ProcedureModuleModel : ModuleModel, IParameterisedModuleModel
    {
        public static ProcedureModuleModel Empty => new(SchemaModel.Empty, string.Empty);

        public string? ExecuteAs { get; set; }

        public ParameterModel[] Parameters { get; set; } = Array.Empty<ParameterModel>();

        public ProcedureModuleModel(SchemaModel schema, string name)
            : base(schema, name, ModuleType.PROCEDURE)
        {
        }
    }
}
