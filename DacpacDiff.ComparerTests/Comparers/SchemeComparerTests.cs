using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers.Tests
{
    [TestClass]
    public class SchemeComparerTests
    {
        [TestMethod]
        public void Compare__Compares_left_to_right()
        {
            // Arrange
            var lftScheme = new SchemeModel("left");
            var lftDb = new DatabaseModel("left");
            lftScheme.Databases["left"] = lftDb;

            var rgtScheme = new SchemeModel("right");
            var rgtDb = new DatabaseModel("right");
            rgtScheme.Databases["right"] = rgtDb;

            var diffMock = new Mock<IDifference>();

            var comparerMock = new Mock<IModelComparer<DatabaseModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(lftDb, rgtDb))
                .Returns(new [] { diffMock.Object });

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<DatabaseModel>())
                .Returns(comparerMock.Object);

            var comparer = new SchemeComparer(comparerFactMock.Object);

            // Act
            var res = comparer.Compare(lftScheme, rgtScheme);

            // Assert
            Assert.AreSame(diffMock.Object, res.Single());
        }
        
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Compare__Only_supports_single_left_database()
        {
            // Arrange
            var lftScheme = new SchemeModel("left");
            var lftDb = new DatabaseModel("left");
            lftScheme.Databases["leftA"] = lftDb;
            lftScheme.Databases["leftB"] = lftDb;

            var rgtScheme = new SchemeModel("right");
            var rgtDb = new DatabaseModel("right");
            rgtScheme.Databases["right"] = rgtDb;

            var diffMock = new Mock<IDifference>();

            var comparerMock = new Mock<IModelComparer<DatabaseModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(lftDb, rgtDb))
                .Returns(new [] { diffMock.Object });

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<DatabaseModel>())
                .Returns(comparerMock.Object);

            var comparer = new SchemeComparer(comparerFactMock.Object);

            // Act
            comparer.Compare(lftScheme, rgtScheme);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Compare__Only_supports_single_right_database()
        {
            // Arrange
            var lftScheme = new SchemeModel("left");
            var lftDb = new DatabaseModel("left");
            lftScheme.Databases["left"] = lftDb;

            var rgtScheme = new SchemeModel("right");
            var rgtDb = new DatabaseModel("right");
            rgtScheme.Databases["rightA"] = rgtDb;
            rgtScheme.Databases["rightB"] = rgtDb;

            var diffMock = new Mock<IDifference>();

            var comparerMock = new Mock<IModelComparer<DatabaseModel>>(MockBehavior.Strict);
            comparerMock.Setup(m => m.Compare(lftDb, rgtDb))
                .Returns(new [] { diffMock.Object });

            var comparerFactMock = new Mock<IModelComparerFactory>(MockBehavior.Strict);
            comparerFactMock.Setup(m => m.GetComparer<DatabaseModel>())
                .Returns(comparerMock.Object);

            var comparer = new SchemeComparer(comparerFactMock.Object);

            // Act
            comparer.Compare(lftScheme, rgtScheme);
        }
    }
}