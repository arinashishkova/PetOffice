using PetOffice.Core.Interfaces;
using PetOffice.Core.Models;

namespace PetOffice.Core.Repositories
{
    public class Repository<T> : IRepository<T>
        where T : OfficePet
    {
        private readonly List<T> items = new();

        // main repository events
        public event EventHandler<OfficePetEventArgs>? ItemAdded;
        public event EventHandler<OfficePetEventArgs>? ItemRemoved;
        public event EventHandler<OfficePetEventArgs>? CrazyActionPerformed;

        // sends reaction messages to ui
        public event EventHandler<ReactionEventArgs>? ReactionOccurred;

        public void Add(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            items.Add(item);

            Subscribe(item);

            OfficePetEventArgs eventArgs =
                new OfficePetEventArgs(item);

            ItemAdded?.Invoke(
                this,
                eventArgs);

            PublishReactions(eventArgs);
        }

        public bool Remove(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!items.Remove(item))
            {
                return false;
            }

            Unsubscribe(item);

            OfficePetEventArgs eventArgs =
                new OfficePetEventArgs(item);

            ItemRemoved?.Invoke(
                this,
                eventArgs);

            PublishReactions(eventArgs);

            return true;
        }

        public IEnumerable<T> GetAll()
        {
            return items.AsReadOnly();
        }

        public T? Find(
            Func<T, bool> predicate)
        {
            return items.FirstOrDefault(
                predicate);
        }

        public void ReplaceAll(
            IEnumerable<T> newItems)
        {
            ArgumentNullException.ThrowIfNull(
                newItems);

            List<T> replacement =
                newItems.ToList();

            if (replacement.Any(
                item => item is null))
            {
                throw new ArgumentException(
                    "Repository cannot contain null items.",
                    nameof(newItems));
            }

            // remove old event subscriptions
            foreach (T item in items)
            {
                Unsubscribe(item);
            }

            items.Clear();

            // restore pets without raising add events
            foreach (T item in replacement)
            {
                items.Add(item);
                Subscribe(item);
            }
        }

        public string PerformCrazyAction(
            T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!items.Contains(item))
            {
                throw new InvalidOperationException(
                    "The pet is not in this repository.");
            }

            string message =
                item.CrazyAction();

            OfficePetEventArgs eventArgs =
                new OfficePetEventArgs(item);

            CrazyActionPerformed?.Invoke(
                this,
                eventArgs);

            PublishReactions(eventArgs);

            return message;
        }

        private void Subscribe(T item)
        {
            ItemAdded += item.HandlePetAdded;
            ItemRemoved += item.HandlePetRemoved;
            CrazyActionPerformed +=
                item.HandleCrazyAction;
        }

        private void Unsubscribe(T item)
        {
            ItemAdded -= item.HandlePetAdded;
            ItemRemoved -= item.HandlePetRemoved;
            CrazyActionPerformed -=
                item.HandleCrazyAction;
        }

        private void PublishReactions(
            OfficePetEventArgs eventArgs)
        {
            foreach (string message
                     in eventArgs.Reactions)
            {
                ReactionOccurred?.Invoke(
                    this,
                    new ReactionEventArgs(message));
            }
        }
    }
}