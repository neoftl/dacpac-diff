using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffFieldCreateTests
    {
        [TestMethod]
        public void Model__field()
        {
            // Arrange
            var fld = new FieldModel(TableModel.Empty, "field");

            var diff = new DiffFieldCreate(fld);

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

            var diff = new DiffFieldCreate(fld);

            // Act
            var res = diff.Name;

            // Assert
            Assert.AreEqual("[schema].[table].[field]", res);
        }

        [TestMethod]
        public void Title__Constant()
        {
            // Arrange
            var diff = new DiffFieldCreate(FieldModel.Empty);

            // Act
            var res = diff.Title;

            // Assert
            Assert.AreEqual(DiffFieldCreate.TITLE, res);
        }

        [TestMethod]
        public void Constructor__Null_field__Exception()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new DiffFieldCreate(null);
            });
        }
    }
}