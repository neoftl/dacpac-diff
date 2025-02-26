using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IFormatProvider = DacpacDiff.Core.IFormatProvider;

namespace DacpacDiff.Mssql.Tests;

[TestClass]
public class MssqlFileBuilderTests
{
    private Mock<IFormatProvider> _formatMock = null!;

    [TestInitialize]
    public void Init()
    {
        _formatMock = new Mock<IFormatProvider>(MockBehavior.Strict);
        _formatMock.SetupGet(m => m.FormatName).Returns("TEST");
    }

    [TestMethod]
    public void Generate__Without_items()
    {
        // Arrange
        var optionsMock = new Mock<IOutputOptions>(MockBehavior.Strict);
        optionsMock.SetupGet(m => m.PrettyPrint).Returns(false);
        optionsMock.SetupGet(m => m.DisableDatalossCheck).Returns(false);
        optionsMock.SetupGet(m => m.ChangeDisableOption).Returns(false);

        var fb = new MssqlFileBuilder(_formatMock.Object)
        {
            Options = optionsMock.Object
        };

        // Act
        var res = fb.Generate("target.dacpac", "current.dacpac", "1.2.3.4", []);

        // Assert
        StringAssert.StartsWith(res, "-- Delta upgrade from current.dacpac to target.dacpac");
        StringAssert.Contains(res, "-- Changes (0):");
        StringAssert.Contains(res, "-- Pre-flight checks");
        StringAssert.Contains(res, "IF (@CurVersion <> '1.2.3.4') BEGIN");
        StringAssert.Contains(res, "-- Release framework");
        StringAssert.Contains(res, "EXEC #print 0, 'Complete'");
    }

    [TestMethod]
    public void Generate__Formats_items()
    {
        // Arrange
        var optionsMock = new Mock<IOutputOptions>(MockBehavior.Strict);
        optionsMock.SetupGet(m => m.PrettyPrint).Returns(false);
        optionsMock.SetupGet(m => m.DisableDatalossCheck).Returns(false);
        optionsMock.SetupGet(m => m.ChangeDisableOption).Returns(false);

        var fb = new MssqlFileBuilder(_formatMock.Object)
        {
            Options = optionsMock.Object
        };

        var sqlItemMock = new Mock<ISqlFormattable>(MockBehavior.Strict);
        sqlItemMock.Setup(m => m.Name).Returns("SqlItemName");
        sqlItemMock.Setup(m => m.Title).Returns("SqlItemTitle");

        var sqlItemFormatterMock = new Mock<ISqlFormatter>(MockBehavior.Strict);
        sqlItemFormatterMock.Setup(m => m.Format(fb))
            .Callback<ISqlFileBuilder>(f => f.AppendLine("ISqlFormatter.Format"));

        _formatMock.Setup(m => m.GetSqlFormatter(sqlItemMock.Object))
            .Returns(sqlItemFormatterMock.Object);

        // Act
        var res = fb.Generate("target.dacpac", "current.dacpac", "1.2.3.4", [sqlItemMock.Object]);

        // Assert
        Assert.IsTrue(res.Contains("-- Changes (1):"));
        Assert.IsTrue(res.Contains("-- [1] SqlItemTitle: SqlItemName"));
        Assert.IsTrue(res.Contains("ISqlFormatter.Format"));
        Assert.IsTrue(res.Contains("#print 0, '> [1] SqlItemTitle: SqlItemName (99.99%%)'"));
    }

