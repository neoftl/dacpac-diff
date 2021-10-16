using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Parser.Tests
{
    partial class DacpacSchemeParserTests
    {
        #region Scalar

        [TestMethod]
        public void ParseContent__Parses_scalar_functions()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlScalarFunction"" Name=""[dbo].[fn_Test]"">
        <Relationship Name=""Type"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Relationship>
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var fn = (FunctionModuleModel)sch.Modules["fn_Test"];

            // Assert
            Assert.AreEqual(1, sch.Modules.Count);
            Assert.AreEqual("fn_Test", fn.Name);
            Assert.AreEqual(ModuleModel.ModuleType.FUNCTION, fn.Type);
            Assert.AreEqual("varchar", fn.ReturnType);
            Assert.AreEqual(0, fn.Parameters.Length);
            Assert.IsFalse(fn.ReturnNullForNullInput);
            Assert.IsNull(fn.ReturnTable);
            Assert.IsNull(fn.ExecuteAs);
            Assert.AreEqual("BODY", fn.Body);
        }

        #endregion Scalar

        #region Inline table

        [TestMethod]
        public void ParseContent__Parses_itable_functions()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlInlineTableValuedFunction"" Name=""[dbo].[fn_Test]"">
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var fn = (FunctionModuleModel)sch.Modules["fn_Test"];

            // Assert
            Assert.AreEqual(1, sch.Modules.Count);
            Assert.AreEqual("fn_Test", fn.Name);
            Assert.AreEqual(ModuleModel.ModuleType.FUNCTION, fn.Type);
            Assert.AreEqual("TABLE", fn.ReturnType);
            Assert.AreEqual(0, fn.Parameters.Length);
            Assert.IsFalse(fn.ReturnNullForNullInput);
            Assert.IsNull(fn.ReturnTable);
            Assert.IsNull(fn.ExecuteAs);
            Assert.AreEqual("BODY", fn.Body);
        }

        #endregion Inline table

        #region Full table

        // TODO: primary key?
        // TODO: reference?
        // TODO: unique?
        // TODO: default?

        [TestMethod]
        public void ParseContent__Parses_table_functions()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlMultiStatementTableValuedFunction"" Name=""[dbo].[fn_Test]"">
        <Property Name=""ReturnTableVariable"" Value=""@retvar"" />
        <Relationship Name=""Columns"" />
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var fn = (FunctionModuleModel)sch.Modules["fn_Test"];

            // Assert
            Assert.AreEqual(1, sch.Modules.Count);
            Assert.AreEqual("fn_Test", fn.Name);
            Assert.AreEqual(ModuleModel.ModuleType.FUNCTION, fn.Type);
            Assert.AreEqual("@retvar", fn.ReturnType);
            Assert.AreEqual(0, fn.Parameters.Length);
            Assert.IsFalse(fn.ReturnNullForNullInput);
            Assert.IsNotNull(fn.ReturnTable);
            Assert.IsNull(fn.ExecuteAs);
            Assert.AreEqual("BODY", fn.Body);
        }

        [TestMethod]
        public void ParseContent__Parses_table_functions_with_column_definitions()
        {
            // Arrange
            var xml = @"<root><Model>
    <Element Type=""SqlMultiStatementTableValuedFunction"" Name=""[dbo].[fn_Test]"">
        <Property Name=""ReturnTableVariable"" Value=""@retvar"" />
        <Relationship Name=""Columns"">
            <Element Type=""SqlSimpleColumn"" Name=""[dbo].[fn_Test].[VarcharMax]"">
                <Element Type=""SqlTypeSpecifier"">
                    <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
                    <Property Name=""IsMax"" Value=""True"" />
                </Element>
                <Property Name=""IsIdentity"" Value=""True"" />
            </Element>
            <Element Type=""SqlSimpleColumn"" Name=""[dbo].[fn_Test].[Char10]"">
                <Element Type=""SqlTypeSpecifier"">
                    <Relationship Name=""Type""><Entry><References Name=""char"" /></Entry></Relationship>
                    <Property Name=""Length"" Value=""10"" />
                </Element>
            </Element>
            <Element Type=""SqlSimpleColumn"" Name=""[dbo].[fn_Test].[Decimal]"">
                <Element Type=""SqlTypeSpecifier"">
                    <Relationship Name=""Type""><Entry><References Name=""decimal"" /></Entry></Relationship>
                    <Property Name=""Precision"" Value=""19"" />
                    <Property Name=""Scale"" Value=""5"" />
                </Element>
                <Property Name=""IsNullable"" Value=""False"" />
            </Element>
            <Element Type=""SqlComputedColumn"" Name=""[dbo].[fn_Test].[Computed]"">
                <Property Name=""ExpressionScript""><Value>COMPUTED</Value></Property>
            </Element>
        </Relationship>
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var fn = (FunctionModuleModel)sch.Modules["fn_Test"];

            // Assert
            var rtbl = fn.ReturnTable ?? throw new NullReferenceException();
            Assert.AreEqual(4, rtbl.Fields.Length);
            Assert.AreEqual(new FieldModel(rtbl, "VarcharMax") { Type = "varchar(MAX)", Identity = true, Nullable = true, Order = 1 }, rtbl.Fields[0]);
            Assert.AreEqual(new FieldModel(rtbl, "Char10") { Type = "char(10)", Nullable = true, Order = 2 }, rtbl.Fields[1]);
            Assert.AreEqual(new FieldModel(rtbl, "Decimal") { Type = "decimal(19, 5)", Order = 3 }, rtbl.Fields[2]);
            Assert.AreEqual(new FieldModel(rtbl, "Computed") { Computation = "COMPUTED", Nullable = true, Order = 4 }, rtbl.Fields[3]);
        }

        #endregion Full table

        #region Shared

        [TestMethod]
        [DataRow("SqlScalarFunction")]
        [DataRow("SqlInlineTableValuedFunction")]
        [DataRow("SqlMultiStatementTableValuedFunction")]
        public void ParseContent__Parses_functions_with_parameters(string type)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""{type}"" Name=""[dbo].[fn_Test]"">
        <Relationship Name=""Parameters"">
            <Entry>
                <Element Type=""SqlSubroutineParameter"" Name=""[dbo].[fn_Test].[@VarcharMax]"">
                    <Element Type=""SqlTypeSpecifier"">
                        <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
                        <Property Name=""IsMax"" Value=""True"" />
                    </Element>
                    <Property Name=""DefaultExpressionScript""><Value>'default'</Value></Property>
                </Element>
            </Entry>
            <Entry>
                <Element Type=""SqlSubroutineParameter"" Name=""[dbo].[fn_Test].[@Char10]"">
                    <Element Type=""SqlTypeSpecifier"">
                        <Relationship Name=""Type""><Entry><References Name=""char"" /></Entry></Relationship>
                        <Property Name=""Length"" Value=""10"" />
                    </Element>
                    <Property Name=""IsReadOnly"" Value=""True"" />
                </Element>
                <Element Type=""SqlSubroutineParameter"" Name=""[dbo].[fn_Test].[@Decimal]"">
                    <Element Type=""SqlTypeSpecifier"">
                        <Relationship Name=""Type""><Entry><References Name=""decimal"" /></Entry></Relationship>
                        <Property Name=""Precision"" Value=""19"" />
                        <Property Name=""Scale"" Value=""5"" />
                    </Element>
                    <Property Name=""IsOutput"" Value=""True"" />
                </Element>
            </Entry>
        </Relationship>
        <Relationship Name=""Type"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Relationship>
        <Property Name=""ReturnTableVariable"" Value=""@retvar"" />
        <Relationship Name=""Columns"" />
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var fn = (FunctionModuleModel)sch.Modules["fn_Test"];

            // Assert
            Assert.AreEqual(3, fn.Parameters.Length);
            Assert.AreEqual(new ParameterModel(fn, "@VarcharMax") { Type = "varchar(MAX)", DefaultValue = "'default'", Order = 1 }, fn.Parameters[0]);
            Assert.AreEqual(new ParameterModel(fn, "@Char10") { Type = "char(10)", IsReadOnly = true, Order = 2 }, fn.Parameters[1]);
            Assert.AreEqual(new ParameterModel(fn, "@Decimal") { Type = "decimal(19, 5)", IsOutput = true, Order = 3 }, fn.Parameters[2]);
        }

        [TestMethod]
        [DataRow("SqlScalarFunction")]
        [DataRow("SqlInlineTableValuedFunction")]
        [DataRow("SqlMultiStatementTableValuedFunction")]
        public void ParseContent__Parses_functions_with_executeas_caller(string type)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""{type}"" Name=""[dbo].[fn_Test]"">
        <Property Name=""IsCaller"" Value=""True"" />
        <Relationship Name=""Type"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Relationship>
        <Property Name=""ReturnTableVariable"" Value=""@retvar"" />
        <Relationship Name=""Columns"" />
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var fn = (FunctionModuleModel)sch.Modules["fn_Test"];

            // Assert
            Assert.AreEqual("CALLER", fn.ExecuteAs);
        }

        [TestMethod]
        [DataRow("SqlScalarFunction")]
        [DataRow("SqlInlineTableValuedFunction")]
        [DataRow("SqlMultiStatementTableValuedFunction")]
        public void ParseContent__Parses_functions_with_executeas_owner(string type)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""{type}"" Name=""[dbo].[fn_Test]"">
        <Property Name=""IsOwner"" Value=""True"" />
        <Relationship Name=""Type"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Relationship>
        <Property Name=""ReturnTableVariable"" Value=""@retvar"" />
        <Relationship Name=""Columns"" />
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var fn = (FunctionModuleModel)sch.Modules["fn_Test"];

            // Assert
            Assert.AreEqual("OWNER", fn.ExecuteAs);
        }

        [TestMethod]
        [DataRow("SqlScalarFunction")]
        [DataRow("SqlInlineTableValuedFunction")]
        [DataRow("SqlMultiStatementTableValuedFunction")]
        public void ParseContent__Parses_functions_with_NullAsNull(string type)
        {
            // Arrange
            var xml = $@"<root><Model>
    <Element Type=""{type}"" Name=""[dbo].[fn_Test]"">
        <Property Name=""DoReturnNullForNullInput"" Value=""True"" />
        <Relationship Name=""Type"">
            <Element Type=""SqlTypeSpecifier"">
                <Relationship Name=""Type""><Entry><References Name=""varchar"" /></Entry></Relationship>
            </Element>
        </Relationship>
        <Property Name=""ReturnTableVariable"" Value=""@retvar"" />
        <Relationship Name=""Columns"" />
        <Property Name=""BodyScript""><Value>BODY</Value></Property>
    </Element>
</Model></root>";

            // Act
            var res = DacpacSchemeParser.ParseContent("test", xml);
            var sch = res.Databases["database"].Schemas["dbo"];
            var fn = (FunctionModuleModel)sch.Modules["fn_Test"];

            // Assert
            Assert.IsTrue(fn.ReturnNullForNullInput);
        }

        #endregion Shared
    }
}
