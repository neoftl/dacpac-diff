using System;
using System.IO;

namespace DacpacDiff.Core.Parser
{
    public class SchemeParserFactory : ISchemeParserFactory
    {
        public ISchemeParser GetFileFormat(FileInfo fileInfo)
        {
            if (fileInfo.Extension.Equals(".dacpac", StringComparison.InvariantCultureIgnoreCase))
            {
                return new DacpacSchemeParser();
            }

            // TODO: Other format could be supported here

            throw new NotImplementedException();
        }
    }
}