    [TestMethod]
    public void Generate__Marks_dataloss_items()
    {
        // Arrange
        var optionsMock = new Mock<IOutputOptions>(MockBehavior.Strict);
        optionsMock.SetupGet(m => m.PrettyPrint).Returns(false);
        optionsMock.SetupGet(m => m.DisableDatalossCheck).Returns(false);
        optionsMock.SetupGet(m => m.ChangeDisableOption).Returns(false);

        var fb = new MssqlFileBuilder(_formatMock.Object)
        {
            Options = optionsMock.Object
        };

        var sqlItemMock = new Mock<ISqlFormattable>(MockBehavior.Strict);
        sqlItemMock.Setup(m => m.Name).Returns("SqlItemName");
        sqlItemMock.Setup(m => m.Title).Returns("SqlItemTitle");

        var datalossTable = "";
        var sqlItemDatalossMock = sqlItemMock.As<IDataLossChange>();
        sqlItemDatalossMock.Setup(m => m.GetDataLossTable(out datalossTable))
            .Returns(true);

        var sqlItemFormatterMock = new Mock<ISqlFormatter>(MockBehavior.Strict);
        sqlItemFormatterMock.Setup(m => m.Format(fb))
            .Callback<ISqlFileBuilder>(f => f.AppendLine("ISqlFormatter.Format"));

        _formatMock.Setup(m => m.GetSqlFormatter(sqlItemMock.Object))
            .Returns(sqlItemFormatterMock.Object);

        // Act
        var res = fb.Generate("target.dacpac", "current.dacpac", "1.2.3.4", [sqlItemMock.Object]);

        // Assert
        Assert.IsTrue(res.Contains("-- Changes (1):"));
        Assert.IsTrue(res.Contains("-- [1] SqlItemTitle: SqlItemName (potential data-loss)"));
        Assert.IsTrue(res.Contains("#print 0, '> [1] SqlItemTitle: SqlItemName (99.99%%)'"));
    }

    [TestMethod]
    public void Generate__Does_not_mark_dataloss_items_if_no_dataloss()
    {
        // Arrange
        var optionsMock = new Mock<IOutputOptions>(MockBehavior.Strict);
        optionsMock.SetupGet(m => m.PrettyPrint).Returns(false);
        optionsMock.SetupGet(m => m.DisableDatalossCheck).Returns(false);
        optionsMock.SetupGet(m => m.ChangeDisableOption).Returns(false);

        var fb = new MssqlFileBuilder(_formatMock.Object)
        {
            Options = optionsMock.Object
        };

        var sqlItemMock = new Mock<ISqlFormattable>(MockBehavior.Strict);
        sqlItemMock.Setup(m => m.Name).Returns("SqlItemName");
        sqlItemMock.Setup(m => m.Title).Returns("SqlItemTitle");

        var datalossTable = "";
        var sqlItemDatalossMock = sqlItemMock.As<IDataLossChange>();
        sqlItemDatalossMock.Setup(m => m.GetDataLossTable(out datalossTable))
            .Returns(false);

        var sqlItemFormatterMock = new Mock<ISqlFormatter>(MockBehavior.Strict);
        sqlItemFormatterMock.Setup(m => m.Format(fb))
            .Callback<ISqlFileBuilder>(f => f.AppendLine("ISqlFormatter.Format"));

        _formatMock.Setup(m => m.GetSqlFormatter(sqlItemMock.Object))
            .Returns(sqlItemFormatterMock.Object);

        // Act
        var res = fb.Generate("target.dacpac", "current.dacpac", "1.2.3.4", [sqlItemMock.Object]);

        // Assert
        Assert.IsTrue(res.Contains("-- Changes (1):"));
        Assert.IsFalse(res.Contains("-- [1] SqlItemTitle: SqlItemName (potential data-loss)"));
    }

    [TestMethod]
    public void Generate__Does_not_count_items_without_title()
    {
        // Arrange
        var optionsMock = new Mock<IOutputOptions>(MockBehavior.Strict);
        optionsMock.SetupGet(m => m.PrettyPrint).Returns(false);
        optionsMock.SetupGet(m => m.DisableDatalossCheck).Returns(false);
        optionsMock.SetupGet(m => m.ChangeDisableOption).Returns(false);

        var fb = new MssqlFileBuilder(_formatMock.Object)
        {
            Options = optionsMock.Object
        };

        var sqlItemMock = new Mock<ISqlFormattable>(MockBehavior.Strict);
        sqlItemMock.Setup(m => m.Name).Returns("SqlItemName");
        sqlItemMock.Setup(m => m.Title).Returns((string?)null);

        var sqlItemFormatterMock = new Mock<ISqlFormatter>(MockBehavior.Strict);
        sqlItemFormatterMock.Setup(m => m.Format(fb));

        _formatMock.Setup(m => m.GetSqlFormatter(sqlItemMock.Object))
            .Returns(sqlItemFormatterMock.Object);

        // Act
        var res = fb.Generate("target.dacpac", "current.dacpac", "1.2.3.4", [sqlItemMock.Object]);

        // Assert
        Assert.IsTrue(res.Contains("-- Changes (0):"));
        Assert.IsTrue(!res.Contains("SqlItemName"));
    }
}