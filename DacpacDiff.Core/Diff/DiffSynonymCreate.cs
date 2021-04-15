using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
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
    }
}
