using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Model.Tests
{
    [TestClass]
    public class TableModelTests
    {
        [TestMethod]
        public void Dependencies__Union_of_field_references()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "Schema"), "Table")
            {
                Fields = new FieldModel[3]
            };

            tbl.Fields[0] = new FieldModel(tbl, "Fld1");

            tbl.Fields[1] = new FieldModel(tbl, "Fld2");
            tbl.Fields[1].Ref = new FieldRefModel(tbl.Fields[1], tbl.Fields[0]);

            tbl.Fields[2] = new FieldModel(tbl, "Fld3");
            tbl.Fields[2].Ref = new FieldRefModel(tbl.Fields[2], tbl.Fields[0]);

            // Act
            var deps = tbl.Dependencies;

            // Assert
            Assert.AreEqual(0, deps.Length);
        }

        [TestMethod]
        public void Dependencies__Union_of_field_references_to_other_tables()
        {
            // Arrange
            var rtbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "RSchema"), "RTable")
            {
                Fields = new FieldModel[1]
            };
            rtbl.Fields[0] = new FieldModel(rtbl, "RFld");

            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "Schema"), "Table")
            {
                Fields = new FieldModel[2]
            };

            tbl.Fields[0] = new FieldModel(tbl, "Fld1");
            tbl.Fields[0].Ref = new FieldRefModel(tbl.Fields[0], rtbl.Fields[0]);

            tbl.Fields[1] = new FieldModel(tbl, "Fld2");
            tbl.Fields[1].Ref = new FieldRefModel(tbl.Fields[1], rtbl.Fields[0]);

            // Act
            var deps = tbl.Dependencies;

            // Assert
            Assert.AreEqual(1, deps.Length);
            Assert.AreEqual("RTable", deps[0]);
        }

        [TestMethod]
        public void Equals__Same__True()
        {
            // Arrange
            var tbl1 = new TableModel(SchemaModel.Empty, "Name")
            {
                Fields = new FieldModel[2],
                PrimaryKeyName = "PKey",
                IsPrimaryKeyUnclustered = true,
            };
            tbl1.Temporality = new TemporalityModel(tbl1);
            tbl1.Fields[0] = new FieldModel(tbl1, "Field1");
            tbl1.Fields[1] = new FieldModel(tbl1, "Field2") { IsPrimaryKey = true };
            tbl1.Checks.Add(new TableCheckModel(tbl1, null, "CHECK"));

            var tbl2 = new TableModel(SchemaModel.Empty, "Name")
            {
                Fields = new FieldModel[2],
                PrimaryKeyName = "PKey",
                IsPrimaryKeyUnclustered = true,
            };
            tbl2.Temporality = new TemporalityModel(tbl2);
            tbl2.Fields[0] = new FieldModel(tbl2, "Field1");
            tbl2.Fields[1] = new FieldModel(tbl2, "Field2") { IsPrimaryKey = true };
            tbl2.Checks.Add(new TableCheckModel(tbl2, null, "CHECK"));

            // Act
            var res = tbl1.Equals(tbl2);

            // Assert
            Assert.IsTrue(res);
        }

        [TestMethod]
        [DataRow(1), DataRow(2), DataRow(3), DataRow(4), DataRow(5), DataRow(6), DataRow(7)]
        public void Equals__Diff_value__True(int diff)
        {
            bool b(int ifDiff) => ifDiff == diff;
            string s(int ifDiff, string val) => ifDiff == diff ? val : val + "X";

            // Arrange
            var tbl1 = new TableModel(SchemaModel.Empty, "Name")
            {
                Fields = new FieldModel[2],
                PrimaryKeyName = "PKey",
                IsPrimaryKeyUnclustered = true,
            };
            tbl1.Temporality = new TemporalityModel(tbl1);
            tbl1.Fields[0] = new FieldModel(tbl1, "Field1");
            tbl1.Fields[1] = new FieldModel(tbl1, "Field2") { IsPrimaryKey = true };
            tbl1.Checks.Add(new TableCheckModel(tbl1, null, "CHECK"));

            var tbl2 = new TableModel(SchemaModel.Empty, s(1, "Name"))
            {
                Fields = new FieldModel[2],
                PrimaryKeyName = s(2, "PKey"),
                IsPrimaryKeyUnclustered = b(3),
            };
            tbl2.Temporality = new TemporalityModel(tbl2);
            tbl2.Fields[0] = new FieldModel(tbl2, s(5, "Field1"));
            tbl2.Fields[1] = new FieldModel(tbl2, "Field2") { IsPrimaryKey = b(6) };
            tbl2.Checks.Add(new TableCheckModel(tbl2, null, s(7, "CHECK")));

            // Act
            var res = tbl1.Equals(tbl2);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void Equals__Other_model__False()
        {
            // Arrange
            var mdl = new TableModel(SchemaModel.Empty, string.Empty);

            // Act
            var res = mdl.Equals(DatabaseModel.Empty);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void GetHashCode__Changes_for_each_name_difference()
        {
            // Arrange
            var hashcodes = new List<int>(10);

            TableModel getNextTable(int diff)
            {
                string s(int ifDiff, string val) => ifDiff == diff ? val : val + "X";
                return new TableModel(new SchemaModel(DatabaseModel.Empty, s(1, "Schema")), s(2, "Name"));
            }

            // Act
            for (var i = 1; i <= 2; ++i)
            {
                hashcodes.Add(getNextTable(i).GetHashCode());
            }

            // Assert
            Assert.AreEqual(2, hashcodes.Distinct().Count());
        }

        [TestMethod]
        public void GetHashCode__Does_not_change_for_nonname_differences()
        {
            // Arrange
            var hashcodes = new List<int>(10);

            TableModel getNextTable(int diff)
            {
                bool b(int ifDiff) => ifDiff == diff;
                string s(int ifDiff, string val) => ifDiff == diff ? val : val + "X";

                var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "Schema"), "Name")
                {
                    Fields = new FieldModel[2],
                    PrimaryKeyName = s(1, "PKey"),
                    IsPrimaryKeyUnclustered = b(2),
                };
                tbl.Fields[0] = new FieldModel(tbl, s(3, "Field1"));
                tbl.Fields[1] = new FieldModel(tbl, "Field2") { IsPrimaryKey = b(4) };
                tbl.Checks.Add(new TableCheckModel(tbl, null, s(5, "CHECK")));
                tbl.Temporality = diff == 6 ? new TemporalityModel(tbl) : TemporalityModel.Empty;
                return tbl;
            }

            // Act
            for (var i = 1; i <= 6; ++i)
            {
                hashcodes.Add(getNextTable(i).GetHashCode());
            }

            // Assert
            Assert.AreEqual(1, hashcodes.Distinct().Count());
        }
    }
}