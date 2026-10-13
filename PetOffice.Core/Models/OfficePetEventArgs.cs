namespace PetOffice.Core.Models
{
    public class OfficePetEventArgs : EventArgs
    {
        public OfficePet Source { get; }

        public List<string> Reactions { get; } = new();

        public OfficePetEventArgs(OfficePet source)
        {
            Source = source;
        }

        public void AddReaction(string message)
        {
            Reactions.Add(message);
        }
    }
}