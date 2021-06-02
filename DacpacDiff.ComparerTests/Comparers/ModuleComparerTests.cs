using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers.Tests
{
    [TestClass]
    public class ModuleComparerTests
    {
        private static IEnumerable<object[]> getModules()
        {
            return new[]
            {
                new object[] { new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod") },
                new object[] { new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod") },
                new object[] { new ProcedureModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod") },
                new object[] { new TriggerModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod") },
                new object[] { new ViewModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod") },
            };
        }
        private static IEnumerable<object[]> getTwoModules()
        {
            var rgt = new ModuleModel[]
            {
                new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
                new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod") ,
                new ProcedureModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod") ,
                new TriggerModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
                new ViewModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
            };

            return getModules().SelectMany(m => m)
                .SelectMany(m => rgt.Where(r => r.GetType() == m.GetType()).Select(n => new[] { m, n }).ToArray());
        }
        private static IEnumerable<object[]> getAnyTwoModules()
        {
            var rgt = new ModuleModel[]
            {
                new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
                new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod") ,
                new ProcedureModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod") ,
                new TriggerModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
                new ViewModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
            };

            return getModules().SelectMany(m => m)
                .SelectMany(m => rgt.Select(n => new[] { m, n }).ToArray());
        }

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
        [DynamicData(nameof(getModules), DynamicDataSourceType.Method)]
        public void Compare__Null_right__Create_module(ModuleModel lft)
        {
            // Arrange
            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, null).ToArray();

            // Assert
            var diff = (DiffModuleCreate)res.First();
            Assert.AreSame(lft, diff.Module);
            if (diff.NeedsStub)
            {
                var diff2 = (DiffModuleAlter)res.Skip(1).Single();
                Assert.AreSame(lft, diff2.Module);
            }
        }

        [TestMethod]
        [DynamicData(nameof(getModules), DynamicDataSourceType.Method)]
        public void Compare__Null_left__Drop_module(ModuleModel rgt)
        {
            // Arrange
            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(null, rgt).ToArray();

            // Assert
            var diff = (DiffObjectDrop)res.Single();
            Assert.AreSame(rgt, diff.Model);
        }

        [TestMethod]
        [DynamicData(nameof(getTwoModules), DynamicDataSourceType.Method)]
        public void Compare__Similar_body__Noop(ModuleModel lft, ModuleModel rgt)
        {
            // Arrange
            if (lft is not IModuleWithBody modL || rgt is not IModuleWithBody modR)
            {
                return;
            }

            modL.Body = "Def[]";
            modR.Body = "Def()";

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        [DynamicData(nameof(getAnyTwoModules), DynamicDataSourceType.Method)]
        public void Compare__Type_change__Drop_right_Create_left_No_stub(ModuleModel lft, ModuleModel rgt)
        {
            if (lft.Type == rgt.Type
                || !new[] { ModuleModel.ModuleType.INDEX, ModuleModel.ModuleType.SEQUENCE, ModuleModel.ModuleType.TRIGGER }.Contains(lft.Type))
            {
                return;
            }

            // Arrange
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
        [DynamicData(nameof(getAnyTwoModules), DynamicDataSourceType.Method)]
        public void Compare__Type_change__Drop_right_Create_left_With_stub(ModuleModel lft, ModuleModel rgt)
        {
            if (lft.Type == rgt.Type
                || !new[] { ModuleModel.ModuleType.FUNCTION, ModuleModel.ModuleType.PROCEDURE, ModuleModel.ModuleType.VIEW }.Contains(lft.Type))
            {
                return;
            }

            // Arrange
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
        [DynamicData(nameof(getTwoModules), DynamicDataSourceType.Method)]
        public void Compare__Non_index_definition_change__Alter(ModuleModel lft, ModuleModel rgt)
        {
            if (lft.Type == ModuleModel.ModuleType.INDEX)
            {
                return;
            }
         
            // Arrange
            if (rgt is IModuleWithBody m)
            {
                m.Body = "XBody";
            }

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
            var lft = new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                IndexedColumns = new [] { "Col1" }
            };
            var rgt = new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod")
            {
                IndexedColumns = new [] { "Col2" }
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