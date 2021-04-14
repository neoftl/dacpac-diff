using DacpacDiff.Core.Output;
using DacpacDiff.Mssql.Output;
using System;

namespace DacpacDiff.Comparer.Output
{
    public class FileFormatFactory : IFileFormatFactory
    {
        public IFileFormat GetFormat(string format)
        {
            return format switch
            {
                "mssql" => new MssqlFileFormat(),

                // TODO: Other format could be supported here

                _ => throw new NotImplementedException(),
            };
        }
    }
}
