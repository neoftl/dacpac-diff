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
            var tgt = new SchemaModel(DatabaseModel.Empty, "LSchema");

            var comparerFactMock = new Mock<IModelComparerFactory>();

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, null).ToArray();

            // Assert
            var diff = (DiffSchemaCreate)res.Single();
            Assert.AreSame(tgt, diff.Schema);
        }

        [TestMethod]
        public void Compare__Null_left__Drop_schema()
        {
            // Arrange
            var cur = new SchemaModel(DatabaseModel.Empty, "RSchema");

            var comparerFactMock = new Mock<IModelComparerFactory>();

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(null, cur).ToArray();

            // Assert
            var diff = (DiffObjectDrop)res.Single();
            Assert.AreSame(cur, diff.Model);
        }
        
        [TestMethod]
        public void Compare__Null_right__Compares_objects_to_null()
        {
            // Arrange
            var tgt = new SchemaModel(DatabaseModel.Empty, "LSchema");
            tgt.Modules["LMod1"] = new FunctionModuleModel(tgt, "LMod1");
            tgt.Synonyms["LSyn1"] = new SynonymModel(tgt, "LSyn1", "");
            tgt.Tables["LTbl1"] = new TableModel(tgt, "LTbl1");
            tgt.UserTypes["LUType1"] = new UserTypeModel(tgt, "LUType1");
            
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
            _ = comp.Compare(tgt, null).ToArray();

            // Assert
            mocks.VerifyAll();
        }
        
        [TestMethod]
        public void Compare__Null_left__Compares_objects_to_null()
        {
            // Arrange
            var cur = new SchemaModel(DatabaseModel.Empty, "RSchema");
            cur.Modules["RMod1"] = new FunctionModuleModel(cur, "RMod1" );
            cur.Synonyms["RSyn1"] = new SynonymModel(cur, "RSyn1", "");
            cur.Tables["RTbl1"] = new TableModel(cur, "RTbl1");
            cur.UserTypes["RUType1"] = new UserTypeModel(cur, "RUType1");
            
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
            _ = comp.Compare(null, cur).ToArray();

            // Assert
            mocks.VerifyAll();
        }

        [TestMethod]
        public void Compare__Compares_modules()
        {
            // Arrange
            var tgt = new SchemaModel(DatabaseModel.Empty, "LSchema");
            tgt.Modules["LMod1"] = new FunctionModuleModel(tgt, "LMod1");
            tgt.Modules["XMod2"] = new FunctionModuleModel(tgt, "XMod2");

            var cur = new SchemaModel(DatabaseModel.Empty, "RSchema");
            cur.Modules["XMod2"] = new FunctionModuleModel(cur, "XMod2");
            cur.Modules["RMod3"] = new FunctionModuleModel(cur, "RMod3");

            var comparerMock = new Mock<IModelComparer<ModuleModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<ModuleModel>(), It.IsAny<ModuleModel>()))
                .Returns(new[] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<ModuleModel>())
                .Returns(comparerMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(tgt.Modules["LMod1"], null), Times.Once);
            comparerMock.Verify(m => m.Compare(tgt.Modules["XMod2"], cur.Modules["XMod2"]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, cur.Modules["RMod3"]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_synonyms()
        {
            // Arrange
            var tgt = new SchemaModel(DatabaseModel.Empty, "LSchema");
            tgt.Synonyms["LSyn1"] = new SynonymModel(tgt, "LSyn1", "");
            tgt.Synonyms["XSyn2"] = new SynonymModel(tgt, "XSyn2", "");

            var cur = new SchemaModel(DatabaseModel.Empty, "RSchema");
            cur.Synonyms["XSyn2"] = new SynonymModel(cur, "XSyn2", "");
            cur.Synonyms["RSyn3"] = new SynonymModel(cur, "RSyn3", "");

            var comparerMock = new Mock<IModelComparer<SynonymModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<SynonymModel>(), It.IsAny<SynonymModel>()))
                .Returns(new[] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<SynonymModel>())
                .Returns(comparerMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(tgt.Synonyms["LSyn1"], null), Times.Once);
            comparerMock.Verify(m => m.Compare(tgt.Synonyms["XSyn2"], cur.Synonyms["XSyn2"]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, cur.Synonyms["RSyn3"]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_tables()
        {
            // Arrange
            var tgt = new SchemaModel(DatabaseModel.Empty, "LSchema");
            tgt.Tables["LTbl1"] = new TableModel(tgt, "LTbl1");
            tgt.Tables["XTbl2"] = new TableModel(tgt, "XTbl2");

            var cur = new SchemaModel(DatabaseModel.Empty, "RSchema");
            cur.Tables["XTbl2"] = new TableModel(cur, "XTbl2");
            cur.Tables["RTbl3"] = new TableModel(cur, "RTbl3");

            var comparerMock = new Mock<IModelComparer<TableModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<TableModel>(), It.IsAny<TableModel>()))
                .Returns(new[] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<TableModel>())
                .Returns(comparerMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(tgt.Tables["LTbl1"], null), Times.Once);
            comparerMock.Verify(m => m.Compare(tgt.Tables["XTbl2"], cur.Tables["XTbl2"]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, cur.Tables["RTbl3"]), Times.Once);
        }

        [TestMethod]
        public void Compare__Compares_usertypes()
        {
            // Arrange
            var tgt = new SchemaModel(DatabaseModel.Empty, "LSchema");
            tgt.UserTypes["LUType1"] = new UserTypeModel(tgt, "LUType1");
            tgt.UserTypes["XUType2"] = new UserTypeModel(tgt, "XUType2");

            var cur = new SchemaModel(DatabaseModel.Empty, "RSchema");
            cur.UserTypes["XUType2"] = new UserTypeModel(cur, "XUType2");
            cur.UserTypes["RUType3"] = new UserTypeModel(cur, "RUType3");

            var comparerMock = new Mock<IModelComparer<UserTypeModel>>();
            comparerMock.Setup(m => m.Compare(It.IsAny<UserTypeModel>(), It.IsAny<UserTypeModel>()))
                .Returns(new[] { new Mock<IDifference>().Object });

            var comparerFactMock = new Mock<IModelComparerFactory>();
            comparerFactMock.Setup(m => m.GetComparer<UserTypeModel>())
                .Returns(comparerMock.Object);

            var comp = new SchemaComparer(comparerFactMock.Object);

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            comparerMock.Verify(m => m.Compare(tgt.UserTypes["LUType1"], null), Times.Once);
            comparerMock.Verify(m => m.Compare(tgt.UserTypes["XUType2"], cur.UserTypes["XUType2"]), Times.Once);
            comparerMock.Verify(m => m.Compare(null, cur.UserTypes["RUType3"]), Times.Once);
        }
    }
}