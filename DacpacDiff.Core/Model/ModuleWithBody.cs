namespace DacpacDiff.Core.Model;

public abstract class ModuleWithBody(SchemaModel schema, string name, ModuleModel.ModuleType type)
    : ModuleModel(schema, name, type)
{
    private string _body = string.Empty;
    public string Body
    {
        get => _body;
        set
        {
            value ??= string.Empty;

            // Sanitise incompatible characters
            _body = value.Replace("\u200B", ""); // Zero-width space
        }
    }
}