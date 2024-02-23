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
            var tgt = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");

            var comparerFactMock = new Mock<IModelComparerFactory>();

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, null).ToArray();

            // Assert
            var diff = (DiffTableCreate)res.Single();
            Assert.AreSame(tgt, diff.Table);
        }

        [TestMethod]
        public void Compare__Null_left__Drop_table()
        {
            // Arrange
            var cur = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");

            var comparerFactMock = new Mock<IModelComparerFactory>();

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(null, cur).ToArray();

            // Assert
            var diff = (DiffObjectDrop)res.Single();
            Assert.AreSame(cur, diff.Model);
        }

        [TestMethod]
        public void Compare__Compares_fields()
        {
            // Arrange
            var tgt = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            tgt.Fields = new[]
            {
                new FieldModel(tgt, "LFld1"),
                new FieldModel(tgt, "XFld2"),
            };

            var cur = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            cur.Fields = new[]
            {
                new FieldModel(cur, "XFld2"),
                new FieldModel(cur, "RFld3"),
            };

            var comparerMock = new Mock<IModelComparer<FieldModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<FieldModel>(), It.IsAny<FieldModel>()))
                .Returns(new [] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<FieldModel>()).Returns(comparerMock.Object);

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(tgt.Fields[0], null), Times.Once);
            comparerMock.Verify(m => m.Compare(tgt.Fields[1], cur.Fields[0]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, cur.Fields[1]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_named_checks()
        {
            // Arrange
            var tgt = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            tgt.Checks = new[]
            {
                new TableCheckModel(tgt, "LChk1", "ChkDef"),
                new TableCheckModel(tgt, "XChk2", "ChkDef"),
            };

            var cur = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            cur.Checks = new[]
            {
                new TableCheckModel(cur, "XChk2", "ChkDef"),
                new TableCheckModel(cur, "RChk3", "ChkDef"),
            };

            var comparerMock = new Mock<IModelComparer<TableCheckModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<TableCheckModel>(), It.IsAny<TableCheckModel>()))
                .Returns(new [] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<TableCheckModel>()).Returns(comparerMock.Object);

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(tgt.Checks[0], null), Times.Once);
            comparerMock.Verify(m => m.Compare(tgt.Checks[1], cur.Checks[0]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, cur.Checks[1]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_unnamed_checks()
        {
            // Arrange
            var tgt = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            tgt.Checks = new[]
            {
                new TableCheckModel(tgt, null, "ChkDef1"),
                new TableCheckModel(tgt, null, "ChkDef2"),
            };

            var cur = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable");
            cur.Checks = new[]
            {
                new TableCheckModel(cur, null, "ChkDef2"),
                new TableCheckModel(cur, null, "ChkDef3"),
            };

            var comparerMock = new Mock<IModelComparer<TableCheckModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<TableCheckModel>(), It.IsAny<TableCheckModel>()))
                .Returns(new [] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<TableCheckModel>()).Returns(comparerMock.Object);

            var comp = new TableComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(tgt.Checks[0], null), Times.Once);
            comparerMock.Verify(m => m.Compare(tgt.Checks[1], cur.Checks[0]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, cur.Checks[1]), Times.Once);
        }
    }
}