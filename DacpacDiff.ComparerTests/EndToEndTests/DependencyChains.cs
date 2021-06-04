using DacpacDiff.Comparer.Comparers;
using DacpacDiff.Comparer.Tests.TestHelpers;
using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DacpacDiff.Comparer.Tests.EndToEndTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class DependencyChains
    {
        private static SchemeModel getScheme1()
        {
            var db1 = new DatabaseModel("db1");
            var sch = new SchemaModel(db1, "dbo");
            db1.Schemas[sch.Name] = sch;

            // A function that is depended on by everything possible
            var fn1 = new FunctionModuleModel(sch, "sfn_Func1")
            {
                ReturnType = "INT",
                Body = "RETURN 1",
            };
            sch.Modules[fn1.Name] = fn1;

            // A function that is never depended on
            var fn2 = new FunctionModuleModel(sch, "sfn_Func2")
            {
                ReturnType = "INT",
                Body = "RETURN 1",
            };
            sch.Modules[fn2.Name] = fn2;

            // A table with an unnamed field default of function 1
            var tbl1 = new TableModel(sch, "Table1");
            sch.Tables[tbl1.Name] = tbl1;
            var fld1A = new FieldModel(tbl1, "Field1A") { Type = "INT" };
            fld1A.Default = new FieldDefaultModel(fld1A, null, fn1.FullName + "()")
            {
                Dependencies = new[] { fn1.FullName }
            };
            tbl1.Fields = new[]
            {
                fld1A
            };

            // A table with an unnamed check of function 1
            var tbl2 = new TableModel(sch, "Table2");
            sch.Tables[tbl2.Name] = tbl2;
            var fld2A = new FieldModel(tbl2, "Field2A") { Type = "INT", IsPrimaryKey = true };
            tbl2.Fields = new[]
            {
                fld2A
            };
            var chk2 = new TableCheckModel(tbl2, null, fn1.FullName + "() = 1")
            {
                Dependencies = new[] { fn1.FullName }
            };
            tbl2.Checks.Add(chk2);

            // A function that uses function 1
            var fn3 = new FunctionModuleModel(sch, "sfn_Func3")
            {
                ReturnType = "INT",
                Body = $"RETURN {fn1.FullName}()",
                Dependencies = new[] { fn1.FullName }
            };
            sch.Modules[fn3.Name] = fn3;

            // An index that uses function 1 on table 1
            var idx1 = new IndexModuleModel(sch, "idx_Index1")
            {
                IndexedObjectFullName = tbl1.FullName,
                IndexedColumns = new[] { fld1A.Name },
                Dependencies = new[] { tbl1.FullName, fn1.FullName }
            };
            sch.Modules[idx1.Name] = idx1;

            // A proc that uses function 1
            var sp1 = new ProcedureModuleModel(sch, "usp_Procedure1")
            {
                Body = $"BEGIN SELECT {fn1.FullName}() END",
                Dependencies = new[] { fn1.FullName }
            };
            sch.Modules[sp1.Name] = sp1;

            // A trigger that uses function 1
            var tr1 = new TriggerModuleModel(sch, "tr_Trigger1")
            {
                Before = true,
                ForInsert = true,
                Body = $"BEGIN SELECT {fn1.FullName}() END",
                Dependencies = new[] { fn1.FullName }
            };
            sch.Modules[tr1.Name] = tr1;

            // A view that uses function 1
            var vw1 = new ViewModuleModel(sch, "vw_View1")
            {
                Body = $"SELECT {fn1.FullName}() [FN]",
                Dependencies = new[] { fn1.FullName }
            };
            sch.Modules[vw1.Name] = vw1;

            var scheme = new SchemeModel("scheme");
            scheme.Databases[db1.Name] = db1;
            return scheme;
        }

        [TestMethod]
        public void Function_alter_requires_some_dependents_to_be_dropped_and_recreated()
        {
            /// Arrange
            var oldScheme = getScheme1();

            var newScheme = getScheme1();
            var newDB = newScheme.Databases.Values.Single();

            Assert.IsTrue(newDB.TryGet<FunctionModuleModel>("[dbo].[sfn_Func1]", out var fn1)
                & newDB.TryGet<FunctionModuleModel>("[dbo].[sfn_Func3]", out var fn3)
                & newDB.TryGet<ProcedureModuleModel>("[dbo].[usp_Procedure1]", out var sp1)
                & newDB.TryGet<TableModel>("[dbo].[Table1]", out var tbl1)
                & newDB.TryGet<TableModel>("[dbo].[Table2]", out var tbl2)
                & newDB.TryGet<TriggerModuleModel>("[dbo].[tr_Trigger1]", out var tr1)
                & newDB.TryGet<ViewModuleModel>("[dbo].[vw_View1]", out var vw1));

            fn1.Body = "RETURN 2";

            var mcf = new ModelComparerFactory();
            var sc = new SchemeComparer(mcf);

            /// Act
            var res = sc.Compare(newScheme, oldScheme).ToArray();

            /// Assert
            var chg = res.Single(e => e is DiffModuleAlter d && d.Model == fn1);
            Assert.That.DoesNotContain(res, e => e is DiffObjectDrop d && d.Model == fn1);
            Assert.That.DoesNotContain(res, e => e is DiffModuleCreate d && d.Model == fn1);
            Assert.That.DoesNotContain(res, e => e is IDifference d && d.Model == sp1);
            Assert.That.DoesNotContain(res, e => e is IDifference d && d.Model == tr1);
            Assert.That.DoesNotContain(res, e => e is IDifference d && d.Model == vw1);
            Assert.That.ItemAppearsBefore(res, e => e is DiffFieldAlter d && d.RightField == tbl1.Fields[0], chg);
            Assert.That.ItemAppearsBefore(res, e => e is DiffModuleCreate d && d.Model == fn3, chg);
            Assert.That.ItemAppearsBefore(res, e => e is DiffTableCheckDrop d && d.Model == tbl2.Checks[0], chg);
            Assert.That.ItemAppearsBefore(res, chg, e => e is DiffFieldAlter d && d.LeftField == tbl1.Fields[0]);
            Assert.That.ItemAppearsBefore(res, chg, e => e is DiffModuleAlter d && d.Model == fn3);
            Assert.That.ItemAppearsBefore(res, chg, e => e is DiffTableCheckCreate d && d.Model == tbl2.Checks[0]);
        }

        [TestMethod]
        public void Function_alter_where_dependencies_did_not_exist_before()
        {
            /// Arrange
            var oldScheme = getScheme1();
            var oldDB = oldScheme.Databases.Values.Single();
            var oldSch = oldDB.Schemas["dbo"];
            oldSch.Modules.Remove("sfn_Func3");
            oldSch.Modules.Remove("idx_Index1");
            oldSch.Modules.Remove("tr_Trigger1");
            oldSch.Modules.Remove("usp_Procedure1");
            oldSch.Modules.Remove("vw_View1");
            oldSch.Tables.Remove("Table1");
            oldSch.Tables.Remove("Table2");

            var newScheme = getScheme1();
            var newDB = newScheme.Databases.Values.Single();

            Assert.IsTrue(newDB.TryGet<FunctionModuleModel>("[dbo].[sfn_Func1]", out var fn1)
                & newDB.TryGet<FunctionModuleModel>("[dbo].[sfn_Func3]", out var fn3)
                & newDB.TryGet<TableModel>("[dbo].[Table1]", out var tbl1));

            fn1.Body = "RETURN 2";

            var mcf = new ModelComparerFactory();
            var sc = new SchemeComparer(mcf);

            /// Act
            var res = sc.Compare(newScheme, oldScheme).ToArray();

            /// Assert
            var chg = res.Single(e => e is DiffModuleAlter d && d.Model == fn1);
            Assert.That.DoesNotContain(res, e => e is DiffObjectDrop);
            Assert.That.ItemAppearsBefore(res, chg, e => e != chg && e is DiffModuleAlter);
        }
    }
}
