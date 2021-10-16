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
            var lft = new DatabaseModel("left");
            lft.Schemas["A"] = SchemaModel.Empty;
            lft.Schemas["B"] = SchemaModel.Empty;
            lft.Schemas["C"] = SchemaModel.Empty;

            var comparerMock = new Mock<IModelComparer<SchemaModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => lft.Schemas.Values.Contains(s)), null))
                .Returns(new IDifference[1]);

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<SchemaModel>())
                .Returns(comparerMock.Object);

            var comparer = new DatabaseComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(lft, null);

            // Assert
            Assert.AreEqual(3, res.Count());
            comparerMock.Verify(m => m.Compare(It.IsAny<SchemaModel>(), It.IsAny<SchemaModel>()), Times.Exactly(3));
        }
        
        [TestMethod]
        public void Compare__Checks_each_right_schema_against_null_left()
        {
            // Arrange
            var rgt = new DatabaseModel("right");
            rgt.Schemas["A"] = SchemaModel.Empty;
            rgt.Schemas["B"] = SchemaModel.Empty;
            rgt.Schemas["C"] = SchemaModel.Empty;

            var comparerMock = new Mock<IModelComparer<SchemaModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(null, It.Is<SchemaModel>(s => rgt.Schemas.Values.Contains(s))))
                .Returns(new IDifference[1]);

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<SchemaModel>())
                .Returns(comparerMock.Object);

            var comparer = new DatabaseComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(null, rgt);

            // Assert
            Assert.AreEqual(3, res.Count());
            comparerMock.Verify(m => m.Compare(It.IsAny<SchemaModel>(), It.IsAny<SchemaModel>()), Times.Exactly(3));
        }
        
        [TestMethod]
        public void Compare__Checks_each_schema_against_the_same_named_schema()
        {
            // Arrange
            var lft = new DatabaseModel("left");
            lft.Schemas["A"] = new SchemaModel(lft, "A");
            lft.Schemas["B"] = new SchemaModel(lft, "B");
            lft.Schemas["C"] = new SchemaModel(lft, "C");

            var rgt = new DatabaseModel("right");
            rgt.Schemas["A"] = new SchemaModel(rgt, "A");
            rgt.Schemas["B"] = new SchemaModel(rgt, "B");
            rgt.Schemas["C"] = new SchemaModel(rgt, "C");

            var comparerMock = new Mock<IModelComparer<SchemaModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => s.Name == "A"), It.Is<SchemaModel>(s => s.Name == "A"))).Returns(new IDifference[1]);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => s.Name == "B"), It.Is<SchemaModel>(s => s.Name == "B"))).Returns(new IDifference[1]);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => s.Name == "C"), It.Is<SchemaModel>(s => s.Name == "C"))).Returns(new IDifference[1]);

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<SchemaModel>())
                .Returns(comparerMock.Object);

            var comparer = new DatabaseComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(lft, rgt);

            // Assert
            Assert.AreEqual(3, res.Count());
            comparerMock.Verify(m => m.Compare(It.IsAny<SchemaModel>(), It.IsAny<SchemaModel>()), Times.Exactly(3));
        }
        
        [TestMethod]
        public void Compare__Checks_each_schema_against_null_if_no_name_match()
        {
            // Arrange
            var lft = new DatabaseModel("left");
            lft.Schemas["A"] = new SchemaModel(lft, "A");

            var rgt = new DatabaseModel("right");
            rgt.Schemas["B"] = new SchemaModel(rgt, "B");

            var comparerMock = new Mock<IModelComparer<SchemaModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(It.Is<SchemaModel>(s => s.Name == "A"), null)).Returns(new IDifference[1]);
            comparerMock.Setup(m => m.Compare(null, It.Is<SchemaModel>(s => s.Name == "B"))).Returns(new IDifference[1]);

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<SchemaModel>())
                .Returns(comparerMock.Object);

            var comparer = new DatabaseComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(lft, rgt);

            // Assert
            Assert.AreEqual(2, res.Count());
            comparerMock.Verify(m => m.Compare(It.IsAny<SchemaModel>(), It.IsAny<SchemaModel>()), Times.Exactly(2));
        }
    }
}