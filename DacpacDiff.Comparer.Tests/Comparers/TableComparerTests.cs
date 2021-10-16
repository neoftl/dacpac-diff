using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers.Tests
{
    [TestClass]
    public class TableComparerTests
    {
        [TestMethod]
        public void Compare__Both_null__Noop()
        {
            // Arrange
            var comp = new TableComparer(new Mock<IModelComparerFactory>().Object);

            // Act
            var res = comp.Compare(null, null).ToArray();

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        public void Compare__Null_right__Create_table()
        {
            // Arrange
            var lft = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");

            var comparerFactMock = new Mock<IModelComparerFactory>();

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, null).ToArray();

            // Assert
            var diff = (DiffTableCreate)res.Single();
            Assert.AreSame(lft, diff.Table);
        }

        [TestMethod]
        public void Compare__Null_left__Drop_table()
        {
            // Arrange
            var rgt = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");

            var comparerFactMock = new Mock<IModelComparerFactory>();

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(null, rgt).ToArray();

            // Assert
            var diff = (DiffObjectDrop)res.Single();
            Assert.AreSame(rgt, diff.Model);
        }

        [TestMethod]
        public void Compare__Compares_fields()
        {
            // Arrange
            var lft = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            lft.Fields = new[]
            {
                new FieldModel(lft, "LFld1"),
                new FieldModel(lft, "XFld2"),
            };

            var rgt = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            rgt.Fields = new[]
            {
                new FieldModel(rgt, "XFld2"),
                new FieldModel(rgt, "RFld3"),
            };

            var comparerMock = new Mock<IModelComparer<FieldModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<FieldModel>(), It.IsAny<FieldModel>()))
                .Returns(new [] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<FieldModel>()).Returns(comparerMock.Object);

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(lft.Fields[0], null), Times.Once);
            comparerMock.Verify(m => m.Compare(lft.Fields[1], rgt.Fields[0]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, rgt.Fields[1]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_named_checks()
        {
            // Arrange
            var lft = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            lft.Checks = new[]
            {
                new TableCheckModel(lft, "LChk1", "ChkDef"),
                new TableCheckModel(lft, "XChk2", "ChkDef"),
            };

            var rgt = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            rgt.Checks = new[]
            {
                new TableCheckModel(rgt, "XChk2", "ChkDef"),
                new TableCheckModel(rgt, "RChk3", "ChkDef"),
            };

            var comparerMock = new Mock<IModelComparer<TableCheckModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<TableCheckModel>(), It.IsAny<TableCheckModel>()))
                .Returns(new [] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<TableCheckModel>()).Returns(comparerMock.Object);

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(lft.Checks[0], null), Times.Once);
            comparerMock.Verify(m => m.Compare(lft.Checks[1], rgt.Checks[0]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, rgt.Checks[1]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_unnamed_checks()
        {
            // Arrange
            var lft = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            lft.Checks = new[]
            {
                new TableCheckModel(lft, null, "ChkDef1"),
                new TableCheckModel(lft, null, "ChkDef2"),
            };

            var rgt = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            rgt.Checks = new[]
            {
                new TableCheckModel(rgt, null, "ChkDef2"),
                new TableCheckModel(rgt, null, "ChkDef3"),
            };

            var comparerMock = new Mock<IModelComparer<TableCheckModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<TableCheckModel>(), It.IsAny<TableCheckModel>()))
                .Returns(new [] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<TableCheckModel>()).Returns(comparerMock.Object);

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(lft.Checks[0], null), Times.Once);
            comparerMock.Verify(m => m.Compare(lft.Checks[1], rgt.Checks[0]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, rgt.Checks[1]), Times.Once);
        }
    }
}