namespace DacpacDiff.Core.Model
{
    public class ProcedureModuleModel : ModuleModel
    {
        public string? ExecuteAs { get; set; }

        public ProcedureModuleModel()
        {
            Type = ModuleType.PROCEDURE;
        }
    }
}
