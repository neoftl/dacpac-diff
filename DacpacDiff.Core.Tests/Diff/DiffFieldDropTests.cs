using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffFieldDropTests
    {
        [TestMethod]
        public void Model__field()
        {
            // Arrange
            var fld = new FieldModel(TableModel.Empty, "field");

            var diff = new DiffFieldDrop(fld);

            // Act
            var res = diff.Model;

            // Assert
            Assert.AreSame(fld, res);
        }

        [TestMethod]
        public void Name__Based_on_table()
        {
            // Arrange
            var fld = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "schema"), "table"), "field");

            var diff = new DiffFieldDrop(fld);

            // Act
            var res = diff.Name;

            // Assert
            Assert.AreEqual("[schema].[table].[field]", res);
        }

        [TestMethod]
        public void Title__Constant()
        {
            // Arrange
            var diff = new DiffFieldDrop(FieldModel.Empty);

            // Act
            var res = diff.Title;

            // Assert
            Assert.AreEqual(DiffFieldDrop.TITLE, res);
        }

        [TestMethod]
        public void Constructor__Null_field__Exception()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _ = new DiffFieldDrop(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            });
        }

        [TestMethod]
        public void GetDataLossTable__True_with_right_table_name()
        {
            // Arrange
            var fld = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "schema"), "table"), "field");

            var diff = new DiffFieldDrop(fld);

            // Act
            var res = diff.GetDataLossTable(out var dataLossTable);

            // Assert
            Assert.IsTrue(res);
            Assert.AreEqual("[schema].[table]", dataLossTable);
        }
    }
}