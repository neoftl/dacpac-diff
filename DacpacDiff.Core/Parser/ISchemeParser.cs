using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Parser
{
    public interface ISchemeParser
    {
        SchemeModel? ParseFile(string filename);
    }
}
