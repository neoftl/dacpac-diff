using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Core.Model
{
    public class SchemaModel : IModel<SchemaModel, DatabaseModel>
    {
        public static readonly SchemaModel Empty = new(DatabaseModel.Empty, string.Empty);

        public DatabaseModel Db { get; set; }
        public string FullName => $"[{Name}]";
        public string Name { get; set; }
        public IDictionary<string, ModuleModel> Modules { get; } = new Dictionary<string, ModuleModel>();
        public IDictionary<string, SynonymModel> Synonyms { get; } = new Dictionary<string, SynonymModel>();
        public IDictionary<string, TableModel> Tables { get; } = new Dictionary<string, TableModel>();
        public IDictionary<string, UserTypeModel> UserTypes { get; } = new Dictionary<string, UserTypeModel>();

        public SchemaModel(DatabaseModel db, string name)
        {
            Db = db ?? throw new ArgumentNullException(nameof(db));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Get any object type from this schema by name.
        /// Returns null if no match.
        /// </summary>
        public IModelInSchema? Get(string name)
        {
            return Modules.Get(name)
                ?? Synonyms.Get(name)
                ?? Tables.Get(name)
                ?? (IModelInSchema?)UserTypes.Get(name);
        }
    }
}
