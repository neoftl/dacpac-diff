using System.IO;

namespace DacpacDiff.Core.Utility
{
    internal static class FileUtilities
    {
        public static bool TryParsePath(string? fileName, out FileInfo? fileInfo)
        {
            fileInfo = default;
            if (fileName is null)
            {
                return false;
            }

            try
            {
                fileInfo = new FileInfo(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
