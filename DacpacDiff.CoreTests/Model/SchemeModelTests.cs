using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DacpacDiff.Core.Model.Tests
{
    [TestClass]
    public class SchemeModelTests
    {
        private static DatabaseModel getVersionedDatabase()
        {
            var db = new DatabaseModel("db");
            var sch = new SchemaModel(db, "dbo");
            sch.Modules["tfn_DatabaseVersion"] = new ModuleModel(sch, "tfn_DatabaseVersion", ModuleModel.ModuleType.PROCEDURE)
            {
                Definition = ">>>'123.456.789' [BuildNumber]<<<"
            };
            db.Schemas[sch.Name] = sch;
            return db;
        }

        [TestMethod]
        public void GetDatabaseVersion__Returns_version_from_proc_dbo_tfn_DatabaseVersion()
        {
            // Arrange
            var scheme = new SchemeModel("scheme");

            var db = getVersionedDatabase();
            scheme.Databases[db.Name] = db;

            // Act
            var res = scheme.GetDatabaseVersion();

            // Assert
            Assert.AreEqual("123.456.789", res);
        }

        [TestMethod]
        public void GetDatabaseVersion__No_databases__Unknown()
        {
            // Arrange
            var scheme = new SchemeModel("scheme");

            // Act
            var res = scheme.GetDatabaseVersion();

            // Assert
            Assert.AreEqual(SchemeModel.UNKNOWN_VER, res);
        }

        [TestMethod]
        public void GetDatabaseVersion__Multiple_databases__Unknown()
        {
            // Arrange
            var scheme = new SchemeModel("scheme");

            var db1 = getVersionedDatabase();
            scheme.Databases["db1"] = db1;

            var db2 = getVersionedDatabase();
            scheme.Databases["db2"] = db2;

            // Act
            var res = scheme.GetDatabaseVersion();

            // Assert
            Assert.AreEqual(SchemeModel.UNKNOWN_VER, res);
        }

        [TestMethod]
        public void GetDatabaseVersion__No_dbo_schema__Unknown()
        {
            // Arrange
            var scheme = new SchemeModel("scheme");

            var db = getVersionedDatabase();
            db.Schemas["dbox"] = db.Schemas["dbo"];
            db.Schemas.Remove("dbo");
            scheme.Databases[db.Name] = db;

            // Act
            var res = scheme.GetDatabaseVersion();

            // Assert
            Assert.AreEqual(SchemeModel.UNKNOWN_VER, res);
        }

        [TestMethod]
        public void GetDatabaseVersion__No_DatabaseVersion_function__Unknown()
        {
            // Arrange
            var scheme = new SchemeModel("scheme");
            
            var db = getVersionedDatabase();
            db.Schemas["dbo"].Modules["tfn_DatabaseVersionX"] = db.Schemas["dbo"].Modules["tfn_DatabaseVersion"];
            db.Schemas["dbo"].Modules.Remove("tfn_DatabaseVersion");
            scheme.Databases[db.Name] = db;

            // Act
            var res = scheme.GetDatabaseVersion();

            // Assert
            Assert.AreEqual(SchemeModel.UNKNOWN_VER, res);
        }

        [TestMethod]
        public void GetDatabaseVersion__DatabaseVersion_function_definition_mismatch__Unknown()
        {
            // Arrange
            var scheme = new SchemeModel("scheme");

            var db = getVersionedDatabase();
            db.Schemas["dbo"].Modules["tfn_DatabaseVersion"].Definition = ">>>'123.456.789' [BuildNumberX]<<<";
            scheme.Databases[db.Name] = db;

            // Act
            var res = scheme.GetDatabaseVersion();

            // Assert
            Assert.AreEqual(SchemeModel.UNKNOWN_VER, res);
        }
    }
}