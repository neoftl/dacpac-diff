using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffTableCheckDropTests
    {
        [TestMethod]
        public void DiffTableCheckDrop__Unnamed()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            var chk = new TableCheckModel(tbl, null, "RDef");

            // Act
            var diff = new DiffTableCheckDrop(chk);

            // Assert
            Assert.AreSame(chk, diff.TableCheck);
            Assert.AreSame(chk, diff.Model);
            Assert.AreEqual("Drop check constraint", diff.Title);
            Assert.AreEqual("[RSchema].[RTable].*", diff.Name);
        }

        [TestMethod]
        public void DiffTableCheckDrop__Named()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            var chk = new TableCheckModel(tbl, "RCheck", "RDef");

            // Act
            var diff = new DiffTableCheckDrop(chk);

            // Assert
            Assert.AreSame(chk, diff.TableCheck);
            Assert.AreSame(chk, diff.Model);
            Assert.AreEqual("Drop check constraint", diff.Title);
            Assert.AreEqual("[RSchema].[RTable].[RCheck]", diff.Name);
        }

        [TestMethod]
        public void DiffTableCheckDrop__Null_arg__Fail()
        {
            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new DiffTableCheckDrop(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}