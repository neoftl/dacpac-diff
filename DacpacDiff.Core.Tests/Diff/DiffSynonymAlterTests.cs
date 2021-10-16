using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffSynonymAlterTests
    {
        [TestMethod]
        public void DiffSynonymAlter()
        {
            // Arrange
            var syn = new SynonymModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LSynonym", "[TSchema].[TObject]");

            // Act
            var diff = new DiffSynonymAlter(syn);

            // Assert
            Assert.AreSame(syn, diff.Synonym);
            Assert.AreSame(syn, diff.Model);
            Assert.AreEqual("Alter synonym", diff.Title);
            Assert.AreEqual("[LSchema].[LSynonym]", diff.Name);
        }

        [TestMethod]
        public void DiffSynonymAlter__Null_arg__Fail()
        {
            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new DiffSynonymAlter(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}