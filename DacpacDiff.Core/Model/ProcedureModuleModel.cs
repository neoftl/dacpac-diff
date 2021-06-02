using DacpacDiff.Core.Utility;
using System;

namespace DacpacDiff.Core.Model
{
    public class ProcedureModuleModel : ModuleModel, IParameterisedModuleModel, IModuleWithBody
    {
        public string? ExecuteAs { get; set; }

        public ParameterModel[] Parameters { get; set; } = Array.Empty<ParameterModel>();

        public string Body { get; set; } = string.Empty;

        public ProcedureModuleModel(SchemaModel schema, string name)
            : base(schema, name, ModuleType.PROCEDURE)
        {
        }

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
}
