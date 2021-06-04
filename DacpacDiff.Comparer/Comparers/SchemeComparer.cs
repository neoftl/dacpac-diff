using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace DacpacDiff.Comparer.Comparers
{
    public class SchemeComparer
    {
        // TODO: move to IDifference as a comparison
        public static readonly Func<IEnumerable<IDifference>, IDifference, bool>[] DiffOrder = new Func<IEnumerable<IDifference>, IDifference, bool>[] {
            (a, d) => d is DiffTableCheckDrop,
            (a, d) => d is DiffObjectDrop drop && drop.Type != DiffObjectDrop.ObjectType.SCHEMA,
            (a, d) => d is DiffObjectDrop drop && drop.Type == DiffObjectDrop.ObjectType.SCHEMA,
            (a, d) => d is DiffSchemaCreate,
            (a, d) => d is DiffUserTypeCreate,
            (a, d) => d is DiffModuleCreate create && create.Module.StubOnCreate,
            (a, d) => d is DiffFieldAlter && !ReferencesRemain(a, d),
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

        private readonly IModelComparerFactory _comparerFactory;

        public SchemeComparer(IModelComparerFactory comparerFactory)
        {
            _comparerFactory = comparerFactory;
        }

        public IEnumerable<ISqlFormattable> Compare(SchemeModel lft, SchemeModel rgt)
        {
            if (lft.Databases.Count > 1)
            {
                throw new NotSupportedException("DacpacDiff does not yet support multiple databases per scheme");
            }
            var leftDb = lft.Databases.Values.Single();

            if (rgt.Databases.Count > 1)
            {
                throw new NotSupportedException("DacpacDiff does not yet support multiple databases per scheme");
            }
            var rightDb = rgt.Databases.Values.Single();

            // Generate diffs
            var diffs = _comparerFactory.GetComparer<DatabaseModel>()
                .Compare(leftDb, rightDb).ToList();

            // Ensure all chained diffs are added
            var diffQueue = new Queue<IChangeProvider>(diffs.OfType<IChangeProvider>());
            while (diffQueue.TryDequeue(out var d))
            {
                var cdiffs = d.GetAdditionalChanges();
                // TODO: ignore duplicate items already in diffs
                diffs.AddRange(cdiffs);
                cdiffs.OfType<IChangeProvider>().ToList().ForEach(diffQueue.Enqueue);
            }

            // Remove non-changes
            diffs.RemoveAll(d => d is INoopDifference);

            // TODO: Remove duplicate diffs

            // TODO: Group related diffs (e.g., small alters to same table)

            // Put diffs in execution order
            var sqlParts = OrderDiffsByDependency(diffs).ToList();
            return sqlParts;
        }

        /// <summary>
        /// Does <paramref name="diff"/> reference something in <paramref name="dict"/>.
        /// </summary>
        internal static bool ReferencesRemain(IEnumerable<IDifference> dict, IDifference diff)
        {
            if (diff.Model is IDependentModel d
                && dict.Where(m => m != diff).Any(m => d.Dependencies?.Contains(m.Name) == true))
            {
                return true;
            }

            // TODO?
            var sql = diff.ToString() ?? string.Empty;
            var refs = dict.Where(o => o.Model is not null && o != diff && NameInSql(sql, o.Model)).ToArray();
            return refs.Any();
        }

        // TODO: obsolete?
        [ExcludeFromCodeCoverage(Justification = "To be removed")]
        internal static bool NameInSql(string sql, IModel model)
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

        public static IEnumerable<ISqlFormattable> OrderDiffsByDependency(IEnumerable<IDifference> diffs)
        {
            var result = new List<ISqlFormattable>();
            var remain = diffs
                .Where(d => !string.IsNullOrWhiteSpace(d.ToString())) // TODO
                .ToList();
            int partL = 0, partR = 0;
            foreach (var inOrder in DiffOrder)
            {
                ++partL; partR = 0;
                processMatcher(inOrder);
            }

            if (remain.Count > 0)
            {
                Console.Error.WriteLine($"[WARN] Including {remain.Count} unordered changes. First: {remain.First().GetType().FullName}");
                processMatcher((a, d) => !ReferencesRemain(a, d));
                result.AddRange(remain);
            }

            return result.ToArray();

            void processMatcher(Func<IEnumerable<IDifference>, IDifference, bool> matcher)
            {
                while (true)
                {
                    var matches = remain.Where(r => matcher(remain, r)).ToArray();
                    if (matches.Length == 0)
                    {
                        break;
                    }

                    //result.Add(new SqlComment { Comment = $"{Environment.NewLine}L{partL}R{partR} ({matches.Length} | Rem {remain.Count})" });
                    result.AddRange(matches);
                    remain.RemoveAll(d => matches.Any(m => m == d));
                    ++partR;
                }
            }
        }
    }
}
