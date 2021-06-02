using DacpacDiff.Core.Utility;
using System;

namespace DacpacDiff.Core.Model
{
    public class FunctionModuleModel : ModuleModel, IParameterisedModuleModel, IModuleWithBody
    {
        public bool ReturnNullForNullInput { get; set; }

        public string? ExecuteAs { get; set; }

        public ParameterModel[] Parameters { get; set; } = Array.Empty<ParameterModel>();

        public string ReturnType { get; set; } = string.Empty;

        public TableModel? ReturnTable { get; set; }

        public string Body { get; set; } = string.Empty;

        public FunctionModuleModel(SchemaModel schema, string name)
            : base(schema, name, ModuleType.FUNCTION)
        {
        }

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
}
