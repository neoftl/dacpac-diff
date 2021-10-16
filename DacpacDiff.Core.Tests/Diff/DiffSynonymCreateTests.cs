using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Core.Diff.Tests
{
    [TestClass]
    public class DiffSynonymCreateTests
    {
        [TestMethod]
        public void DiffSynonymCreate()
        {
            // Arrange
            var syn = new SynonymModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LSynonym", "[TSchema].[TObject]");

            // Act
            var diff = new DiffSynonymCreate(syn);

            // Assert
            Assert.AreSame(syn, diff.Synonym);
            Assert.AreSame(syn, diff.Model);
            Assert.AreEqual("Create synonym", diff.Title);
            Assert.AreEqual("[LSchema].[LSynonym]", diff.Name);
        }

        [TestMethod]
        public void DiffSynonymCreate__Null_arg__Fail()
        {
            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new DiffSynonymCreate(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}