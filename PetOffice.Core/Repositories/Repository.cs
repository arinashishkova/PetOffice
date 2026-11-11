using PetOffice.Core.Interfaces;
using PetOffice.Core.Models;

namespace PetOffice.Core.Repositories
{
    public class Repository<T> : IRepository<T>
        where T : OfficePet
    {
        private readonly List<T> items = new();

        // Main repository events
        public event EventHandler<OfficePetEventArgs>? ItemAdded;
        public event EventHandler<OfficePetEventArgs>? ItemRemoved;
        public event EventHandler<OfficePetEventArgs>? CrazyActionPerformed;

        // Sends grouped reactions to UI
        public event EventHandler<ReactionEventArgs>? ReactionOccurred;


        public void Add(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            items.Add(item);
            Subscribe(item);

            OfficePetEventArgs eventArgs = new(item);

            ItemAdded?.Invoke(this, eventArgs);

            PublishReactions(eventArgs, ReactionKind.Welcome);
        }

        public bool Remove(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!items.Remove(item)) return false;

            Unsubscribe(item);

            OfficePetEventArgs eventArgs = new(item);

            ItemRemoved?.Invoke(this, eventArgs);

            PublishReactions(eventArgs, ReactionKind.Goodbye);

            return true;
        }

        public IEnumerable<T> GetAll() => items.AsReadOnly();

        public T? Find(Func<T, bool> predicate) => items.FirstOrDefault(predicate);

        public void ReplaceAll(IEnumerable<T> newItems)
        {
            ArgumentNullException.ThrowIfNull(newItems);

            List<T> replacement = newItems.ToList();

            if (replacement.Any(item => item is null))
            {
                throw new ArgumentException(
                    "Repository cannot contain null items.",
                    nameof(newItems));
            }

            // Remove old event subscriptions
            foreach (T item in items)
            {
                Unsubscribe(item);
            }

            items.Clear();

            // Restore pets without raising add events
            foreach (T item in replacement)
            {
                items.Add(item);
                Subscribe(item);
            }
        }

        public string PerformCrazyAction(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!items.Contains(item))
            {
                throw new InvalidOperationException("The pet is not in this repository.");
            }

            string message = item.CrazyAction();
            OfficePetEventArgs eventArgs = new(item);

            CrazyActionPerformed?.Invoke(this, eventArgs);

            PublishReactions(eventArgs, ReactionKind.CrazyAction);

            return message;
        }


        private void Subscribe(T item)
        {
            ItemAdded += item.HandlePetAdded;
            ItemRemoved += item.HandlePetRemoved;
            CrazyActionPerformed += item.HandleCrazyAction;
        }

        private void Unsubscribe(T item)
        {
            ItemAdded -= item.HandlePetAdded;
            ItemRemoved -= item.HandlePetRemoved;
            CrazyActionPerformed -= item.HandleCrazyAction;
        }

        private void PublishReactions(OfficePetEventArgs eventArgs, ReactionKind kind)
        {
            List<OfficePet> reactors = items
                .Cast<OfficePet>()
                .Where(pet => !ReferenceEquals(pet, eventArgs.Source))
                .ToList();

            if (reactors.Count == 0) return;

            ReactionOccurred?.Invoke(
                this,
                new ReactionEventArgs(eventArgs.Source, reactors, kind));
        }
    }
}