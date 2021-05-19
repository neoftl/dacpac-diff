using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DacpacDiff.Core.Parser.Tests
{
    [TestClass]
    public partial class DacpacSchemeParserTests
    {
        [TestMethod]
        public void ParseFile__Blank_returns_blank_scheme()
        {
            // Arrange
            var parser = new DacpacSchemeParser();

            // Act
            var res = parser.ParseFile("blank.dacpac");

            // Assert
            Assert.AreEqual("blank", res.Name);
            Assert.AreEqual("dbo", res.Databases.Values.Single().Schemas.Keys.Single());
            Assert.AreEqual(0, res.Databases.Values.Single().Schemas["dbo"].Modules.Count);
            Assert.AreEqual(0, res.Databases.Values.Single().Schemas["dbo"].Tables.Count);
        }

        [TestMethod]
        public void ParseContent__Parses_schemas()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlSchema"" Name=""dbo"" />
    <Element Type=""SqlSchema"" Name=""schema2"" />
    <Element Type=""SqlSchema"" Name=""schema3"" />
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var db = res.Databases["database"];

            // Assert
            Assert.AreEqual(3, db.Schemas.Count);
            Assert.IsTrue(db.Schemas.ContainsKey("dbo"));
            Assert.IsTrue(db.Schemas.ContainsKey("schema2"));
            Assert.IsTrue(db.Schemas.ContainsKey("schema3"));
        }

        [TestMethod]
        public void ParseContent__Parses_views()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlView"" Name=""[dbo].[vw_Test]"">
        <Property Name=""QueryScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var mod = sch.Modules["vw_Test"];
            Assert.AreEqual("vw_Test", mod.Name);
            Assert.AreEqual(ModuleModel.ModuleType.VIEW, mod.Type);
            Assert.IsTrue(mod.Definition.Length > 0);
        }

        #region Procedures

        [TestMethod]
        public void ParseContent__Parses_procedures()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlProcedure"" Name=""[dbo].[usp_Test]"">
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var proc = (ProcedureModuleModel)sch.Modules["usp_Test"];
            Assert.AreEqual("usp_Test", proc.Name);
            Assert.AreEqual(ModuleModel.ModuleType.PROCEDURE, proc.Type);
            Assert.IsTrue(proc.Definition.Length > 0);
            Assert.IsNull(proc.ExecuteAs);
        }

        [TestMethod]
        public void ParseContent__Parses_procedures_with_parameters()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlProcedure"" Name=""[dbo].[usp_Test]"">
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
        <Relationship Name=""Parameters"">
            <Entry>
                <Element Type=""SqlSubroutineParameter"" Name=""[dbo].[usp_Test].[@VarcharMax]"">
                    <Element Type=""SqlTypeSpecifier"">
                        <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
                        <Property Name=""IsMax"" Value=""True"" />
                    </Element>
                    <Property Name=""DefaultExpressionScript""><Value>'default'</Value></Property>
                </Element>
                <Element Type=""SqlSubroutineParameter"" Name=""[dbo].[usp_Test].[@Char10]"">
                    <Element Type=""SqlTypeSpecifier"">
                        <Relationship Name=""Type""><Entry><References Name=""char"" /></Entry></Relationship>
                        <Property Name=""Length"" Value=""10"" />
                    </Element>
                    <Property Name=""IsReadOnly"" Value=""True"" />
                </Element>
                <Element Type=""SqlSubroutineParameter"" Name=""[dbo].[usp_Test].[@Decimal]"">
                    <Element Type=""SqlTypeSpecifier"">
                        <Relationship Name=""Type""><Entry><References Name=""decimal"" /></Entry></Relationship>
                        <Property Name=""Precision"" Value=""19"" />
                        <Property Name=""Scale"" Value=""5"" />
                    </Element>
                    <Property Name=""IsOutput"" Value=""True"" />
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var proc = (ProcedureModuleModel)sch.Modules["usp_Test"];
            Assert.IsTrue(proc.Definition.Contains("@VarcharMax varchar(MAX) = 'default'"), proc.Definition);
            Assert.IsTrue(proc.Definition.Contains("@Char10 char(10) READONLY"), proc.Definition);
            Assert.IsTrue(proc.Definition.Contains("@Decimal decimal(19, 5) OUTPUT"), proc.Definition);
        }

        [TestMethod]
        public void ParseContent__Parses_procedures_with_caller_execute()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlProcedure"" Name=""[dbo].[usp_Test]"">
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
        <Property Name=""IsCaller"" Value=""True"" />
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var proc = (ProcedureModuleModel)sch.Modules["usp_Test"];
            Assert.AreEqual("CALLER", proc.ExecuteAs);
            Assert.IsTrue(proc.Definition.Contains("WITH EXECUTE AS CALLER"));
        }

        [TestMethod]
        public void ParseContent__Parses_procedures_with_owner_execute()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlProcedure"" Name=""[dbo].[usp_Test]"">
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
        <Property Name=""IsOwner"" Value=""True"" />
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var proc = (ProcedureModuleModel)sch.Modules["usp_Test"];
            Assert.AreEqual("OWNER", proc.ExecuteAs);
            Assert.IsTrue(proc.Definition.Contains("WITH EXECUTE AS OWNER"));
        }
 
        #endregion Procedures

        #region Indexes
        
        [TestMethod]
        public void ParseContent__Parses_indexes()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlIndex"" Name=""[dbo].[Test].[ix_Test]"">
        <Relationship Name=""IndexedObject"">
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

            // Assert
            var idx = (IndexModuleModel)sch.Modules["ix_Test"];
            Assert.AreEqual("ix_Test", idx.Name);
            Assert.AreEqual(ModuleModel.ModuleType.INDEX, idx.Type);
            Assert.AreEqual("[dbo].[Test]", idx.IndexedObject);
            Assert.IsNull(idx.Condition);
            Assert.AreEqual(0, idx.IncludedColumns.Length);
        }
        
        [TestMethod]
        public void ParseContent__Parses_indexes_with_multiple_fields()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlIndex"" Name=""[dbo].[Test].[ix_Test]"">
        <Relationship Name=""IndexedObject"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry></Relationship>
                </Element>
            </Entry>
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColB]"" /></Entry></Relationship>
                </Element>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColC]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var idx = (IndexModuleModel)sch.Modules["ix_Test"];
            Assert.AreEqual(3, idx.IndexedColumns.Length);
            Assert.IsTrue(idx.Definition.Contains("ON [dbo].[Test]([ColA], [ColB], [ColC])"));
        }
        
        [TestMethod]
        [DataRow(false, false)]
        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataRow(true, true)]
        public void ParseContent__Parses_indexes_of_different_types(bool isUnique, bool isClustered)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""SqlIndex"" Name=""[dbo].[Test].[ix_Test]"">
        <Relationship Name=""IndexedObject"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
        <Property Name=""IsClustered"" Value=""{isClustered}"" />
        <Property Name=""IsUnique"" Value=""{isUnique}"" />
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var idx = (IndexModuleModel)sch.Modules["ix_Test"];
            Assert.AreEqual(isClustered, idx.IsClustered);
            Assert.AreEqual(isUnique, idx.IsUnique);
        }
        
        [TestMethod]
        public void ParseContent__Parses_indexes_with_includes()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlIndex"" Name=""[dbo].[Test].[ix_Test]"">
        <Relationship Name=""IndexedObject"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
        <Relationship Name=""IncludedColumns"">
            <Entry><References Name=""[dbo].[Test].[ColB]"" /></Entry>
            <Entry><References Name=""[dbo].[Test].[ColC]"" /></Entry>
        </Relationship>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var idx = (IndexModuleModel)sch.Modules["ix_Test"];
            Assert.AreEqual(2, idx.IncludedColumns.Length);
            Assert.IsTrue(idx.IncludedColumns.Contains("ColB"));
            Assert.IsTrue(idx.IncludedColumns.Contains("ColC"));
        }
        
        [TestMethod]
        public void ParseContent__Parses_indexes_with_filter()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlIndex"" Name=""[dbo].[Test].[ix_Test]"">
        <Relationship Name=""IndexedObject"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColA]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
        <Property Name=""FilterPredicate"">
            <Value>(((CONDITION)))</Value>
        </Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var idx = (IndexModuleModel)sch.Modules["ix_Test"];
            Assert.AreEqual("(CONDITION)", idx.Condition);
        }

        #endregion Indexes
    }
}