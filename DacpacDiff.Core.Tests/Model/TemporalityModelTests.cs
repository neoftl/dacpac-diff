using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Model.Tests
{
    [TestClass]
    public class TemporalityModelTests
    {
        [TestMethod]
        public void Equals__Same__True()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "schema"), "Table");
            var mdl1 = new TemporalityModel(tbl)
            {
                PeriodFieldFrom = "From",
                PeriodFieldTo = "To",
                HistoryTable = "History"
            };

            var mdl2 = new TemporalityModel(tbl)
            {
                PeriodFieldFrom = "From",
                PeriodFieldTo = "To",
                HistoryTable = "History"
            };

            // Act
            var res = mdl1.Equals(mdl2);

            // Assert
            Assert.IsTrue(res);
        }

        [TestMethod]
        [DataRow(1), DataRow(2), DataRow(3)]
        public void Equals__Diff_value__True(int diff)
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "schema"), "Table");
            var mdl1 = new TemporalityModel(tbl)
            {
                PeriodFieldFrom = "From",
                PeriodFieldTo = "To",
                HistoryTable = "History"
            };

            var mdl2 = new TemporalityModel(tbl)
            {
                PeriodFieldFrom = diff != 1 ? "From" : "FromX",
                PeriodFieldTo = diff != 2 ? "To" : "ToX",
                HistoryTable = diff != 3 ? "History" : "HistoryX"
            };

            // Act
            var res = mdl1.Equals(mdl2);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void Equals__Other_model__False()
        {
            // Arrange
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "schema"), "Table");
            var mdl = new TemporalityModel(tbl);

            // Act
            var res = mdl.Equals(DatabaseModel.Empty);

            // Assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void GetHashCode__Different_result_for_each_change()
        {
            // Arrange
            var hashcodes = new List<int>(10);
            
            var tbl = new TableModel(new SchemaModel(DatabaseModel.Empty, "schema"), "Table");
            var mdl = new TemporalityModel(tbl)
            {
                PeriodFieldFrom = "From",
                PeriodFieldTo = "To",
                HistoryTable = "History"
            };

            // Act
            hashcodes.Add(mdl.GetHashCode());

            mdl.PeriodFieldFrom = "FromX";
            hashcodes.Add(mdl.GetHashCode());

            mdl.PeriodFieldTo = "ToX";
            hashcodes.Add(mdl.GetHashCode());

            mdl.HistoryTable = "HistoryX";
            hashcodes.Add(mdl.GetHashCode());

            // Assert
            Assert.AreEqual(4, hashcodes.Distinct().Count());
        }
    }
}