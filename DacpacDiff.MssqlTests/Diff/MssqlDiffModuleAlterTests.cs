using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Mssql.Diff.Tests
{
    [TestClass]
    public class MssqlDiffModuleAlterTests
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
        
        [ExcludeFromCodeCovarage]
        class FakeModuleModel : ModuleModel
        {
            public FakeModuleModel(SchemaModel schema, string name, ModuleType type)
                : base(schema, name, type)
            {
            }

            public override bool IsSimilarDefinition(ModuleModel other) => throw new NotImplementedException();
        }

        [TestMethod]
        [DynamicData(nameof(getModules), DynamicDataSourceType.Method)]
        public void MssqlDiffModuleAlter__Alter_nonIndex(ModuleModel mod)
        {
            if (mod.Type == ModuleModel.ModuleType.INDEX) { return; }

            // Arrange
            var diff = new DiffModuleAlter(mod);

            // Act
            var res = new MssqlDiffModuleAlter(diff).ToString().Trim();

            // Assert
            StringAssert.StartsWith(res, $"ALTER {mod.Type} [LSchema].[LMod]");
        }

        [TestMethod]
        [DataRow(false, false, "INDEX")]
        [DataRow(true, false, "UNIQUE INDEX")]
        [DataRow(false, true, "CLUSTERED INDEX")]
        [DataRow(true, true, "UNIQUE CLUSTERED INDEX")]
        public void MssqlDiffModuleAlter__Index(bool isUnique, bool isClustered, string exp)
        {
            // Arrange
            var mod = new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                IsUnique = isUnique,
                IsClustered = isClustered
            };

            var diff = new DiffModuleAlter(mod);

            // Act
            var res = new MssqlDiffModuleAlter(diff).ToString().Trim();

            // Assert
            StringAssert.StartsWith(res, $"ALTER {exp} [LMod] ON ");
        }

        //[TestMethod]
        //[DynamicData(nameof(getModules), DynamicDataSourceType.Method)]
        //public void MssqlDiffModuleAlter__Unhandled_type(ModuleModel mod)
        //{
        //    if (mod.Type == ModuleModel.ModuleType.INDEX) { return; }

        //    // Arrange
        //    var mod = new FakeModuleModel(SchemaModel.Empty, string.Empty, ModuleModel.ModuleType.NONE);

        //    var diff = new DiffModuleAlter(mod);

        //    // Act
        //    var res = new MssqlDiffModuleAlter(diff).ToString().Trim();

        //    // Assert
        //    StringAssert.StartsWith(res, $"ALTER {mod.Type} [LSchema].[LMod]");
        //}
    }
}