using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DacpacDiff.Core.Model
{
    public class DatabaseModel : IModel
    {
        public static readonly DatabaseModel Empty = new();

        public string Name { get; }
        public string FullName => Name;
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
        
        //public bool TryGet(string fullName, [MaybeNullWhen(false)] out IModel model)
        //{
        //    model = Get(fullName);
        //    return model != null;
        //}
        public bool TryGet<T>(string fullName, [MaybeNullWhen(false)] out T model)
            where T : class, IModel
        {
            model = Get(fullName) as T;
            return model != null;
        }

        public IModel[] FindAllDependents(IModel me, params Type[] dependentTypes)
        {
            // Find all items
            var mods = Schemas.Values.SelectMany(s => s.Modules.Values);
            var tbls = Schemas.Values.SelectMany(s => s.Tables.Values);
            var flds = tbls.SelectMany(t => t.Fields);
            var chks = tbls.SelectMany(t => t.Checks);
            var defs = flds.Select(f => f.Default).NotNull();

            // Only want items that can be dependent
            IEnumerable<IDependentModel> deps = mods.OfType<IDependentModel>()
                .Concat(tbls.OfType<IDependentModel>())
                .Concat(flds.OfType<IDependentModel>())
                .Concat(chks.OfType<IDependentModel>())
                .Concat(defs.OfType<IDependentModel>())
                .ToArray();

            // Ignore unwanted types
            if (dependentTypes.Length > 0)
            {
                deps = deps.Where(d => dependentTypes.Contains(d.GetType()));
            }

            // Filter to actual dependents
            var fullName = me.FullName;
            deps = deps.Where(d => d.Dependencies.Contains(fullName));

            return deps.ToArray();
        }
    }
}
