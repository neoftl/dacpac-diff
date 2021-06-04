using DacpacDiff.Comparer.Tests.TestHelpers;
using DacpacDiff.Core.Changes;
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
            CollectionAssert.That.AreEqual(res,
                e => e is DiffModuleCreate d && d.Model == lft);
            Assert.IsFalse(((DiffModuleCreate)res[0]).DoAsAlter);
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
        public void Compare__Type_change__Drop_right_Create_left(ModuleModel lft, ModuleModel rgt)
        {
            if (lft.Type == rgt.Type)
            {
                return;
            }

            // Arrange
            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            CollectionAssert.That.AreEqual(res,
                e => e is RecreateObject<ModuleModel> d && d.Model == lft && d.OldModel == rgt);
        }

        [TestMethod]
        [DynamicData(nameof(getTwoModules), DynamicDataSourceType.Method)]
        public void Compare__Definition_change__Alter(ModuleModel lft, ModuleModel rgt)
        {
            // Arrange
            if (rgt is IModuleWithBody m)
            {
                m.Body = "XBody";
            }
            else if (rgt is IndexModuleModel idx)
            {
                idx.Condition = "X";
            }

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(lft, rgt).ToArray();

            // Assert
            CollectionAssert.That.AreEqual(res,
                e => e is AlterObject<ModuleModel> d && d.Model == lft && d.OldModel == rgt);
        }
    }
}