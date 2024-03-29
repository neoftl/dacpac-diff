﻿using DacpacDiff.Core.Utility;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public class SchemeModel : IModel
    {
        public const string UNKNOWN_VER = "(unknown)";

        public string Name { get; }
        public string FullName => Name;
        public IDictionary<string, DatabaseModel> Databases { get; } = new Dictionary<string, DatabaseModel>();

        public SchemeModel(string name)
        {
            Name = name;
        }

        public string GetDatabaseVersion()
        {
            if (Databases.Count == 1)
            {
                var db = Databases.Values.Single(); // TODO

                // Resolve the version of the scheme from the automatic object
                if (db.Schemas.TryGetValue("dbo", out var dboSchema)
                    && dboSchema.Modules.TryGetValue("tfn_DatabaseVersion", out var mod) == true
                    && mod is FunctionModuleModel fnVer
                    && fnVer.Body.TryMatch(@"'([\d\.]+)'\s+\[BuildNumber\]", out var m) == true && m != null)
                {
                    return m.Groups[1].Value;
                }
            }
            return UNKNOWN_VER;
        }
    }
}
