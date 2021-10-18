using Microsoft.VisualStudio.TestTools.UnitTesting;
using DacpacDiff.Mssql.Diff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacpacDiff.Core.Output;
using Moq;

namespace DacpacDiff.Mssql.Diff.Tests
{
    [TestClass]
    public class MssqlCommentTests
    {
        [TestMethod]
        public void MssqlComment__Arg_null__Fail()
        {
            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new MssqlComment(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public void Format__Blank__Noop()
        {
            // Arrange
            var sql = new SqlComment();

            var fmt = new MssqlComment(sql);

            var sfbMock = new Mock<ISqlFileBuilder>(MockBehavior.Strict);

            // Act
            fmt.Format(sfbMock.Object);
        }

        [TestMethod]
        public void Format__Single_line_formatted()
        {
            // Arrange
            var sql = new SqlComment
            {
                Comment = "COMMENT"
            };
            
            var fmt = new MssqlComment(sql);

            var sfbMock = new Mock<ISqlFileBuilder>();

            // Act
            fmt.Format(sfbMock.Object);

            // Assert
            sfbMock.Verify(m => m.AppendLine("-- COMMENT"), Times.Once);
        }

        [TestMethod]
        public void Format__Multi_line_formatted()
        {
            // Arrange
            var sql = new SqlComment
            {
                Comment = "\r\nCOMMENT\r\nLINE 2\r\nLINE 3\r\n"
            };
            
            var fmt = new MssqlComment(sql);

            var sfbMock = new Mock<ISqlFileBuilder>();

            // Act
            fmt.Format(sfbMock.Object);

            // Assert
            sfbMock.Verify(m => m.AppendLine("-- COMMENT\r\n-- LINE 2\r\n-- LINE 3"), Times.Once);
        }

        [TestMethod]
        public void ToString__Comment()
        {
            // Arrange
            var sql = new SqlComment
            {
                Comment = "COMMENT"
            };

            // Act
            var res = new MssqlComment(sql).ToString();

            // Assert
            Assert.AreEqual("COMMENT", res);
        }
    }
}