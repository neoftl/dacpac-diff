using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffTableCheckCreateTests
    {
        [TestMethod]
        public void DiffTableCheckCreate__Unnamed()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            var chk = new TableCheckModel(tbl, null, "LDef");

            // Act
            var diff = new DiffTableCheckCreate(chk);

            // Assert
            Assert.AreSame(chk, diff.TableCheck);
            Assert.AreSame(chk, diff.Model);
            Assert.AreEqual("Create check constraint", diff.Title);
            Assert.AreEqual("[LSchema].[LTable].*", diff.Name);
        }

        [TestMethod]
        public void DiffTableCheckCreate__Named()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            var chk = new TableCheckModel(tbl, "LCheck", "LDef");

            // Act
            var diff = new DiffTableCheckCreate(chk);

            // Assert
            Assert.AreSame(chk, diff.TableCheck);
            Assert.AreSame(chk, diff.Model);
            Assert.AreEqual("Create check constraint", diff.Title);
            Assert.AreEqual("[LSchema].[LTable].[LCheck]", diff.Name);
        }

        [TestMethod]
        public void DiffTableCheckCreate__Null_arg__Fail()
        {
            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new DiffTableCheckCreate(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}