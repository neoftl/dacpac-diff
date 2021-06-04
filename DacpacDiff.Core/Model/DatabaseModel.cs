using DacpacDiff.Core.Utility;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public class DatabaseModel : IModel
    {
        public static readonly DatabaseModel Empty = new();

        public string Name { get; set; }
        public string? Version { get; set; } // Database version (for reference only)
        public object? Users { get; set; } // TODO
        public object? Logins { get; set; } // TODO
        public IDictionary<string, SchemaModel> Schemas { get; } = new Dictionary<string, SchemaModel>();

        private DatabaseModel()
        {
            Name = string.Empty;
        }
        public DatabaseModel(string name)
        {
            Name = name;
        }

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
        public T? Get<T>(string fullName) where T : class, IModel => Get(fullName) as T;

        public bool TryGet<T>(string fullName, [MaybeNullWhen(false)] out T model)
            where T : class, IModel
        {
            model = Get(fullName) as T;
            return model != null;
        }

        public IModel[] FindAllDependents<T, U>(IModel<T, U> me)
            where T : IModel<T, U>
            where U : IModel
        {
            var mods = Schemas.Values.SelectMany(s => s.Modules.Values).ToArray();
            var tbls = Schemas.Values.SelectMany(s => s.Tables.Values).ToArray();
            var chks = tbls.SelectMany(t => t.Checks).ToArray();
            var defs = tbls.SelectMany(t => t.Fields.Select(f => f.Default).NotNull()).ToArray();

            var fullName = me.FullName;
            var deps = mods.OfType<IDependentModel>()
                .Union(tbls.OfType<IDependentModel>())
                .Union(chks.OfType<IDependentModel>())
                .Union(defs.OfType<IDependentModel>())
                .Where(d => d.Dependencies.Contains(fullName))
                .ToArray();

            return deps;
        }
    }
}
