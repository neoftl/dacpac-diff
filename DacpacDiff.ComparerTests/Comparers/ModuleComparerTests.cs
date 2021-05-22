using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers.Tests
{
    [TestClass]
    public class ModuleComparerTests
    {
        [TestMethod]
        public void Compare__Both_null__Noop()
        {
            // Arrange
            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(null, null).ToArray();

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        public void Compare__Null_right__Create_module()
        {
            // Arrange
            var lft = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod", ModuleModel.ModuleType.NONE);

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, null).ToArray();

            // Assert
            var diff = (DiffModuleCreate)res.Single();
            Assert.AreSame(lft, diff.Module);
        }

        [TestMethod]
        public void Compare__Null_left__Drop_module()
        {
            // Arrange
            var rgt = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod", ModuleModel.ModuleType.FUNCTION);

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(null, rgt).ToArray();

            // Assert
            var diff = (DiffObjectDrop)res.Single();
            Assert.AreSame(rgt, diff.Model);
        }

        [TestMethod]
        public void Compare__No_change__Noop()
        {
            // Arrange
            var lft = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod", ModuleModel.ModuleType.FUNCTION)
            {
                Definition = "Def[]"
            };
            var rgt = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod", ModuleModel.ModuleType.FUNCTION)
            {
                Definition = "Def()"
            };

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        [DataRow(ModuleModel.ModuleType.INDEX)]
        [DataRow(ModuleModel.ModuleType.SEQUENCE)]
        [DataRow(ModuleModel.ModuleType.TRIGGER)]
        public void Compare__Type_change__Drop_right_Create_left_No_stub(ModuleModel.ModuleType type)
        {
            // Arrange
            var lft = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod", type);
            var rgt = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod", ModuleModel.ModuleType.FUNCTION);

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(2, res.Length);
            Assert.AreSame(rgt, ((DiffObjectDrop)res[0]).Model);
            Assert.AreSame(lft, ((DiffModuleCreate)res[1]).Model);
            Assert.IsFalse(((DiffModuleCreate)res[1]).NeedsStub); // Based on type
        }

        [TestMethod]
        [DataRow(ModuleModel.ModuleType.FUNCTION)]
        [DataRow(ModuleModel.ModuleType.PROCEDURE)]
        [DataRow(ModuleModel.ModuleType.VIEW)]
        public void Compare__Type_change__Drop_right_Create_left_With_stub(ModuleModel.ModuleType type)
        {
            // Arrange
            var lft = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod", type);
            var rgt = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod", ModuleModel.ModuleType.INDEX);

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(3, res.Length);
            Assert.AreSame(rgt, ((DiffObjectDrop)res[0]).Model);
            Assert.AreSame(lft, ((DiffModuleCreate)res[1]).Module);
            Assert.IsTrue(((DiffModuleCreate)res[1]).NeedsStub); // Based on type
            Assert.AreSame(lft, ((DiffModuleAlter)res[2]).Module);
        }

        [TestMethod]
        [DataRow(ModuleModel.ModuleType.FUNCTION)]
        [DataRow(ModuleModel.ModuleType.PROCEDURE)]
        [DataRow(ModuleModel.ModuleType.SEQUENCE)]
        [DataRow(ModuleModel.ModuleType.TRIGGER)]
        [DataRow(ModuleModel.ModuleType.VIEW)]
        public void Compare__Non_index_definition_change__Alter(ModuleModel.ModuleType type)
        {
            // Arrange
            var lft = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod", type)
            {
                Definition = "LDef"
            };
            var rgt = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod", type)
            {
                Definition = "RDef"
            };

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreSame(lft, ((DiffModuleAlter)res.Single()).Module);
        }

        [TestMethod]
        public void Compare__Index_definition_change__Drop_right_Create_left()
        {
            // Arrange
            var lft = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod", ModuleModel.ModuleType.INDEX)
            {
                Definition = "LDef"
            };
            var rgt = new ModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod", ModuleModel.ModuleType.INDEX)
            {
                Definition = "RDef"
            };

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(2, res.Length);
            Assert.AreSame(rgt, ((DiffObjectDrop)res[0]).Model);
            Assert.AreSame(lft, ((DiffModuleCreate)res[1]).Module);
        }
    }
}