using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DacpacDiff.Core.Diff.Tests;

[TestClass]
public class DiffObjectDropTests
{
    [TestMethod]
    [DynamicData(nameof(getModules), DynamicDataSourceType.Method)]
    public void DiffObjectDrop_Module__Maps_module_types(ModuleModel mod)
    {
        // Act
        var obj = new DiffObjectDrop(mod);

        // Assert
        Assert.AreSame(mod, obj.Model);
        Assert.AreEqual("[LSchema].[LMod]", obj.Name);
        Assert.AreEqual("Drop " + mod.Type.ToString().ToLower(), obj.Title);
        Assert.AreEqual(mod.Type.ToString(), obj.Type.ToString());
    }
    private static IEnumerable<object[]> getModules()
    {
        return
        [
            [new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")],
            [new IndexModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")],
            [new ProcedureModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")],
            [new TriggerModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")],
            [new ViewModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")],
        ];
    }

    [ExcludeFromCodeCoverage]
    class NoneModuleModel : ModuleModel
    {
        public NoneModuleModel() : base(SchemaModel.Empty, string.Empty, ModuleType.NONE) { }
        public override bool IsSimilarDefinition(ModuleModel other) => throw new NotImplementedException();
    }
    [TestMethod]
    public void DiffObjectDrop_Module__Does_not_support_NONE()
    {
        // Arrange
        var mod = new NoneModuleModel();

        // Act
        Assert.ThrowsException<NotSupportedException>(() => new DiffObjectDrop(mod));
    }

    [TestMethod]
    public void DiffObjectDrop_Schema()
    {
        // Arrange
        var sch = new SchemaModel(DatabaseModel.Empty, "schema");

        // Act
        var obj = new DiffObjectDrop(sch);

        // Assert
        Assert.AreSame(sch, obj.Model);
        Assert.AreEqual("[schema]", obj.Name);
        Assert.AreEqual("Drop schema", obj.Title);
        Assert.AreEqual(DiffObjectDrop.ObjectType.SCHEMA, obj.Type);
    }

    [TestMethod]
    public void DiffObjectDrop_Synonym()
    {
        // Arrange
        var syn = new SynonymModel(new SchemaModel(DatabaseModel.Empty, "schema"), "RSynonym", "[schema2].[target]");

        // Act
        var obj = new DiffObjectDrop(syn);

        // Assert
        Assert.AreSame(syn, obj.Model);
        Assert.AreEqual("[schema].[RSynonym]", obj.Name);
        Assert.AreEqual("Drop synonym", obj.Title);
        Assert.AreEqual(DiffObjectDrop.ObjectType.SYNONYM, obj.Type);
    }

    [TestMethod]
    public void DiffObjectDrop_Table()
    {
        // Arrange
        var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "schema"), "RTable");

        // Act
        var obj = new DiffObjectDrop(tbl);

        // Assert
        Assert.AreSame(tbl, obj.Model);
        Assert.AreEqual("[schema].[RTable]", obj.Name);
        Assert.AreEqual("Drop table", obj.Title);
        Assert.AreEqual(DiffObjectDrop.ObjectType.TABLE, obj.Type);
    }

    [TestMethod]
    [DynamicData(nameof(getMostObjectTypes), DynamicDataSourceType.Method)]
    public void GetDataLossTable__Most_object_types__False(DiffObjectDrop.ObjectType objType)
    {
        // Arrange
        var sch = new SchemaModel(DatabaseModel.Empty, "schema");

        var obj = new DiffObjectDrop(sch);
        typeof(DiffObjectDrop).GetField("<Type>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(obj, objType);

        // Act
        var res = obj.GetDataLossTable(out _);

        // Assert
        Assert.IsFalse(res);
    }
    [ExcludeFromCodeCoverage]
    private static object[][] getMostObjectTypes()
    {
        return Enum.GetValues<DiffObjectDrop.ObjectType>()
            .Where(e => e is not DiffObjectDrop.ObjectType.SEQUENCE and not DiffObjectDrop.ObjectType.TABLE)
            .Select(e => new object[] { e }).ToArray();
    }

    [TestMethod]
    [DataRow(DiffObjectDrop.ObjectType.SEQUENCE)]
    [DataRow(DiffObjectDrop.ObjectType.TABLE)]
    public void GetDataLossTable__Some_types__True(DiffObjectDrop.ObjectType objType)
    {
        // Arrange
        var sch = new SchemaModel(DatabaseModel.Empty, "schema");

        var obj = new DiffObjectDrop(sch);
        typeof(DiffObjectDrop).GetField("<Type>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(obj, objType);

        // Act
        var res = obj.GetDataLossTable(out var tableName);

        // Assert
        Assert.IsTrue(res);
        Assert.AreEqual(obj.Name, tableName);
    }
}