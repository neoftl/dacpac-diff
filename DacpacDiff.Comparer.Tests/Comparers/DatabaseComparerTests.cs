using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers.Tests
{
    [TestClass()]
    public class DatabaseComparerTests
    {
        [TestMethod]
        public void Compare__Checks_each_left_schema_against_null_right()
        {
            // Arrange
            var tgt = new DatabaseModel("left");
            tgt.Schemas["A"] = SchemaModel.Empty;
            tgt.Schemas["B"] = SchemaModel.Empty;
            tgt.Schemas["C"] = SchemaModel.Empty;

            var comparerMock = new Mock<IModelComparer<SchemaModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => tgt.Schemas.Values.Contains(s)), null))
                .Returns(new IDifference[1]);

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<SchemaModel>())
                .Returns(comparerMock.Object);

            var comparer = new DatabaseComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(tgt, null);

            // Assert
            Assert.AreEqual(3, res.Count());
            comparerMock.Verify(m => m.Compare(It.IsAny<SchemaModel>(), It.IsAny<SchemaModel>()), Times.Exactly(3));
        }
        
        [TestMethod]
        public void Compare__Checks_each_right_schema_against_null_left()
        {
            // Arrange
            var cur = new DatabaseModel("right");
            cur.Schemas["A"] = SchemaModel.Empty;
            cur.Schemas["B"] = SchemaModel.Empty;
            cur.Schemas["C"] = SchemaModel.Empty;

            var comparerMock = new Mock<IModelComparer<SchemaModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(null, It.Is<SchemaModel>(s => cur.Schemas.Values.Contains(s))))
                .Returns(new IDifference[1]);

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<SchemaModel>())
                .Returns(comparerMock.Object);

            var comparer = new DatabaseComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(null, cur);

            // Assert
            Assert.AreEqual(3, res.Count());
            comparerMock.Verify(m => m.Compare(It.IsAny<SchemaModel>(), It.IsAny<SchemaModel>()), Times.Exactly(3));
        }
        
        [TestMethod]
        public void Compare__Checks_each_schema_against_the_same_named_schema()
        {
            // Arrange
            var tgt = new DatabaseModel("left");
            tgt.Schemas["A"] = new SchemaModel(tgt, "A");
            tgt.Schemas["B"] = new SchemaModel(tgt, "B");
            tgt.Schemas["C"] = new SchemaModel(tgt, "C");

            var cur = new DatabaseModel("right");
            cur.Schemas["A"] = new SchemaModel(cur, "A");
            cur.Schemas["B"] = new SchemaModel(cur, "B");
            cur.Schemas["C"] = new SchemaModel(cur, "C");

            var comparerMock = new Mock<IModelComparer<SchemaModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => s.Name == "A"), It.Is<SchemaModel>(s => s.Name == "A"))).Returns(new IDifference[1]);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => s.Name == "B"), It.Is<SchemaModel>(s => s.Name == "B"))).Returns(new IDifference[1]);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => s.Name == "C"), It.Is<SchemaModel>(s => s.Name == "C"))).Returns(new IDifference[1]);

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<SchemaModel>())
                .Returns(comparerMock.Object);

            var comparer = new DatabaseComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(tgt, cur);

            // Assert
            Assert.AreEqual(3, res.Count());
            comparerMock.Verify(m => m.Compare(It.IsAny<SchemaModel>(), It.IsAny<SchemaModel>()), Times.Exactly(3));
        }
        
        [TestMethod]
        public void Compare__Checks_each_schema_against_null_if_no_name_match()
        {
            // Arrange
            var tgt = new DatabaseModel("left");
            tgt.Schemas["A"] = new SchemaModel(tgt, "A");

            var cur = new DatabaseModel("right");
            cur.Schemas["B"] = new SchemaModel(cur, "B");

            var comparerMock = new Mock<IModelComparer<SchemaModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => s.Name == "A"), null)).Returns(new IDifference[1]);
            comparerMock.Setup(m => m.Compare(null, It.Is<SchemaModel>(s => s.Name == "B"))).Returns(new IDifference[1]);

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<SchemaModel>())
                .Returns(comparerMock.Object);

            var comparer = new DatabaseComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(tgt, cur);

            // Assert
            Assert.AreEqual(2, res.Count());
            comparerMock.Verify(m => m.Compare(It.IsAny<SchemaModel>(), It.IsAny<SchemaModel>()), Times.Exactly(2));
        }
    }
}