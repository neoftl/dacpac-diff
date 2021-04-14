using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Diff
{
    public class DiffSynonymCreate : IDifference
    {
        public SynonymModel Synonym { get; }

        public IModel Model => Synonym;
        public string Title => "Create synonym";
        public string Name => Synonym.FullName;

        public DiffSynonymCreate(SynonymModel synonym)
        {
            Synonym = synonym;
        }

        public override string ToString()
        {
            return $"CREATE SYNONYM {Name} FOR {Synonym.BaseObject}";
        }
    }
}
