using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace DacpacDiff.Core.Utility.Tests
{
    [TestClass]
    public class FileUtilitiesTests
    {
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void TryParsePath__Invalid_filename__False(string filename)
        {
            // Act
            var res = FileUtilities.TryParsePath(filename, out var fi);

            // Assert
            Assert.IsFalse(res);
            Assert.IsNull(fi);
        }

        [TestMethod]
        public void TryParsePath__Valid_directory__False()
        {
            // Arrange
            var dir = Path.GetTempPath();

            // Act
            var res = FileUtilities.TryParsePath(dir, out var fi);

            // Assert
            Assert.IsFalse(res);
            Assert.AreEqual(dir, fi.FullName);
        }
        
        [TestMethod]
        public void TryParsePath__Unknown_filename__True_with_info()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.Delete(tempFile);

            // Act
            var res = FileUtilities.TryParsePath(tempFile, out var fi);

            // Assert
            Assert.IsTrue(res);
            Assert.AreEqual(tempFile, fi.FullName);
        }

        [TestMethod]
        public void TryParsePath__Valid_filename__True_with_info()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            Assert.IsTrue(File.Exists(tempFile));

            // Act
            var res = FileUtilities.TryParsePath(tempFile, out var fi);

            // Assert
            Assert.IsTrue(res);
            Assert.AreEqual(tempFile, fi.FullName);
            File.Delete(tempFile);
        }
    }
}