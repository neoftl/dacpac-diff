using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DacpacDiff.Comparer.Comparers
{
    public class SchemeComparer
    {
        public static readonly Func<IEnumerable<IDifference>, IDifference, bool>[] DiffOrder = new Func<IEnumerable<IDifference>, IDifference, bool>[] {
            (a, d) => d is DiffTableCheckDrop,
            (a, d) => d is DiffObjectDrop drop && drop.Type != DiffObjectDrop.ObjectType.SCHEMA,
            (a, d) => d is DiffObjectDrop drop && drop.Type == DiffObjectDrop.ObjectType.SCHEMA,
            (a, d) => d is DiffSchemaCreate,
            (a, d) => d is DiffUserTypeCreate,
            (a, d) => d is DiffModuleCreate create && create.NeedsStub,
            (a, d) => d is DiffFieldAlter,
            (a, d) => d is DiffModuleAlter && !ReferencesRemain(a, d),
            (a, d) => d is DiffSynonymAlter,
            (a, d) => d is DiffFieldDrop,
            (a, d) => d is DiffFieldCreate,
            (a, d) => d is DiffTableCheckAlter,
            (a, d) => d is DiffTableCreate,
            (a, d) => d.Model is not null && new[] { typeof(FieldModel), typeof(ModuleModel), typeof(TableModel) }.Contains(d.Model.GetType()) && !ReferencesRemain(a, d),
            (a, d) => d is DiffModuleAlter,
            (a, d) => d is DiffTableCheckCreate,
            (a, d) => d is DiffRefCreate,
            (a, d) => d is DiffModuleCreate,
            (a, d) => d is DiffSynonymCreate,
        };

        private readonly IComparerFactory _comparerFactory;

        public SchemeComparer(IComparerFactory comparerFactory)
        {
            _comparerFactory = comparerFactory;
        }

        public IEnumerable<IDifference> Compare(SchemeModel leftScheme, SchemeModel rightScheme)
        {
            if (leftScheme.Databases.Count > 1)
            {
                throw new NotSupportedException("DacpacDiff does not yet support multiple databases per scheme");
            }
            var leftDb = leftScheme.Databases.Values.Single();

            if (rightScheme.Databases.Count > 1)
            {
                throw new NotSupportedException("DacpacDiff does not yet support multiple databases per scheme");
            }
            var rightDb = rightScheme.Databases.Values.Single();

            // Produce diffs in execution order
            var diffs = _comparerFactory.GetComparer<DatabaseModel>()
                .Compare(leftDb, rightDb);
            var sqlParts = OrderDiffsByDependency(diffs).ToList();
            return sqlParts;
        }

        /// <summary>
        /// Does <paramref name="diff"/> reference something in <paramref name="dict"/>.
        /// </summary>
        public static bool ReferencesRemain(IEnumerable<IDifference> dict, IDifference diff)
        {
            if (diff.Model is not null && dict.Where(o => o != diff && o.Model is IDependentModel)
                .Any(o => o.Model is IDependentModel m && m.Dependents?.Contains(diff.Model.Name) == true))
            {
                return true;
            }

            // TODO:
            var sql = diff.ToString() ?? string.Empty;
            var refs = dict.Where(o => o.Model is not null && o != diff && NameInSql(sql, o.Model)).ToArray();
            return refs.Any();

            //if (diff.Model is IDependentModel dm
            //    && (dm.Dependents?.Any(d => dict.Keys.Any(o => o.Name == d.Name)) ?? false))
            //{
            //    return true;
            //}

            //return dict.Any(k => k.Key != diff
            //    && k.Value.Contains(diff.Name));
        }

        public static bool NameInSql(string sql, IModel model)
        {
            return model switch
            {
                FieldModel fld => NameInSql(sql, fld.Table)
                    && (sql.Contains($"[{fld.Name}]")
                    || Regex.IsMatch(sql, $@"\W{Regex.Escape(fld.Name)}\W")),
                IModelInSchema mdl => sql.Contains($"[{mdl.Schema.Name}].[{mdl.Name}]")
                    || sql.Contains($"[{mdl.Schema.Name}].{mdl.Name}")
                    || sql.Contains($"{mdl.Schema.Name}.[{mdl.Name}]")
                    || sql.Contains($"{mdl.Schema.Name}.{mdl.Name}")
                    || (mdl.Schema.Name == "dbo" && sql.Contains($"[{mdl.Name}]")),
                _ => false,
            };
        }

        public static IEnumerable<IDifference> OrderDiffsByDependency(IEnumerable<IDifference> diffs)
        {
            var result = new List<IDifference>();
            var remain = diffs
                .Where(d => !string.IsNullOrWhiteSpace(d.ToString())) // TODO
                .ToList();
            int partL = 0, partR = 0;
            foreach (var inOrder in DiffOrder)
            {
                ++partL; partR = 0;
                IDifference[] matches;
                while (true)
                {
                    matches = remain.Where(r => inOrder(remain, r)).ToArray();
                    if (matches.Length == 0)
                    {
                        break;
                    }

                    result.Add(new DiffComment { Comment = $"{Environment.NewLine}L{partL}R{partR} ({matches.Length} | Rem {remain.Count})" });
                    result.AddRange(matches);
                    remain.RemoveAll(d => matches.Any(m => m == d));
                    ++partR;
                }
            }

            if (remain.Count > 0)
            {
                Console.Error.WriteLine($"[WARN] Including {remain.Count} unordered changes. First: {remain.First().GetType().FullName}");
            }
            result.AddRange(remain);

            return result.ToArray();
        }
    }
}
