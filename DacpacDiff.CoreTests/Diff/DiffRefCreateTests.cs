using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffRefCreateTests
    {
        [TestMethod]
        public void DiffRefCreate__Unnamed()
        {
            // Arrange
            var lft = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
            {
                Type = "LType"
            };
            var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
            {
                Computation = "COMPUTATION"
            };

            var fref = new FieldRefModel(lft, rgt);

            // Act
            var diff = new DiffRefCreate(fref);

            // Assert
            Assert.AreSame(fref, diff.Ref);
            Assert.AreSame(fref, diff.Model);
            Assert.AreEqual("Create reference", diff.Title);
            Assert.AreEqual("[LSchema].[LTable].[LField]:*", diff.Name);
        }

        [TestMethod]
        public void DiffRefCreate__Named()
        {
            // Arrange
            var lft = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
            {
                Type = "LType"
            };
            var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
            {
                Computation = "COMPUTATION"
            };

            var fref = new FieldRefModel(lft, rgt)
            {
                Name = "FRef",
                IsSystemNamed = false
            };

            // Act
            var diff = new DiffRefCreate(fref);

            // Assert
            Assert.AreSame(fref, diff.Ref);
            Assert.AreSame(fref, diff.Model);
            Assert.AreEqual("Create reference", diff.Title);
            Assert.AreEqual("[LSchema].[LTable].[LField]:[FRef]", diff.Name);
        }

        [TestMethod]
        public void DiffRefCreate__Null_arg__Fail()
        {
            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new DiffRefCreate(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}