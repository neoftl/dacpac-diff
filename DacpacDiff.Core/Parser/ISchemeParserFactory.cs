using System.IO;

namespace DacpacDiff.Core.Parser
{
    public interface ISchemeParserFactory
    {
        ISchemeParser GetFileFormat(FileInfo fileInfo);
    }
}