namespace PetOffice.Core.Models
{
    public enum ReactionKind
    {
        Welcome,
        Goodbye,
        CrazyAction
    }


    public class ReactionEventArgs : EventArgs
    {
        public OfficePet Source { get; }

        public IReadOnlyList<OfficePet> Reactors { get; }

        public ReactionKind Kind { get; }


        public ReactionEventArgs(
            OfficePet source,
            IEnumerable<OfficePet> reactors,
            ReactionKind kind)
        {
            Source = source;
            Reactors = reactors.ToList().AsReadOnly();
            Kind = kind;
        }
    }
}