using System.Text;

namespace DacpacDiff.Core.Output
{
    public interface ISqlFormatter
    {
        void Format(ISqlFileBuilder sb);
    }
}
