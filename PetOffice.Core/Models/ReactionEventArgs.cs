namespace PetOffice.Core.Models
{
    public class ReactionEventArgs : EventArgs
    {
        public string Message { get; }

        public ReactionEventArgs(string message)
        {
            Message = message;
        }
    }
}