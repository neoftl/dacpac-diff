using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Mssql.Diff.Tests
{
    [TestClass]
    public class MssqlDiffObjectDropTests
    {
        private static IEnumerable<object[]> getNotNoneModuleTypes()
        {
            return Enum.GetValues<ModuleModel.ModuleType>()
                .Where(e => e != ModuleModel.ModuleType.NONE)
                .Select(e => new object[] { e });
        }

        class FakeModuleModel : ModuleModel
        {
            public FakeModuleModel(SchemaModel schema, string name, ModuleType type)
                : base(schema, name, type)
            {
            }

            public override bool IsSimilarDefinition(ModuleModel other) => throw new NotImplementedException();
        }

        [TestMethod]
        [DynamicData(nameof(getNotNoneModuleTypes), DynamicDataSourceType.Method)]
        public void MssqlDiffObjectDrop__NonIndex_drops(ModuleModel.ModuleType modType)
        {
            if (modType == ModuleModel.ModuleType.INDEX) { return; }

            // Arrange
            var mod = new FakeModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod", modType);

            var diff = new DiffObjectDrop(mod);

            // Act
            var res = new MssqlDiffObjectDrop(diff).ToString().Trim();

            // Assert
            Assert.AreEqual($"DROP {modType} [RSchema].[RMod]", res);
        }

        [TestMethod]
        public void MssqlDiffObjectDrop__Index_drop()
        {
            // Arrange
            var mod = new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RMod")
            {
                IndexedObjectFullName = "[ISchema].[ITable]"
            };

            var diff = new DiffObjectDrop(mod);

            // Act
            var res = new MssqlDiffObjectDrop(diff).ToString().Trim();

            // Assert
            Assert.AreEqual($"DROP INDEX [RMod] ON [ISchema].[ITable]", res);
        }
    }
}