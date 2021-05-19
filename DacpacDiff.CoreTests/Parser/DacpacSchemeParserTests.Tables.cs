using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DacpacDiff.Core.Parser.Tests
{
    partial class DacpacSchemeParserTests
    {
        // TODO: primary key
        // TODO: reference
        // TODO: unique
        // TODO: default

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
    }
}