namespace DacpacDiff.Core.Output
{
    public interface IFileFormatFactory
    {
        IFileFormat GetFormat(string format);
    }
}