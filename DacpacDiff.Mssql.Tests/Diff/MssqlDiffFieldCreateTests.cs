using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DacpacDiff.Mssql.Diff.Tests
{
    [TestClass]
    public class MssqlDiffFieldCreateTests
    {
        [TestMethod]
        [DataRow(true, "NULL")]
        [DataRow(false, "NULL -- NOTE: Cannot create NOT NULL column")]
        public void MssqlDiffFieldCreate__Nullability(bool nullable, string exp)
        {
            // Arrange
            var fld = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
            {
                Type = "FType",
                Nullable = nullable
            };

            var diff = new DiffFieldCreate(fld);

            // Act
            var res = new MssqlDiffFieldCreate(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD [LField] FType " + exp, res);
        }
        
        [TestMethod]
        [DataRow("0", true, "NULL DEFAULT (0)")]
        [DataRow("1", false, "NOT NULL DEFAULT (1)")]
        public void MssqlDiffFieldCreate__Unnamed_default(string defValue, bool nullable, string exp)
        {
            // Arrange
            var fld = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
            {
                Type = "FType",
                Nullable = nullable
            };
            fld.Default = new FieldDefaultModel(fld, null, defValue);

            var diff = new DiffFieldCreate(fld);

            // Act
            var res = new MssqlDiffFieldCreate(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD [LField] FType " + exp, res);
        }
        
        [TestMethod]
        [DataRow("0", true, "NULL")]
        [DataRow("0", false, "NOT NULL")]
        public void MssqlDiffFieldCreate__Named_default(string defValue, bool nullable, string exp)
        {
            // Arrange
            var fld = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
            {
                Type = "FType",
                Nullable = nullable
            };
            fld.Default = new FieldDefaultModel(fld, "FDef", defValue);

            var diff = new DiffFieldCreate(fld);

            // Act
            var res = new MssqlDiffFieldCreate(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD [LField] FType " + exp, res);
        }
        
        [TestMethod]
        public void MssqlDiffFieldCreate__Unnamed_reference()
        {
            // Arrange
            var fld = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
            {
                Type = "FType",
                Nullable = true
            };
            fld.Ref = new FieldRefModel(fld, new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField"));

            var diff = new DiffFieldCreate(fld);

            // Act
            var res = new MssqlDiffFieldCreate(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD [LField] FType NULL REFERENCES [TSchema].[TTable] ([TField])", res);
        }
        
        [TestMethod]
        public void MssqlDiffFieldCreate__Named_reference__NOOP()
        {
            // Arrange
            var fld = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
            {
                Type = "FType",
                Nullable = true
            };
            fld.Ref = new FieldRefModel(fld, new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField"))
            {
                Name = "FRef",
                IsSystemNamed = false
            };

            var diff = new DiffFieldCreate(fld);

            // Act
            var res = new MssqlDiffFieldCreate(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD [LField] FType NULL", res);
        }
        
        [TestMethod]
        public void MssqlDiffFieldCreate__Computed()
        {
            // Arrange
            var fld = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
            {
                Type = "FType", // Ignored
                Computation = "COMPUTATION"
            };

            fld.Default = new FieldDefaultModel(fld, null, "0"); // Ignored
            fld.Ref = new FieldRefModel(fld, new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField")); // Ignored

            var diff = new DiffFieldCreate(fld);

            // Act
            var res = new MssqlDiffFieldCreate(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD [LField] AS COMPUTATION", res);
        }
    }
}