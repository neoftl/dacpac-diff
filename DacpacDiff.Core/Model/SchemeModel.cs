using DacpacDiff.Core.Utility;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public class SchemeModel : IModel
    {
        public string Name { get; }
        public IDictionary<string, DatabaseModel> Databases { get; } = new Dictionary<string, DatabaseModel>();

        public SchemeModel(string name)
        {
            Name = name;
        }

        public string GetDatabaseVersion()
        {
            var db = Databases.Values.Single();

            // Resolve the version of the scheme from the automatic object
            ModuleModel? fnVer = null;
            if (db.Schemas.TryGetValue("dbo", out var dboSchema)
                && dboSchema.Modules?.TryGetValue("tfn_DatabaseVersion", out fnVer) == true && fnVer != null
                && fnVer.Definition.TryMatch(@"'([\d\.]+)'\s+\[BuildNumber\]", out var m) == true && m != null)
            {
                return m.Groups[1].Value;
            }
            return "(unknown)";
        }
    }
}
