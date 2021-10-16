using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffFieldAlterTests
    {
        [TestMethod]
        public void Model__Left_field()
        {
            // Arrange
            var lft = new FieldModel(TableModel.Empty, "lfield");
            var rgt = new FieldModel(TableModel.Empty, "rfield");

            var diff = new DiffFieldAlter(lft, rgt);

            // Act
            var res = diff.Model;

            // Assert
            Assert.AreSame(lft, res);
        }

        [TestMethod]
        public void Name__Based_on_left_table()
        {
            // Arrange
            var lft = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "lschema"), "ltable"), "lfield");
            var rgt = new FieldModel(TableModel.Empty, "rfield");

            var diff = new DiffFieldAlter(lft, rgt);

            // Act
            var res = diff.Name;

            // Assert
            Assert.AreEqual("[lschema].[ltable].[lfield]", res);
        }

        [TestMethod]
        public void Title__Constant()
        {
            // Arrange
            var diff = new DiffFieldAlter(FieldModel.Empty, FieldModel.Empty);

            // Act
            var res = diff.Title;

            // Assert
            Assert.AreEqual(DiffFieldAlter.TITLE, res);
        }

        [TestMethod]
        public void Constructor__Null_left_field__Exception()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _ = new DiffFieldAlter(null, FieldModel.Empty);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            });
        }

        [TestMethod]
        public void Constructor__Null_right_field__Exception()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _ = new DiffFieldAlter(FieldModel.Empty, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            });
        }

        [TestMethod]
        public void GetDataLossTable__Types_mismatch__True_with_right_table_name()
        {
            // Arrange
            var lft = new FieldModel(TableModel.Empty, "lfield")
            {
                Type = "A"
            };

            var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "rschema"), "rtable"), "rfield")
            {
                Type = "B"
            };

            var diff = new DiffFieldAlter(lft, rgt);

            // Act
            var res = diff.GetDataLossTable(out var dataLossTable);

            // Assert
            Assert.IsTrue(res);
            Assert.AreEqual("[rschema].[rtable]", dataLossTable);
        }

        [TestMethod]
        public void GetDataLossTable__Types_match__False()
        {
            // Arrange
            var lft = new FieldModel(TableModel.Empty, "lfield")
            {
                Type = "A"
            };

            var rgt = new FieldModel(TableModel.Empty, "rfield")
            {
                Type = "A"
            };

            var diff = new DiffFieldAlter(lft, rgt);

            // Act
            var res = diff.GetDataLossTable(out _);

            // Assert
            Assert.IsFalse(res);
        }
    }
}