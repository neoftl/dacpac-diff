using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DacpacDiff.Mssql.Diff.Tests
{
    [TestClass]
    public class MssqlDiffTableCheckDropTests
    {
        [TestMethod]
        [DataRow("'CHECK'",    "0x095fceddb7a8ff1b00d2630141da89d9")]
        [DataRow("CHECK[()]3", "0xe5058a61e22656b980153c4e10b46fa6")]
        [DataRow("CHECK    3", "0xe5058a61e22656b980153c4e10b46fa6")]
        public void MssqlDiffTableCheckDrop__Unnamed__Uses_sproc(string def, string md5)
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "Schema"), "Table");

            var chk = new TableCheckModel(tbl, null, def);

            var diff = new DiffTableCheckDrop(chk);

            // Act
            var res = new MssqlDiffTableCheckDrop(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("EXEC #usp_DropUnnamedCheckConstraint '[Schema].[Table]', " + md5, res);
        }

        [TestMethod]
        public void MssqlDiffTableCheckDrop__Named__Uses_sproc()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "Schema"), "Table");

            var chk = new TableCheckModel(tbl, "CheckName", "CHECK");

            var diff = new DiffTableCheckDrop(chk);

            // Act
            var res = new MssqlDiffTableCheckDrop(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("ALTER TABLE [Schema].[Table] DROP CONSTRAINT [CheckName]", res);
        }
    }
}