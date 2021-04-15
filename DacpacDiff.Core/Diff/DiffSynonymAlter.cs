using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
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
    }
}
