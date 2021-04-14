using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Core.Model
{
    public class SchemaModel : IModel<SchemaModel, DatabaseModel>
    {
        public static readonly SchemaModel Empty = new SchemaModel(DatabaseModel.Empty, string.Empty);

        public DatabaseModel Db { get; private set; }
        public string Name { get; private set; }
        public IDictionary<string, ModuleModel> Modules { get; set; }
        public IDictionary<string, SynonymModel> Synonyms { get; set; }
        public IDictionary<string, TableModel> Tables { get; set; }
        public IDictionary<string, UserTypeModel> UserTypes { get; set; }

        public SchemaModel(DatabaseModel db, string name)
        {
            Db = db ?? throw new ArgumentNullException(nameof(db));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public SchemaModel SetState(DatabaseModel db, string name)
        {
            Db = db ?? throw new ArgumentNullException(nameof(db));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            return this;
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
