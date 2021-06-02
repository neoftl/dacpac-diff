using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Mssql.Diff.Tests
{
    [TestClass]
    public class MssqlDiffTableCreateTests
    {
        [TestMethod]
        public void MssqlDiffTableCreate__Minimal()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                ")"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Fields_of_different_types()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType" },
                new FieldModel(tbl, "F2")  { Computation = "Computation" },
                new FieldModel(tbl, "F3")  { Type = "UType", IsUnique = true },
                new FieldModel(tbl, "F4")  { Type = "NType", Nullable = true },
                new FieldModel(tbl, "F5")  { Type = "DType" },
            };
            tbl.Fields[4].Default = new FieldDefaultModel(tbl.Fields[4], null, "DefValue");

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL,",
                "    [F2] AS Computation,",
                "    [F3] UType NOT NULL UNIQUE,",
                "    [F4] NType NULL,",
                "    [F5] DType NOT NULL DEFAULT (DefValue)",
                ")"
            }, res, string.Join("\n", res));
        }
        
        [TestMethod]
        public void MssqlDiffTableCreate__Simple_unnamed_primary_key()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F2")  { Type = "FType" },
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL PRIMARY KEY,",
                "    [F2] FType NOT NULL",
                ")"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Simple_named_primary_key()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable")
            {
                PrimaryKeyName = "PKey"
            };
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F2")  { Type = "FType" },
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL CONSTRAINT [PKey] PRIMARY KEY,",
                "    [F2] FType NOT NULL",
                ")"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Composite_unnamed_primary_key()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F2")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F3")  { Type = "FType" },
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL,",
                "    [F2] FType NOT NULL,",
                "    [F3] FType NOT NULL,",
                "    PRIMARY KEY ([F1], [F2])",
                ")"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Composite_named_primary_key()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable")
            {
                PrimaryKeyName = "PKey"
            };
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F2")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F3")  { Type = "FType" },
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL,",
                "    [F2] FType NOT NULL,",
                "    [F3] FType NOT NULL,",
                "    CONSTRAINT [PKey] PRIMARY KEY ([F1], [F2])",
                ")"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Simple_primary_key_nonclustereed()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable")
            {
                IsPrimaryKeyUnclustered = true
            };
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F2")  { Type = "FType" },
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL,",
                "    [F2] FType NOT NULL,",
                "    PRIMARY KEY NONCLUSTERED ([F1])",
                ")"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Composite_primary_key_nonclustereed()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable")
            {
                IsPrimaryKeyUnclustered = true
            };
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F2")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "F3")  { Type = "FType" },
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL,",
                "    [F2] FType NOT NULL,",
                "    [F3] FType NOT NULL,",
                "    PRIMARY KEY NONCLUSTERED ([F1], [F2])",
                ")"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Temporal_table_with_unnamed_history()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable")
            {
                Temporality = new()
                {
                    PeriodFieldFrom = "TemporalFrom",
                    PeriodFieldTo = "TemporalTo",
                }
            };
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "TemporalFrom")  { Type = "FType" },
                new FieldModel(tbl, "TemporalTo")  { Type = "FType" },
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL PRIMARY KEY,",
                "    [TemporalFrom] FType GENERATED ALWAYS AS ROW START,",
                "    [TemporalTo] FType GENERATED ALWAYS AS ROW END,",
                "    PERIOD FOR SYSTEM_TIME ([TemporalFrom], [TemporalTo])",
                ") WITH (SYSTEM_VERSIONING = ON)"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Temporal_table_with_named_history()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable")
            {
                Temporality = new()
                {
                    HistoryTable = "[HSchema].[HTable]",
                    PeriodFieldFrom = "TemporalFrom",
                    PeriodFieldTo = "TemporalTo",
                }
            };
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType", IsPrimaryKey = true },
                new FieldModel(tbl, "TemporalFrom")  { Type = "FType" },
                new FieldModel(tbl, "TemporalTo")  { Type = "FType" },
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL PRIMARY KEY,",
                "    [TemporalFrom] FType GENERATED ALWAYS AS ROW START,",
                "    [TemporalTo] FType GENERATED ALWAYS AS ROW END,",
                "    PERIOD FOR SYSTEM_TIME ([TemporalFrom], [TemporalTo])",
                ") WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [HSchema].[HTable]))"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Field_has_unnamed_reference()
        {
            // Arrange
            var tbl2 = new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable");
            tbl2.Fields = new[]
            {
                new FieldModel(tbl2, "C1")
            };

            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType" },
                new FieldModel(tbl, "F2")  { Type = "RType" },
            };
            tbl.Fields[1].Ref = new FieldRefModel(tbl.Fields[1], tbl2.Fields[0]);

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL,",
                "    [F2] RType NOT NULL REFERENCES [TSchema].[TTable] ([C1])",
                ")"
            }, res, string.Join("\n", res));
        }

        [TestMethod]
        public void MssqlDiffTableCreate__Field_has_named_reference()
        {
            // Arrange
            var tbl2 = new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable");
            tbl2.Fields = new[]
            {
                new FieldModel(tbl2, "C1")
            };

            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable");
            tbl.Fields = new[]
            {
                new FieldModel(tbl, "F1")  { Type = "FType" },
                new FieldModel(tbl, "F2")  { Type = "RType" },
            };
            tbl.Fields[1].Ref = new FieldRefModel(tbl.Fields[1], tbl2.Fields[0])
            {
                Name = "Ref",
                IsSystemNamed = false
            };

            var diff = new DiffTableCreate(tbl);

            // Act
            var res = new MssqlDiffTableCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TABLE [LSchema].[LTable]",
                "(",
                "    [F1] FType NOT NULL,",
                "    [F2] RType NOT NULL CONSTRAINT [Ref] REFERENCES [TSchema].[TTable] ([C1])",
                ")"
            }, res, string.Join("\n", res));
        }
    }
}