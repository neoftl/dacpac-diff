using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public class DatabaseModel : IModel
    {
        public static readonly DatabaseModel Empty = new DatabaseModel();

        public string Name { get; set; }
        public string Version { get; set; } // Database version (for reference only)
        public object Users { get; set; }
        public object Logins { get; set; }
        public IDictionary<string, SchemaModel> Schemas { get; set; }

        public IModel? Get(string fullName)
        {
            var path = fullName.Split('.').Select(p => p.Trim('[', ']')).ToArray();
            if (Schemas.TryGetValue(path[0], out var schema)
                && path.Length > 1)
            {
                return schema.Get(path[1]);
            }
            return schema;
        }
        public bool TryGet<T>(string fullName, out T? model)
            where T : class, IModel
        {
            model = Get(fullName) as T;
            return model != null;
        }
    }
}
