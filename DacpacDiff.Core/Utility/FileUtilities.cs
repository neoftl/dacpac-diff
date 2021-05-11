using System.IO;

namespace DacpacDiff.Core.Utility
{
    internal static class FileUtilities
    {
        /// <summary>
        /// Returns true if the path is or could be a file (i.e., is valid and not an existing directory).
        /// </summary>
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
                return fileInfo.Attributes < 0 || (fileInfo.Attributes & FileAttributes.Directory) != FileAttributes.Directory;
            }
            catch
            {
                return false;
            }
        }
    }
}
