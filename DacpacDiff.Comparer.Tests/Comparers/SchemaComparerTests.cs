using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers.Tests
{
    [TestClass]
    public class SchemaComparerTests
    {
        [TestMethod]
        public void Compare__Both_null__Noop()
        {
            // Arrange
            var comp = new SchemaComparer(new Mock<IModelComparerFactory>().Object);

            // Act
            var res = comp.Compare(null, null).ToArray();

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        public void Compare__Null_right__Create_schema()
        {
            // Arrange
            var lft = new SchemaModel(DatabaseModel.Empty, "LSchema");

            var comparerFactMock = new Mock<IModelComparerFactory>();

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, null).ToArray();

            // Assert
            var diff = (DiffSchemaCreate)res.Single();
            Assert.AreSame(lft, diff.Schema);
        }

        [TestMethod]
        public void Compare__Null_left__Drop_schema()
        {
            // Arrange
            var rgt = new SchemaModel(DatabaseModel.Empty, "RSchema");

            var comparerFactMock = new Mock<IModelComparerFactory>();

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(null, rgt).ToArray();

            // Assert
            var diff = (DiffObjectDrop)res.Single();
            Assert.AreSame(rgt, diff.Model);
        }
        
        [TestMethod]
        public void Compare__Null_right__Compares_objects_to_null()
        {
            // Arrange
            var lft = new SchemaModel(DatabaseModel.Empty, "LSchema");
            lft.Modules["LMod1"] = new FunctionModuleModel(lft, "LMod1");
            lft.Synonyms["LSyn1"] = new SynonymModel(lft, "LSyn1", "");
            lft.Tables["LTbl1"] = new TableModel(lft, "LTbl1");
            lft.UserTypes["LUType1"] = new UserTypeModel(lft, "LUType1");
            
            var mocks = new MockRepository(MockBehavior.Strict);
            var comparerModMock = mocks.Create<IModelComparer<ModuleModel>>();
            comparerModMock.Setup(m => m.Compare(It.IsAny<ModuleModel>(), null)).Returns(Array.Empty<IDifference>());
            var comparerSynMock = mocks.Create<IModelComparer<SynonymModel>>();
            comparerSynMock.Setup(m => m.Compare(It.IsAny<SynonymModel>(), null)).Returns(Array.Empty<IDifference>());
            var comparerTblMock = mocks.Create<IModelComparer<TableModel>>();
            comparerTblMock.Setup(m => m.Compare(It.IsAny<TableModel>(), null)).Returns(Array.Empty<IDifference>());
            var comparerUTypeMock = mocks.Create<IModelComparer<UserTypeModel>>();
            comparerUTypeMock.Setup(m => m.Compare(It.IsAny<UserTypeModel>(), null)).Returns(Array.Empty<IDifference>());

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<ModuleModel>()).Returns(comparerModMock.Object);
            comparerFactMock.Setup(m => m.GetComparer<SynonymModel>()).Returns(comparerSynMock.Object);
            comparerFactMock.Setup(m => m.GetComparer<TableModel>()).Returns(comparerTblMock.Object);
            comparerFactMock.Setup(m => m.GetComparer<UserTypeModel>()).Returns(comparerUTypeMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            _ = comp.Compare(lft, null).ToArray();

            // Assert
            mocks.VerifyAll();
        }
        
        [TestMethod]
        public void Compare__Null_left__Compares_objects_to_null()
        {
            // Arrange
            var rgt = new SchemaModel(DatabaseModel.Empty, "RSchema");
            rgt.Modules["RMod1"] = new FunctionModuleModel(rgt, "RMod1" );
            rgt.Synonyms["RSyn1"] = new SynonymModel(rgt, "RSyn1", "");
            rgt.Tables["RTbl1"] = new TableModel(rgt, "RTbl1");
            rgt.UserTypes["RUType1"] = new UserTypeModel(rgt, "RUType1");
            
            var mocks = new MockRepository(MockBehavior.Strict);
            var comparerModMock = mocks.Create<IModelComparer<ModuleModel>>();
            comparerModMock.Setup(m => m.Compare(null, It.IsAny<ModuleModel>())).Returns(Array.Empty<IDifference>());
            var comparerSynMock = mocks.Create<IModelComparer<SynonymModel>>();
            comparerSynMock.Setup(m => m.Compare(null, It.IsAny<SynonymModel>())).Returns(Array.Empty<IDifference>());
            var comparerTblMock = mocks.Create<IModelComparer<TableModel>>();
            comparerTblMock.Setup(m => m.Compare(null, It.IsAny<TableModel>())).Returns(Array.Empty<IDifference>());
            var comparerUTypeMock = mocks.Create<IModelComparer<UserTypeModel>>();
            comparerUTypeMock.Setup(m => m.Compare(null, It.IsAny<UserTypeModel>())).Returns(Array.Empty<IDifference>());

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<ModuleModel>()).Returns(comparerModMock.Object);
            comparerFactMock.Setup(m => m.GetComparer<SynonymModel>()).Returns(comparerSynMock.Object);
            comparerFactMock.Setup(m => m.GetComparer<TableModel>()).Returns(comparerTblMock.Object);
            comparerFactMock.Setup(m => m.GetComparer<UserTypeModel>()).Returns(comparerUTypeMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            _ = comp.Compare(null, rgt).ToArray();

            // Assert
            mocks.VerifyAll();
        }

        [TestMethod]
        public void Compare__Compares_modules()
        {
            // Arrange
            var lft = new SchemaModel(DatabaseModel.Empty, "LSchema");
            lft.Modules["LMod1"] = new FunctionModuleModel(lft, "LMod1");
            lft.Modules["XMod2"] = new FunctionModuleModel(lft, "XMod2");

            var rgt = new SchemaModel(DatabaseModel.Empty, "RSchema");
            rgt.Modules["XMod2"] = new FunctionModuleModel(rgt, "XMod2");
            rgt.Modules["RMod3"] = new FunctionModuleModel(rgt, "RMod3");

            var comparerMock = new Mock<IModelComparer<ModuleModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<ModuleModel>(), It.IsAny<ModuleModel>()))
                .Returns(new[] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<ModuleModel>())
                .Returns(comparerMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(lft.Modules["LMod1"], null), Times.Once);
            comparerMock.Verify(m => m.Compare(lft.Modules["XMod2"], rgt.Modules["XMod2"]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, rgt.Modules["RMod3"]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_synonyms()
        {
            // Arrange
            var lft = new SchemaModel(DatabaseModel.Empty, "LSchema");
            lft.Synonyms["LSyn1"] = new SynonymModel(lft, "LSyn1", "");
            lft.Synonyms["XSyn2"] = new SynonymModel(lft, "XSyn2", "");

            var rgt = new SchemaModel(DatabaseModel.Empty, "RSchema");
            rgt.Synonyms["XSyn2"] = new SynonymModel(rgt, "XSyn2", "");
            rgt.Synonyms["RSyn3"] = new SynonymModel(rgt, "RSyn3", "");

            var comparerMock = new Mock<IModelComparer<SynonymModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<SynonymModel>(), It.IsAny<SynonymModel>()))
                .Returns(new[] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<SynonymModel>())
                .Returns(comparerMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(lft.Synonyms["LSyn1"], null), Times.Once);
            comparerMock.Verify(m => m.Compare(lft.Synonyms["XSyn2"], rgt.Synonyms["XSyn2"]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, rgt.Synonyms["RSyn3"]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_tables()
        {
            // Arrange
            var lft = new SchemaModel(DatabaseModel.Empty, "LSchema");
            lft.Tables["LTbl1"] = new TableModel(lft, "LTbl1");
            lft.Tables["XTbl2"] = new TableModel(lft, "XTbl2");

            var rgt = new SchemaModel(DatabaseModel.Empty, "RSchema");
            rgt.Tables["XTbl2"] = new TableModel(rgt, "XTbl2");
            rgt.Tables["RTbl3"] = new TableModel(rgt, "RTbl3");

            var comparerMock = new Mock<IModelComparer<TableModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<TableModel>(), It.IsAny<TableModel>()))
                .Returns(new[] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<TableModel>())
                .Returns(comparerMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(lft.Tables["LTbl1"], null), Times.Once);
            comparerMock.Verify(m => m.Compare(lft.Tables["XTbl2"], rgt.Tables["XTbl2"]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, rgt.Tables["RTbl3"]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_usertypes()
        {
            // Arrange
            var lft = new SchemaModel(DatabaseModel.Empty, "LSchema");
            lft.UserTypes["LUType1"] = new UserTypeModel(lft, "LUType1");
            lft.UserTypes["XUType2"] = new UserTypeModel(lft, "XUType2");

            var rgt = new SchemaModel(DatabaseModel.Empty, "RSchema");
            rgt.UserTypes["XUType2"] = new UserTypeModel(rgt, "XUType2");
            rgt.UserTypes["RUType3"] = new UserTypeModel(rgt, "RUType3");

            var comparerMock = new Mock<IModelComparer<UserTypeModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<UserTypeModel>(), It.IsAny<UserTypeModel>()))
                .Returns(new[] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<UserTypeModel>())
                .Returns(comparerMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(lft.UserTypes["LUType1"], null), Times.Once);
            comparerMock.Verify(m => m.Compare(lft.UserTypes["XUType2"], rgt.UserTypes["XUType2"]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, rgt.UserTypes["RUType3"]), Times.Once);
        }
    }
}