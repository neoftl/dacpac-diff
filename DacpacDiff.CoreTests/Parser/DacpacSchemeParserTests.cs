using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DacpacDiff.Core.Parser.Tests
{
    [TestClass]
    public partial class DacpacSchemeParserTests
    {
        // TODO: dependencies
        
        [TestMethod]
        public void ParseContent__Ignores_unknown_elements()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""Invalid"" />
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);

            // Assert
            Assert.AreEqual("dbo", res.Databases.Values.Single().Schemas.Keys.Single());
            Assert.AreEqual(0, res.Databases.Values.Single().Schemas["dbo"].Modules.Count);
            Assert.AreEqual(0, res.Databases.Values.Single().Schemas["dbo"].Tables.Count);
        }

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
            </Entry>
            <Entry>
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
            Assert.AreEqual(3, proc.Parameters.Length);
            Assert.AreEqual(new ParameterModel(proc, "@VarcharMax") { Type = "varchar(MAX)", DefaultValue = "'default'", Order = 1 }, proc.Parameters[0]);
            Assert.AreEqual(new ParameterModel(proc, "@Char10") { Type = "char(10)", IsReadOnly = true, Order = 2 }, proc.Parameters[1]);
            Assert.AreEqual(new ParameterModel(proc, "@Decimal") { Type = "decimal(19, 5)", IsOutput = true, Order = 3 }, proc.Parameters[2]);
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
        <Property Name=""FilterPredicate""><Value>(((CONDITION)))</Value></Property>
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

        #region Triggers

        [TestMethod]
        public void ParseContent__Parses_triggers()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlDmlTrigger"" Name=""[dbo].[tr_Test]"">
        <Relationship Name=""Parent"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var trig = (TriggerModuleModel)sch.Modules["tr_Test"];
            Assert.AreEqual("tr_Test", trig.Name);
            Assert.AreEqual(ModuleModel.ModuleType.TRIGGER, trig.Type);
            Assert.AreEqual("[dbo].[Test]", trig.Parent);
            Assert.IsNotNull(trig.Definition);
            //Assert.AreEqual("BODY", trig.Definition);
            Assert.IsTrue(trig.Before);
            Assert.IsFalse(trig.ForDelete);
            Assert.IsFalse(trig.ForInsert);
            Assert.IsFalse(trig.ForUpdate);
        }

        [TestMethod]
        [DataRow(1, false, false, false)]
        [DataRow(1, true, false, false)]
        [DataRow(1, false, true, false)]
        [DataRow(1, true, true, false)]
        [DataRow(1, false, false, true)]
        [DataRow(1, true, false, true)]
        [DataRow(1, false, true, true)]
        [DataRow(1, true, true, true)]
        [DataRow(2, false, false, false)]
        [DataRow(2, true, false, false)]
        [DataRow(2, false, true, false)]
        [DataRow(2, true, true, false)]
        [DataRow(2, false, false, true)]
        [DataRow(2, true, false, true)]
        [DataRow(2, false, true, true)]
        [DataRow(2, true, true, true)]
        public void ParseContent__Parses_triggers_of_different_types(int type, bool delete, bool insert, bool update)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""SqlDmlTrigger"" Name=""[dbo].[tr_Test]"">
        <Relationship Name=""Parent"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Property Name=""SqlTriggerType"" Value=""{type}"" />
        <Property Name=""IsDeleteTrigger"" Value=""{delete}"" />
        <Property Name=""IsInsertTrigger"" Value=""{insert}"" />
        <Property Name=""IsUpdateTrigger"" Value=""{update}"" />
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var trig = (TriggerModuleModel)sch.Modules["tr_Test"];
            Assert.AreEqual(type != 2, trig.Before);
            Assert.AreEqual(delete, trig.ForDelete);
            Assert.AreEqual(insert, trig.ForInsert);
            Assert.AreEqual(update, trig.ForUpdate);
        }

        #endregion Triggers

        [TestMethod]
        public void ParseContent__Parses_synonyms()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlSynonym"" Name=""[dbo].[syn_Test]"">
        <Property Name=""ForObjectScript""><Value>[dbo].[usp_Test]</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];

            // Assert
            var syn = sch.Synonyms["syn_Test"];
            Assert.AreEqual("syn_Test", syn.Name);
            Assert.AreEqual("[dbo].[usp_Test]", syn.BaseObject);
        }
    }
}