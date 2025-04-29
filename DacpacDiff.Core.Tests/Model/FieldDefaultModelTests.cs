using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DacpacDiff.Core.Model.Tests;

[TestClass]
public class FieldDefaultModelTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Equals__Different_type__False(bool nullObject)
    {
        // Arrange
        var p1 = new FieldDefaultModel(FieldModel.Empty, "Def", "Value");

        var p2 = nullObject ? null : new object();

        // Act
        var res = p1.Equals(p2);

        // Assert
        Assert.IsFalse(res);
    }

    [TestMethod]
    [DataRow(null, "Def")]
    [DataRow("Def", null)]
    [DataRow("Def1", "Def2")]
    public void Equals__Only_different_names__False(string lDefName, string rDefName)
    {
        // Arrange
        var p1 = new FieldDefaultModel(FieldModel.Empty, lDefName, "Value");

        var p2 = new FieldDefaultModel(FieldModel.Empty, rDefName, "Value");

        // Act
        var res = p1.Equals(p2);

        // Assert
        Assert.IsTrue(res);
    }

    [TestMethod]
    [DataRow("", "")]
    [DataRow("A()B[]C D\rE\nF\tG", "abcdefg")]
    public void Equals__Both_similar__True(string lDefValue, string rDefValue)
    {
        // Arrange
        var p1 = new FieldDefaultModel(FieldModel.Empty, "Def", lDefValue);

        var p2 = new FieldDefaultModel(FieldModel.Empty, "Def", rDefValue);

        // Act
        var res = p1.Equals(p2);

        // Assert
        Assert.IsTrue(res);
    }

    [TestMethod]
    [DataRow("A", "")]
    [DataRow("", "B")]
    [DataRow("A", "B")]
    public void Equals__Different__False(string lDefValue, string rDefValue)
    {
        // Arrange
        var p1 = new FieldDefaultModel(FieldModel.Empty, null, lDefValue);

        var p2 = new FieldDefaultModel(FieldModel.Empty, null, rDefValue);

        // Act
        var res = p1.Equals(p2);

        // Assert
        Assert.IsFalse(res);
    }
}
