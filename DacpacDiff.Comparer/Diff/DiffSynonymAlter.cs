using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Diff
{
    public class DiffSynonymAlter : IDifference
    {
        public SynonymModel Synonym { get; }

        public IModel Model => Synonym;
        public string Title => "Alter synonym";
        public string Name => Synonym.FullName;

        public DiffSynonymAlter(SynonymModel synonym)
        {
            Synonym = synonym;
        }

        public override string ToString()
        {
            return $"DROP SYNONYM {Name}\r\n"
                + "GO\r\n"
                + $"CREATE SYNONYM {Name} FOR {Synonym.BaseObject}";
        }
    }
}
