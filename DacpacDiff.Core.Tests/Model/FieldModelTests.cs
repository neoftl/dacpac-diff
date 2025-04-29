using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Model.Tests
{
    [TestClass]
    public class FieldModelTests
    {
        private static FieldModel getFieldAll(string name = "Field", TableModel? parent = null)
        {
            parent ??= new TableModel(new SchemaModel(DatabaseModel.Empty, "Schema"), "Table");
            return new FieldModel(parent, name)
            {
                Computation = "FComp",
                Type = "FType",
                Default = new FieldDefaultModel(FieldModel.Empty, "FDefault", "def VALUE[]"),
                Order = 5,
                Nullable = true,
                IsUnique = true,
                IsPrimaryKey = true,
                Identity = true,
            };
        }

        [TestMethod]
        public void Equals__Null__False()
        {
            // Arrange
            var p1 = getFieldAll();

            // Act
            var res = p1.Equals(null);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Equals__Full_match__True(bool test)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field1");
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsTrue(res);
        }

        [TestMethod]
        [DataRow(true , "schemaX", "schema", "table", "table")]
        [DataRow(true , "schema", "schema", "table", "table")]
        [DataRow(false, "schemaX", "schema", "tableX", "table")]
        [DataRow(false, "schema", "schema", "tableX", "table")]
        public void Equals__Diff_parent_name__False(bool test, string schema1, string schema2, string table1, string table2)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");

            var par1 = new TableModel(new SchemaModel(DatabaseModel.Empty, schema1), table1);
            var par2 = new TableModel(new SchemaModel(DatabaseModel.Empty, schema2), table2);
            
            var p1 = getFieldAll("Field1", par1);
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field2", par2);
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Equals__Diff_name__False(bool test)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field2");
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Equals__Diff_type__False(bool test)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Type = "XType";
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field2");
            p1.Type = "YType";
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        [DataRow(true, "Comp1", "Comp2")]
        [DataRow(false, "Comp1", null)]
        [DataRow(false, null, "Comp2")]
        public void Equals__Diff_computation__False(bool test, string? comp1, string? comp2)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Computation = comp1;
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field2");
            p1.Computation = comp2;
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(true, true, null, "Val1", true, null, "Val2")]
        [DataRow(true, true, "Def1", "Val1", true, null, "Val2")]
        [DataRow(true, true, null, "Val1", true, "Def2", "Val2")]
        [DataRow(true, true, "Def1", "Val1", true, "Def2", "Val2")]
        [DataRow(true, true, null, "Val1", false, null, null)]
        [DataRow(true, true, "Def1", "Val1", false, null, null)]
        [DataRow(true, false, null, null, true, null, "Val2")]
        [DataRow(true, false, null, null, true, "Def2", "Val2")]
        [DataRow(false, true, null, "Val1", true, null, "Val2")]
        [DataRow(false, true, "Def1", "Val1", true, null, "Val2")]
        [DataRow(false, true, null, "Val1", true, "Def2", "Val2")]
        [DataRow(false, true, "Def1", "Val1", true, "Def2", "Val2")]
        [DataRow(false, true, null, "Val1", false, null, null)]
        [DataRow(false, true, "Def1", "Val1", false, null, null)]
        [DataRow(false, false, null, null, true, null, "Val2")]
        [DataRow(false, false, null, null, true, "Def2", "Val2")]
        public void Equals__Diff_default__False(bool test, bool hasDef1, string? def1Name, string? def1Val, bool hasDef2, string? def2Name, string? def2Val)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Default = hasDef1 ? new FieldDefaultModel(p1, def1Name, def1Val ?? string.Empty) : null;
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field2");
            p1.Default = hasDef2 ? new FieldDefaultModel(p2, def2Name, def2Val ?? string.Empty) : null;
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Equals__Diff_nullable__False(bool test)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Nullable = !test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field1");
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Equals__Diff_isunique__False(bool test)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Nullable = test;
            p1.IsUnique = !test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field1");
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Equals__Diff_pkey__True(bool test) // Primary Key is a table change
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = !test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field1");
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsTrue(res);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Equals__Diff_order__True(bool test)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Order = 1;
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = !test;
            p1.Identity = test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field1");
            p1.Order = 2;
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsTrue(res);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Equals__Diff_identity__False(bool test)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = !test;
            p1.Ref = new FieldRefModel(p1, p0) { Name = "Ref", IsSystemNamed = test };

            var p2 = getFieldAll("Field1");
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = new FieldRefModel(p2, p0) { Name = "Ref", IsSystemNamed = test };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        [DataRow(true, true, null, false, null)]
        [DataRow(true, true, null, true, "Ref2")]
        [DataRow(true, true, "Ref1", true, null)]
        [DataRow(true, true, "Ref1", true, "Ref2")]
        [DataRow(true, false, null, true, "Ref2")]
        public void Equals__Diff_ref__False(bool test, bool hasRef1, string? ref1Name, bool hasRef2, string? ref2Name)
        {
            // Arrange
            var p0 = new FieldModel(TableModel.Empty, "FieldT");
            
            var p1 = getFieldAll("Field1");
            p1.Nullable = test;
            p1.IsUnique = test;
            p1.IsPrimaryKey = test;
            p1.Identity = test;
            p1.Ref = hasRef1 ? new FieldRefModel(p1, p0) { Name = ref1Name ?? string.Empty, IsSystemNamed = ref1Name == null } : null;

            var p2 = getFieldAll("Field1");
            p2.Nullable = test;
            p2.IsUnique = test;
            p2.IsPrimaryKey = test;
            p2.Identity = test;
            p2.Ref = hasRef2 ? new FieldRefModel(p2, p0) { Name = ref2Name ?? string.Empty, IsSystemNamed = ref2Name == null } : null;

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        public void GetHashCode__Changes_for_each_field_change()
        {
            // Arrange
            var hashCodes = new List<int>();
            
            var parTable1 = new TableModel(new SchemaModel(DatabaseModel.Empty, "Schema"), "Table1");
            var parTable2 = new TableModel(new SchemaModel(DatabaseModel.Empty, "Schema"), "Table2");

            // Act
            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());
            
            hashCodes.Add(new FieldModel(parTable1, "Param") // Duplicate
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable2, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param2")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType2",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = null,
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp2",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = null,
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def2", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 6,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = false,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = false, // Not a change
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = false,
                Ref = new FieldRefModel(FieldModel.Empty, FieldModel.Empty),
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = null,
            }.GetHashCode());

            hashCodes.Add(new FieldModel(parTable1, "Param")
            {
                Type = "PType",
                Computation = "Comp",
                Default = new FieldDefaultModel(FieldModel.Empty, "Def", "Value"),
                IsUnique = true,
                Order = 5,
                Nullable = true,
                IsPrimaryKey = true,
                Identity = true,
                Ref = new FieldRefModel(FieldModel.Empty, new FieldModel(TableModel.Empty, "F")),
            }.GetHashCode());

            // Assert
            Assert.AreEqual(hashCodes.Count - 2, hashCodes.Distinct().Count());
        }
    }
}