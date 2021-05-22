using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DacpacDiff.Mssql.Diff.Tests
{
    [TestClass]
    public class MssqlDiffUserTypeCreateTests
    {
        [TestMethod]
        public void MssqlDiffUserTypeCreate__Non_table()
        {
            // Arrange
            var utype = new UserTypeModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LUserType")
            {
                Type = "Type"
            };

            var diff = new DiffUserTypeCreate(utype);

            // Act
            var res = new MssqlDiffUserTypeCreate(diff).ToString();

            // Assert
            Assert.AreEqual("CREATE TYPE [LSchema].[LUserType] FROM Type", res);
        }

        [TestMethod]
        public void MssqlDiffUserTypeCreate__Table()
        {
            // Arrange
            var utype = new UserTypeModel(new SchemaModel(DatabaseModel.Empty, "LSchema"), "LUserType")
            {
                Type = "TABLE"
            };
            utype.Fields = new[]
            {
                new UserTypeFieldModel(utype, "F1")  { Type = "FType", IsPrimaryKey = true, Identity = true },
                new UserTypeFieldModel(utype, "F2")  { Computation = "Computation" },
                new UserTypeFieldModel(utype, "F3")  { Type = "UType", IsUnique = true, Computation = "" },
                new UserTypeFieldModel(utype, "F4")  { Type = "NType", Nullable = true },
                new UserTypeFieldModel(utype, "F5")  { Type = "DType", Default = "DefValue" },
            };

            var diff = new DiffUserTypeCreate(utype);

            // Act
            var res = new MssqlDiffUserTypeCreate(diff).ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            CollectionAssert.AreEqual(new[]
            {
                "CREATE TYPE [LSchema].[LUserType] AS TABLE",
                "(",
                "    [F1] FType NOT NULL PRIMARY KEY IDENTITY(1,1),",
                "    [F2] AS Computation,",
                "    [F3] UType NOT NULL UNIQUE,",
                "    [F4] NType NULL,",
                "    [F5] DType NOT NULL DEFAULT (DefValue)",
                ")"
            }, res, string.Join("\n", res));
        }
    }
}