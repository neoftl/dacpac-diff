using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Mssql.Diff.Tests
{
    [TestClass()]
    public class MssqlDiffModuleCreateTests
    {
        [TestMethod]
        public void MssqlDiffModuleCreate__Function__Args()
        {
            // Arrange
            var lft = new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                ReturnType = "LType",
                Body = "ModuleDefinition"
            };
            lft.Parameters = new[]
            {
                new ParameterModel(lft, "@ArgA") { Type = "INT" },
                new ParameterModel(lft, "@ArgB") { Type = "BIT", DefaultValue = "NULL" },
                new ParameterModel(lft, "@ArgC") { Type = "VARCHAR(MAX)", IsOutput = true },
                new ParameterModel(lft, "@ArgD") { Type = "DECIMAL(19, 5)", IsReadOnly = true },
            };

            var diff = new DiffModuleCreate(lft);

            // Act
            var res = new MssqlDiffModuleCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE FUNCTION [LSchema].[LMod] (",
                "    @ArgA INT,",
                "    @ArgB BIT = NULL,",
                "    @ArgC VARCHAR(MAX) OUTPUT,",
                "    @ArgD DECIMAL(19, 5) READONLY",
                ") RETURNS LType",
                "AS BEGIN",
                "    RETURN NULL",
                "END"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffModuleCreate__Scalar_function__Creates_stub()
        {
            // Arrange
            var lft = new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                ReturnType = "LType",
                Body = "ModuleDefinition"
            };

            var diff = new DiffModuleCreate(lft);

            // Act
            var res = new MssqlDiffModuleCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE FUNCTION [LSchema].[LMod] (",
                ") RETURNS LType",
                "AS BEGIN",
                "    RETURN NULL",
                "END"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffModuleCreate__Unnamed_table_function__Creates_stub()
        {
            // Arrange
            var lft = new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                ReturnType = "TABLE",
                Body = "ModuleDefinition"
            };
            lft.Parameters = new[]
            {
                new ParameterModel(lft, "@Param1")
                {
                    Type = "PType"
                }
            };

            var diff = new DiffModuleCreate(lft);

            // Act
            var res = new MssqlDiffModuleCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE FUNCTION [LSchema].[LMod] (",
                "    @Param1 PType",
                ") RETURNS TABLE",
                "AS",
                "    RETURN SELECT 1 A"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffModuleCreate__Named_table_function__Creates_stub()
        {
            // Arrange
            var lft = new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                ReturnType = "@TableVar",
                ReturnTable = new TableModel(SchemaModel.Empty, "LMod"),
                Body = "ModuleDefinition"
            };
            lft.Parameters = new[]
            {
                new ParameterModel(lft, "@Param1")
                {
                    Type = "PType"
                }
            };

            lft.ReturnTable.Fields = new[]
            {
                new FieldModel(lft.ReturnTable, "FldA") { Type = "INT", IsPrimaryKey = true },
                new FieldModel(lft.ReturnTable, "FldB") { Type = "VARCHAR(MAX)", Nullable = true },
            };

            var diff = new DiffModuleCreate(lft);

            // Act
            var res = new MssqlDiffModuleCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE FUNCTION [LSchema].[LMod] (",
                "    @Param1 PType",
                ") RETURNS @TableVar TABLE (",
                "    [FldA] INT NOT NULL,",
                "    [FldB] VARCHAR(MAX)",
                ") AS BEGIN",
                "    RETURN",
                "END"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffModuleCreate__Scalar_function__ReturnNullForNullInput()
        {
            // Arrange
            var lft = new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                ReturnType = "LType",
                Body = "ModuleDefinition",
                ReturnNullForNullInput = true
            };

            var diff = new DiffModuleCreate(lft);

            // Act
            var res = new MssqlDiffModuleCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE FUNCTION [LSchema].[LMod] (",
                ") RETURNS LType",
                "WITH RETURNS NULL ON NULL INPUT",
                "AS BEGIN",
                "    RETURN NULL",
                "END"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        [DataRow("TABLE", false)]
        [DataRow("@Table", true)]
        public void MssqlDiffModuleCreate__Nonscalar_function__ReturnNullForNullInput_no_effect(string returnType, bool withTable)
        {
            // Arrange
            var lft = new FunctionModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                ReturnType = returnType,
                ReturnTable = withTable ? new TableModel(SchemaModel.Empty, "LMod") : null,
                Body = "ModuleDefinition",
                ReturnNullForNullInput = true
            };

            var diff = new DiffModuleCreate(lft);

            // Act
            var res = new MssqlDiffModuleCreate(diff).ToString();

            // Assert
            Assert.IsFalse(res.Contains("WITH RETURNS NULL ON NULL INPUT"));
        }

        [TestMethod]
        public void MssqlDiffModuleCreate__Procedure__Creates_stub()
        {
            // Arrange
            var lft = new ProcedureModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                Body = "ModuleDefinition"
            };
            lft.Parameters = new[]
            {
                new ParameterModel(lft, "@Param1")
                {
                    Type = "PType"
                }
            };

            var diff = new DiffModuleCreate(lft);

            // Act
            var res = new MssqlDiffModuleCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE PROCEDURE [LSchema].[LMod]",
                "    @Param1 PType",
                "AS RETURN 0"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffModuleCreate__View__Creates_stub()
        {
            // Arrange
            var lft = new ViewModuleModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LMod")
            {
                Body = "ModuleDefinition"
            };

            var diff = new DiffModuleCreate(lft);

            // Act
            var res = new MssqlDiffModuleCreate(diff).ToString().Trim();

            // Assert
            Assert.AreEqual("CREATE VIEW [LSchema].[LMod] AS SELECT 1 A", res);
        }
    }
}