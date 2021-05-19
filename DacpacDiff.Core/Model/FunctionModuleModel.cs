using System;

namespace DacpacDiff.Core.Model
{
    public class FunctionModuleModel : ModuleModel, IParameterisedModuleModel
    {
        public static FunctionModuleModel Empty => new(SchemaModel.Empty, string.Empty);

        public string? ExecuteAs { get; set; }

        public ParameterModel[] Parameters { get; set; } = Array.Empty<ParameterModel>();

        public string ReturnType { get; set; }

        public FunctionModuleModel(SchemaModel schema, string name)
            : base(schema, name, ModuleType.FUNCTION)
        {
        }
    }
}
