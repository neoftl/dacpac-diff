using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers
{
    public class SchemaComparer : IComparer<SchemaModel>
    {
        private readonly IComparerFactory _comparerFactory;

        public SchemaComparer(IComparerFactory comparerFactory)
        {
            _comparerFactory = comparerFactory;
        }

        public IEnumerable<IDifference> Compare(SchemaModel? lft, SchemaModel? rgt)
        {
            if (lft is null && rgt is null)
            {
                return Array.Empty<IDifference>();
            }

            var result = new List<IDifference>();

            // May be a drop/create
            if (lft is null)
            {
                result.Add(new DiffObjectDrop(rgt));
            }
            else if (rgt is null)
            {
                result.Add(new DiffSchemaCreate(lft));
            }

            // Treat synonyms as modules for right
            var rightModules = (rgt?.Modules?.ToArray() ?? Array.Empty<KeyValuePair<string, ModuleModel>>())
                //.Union(rgt?.Synonyms?.ToDictionary(s => s.Key, s => (ModuleModel)s.Value).ToArray() ?? new KeyValuePair<string, ModuleModel>[0])
                .ToDictionary(m => m.Key, m => m.Value);
            var rightSynonyms = rgt?.Synonyms ?? new Dictionary<string, SynonymModel>();

            // Modules
            var keys = (lft?.Modules?.Keys ?? Array.Empty<string>())
                .Union(rgt?.Modules?.Keys ?? Array.Empty<string>())
                .Distinct();
            var modCompr = _comparerFactory.GetComparer<ModuleModel>();
            var diffs = keys.SelectMany(k =>
            {
                var rightMod = rightModules?.Get(k)?.SetState(rgt, k);
                return modCompr.Compare(lft?.Modules?.Get(k)?.SetState(lft, k), rightMod) ?? Array.Empty<IDifference>();
            });
            result.AddRange(diffs);

            // Synonyms
            keys = (lft?.Synonyms?.Keys ?? Array.Empty<string>())
                .Union(rightSynonyms.Keys)
                .Distinct();
            var synCompr = _comparerFactory.GetComparer<SynonymModel>();
            diffs = keys.SelectMany(k =>
                synCompr.Compare(lft?.Synonyms?.Get(k)?.SetState(lft, k), rightSynonyms.Get(k)?.SetState(rgt, k)) ?? Array.Empty<IDifference>()
            );
            result.AddRange(diffs);

            // Tables
            keys = (lft?.Tables?.Keys ?? Array.Empty<string>())
                .Union(rgt?.Tables?.Keys ?? Array.Empty<string>())
                .Distinct();
            var tblCompr = _comparerFactory.GetComparer<TableModel>();
            diffs = keys.SelectMany(k =>
                tblCompr.Compare(lft?.Tables?.Get(k)?.SetState(lft, k), rgt?.Tables?.Get(k)?.SetState(rgt, k)) ?? Array.Empty<IDifference>()
            );
            result.AddRange(diffs);

            // User Types
            keys = (lft?.UserTypes?.Keys ?? Array.Empty<string>())
                .Union(rgt?.UserTypes?.Keys ?? Array.Empty<string>())
                .Distinct();
            var utCompr = _comparerFactory.GetComparer<UserTypeModel>();
            diffs = keys.SelectMany(k =>
                utCompr.Compare(lft?.UserTypes?.Get(k)?.SetState(lft, k), rgt?.UserTypes?.Get(k)?.SetState(rgt, k)) ?? Array.Empty<IDifference>()
            );
            result.AddRange(diffs);

            return result.ToArray();
        }
    }
}
