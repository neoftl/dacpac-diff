using System.Text;

namespace DacpacDiff.Core.Output
{
    public interface IDiffFormatter
    {
        StringBuilder Format(StringBuilder sb, bool checkForDataLoss, bool prettyPrint);
    }
}
