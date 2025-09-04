﻿using DacpacDiff.Core.Model;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace DacpacDiff.Core.Parser.Tests;

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
        Assert.IsEmpty(res.Databases.Values.Single().Schemas["dbo"].Modules);
        Assert.IsEmpty(res.Databases.Values.Single().Schemas["dbo"].Tables);
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
        Assert.IsEmpty(res.Databases.Values.Single().Schemas["dbo"].Modules);
        Assert.IsEmpty(res.Databases.Values.Single().Schemas["dbo"].Tables);
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
        Assert.HasCount(3, db.Schemas);
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
        var vw = (ViewModuleModel)sch.Modules["vw_Test"];
        Assert.AreEqual("vw_Test", vw.Name);
        Assert.AreEqual(ModuleModel.ModuleType.VIEW, vw.Type);
        Assert.AreEqual("BODY", vw.Body);
    }

    [TestMethod]
    [DataRow("SqlView", "QueryScript", "S\u200BQL")]
    [DataRow("SqlProcedure", "BodyScript", "S\u200BQL")]
    public void ParseContent__Strips_invalid_characters_from_body(string elementType, string bodyType, string bodyValue)
    {
        // Arrange
        var xml = $@"<root><Model>
    <Element Type=""{elementType}"" Name=""[dbo].[Test]"">
        <Property Name=""{bodyType}""><Value>{bodyValue}</Value></Property>
    </Element>
</Model></root>";

        // Act
        var res = DacpacSchemeParser.ParseContent("test", xml);
        var sch = res.Databases["database"].Schemas["dbo"];

        // Assert
        var mod = (ModuleWithBody)sch.Modules["Test"];
        Assert.AreEqual(3, mod.Body.Length);
        Assert.AreEqual("SQL", mod.Body);
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
        Assert.AreEqual("BODY", proc.Body);
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
                <Element Type=""SqlSubroutineParameter"" Name=""[dbo].[usp_Test].[@ComplexObject]"">
                    <Element Type=""SqlTypeSpecifier"">
                        <Relationship Name=""Type""><Entry><References Name=""[dbo].[MyType]"" /></Entry></Relationship>
                    </Element>
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
        Assert.AreEqual(4, proc.Parameters.Length);
        Assert.AreEqual(new ParameterModel(proc, "@VarcharMax") { Type = "varchar(MAX)", DefaultValue = "'default'", Order = 1 }, proc.Parameters[0]);
        Assert.AreEqual(new ParameterModel(proc, "@Char10") { Type = "char(10)", IsReadOnly = true, Order = 2 }, proc.Parameters[1]);
        Assert.AreEqual(new ParameterModel(proc, "@Decimal") { Type = "decimal(19, 5)", IsOutput = true, Order = 3 }, proc.Parameters[2]);
        Assert.AreEqual(new ParameterModel(proc, "@ComplexObject") { Type = "[dbo].[MyType]", Order = 4 }, proc.Parameters[3]);
    }

    [TestMethod]
    [DataRow("IsOwner", "OWNER")]
    [DataRow("IsCaller", "CALLER")]
    public void ParseContent__Parses_procedures_with_execute(string element, string executeType)
    {
        // Arrange
        var xml = $@"<root><Model>
    <Element Type=""SqlProcedure"" Name=""[dbo].[usp_Test]"">
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
        <Property Name=""{element}"" Value=""True"" />
    </Element>
</Model></root>";

        // Act
        var res = DacpacSchemeParser.ParseContent("test", xml);
        var sch = res.Databases["database"].Schemas["dbo"];

        // Assert
        var proc = (ProcedureModuleModel)sch.Modules["usp_Test"];
        Assert.AreEqual(executeType, proc.ExecuteAs);
    }

    #endregion Procedures

    #region Indexes

    [TestMethod]
    public void ParseContent__Parses_indexes()
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
        Assert.AreEqual("[dbo].[Test]", idx.IndexedObjectFullName);
        Assert.IsNull(idx.Condition);
        Assert.AreEqual(0, idx.IncludedColumns.Length);
    }

    [TestMethod]
    public void ParseContent__Parses_indexes_No_matching_table__No_index()
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
    <Element Type=""SqlIndex"" Name=""[dbo].[Test].[ix_Test]"">
        <Relationship Name=""IndexedObject"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[TestX].[ColA]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

        // Act
        var res = DacpacSchemeParser.ParseContent("test", xml);
        var sch = res.Databases["database"].Schemas["dbo"];

        // Assert
        Assert.IsEmpty(sch.Modules);
    }

    [TestMethod]
    public void ParseContent__Parses_indexes_No_matching_table_column__No_index()
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
    <Element Type=""SqlIndex"" Name=""[dbo].[Test].[ix_Test]"">
        <Relationship Name=""IndexedObject"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Relationship Name=""ColumnSpecifications"">
            <Entry>
                <Element Type=""SqlIndexedColumnSpecification"">
                    <Relationship Name=""Column""><Entry><References Name=""[dbo].[Test].[ColX]"" /></Entry></Relationship>
                </Element>
            </Entry>
        </Relationship>
    </Element>
</Model></root>";

        // Act
        var res = DacpacSchemeParser.ParseContent("test", xml);
        var sch = res.Databases["database"].Schemas["dbo"];

        // Assert
        Assert.IsEmpty(sch.Modules);
    }

    [TestMethod]
    public void ParseContent__Parses_indexes_with_multiple_fields()
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
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColC]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
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
        CollectionAssert.AreEquivalent(new[] { "ColA", "ColB", "ColC" }, idx.IndexedColumns);
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
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
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
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColC]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
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
    <Element Type=""SqlTable"" Name=""[dbo].[Test]"">
        <Element Type=""SqlSimpleColumn"" Name=""[dbo].[Test].[ColA]"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Element>
    </Element>
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
        Assert.AreEqual("BODY", trig.Body);
        Assert.IsNull(trig.ExecuteAs);
        Assert.IsTrue(trig.Before);
        Assert.IsFalse(trig.ForDelete);
        Assert.IsFalse(trig.ForInsert);
        Assert.IsFalse(trig.ForUpdate);
    }

    [TestMethod]
    [DataRow("IsOwner", "OWNER")]
    [DataRow("IsCaller", "CALLER")]
    public void ParseContent__Parses_triggers_with_execute(string element, string executeType)
    {
        // Arrange
        var xml = $@"<root><Model>
    <Element Type=""SqlDmlTrigger"" Name=""[dbo].[tr_Test]"">
        <Relationship Name=""Parent"">
            <Entry><References Name=""[dbo].[Test]"" /></Entry>
        </Relationship>
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
        <Property Name=""{element}"" Value=""True"" />
    </Element>
</Model></root>";

        // Act
        var res = DacpacSchemeParser.ParseContent("test", xml);
        var sch = res.Databases["database"].Schemas["dbo"];

        // Assert
        var trig = (TriggerModuleModel)sch.Modules["tr_Test"];
        Assert.AreEqual(executeType, trig.ExecuteAs);
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