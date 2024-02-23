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
            var cur = new ModuleModel[]
            {
                new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
                new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod") ,
                new ProcedureModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod") ,
                new TriggerModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
                new ViewModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
            };

            return getModules().SelectMany(m => m)
                .SelectMany(m => cur.Where(r => r.GetType() == m.GetType()).Select(n => new[] { m, n }).ToArray());
        }
        private static IEnumerable<object[]> getAnyTwoModules()
        {
            var cur = new ModuleModel[]
            {
                new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
                new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod") ,
                new ProcedureModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod") ,
                new TriggerModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
                new ViewModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod"),
            };

            return getModules().SelectMany(m => m)
                .SelectMany(m => cur.Select(n => new[] { m, n }).ToArray());
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
        public void Compare__Null_right__Create_module(ModuleModel tgt)
        {
            // Arrange
            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(tgt, null).ToArray();

            // Assert
            CollectionAssert.That.AreEqual(res,
                e => e is DiffModuleCreate d && d.Model == tgt);
            Assert.IsFalse(((DiffModuleCreate)res[0]).DoAsAlter);
        }

        [TestMethod]
        [DynamicData(nameof(getModules), DynamicDataSourceType.Method)]
        public void Compare__Null_left__Drop_module(ModuleModel cur)
        {
            // Arrange
            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(null, cur).ToArray();

            // Assert
            var diff = (DiffObjectDrop)res.Single();
            Assert.AreSame(cur, diff.Model);
        }

        [TestMethod]
        [DynamicData(nameof(getTwoModules), DynamicDataSourceType.Method)]
        public void Compare__Similar_body__Noop(ModuleModel tgt, ModuleModel cur)
        {
            // Arrange
            if (tgt is not IModuleWithBody modL || cur is not IModuleWithBody modR)
            {
                return;
            }

            modL.Body = "Def[]";
            modR.Body = "Def()";

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        [DynamicData(nameof(getAnyTwoModules), DynamicDataSourceType.Method)]
        public void Compare__Type_change__Drop_right_Create_left(ModuleModel tgt, ModuleModel cur)
        {
            if (tgt.Type == cur.Type)
            {
                return;
            }

            // Arrange
            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            CollectionAssert.That.AreEqual(res,
                e => e is RecreateObject<ModuleModel> d && d.Model == tgt && d.OldModel == cur);
        }

        [TestMethod]
        [DynamicData(nameof(getTwoModules), DynamicDataSourceType.Method)]
        public void Compare__Definition_change__Alter(ModuleModel tgt, ModuleModel cur)
        {
            // Arrange
            if (cur is IModuleWithBody m)
            {
                m.Body = "XBody";
            }
            else if (cur is IndexModuleModel idx)
            {
                idx.Condition = "X";
            }

            var comp = new ModuleComparer();

            // Act
            var res = comp.Compare(tgt, cur).ToArray();

            // Assert
            CollectionAssert.That.AreEqual(res,
                e => e is AlterObject<ModuleModel> d && d.Model == tgt && d.OldModel == cur);
        }
    }
}