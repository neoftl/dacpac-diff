using DacpacDiff.Comparer.Tests.TestHelpers;
using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DacpacDiff.Mssql.Diff.Tests;

[TestClass]
public class MssqlDiffFieldAlterTests
{
    #region Computed

    [TestMethod]
    public void MssqlFieldAlter__Change_column_from_computed()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "LType"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Computation = "COMPUTATION"
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        CollectionAssert.AreEqual(new[]
        {
            "ALTER TABLE [RSchema].[RTable] DROP COLUMN [RField]",
            "ALTER TABLE [LSchema].[LTable] ADD [LField] LType NOT NULL",
        }, res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Change_column_to_computed()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Computation = "COMPUTATION"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "RType"
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        CollectionAssert.AreEqual(new[]
        {
            "ALTER TABLE [RSchema].[RTable] DROP COLUMN [RField]",
            "ALTER TABLE [LSchema].[LTable] ADD [LField] AS COMPUTATION",
        }, res);
    }

    #endregion Computed

    #region Default

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void MssqlFieldAlter__Add_unnamed_default(bool nullable)
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType",
            Nullable = nullable
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType",
            Nullable = nullable
        };
        tgt.Default = new FieldDefaultModel(tgt, null, "LDefValue");

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD DEFAULT (LDefValue) FOR [LField]", res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Add_unnamed_default_and_change_signature()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "LType",
            Nullable = true
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "RType",
            Nullable = false
        };
        tgt.Default = new FieldDefaultModel(tgt, null, "LDefValue");

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.That.LinesEqual(new[]
        {
            "ALTER TABLE [LSchema].[LTable] ALTER COLUMN [LField] LType NULL",
            "ALTER TABLE [LSchema].[LTable] ADD DEFAULT (LDefValue) FOR [LField]",
        }, res);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void MssqlFieldAlter__Add_named_default(bool nullable)
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType",
            Nullable = nullable
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType",
            Nullable = nullable
        };
        tgt.Default = new FieldDefaultModel(tgt, "LDefault", "LDefValue");

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD CONSTRAINT [LDefault] DEFAULT (LDefValue) FOR [LField]", res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Add_named_default_and_change_signature()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "LType",
            Nullable = true
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "RType",
            Nullable = false
        };
        tgt.Default = new FieldDefaultModel(tgt, "LDefault", "LDefValue");

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        CollectionAssert.AreEqual(new[]
        {
            "ALTER TABLE [LSchema].[LTable] ALTER COLUMN [LField] LType NULL",
            "ALTER TABLE [LSchema].[LTable] ADD CONSTRAINT [LDefault] DEFAULT (LDefValue) FOR [LField]",
        }, res);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void MssqlFieldAlter__Remove_unnamed_default(bool nullable)
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType",
            Nullable = nullable
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType",
            Nullable = nullable
        };
        rgt.Default = new FieldDefaultModel(rgt, null, "RDefValue");

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("EXEC #usp_DropUnnamedDefault '[RSchema].[RTable]', 'RField'", res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Remove_unnamed_default_and_change_signature()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "LType",
            Nullable = true
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "RType",
            Nullable = false
        };
        rgt.Default = new FieldDefaultModel(rgt, null, "RDefValue");

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        CollectionAssert.AreEqual(new[]
        {
            "EXEC #usp_DropUnnamedDefault '[RSchema].[RTable]', 'RField'",
            "ALTER TABLE [LSchema].[LTable] ALTER COLUMN [LField] LType NULL",
        }, res);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void MssqlFieldAlter__Remove_named_default(bool nullable)
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType",
            Nullable = nullable
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType",
            Nullable = nullable
        };
        rgt.Default = new FieldDefaultModel(rgt, "RDefault", "RDefValue");

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("ALTER TABLE [RSchema].[RTable] DROP CONSTRAINT [RDefault]", res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Remove_named_default_and_change_signature()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "LType",
            Nullable = true
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "RType",
            Nullable = false
        };
        rgt.Default = new FieldDefaultModel(rgt, "RDefault", "RDefValue");

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        CollectionAssert.AreEqual(new[]
        {
            "ALTER TABLE [RSchema].[RTable] DROP CONSTRAINT [RDefault]",
            "ALTER TABLE [LSchema].[LTable] ALTER COLUMN [LField] LType NULL",
        }, res);
    }

    #endregion Default

    #region Nullable

    [TestMethod]
    public void MssqlFieldAlter__Change_to_nullable()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType",
            Nullable = true
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType",
            Nullable = false
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ALTER COLUMN [LField] XType NULL", res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Change_from_nullable()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType",
            Nullable = false
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType",
            Nullable = true
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ALTER COLUMN [LField] XType NULL -- NOTE: Cannot change to NOT NULL without default", res);
    }

    #endregion Nullable

    #region Unique

    [TestMethod]
    public void MssqlFieldAlter__Change_to_unique()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType",
            IsUnique = true
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType",
            IsUnique = false
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("ALTER TABLE [LSchema].[LTable] ADD UNIQUE ([LField])", res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Change_from_unique()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType",
            IsUnique = false
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType",
            IsUnique = true
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual(0, res.Length); // Handled elsewhere
    }

    #endregion Unique

    #region Reference

    [TestMethod]
    public void MssqlFieldAlter__Add_unnamed_reference()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType"
        };

        var end = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField");
        tgt.Ref = new FieldRefModel(tgt, end);

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("ALTER TABLE [LSchema].[LTable] WITH NOCHECK ADD FOREIGN KEY ([LField]) REFERENCES [TSchema].[TTable] ([TField])", res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Add_named_reference()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType"
        };

        var end = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField");
        tgt.Ref = new FieldRefModel(tgt, end)
        {
            Name = "Ref",
            IsSystemNamed = false
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual("ALTER TABLE [LSchema].[LTable] WITH NOCHECK ADD CONSTRAINT [Ref] FOREIGN KEY ([LField]) REFERENCES [TSchema].[TTable] ([TField])", res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Remove_unnamed_reference()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType"
        };

        var end = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField");
        rgt.Ref = new FieldRefModel(rgt, end);

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        CollectionAssert.AreEqual(new[]
        {
            "-- Removing unnamed FKey: [RSchema].[RTable].[RField] -> [TSchema].[TTable].[TField]",
            CommonMssql.REF_GET_FKEYNAME("[RSchema].[RTable]", "RField"),
            CommonMssql.REF_GET_DROP_SQL("[RSchema].[RTable]"),
            "EXEC (@DropConstraintSql)"
        }, res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Remove_named_reference()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType"
        };

        var end = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField");
        rgt.Ref = new FieldRefModel(rgt, end)
        {
            Name = "Ref",
            IsSystemNamed = false
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual(CommonMssql.ALTER_TABLE_DROP_CONSTRAINT("[RSchema].[RTable]", "Ref"), res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Replace_unnamed_reference_with_named()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType"
        };

        var end = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField");
        tgt.Ref = new FieldRefModel(tgt, end)
        {
            Name = "Ref",
            IsSystemNamed = false
        };
        rgt.Ref = new FieldRefModel(rgt, end);

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        CollectionAssert.AreEqual(new[]
        {
            "-- Removing unnamed FKey: [RSchema].[RTable].[RField] -> [TSchema].[TTable].[TField]",
            CommonMssql.REF_GET_FKEYNAME("[RSchema].[RTable]", "RField"),
            CommonMssql.REF_GET_DROP_SQL("[RSchema].[RTable]"),
            "EXEC (@DropConstraintSql)",
            "ALTER TABLE [LSchema].[LTable] WITH NOCHECK ADD CONSTRAINT [Ref] FOREIGN KEY ([LField]) REFERENCES [TSchema].[TTable] ([TField])"
        }, res);
    }

    [TestMethod]
    public void MssqlFieldAlter__Replace_named_reference_with_unnamed()
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LTable"), "LField")
        {
            Type = "XType"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable"), "RField")
        {
            Type = "XType"
        };

        var end = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField");
        tgt.Ref = new FieldRefModel(tgt, end);
        rgt.Ref = new FieldRefModel(rgt, end)
        {
            Name = "Ref",
            IsSystemNamed = false
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        CollectionAssert.AreEqual(new[]
        {
            CommonMssql.ALTER_TABLE_DROP_CONSTRAINT("[RSchema].[RTable]", "Ref"),
            "ALTER TABLE [LSchema].[LTable] WITH NOCHECK ADD FOREIGN KEY ([LField]) REFERENCES [TSchema].[TTable] ([TField])"
        }, res);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void MssqlFieldAlter__No_change(bool isNamed)
    {
        // Arrange
        var tgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "XSchema"), "XTable"), "XField")
        {
            Type = "XType"
        };
        var rgt = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "XSchema"), "XTable"), "XField")
        {
            Type = "XType"
        };

        var end = new FieldModel(new TableModel(new SchemaModel(DatabaseModel.Empty, "TSchema"), "TTable"), "TField");
        tgt.Ref = new FieldRefModel(tgt, end)
        {
            Name = "Ref",
            IsSystemNamed = isNamed
        };
        rgt.Ref = new FieldRefModel(rgt, end)
        {
            Name = "Ref",
            IsSystemNamed = isNamed
        };

        var diff = new DiffFieldAlter(tgt, rgt);

        // Act
        var res = new MssqlDiffFieldAlter(diff).ToString();

        // Assert
        Assert.AreEqual(0, res.Length);
    }

    #endregion Reference
}