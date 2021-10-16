using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace DacpacDiff.Core.Parser.Tests
{
    [TestClass]
    public class SchemeParserFactoryTests
    {
        [TestMethod]
        [DataRow("test.DACPAC"), DataRow("test.Dacpac"), DataRow("test.dacpac")]
        public void GetFileFormat__dacpac(string filename)
        {
            // Arrange
            var fact = new SchemeParserFactory();

            var fi = new FileInfo(filename);

            // Act
            var res = fact.GetFileFormat(fi);

            // Assert
            Assert.IsInstanceOfType(res, typeof(DacpacSchemeParser));
        }
        
        [TestMethod]
        [DataRow("test.txt"), DataRow("test.sql"), DataRow("test.zip"), DataRow("test.json"), DataRow("test.xml")]
        public void GetFileFormat__Non_dacpac__Fail(string filename)
        {
            // Arrange
            var fact = new SchemeParserFactory();

            var fi = new FileInfo(filename);

            // Act
            Assert.ThrowsException<NotImplementedException>(() => fact.GetFileFormat(fi));
        }
    }
}