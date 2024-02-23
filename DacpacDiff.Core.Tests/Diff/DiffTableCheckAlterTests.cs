using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffTableCheckAlterTests
    {
        [TestMethod]
        public void DiffTableCheckAlter__Unnamed()
        {
            // Arrange
            var tblL = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            var chkL = new TableCheckModel(tblL, null, "LDef");

            var tblR = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            var chkR = new TableCheckModel(tblR, "RCheck", "RDef");

            // Act
            var diff = new DiffTableCheckAlter(chkL, chkR);

            // Assert
            Assert.AreSame(chkL, diff.TargetTableCheck);
            Assert.AreSame(chkR, diff.CurrentTableCheck);
            Assert.AreSame(chkL, diff.Model);
            Assert.AreEqual("Alter check constraint", diff.Title);
            Assert.AreEqual("[LSchema].[LTable].*", diff.Name);
        }

        [TestMethod]
        public void DiffTableCheckAlter__Named()
        {
            // Arrange
            var tblL = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            var chkL = new TableCheckModel(tblL, "LCheck", "LDef");

            var tblR = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            var chkR = new TableCheckModel(tblR, "RCheck", "RDef");

            // Act
            var diff = new DiffTableCheckAlter(chkL, chkR);

            // Assert
            Assert.AreSame(chkL, diff.TargetTableCheck);
            Assert.AreSame(chkR, diff.CurrentTableCheck);
            Assert.AreSame(chkL, diff.Model);
            Assert.AreEqual("Alter check constraint", diff.Title);
            Assert.AreEqual("[LSchema].[LTable].[LCheck]", diff.Name);
        }

        [TestMethod]
        public void DiffTableCheckAlter__Left_null__Fail()
        {
            // Arrange
            var tblR = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            var chkR = new TableCheckModel(tblR, "RCheck", "RDef");

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new DiffTableCheckAlter(null, chkR));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public void DiffTableCheckAlter__Right_null__Fail()
        {
            // Arrange
            var tblL = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            var chkL = new TableCheckModel(tblL, "LCheck", "LDef");

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new DiffTableCheckAlter(chkL, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}