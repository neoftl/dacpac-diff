using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace DacpacDiff.Core.Parser.Tests
{
    partial class DacpacSchemeParserTests
    {
        [TestMethod]
        public void ParseContent__Parses_tables_and_fields()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];
            var col = tbl.Fields.Single();

            // Assert
            Assert.AreEqual(1, sch.Tables.Count);
            Assert.AreEqual("ColA", col.Name);
            Assert.IsNull(col.Computation);
            Assert.IsFalse(col.HasDefault);
            Assert.IsFalse(col.HasReference);
            Assert.IsFalse(col.Identity);
            Assert.IsFalse(col.IsPrimaryKey);
            Assert.IsFalse(col.IsUnique);
            Assert.IsTrue(col.Nullable);
        }

        [TestMethod]
        public void ParseContent__Parses_tables_with_different_field_types()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[VarcharMax]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
                <Property Name=""IsMax"" Value=""True"" />
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[Char10]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""char"" /></Entry></Relationship>
                <Property Name=""Length"" Value=""10"" />
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[Decimal]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""decimal"" /></Entry></Relationship>
                <Property Name=""Precision"" Value=""19"" />
                <Property Name=""Scale"" Value=""5"" />
            </Element>
        </Element>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual("varchar(MAX)", tbl.Fields.Single(f => f.Name == "VarcharMax").Type);
            Assert.AreEqual("char(10)", tbl.Fields.Single(f => f.Name == "Char10").Type);
            Assert.AreEqual("decimal(19, 5)", tbl.Fields.Single(f => f.Name == "Decimal").Type);
        }

        [TestMethod]
        public void ParseContent__Parses_tables_with_identity_field()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[Identity]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
            <Property Name=""IsIdentity"" Value=""True"" />
        </Element>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.IsTrue(tbl.Fields.Single(f => f.Name == "Identity").Identity);
        }

        [TestMethod]
        public void ParseContent__Parses_tables_with_nonnullable_field()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[NonNullable]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
            <Property Name=""IsNullable"" Value=""False"" />
        </Element>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.IsFalse(tbl.Fields.Single(f => f.Name == "NonNullable").Nullable);
        }

        [TestMethod]
        public void ParseContent__Parses_tables_with_XML_field()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[Xml]"">
            <Element Type=""SqlXmlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""xml"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual("xml", tbl.Fields.Single(f => f.Name == "Xml").Type);
        }

        [TestMethod]
        public void ParseContent__Parses_tables_with_computed_column()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlComputedColumn"" Name=""[dbo].[Test].[Computed]"">
            <Property Name=""ExpressionScript""><Value>COMPUTED</Value></Property>
        </Element>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual("COMPUTED", tbl.Fields.Single(f => f.Name == "Computed").Computation);
            Assert.IsNull(tbl.Fields.Single(f => f.Name == "Computed").Type);
        }

        [TestMethod]
        public void ParseContent__Parses_temporal_tables()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
            <Property Name=""GeneratedAlwaysType"" Value=""1"" />
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
            <Property Name=""GeneratedAlwaysType"" Value=""2"" />
        </Element>
        <Relationship Name=""TemporalSystemVersioningHistoryTable"">
            <Entry><References Name=""[audit].[Test]"" /></Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual("[audit].[Test]", tbl.Temporality.HistoryTable);
            Assert.AreEqual("ColA", tbl.Temporality.PeriodFieldFrom);
            Assert.AreEqual("ColB", tbl.Temporality.PeriodFieldTo);
        }

        [TestMethod]
        public void ParseContent__Ignores_history_tables()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Property Name=""IsAutoGeneratedHistoryTable"" Value=""True"" />
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            Assert.AreEqual(0, sch.Tables.Count);
        }

        #region Primary keys

        [TestMethod]
        public void ParseContent__Parses_tables_with_single_primary_key()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlPrimaryKeyConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column"">
                        <Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry>
                    </Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            var fldA = tbl.Fields.Single(f => f.Name == "ColA");
            var fldB = tbl.Fields.Single(f => f.Name == "ColB");
            Assert.IsTrue(fldA.IsPrimaryKey);
            Assert.IsFalse(fldB.IsPrimaryKey);
            Assert.AreEqual(1, tbl.PrimaryKeys.Length);
            Assert.AreSame(fldA, tbl.PrimaryKeys.Single());
            Assert.IsFalse(tbl.IsPrimaryKeyUnclustered);
        }

        [TestMethod]
        public void ParseContent__Parses_tables_with_composite_primary_key()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlPrimaryKeyConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column"">
                        <Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry>
                    </Relationship>
                </Element>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column"">
                        <Entry><References Name=""[dbo].[Test].[ColB]"" /></Entry>
                    </Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            var fldA = tbl.Fields.Single(f => f.Name == "ColA");
            var fldB = tbl.Fields.Single(f => f.Name == "ColB");
            Assert.IsTrue(fldA.IsPrimaryKey);
            Assert.IsTrue(fldB.IsPrimaryKey);
            Assert.AreEqual(2, tbl.PrimaryKeys.Length);
            Assert.IsTrue(tbl.PrimaryKeys.All(k => k == fldA || k == fldB));
            Assert.IsFalse(tbl.IsPrimaryKeyUnclustered);
        }

        [TestMethod]
        [DataRow("[dbx].[Test].[ColA]")]
        [DataRow("[dbo].[TestX].[ColA]")]
        [DataRow("[dbo].[Test].[ColX]")]
        public void ParseContent__Ignores_primary_key_for_unknown_table_or_field(string pkey)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlPrimaryKeyConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column"">
                        <Entry><References Name=""{pkey}"" /></Entry>
                    </Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            var fldA = tbl.Fields.Single(f => f.Name == "ColA");
            Assert.IsFalse(fldA.IsPrimaryKey);
            Assert.AreEqual(0, tbl.PrimaryKeys.Length);
        }

        #endregion Primary keys

        #region Foreign keys

        [TestMethod]
        public void ParseContent__Parses_tables_with_unnamed_foreign_key()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[TestA]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestA].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlTable"" Name=""[dbo].[TestB]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestB].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlForeignKeyConstraint"">
        <Relationship Name=""Columns"">
            <Entry><References Name=""[dbo].[TestA].[ColA]"" /></Entry>
        </Relationship>
        <Relationship Name=""ForeignColumns"">
            <Entry><References Name=""[dbo].[TestB].[ColB]"" /></Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tblA = sch.Tables["TestA"];
            var tblB = sch.Tables["TestB"];

            // Assert
            var fldA = tblA.Fields.Single(f => f.Name == "ColA");
            var fldB = tblB.Fields.Single(f => f.Name == "ColB");
            Assert.AreSame(fldA, fldA.Ref.Field);
            Assert.IsTrue(fldA.Ref.IsSystemNamed);
            Assert.AreEqual(0, fldA.Ref.Name.Length);
            Assert.AreSame(tblA, fldA.Ref.Table);
            Assert.AreSame(fldB, fldA.Ref.TargetField);
            Assert.IsNull(fldB.Ref);
        }

        [TestMethod]
        [DataRow("[dbo].[TestA].[ColA]", "[dbo].[TestX].[ColB]")]
        [DataRow("[dbo].[TestA].[ColA]", "[dbo].[TestB].[ColX]")]
        [DataRow("[dbo].[TestX].[ColA]", "[dbo].[TestB].[ColB]")]
        [DataRow("[dbo].[TestA].[ColX]", "[dbo].[TestB].[ColB]")]
        [DataRow("[dbx].[TestA].[ColA]", "[dbo].[TestB].[ColB]")]
        [DataRow("[dbo].[TestA].[ColA]", "[dbx].[TestB].[ColB]")]
        public void ParseContent__Ignores_foreign_key_for_unknown_fields(string src, string dest)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[TestA]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestA].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlTable"" Name=""[dbo].[TestB]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestB].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlForeignKeyConstraint"">
        <Relationship Name=""Columns"">
            <Entry><References Name=""{src}"" /></Entry>
        </Relationship>
        <Relationship Name=""ForeignColumns"">
            <Entry><References Name=""{dest}"" /></Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tblA = sch.Tables["TestA"];
            var tblB = sch.Tables["TestB"];

            // Assert
            var fldA = tblA.Fields.Single(f => f.Name == "ColA");
            var fldB = tblB.Fields.Single(f => f.Name == "ColB");
            Assert.IsNull(fldA.Ref);
            Assert.IsNull(fldB.Ref);
        }

        [TestMethod]
        public void ParseContent__Ignores_foreign_key_for_field_with_one()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[TestA]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestA].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlTable"" Name=""[dbo].[TestB]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestB].[ColB1]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestB].[ColB2]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlForeignKeyConstraint"">
        <Relationship Name=""Columns"">
            <Entry><References Name=""[dbo].[TestA].[ColA]"" /></Entry>
        </Relationship>
        <Relationship Name=""ForeignColumns"">
            <Entry><References Name=""[dbo].[TestB].[ColB1]"" /></Entry>
        </Relationship>
    </Element>
    <Element Type=""SqlForeignKeyConstraint"">
        <Relationship Name=""Columns"">
            <Entry><References Name=""[dbo].[TestA].[ColA]"" /></Entry>
        </Relationship>
        <Relationship Name=""ForeignColumns"">
            <Entry><References Name=""[dbo].[TestB].[ColB2]"" /></Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tblA = sch.Tables["TestA"];
            var tblB = sch.Tables["TestB"];

            // Assert
            var fldA = tblA.Fields.Single(f => f.Name == "ColA");
            var fldB1 = tblB.Fields.Single(f => f.Name == "ColB1");
            var fldB2 = tblB.Fields.Single(f => f.Name == "ColB2");
            Assert.AreSame(fldB1, fldA.Ref.TargetField);
        }

        [TestMethod]
        public void ParseContent__Fails_for_multiple_columns()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[TestA]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestA].[ColA1]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestA].[ColA2]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlTable"" Name=""[dbo].[TestB]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestB].[ColB1]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[TestB].[ColB2]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlForeignKeyConstraint"">
        <Relationship Name=""Columns"">
            <Entry><References Name=""[dbo].[TestA].[ColA1]"" /></Entry>
            <Entry><References Name=""[dbo].[TestA].[ColA2]"" /></Entry>
        </Relationship>
        <Relationship Name=""ForeignColumns"">
            <Entry><References Name=""[dbo].[TestB].[ColB1]"" /></Entry>
            <Entry><References Name=""[dbo].[TestB].[ColB2]"" /></Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                DacpacSchemeParser.ParseContent("test", xml);
            }, "Sequence contains more than one element");
        }

        #endregion Foreign keys

        #region Unique constraints

        [TestMethod]
        public void ParseContent__Parses_tables_with_unique_constraint()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlUniqueConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            var fldA = tbl.Fields.Single(f => f.Name == "ColA");
            var fldB = tbl.Fields.Single(f => f.Name == "ColB");
            Assert.IsTrue(fldA.IsUnique);
            Assert.IsFalse(fldB.IsUnique);
        }

        [TestMethod]
        [DataRow("[dbx].[Test]", "ColA")]
        [DataRow("[dbo].[TestX]", "ColA")]
        [DataRow("[dbo].[Test]", "ColX")]
        public void ParseContent__Ignores_unique_constraint_for_unknown_field(string src, string col)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlUniqueConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""{src}"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""{src}.[{col}]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            var fldA = tbl.Fields.Single(f => f.Name == "ColA");
            var fldB = tbl.Fields.Single(f => f.Name == "ColB");
            Assert.IsFalse(fldA.IsUnique);
            Assert.IsFalse(fldB.IsUnique);
        }

        [TestMethod]
        public void ParseContent__Unique_constraint_fails_for_multiple_fields()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlUniqueConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry></Relationship>
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColB]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                DacpacSchemeParser.ParseContent("test", xml);
            }, "Sequence contains more than one element");
        }

        #endregion Unique constraints

        #region Check constraints

        [TestMethod]
        public void ParseContent__Parses_tables_with_unnamed_check_constraint()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlCheckConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Property Name=""CheckExpressionScript""><Value>(((EXPRESSION)))</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual(1, tbl.Checks.Count);
            Assert.AreSame(tbl, tbl.Checks[0].Table);
            Assert.AreEqual("EXPRESSION", tbl.Checks[0].Definition);
            Assert.AreEqual(0, tbl.Checks[0].Dependencies.Length);
            Assert.IsTrue(tbl.Checks[0].IsSystemNamed);
            Assert.AreEqual(0, tbl.Checks[0].Name.Length);
        }

        [TestMethod]
        public void ParseContent__Parses_tables_with_named_check_constraint()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlCheckConstraint"" Name=""[dbo].[chk_Test]"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Property Name=""CheckExpressionScript""><Value>EXPRESSION</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual(1, tbl.Checks.Count);
            Assert.AreSame(tbl, tbl.Checks[0].Table);
            Assert.AreEqual("EXPRESSION", tbl.Checks[0].Definition);
            Assert.AreEqual(0, tbl.Checks[0].Dependencies.Length);
            Assert.IsFalse(tbl.Checks[0].IsSystemNamed);
            Assert.AreEqual("chk_Test", tbl.Checks[0].Name);
        }

        [TestMethod]
        public void ParseContent__Check_constraint_maps_dependencies()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlCheckConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Property Name=""CheckExpressionScript""><Value>EXPRESSION</Value></Property>
        <Relationship Name=""CheckExpressionDependencies"">
            <Entry><References Name=""[dbo].[TestA]"" /></Entry>
            <Entry><References Name=""[dbo].[TestB]"" /></Entry>
            <Entry><References Name=""[dbo].[TestC]"" /></Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual(3, tbl.Checks[0].Dependencies.Length);
            Assert.IsTrue(tbl.Checks[0].Dependencies.Contains("[dbo].[TestA]"));
            Assert.IsTrue(tbl.Checks[0].Dependencies.Contains("[dbo].[TestB]"));
            Assert.IsTrue(tbl.Checks[0].Dependencies.Contains("[dbo].[TestC]"));
        }

        [TestMethod]
        [DataRow("[dbx].[Test]")]
        [DataRow("[dbo].[TestX]")]
        public void ParseContent__Ignores_check_constraint_for_unknown_table(string src)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlCheckConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""{src}"" /></Entry>
        </Relationship>
        <Property Name=""CheckExpressionScript""><Value>EXPRESSION</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual(0, tbl.Checks.Count);
        }

        #endregion Check constraints

        #region Default constraints

        [TestMethod]
        public void ParseContent__Parses_tables_with_unnamed_default_constraint()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlDefaultConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ForColumn"">
            <Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry>
        </Relationship>
        <Property Name=""DefaultExpressionScript""><Value>(((EXPRESSION)))</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            var fldA = tbl.Fields.Single(f => f.Name == "ColA");
            var fldB = tbl.Fields.Single(f => f.Name == "ColB");
            Assert.IsTrue(fldA.HasDefault);
            Assert.AreEqual("", fldA.DefaultName);
            Assert.AreEqual("", fldA.Default.Name);
            Assert.IsTrue(fldA.IsDefaultSystemNamed);
            Assert.IsTrue(fldA.Default.IsSystemNamed);
            Assert.AreSame(fldA, fldA.Default.Field);
            Assert.AreEqual("EXPRESSION", fldA.DefaultValue);
            Assert.AreEqual("EXPRESSION", fldA.Default.Value);
            Assert.IsFalse(fldB.HasDefault);
            Assert.IsFalse(fldB.IsDefaultSystemNamed);
            Assert.IsNull(fldB.Default);
            Assert.IsNull(fldB.DefaultName);
            Assert.IsNull(fldB.DefaultValue);
        }

        [TestMethod]
        public void ParseContent__Parses_tables_with_named_default_constraint()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlDefaultConstraint"" Name=""[dbo].[Test].[def_Test]"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ForColumn"">
            <Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry>
        </Relationship>
        <Property Name=""DefaultExpressionScript""><Value>EXPRESSION</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            var fldA = tbl.Fields.Single(f => f.Name == "ColA");
            var fldB = tbl.Fields.Single(f => f.Name == "ColB");
            Assert.IsTrue(fldA.HasDefault);
            Assert.AreEqual("def_Test", fldA.DefaultName);
            Assert.AreEqual("def_Test", fldA.Default.Name);
            Assert.IsFalse(fldA.IsDefaultSystemNamed);
            Assert.IsFalse(fldA.Default.IsSystemNamed);
            Assert.AreSame(fldA, fldA.Default.Field);
            Assert.AreEqual("EXPRESSION", fldA.DefaultValue);
            Assert.AreEqual("EXPRESSION", fldA.Default.Value);
            Assert.IsFalse(fldB.HasDefault);
            Assert.IsFalse(fldB.IsDefaultSystemNamed);
            Assert.IsNull(fldB.Default);
            Assert.IsNull(fldB.DefaultName);
            Assert.IsNull(fldB.DefaultValue);
        }

        [TestMethod]
        public void ParseContent__Default_constraint_maps_dependencies()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlDefaultConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ForColumn"">
            <Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry>
        </Relationship>
        <Property Name=""DefaultExpressionScript""><Value>EXPRESSION</Value></Property>
        <Relationship Name=""ExpressionDependencies"">
            <Entry><References Name=""[dbo].[TestA]"" /></Entry>
            <Entry><References Name=""[dbo].[TestB]"" /></Entry>
            <Entry><References Name=""[dbo].[TestC]"" /></Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            var fldA = tbl.Fields.Single(f => f.Name == "ColA");
            Assert.AreEqual(3, fldA.Default.Dependencies.Length);
            Assert.IsTrue(fldA.Default.Dependencies.Contains("[dbo].[TestA]"));
            Assert.IsTrue(fldA.Default.Dependencies.Contains("[dbo].[TestB]"));
            Assert.IsTrue(fldA.Default.Dependencies.Contains("[dbo].[TestC]"));
        }

        [TestMethod]
        [DataRow("[dbx].[Test]", "ColA")]
        [DataRow("[dbo].[TestX]", "ColA")]
        [DataRow("[dbo].[Test]", "ColX")]
        public void ParseContent__Ignores_default_constraint_for_unknown_table(string src, string col)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColB]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
    <Element Type=""SqlDefaultConstraint"">
        <Relationship Name=""DefiningTable"">
            <Entry><References Name=""{src}"" /></Entry>
        </Relationship>
        <Relationship Name=""ForColumn"">
            <Entry><References Name=""{src}.[{col}]"" /></Entry>
        </Relationship>
        <Property Name=""DefaultExpressionScript""><Value>EXPRESSION</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var tbl = sch.Tables["Test"];

            // Assert
            Assert.AreEqual(0, tbl.Checks.Count);
        }

        #endregion Check constraints
    }
}