using System;

namespace PetOffice.Core.Models
{
    public abstract class OfficePet
    {
        public Guid Id { get; private set; }

        public string Name { get; }

        public int Energy { get; private set; }

        public int TasksCompleted { get; private set; }

        // reaction state
        public int ReactionsCount { get; private set; }


        public string DisplayName =>
            $"{Name} - {GetType().Name}";


        protected OfficePet(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Name is required.");
            }

            Id = Guid.NewGuid();
            Name = name.Trim();
            Energy = 100;
            TasksCompleted = 0;
            ReactionsCount = 0;
        }


        public abstract string Work();

        public abstract string CrazyAction();


        // restores saved state after loading
        internal void RestoreState(
            Guid id,
            int energy,
            int tasksCompleted,
            int reactionsCount)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Pet ID is required.");
            }

            if (energy < 0 || energy > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(energy),
                    "Energy must be between 0 and 100.");
            }

            if (tasksCompleted < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tasksCompleted),
                    "Tasks completed cannot be negative.");
            }

            if (reactionsCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reactionsCount),
                    "Reactions count cannot be negative.");
            }

            Id = id;
            Energy = energy;
            TasksCompleted = tasksCompleted;
            ReactionsCount = reactionsCount;
        }


        protected void SpendEnergy(
            int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Energy amount must be greater than 0.");
            }

            if (amount > Energy)
            {
                throw new InvalidOperationException(
                    "Not enough energy.");
            }

            Energy -= amount;
        }


        protected void RestoreEnergy(
            int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Energy amount must be greater than 0.");
            }

            Energy = Math.Min(
                100,
                Energy + amount);
        }


        public string RestoreToFullEnergy()
        {
            if (Energy >= 100)
            {
                return
                    $"{DisplayName} already has full energy.";
            }

            int restoredAmount =
                100 - Energy;

            RestoreEnergy(
                restoredAmount);

            return GetRestoreEnergyMessage();
        }


        protected virtual string GetRestoreEnergyMessage()
        {
            return
                $"{DisplayName} took a break and restored energy to 100.";
        }


        protected void CompleteTask()
        {
            TasksCompleted++;
        }


        internal void HandlePetAdded(
            object? sender,
            OfficePetEventArgs e)
        {
            if (ReferenceEquals(
                    this,
                    e.Source))
            {
                return;
            }

            ReactionsCount++;

            e.AddReaction(
                $"{DisplayName} welcomes {e.Source.DisplayName}.");
        }


        internal void HandlePetRemoved(
            object? sender,
            OfficePetEventArgs e)
        {
            if (ReferenceEquals(
                    this,
                    e.Source))
            {
                return;
            }

            ReactionsCount++;

            e.AddReaction(
                $"{DisplayName} says goodbye to {e.Source.DisplayName}.");
        }


        internal void HandleCrazyAction(
            object? sender,
            OfficePetEventArgs e)
        {
            if (ReferenceEquals(
                    this,
                    e.Source))
            {
                return;
            }

            ReactionsCount++;

            e.AddReaction(
                $"{DisplayName} reacts to {e.Source.DisplayName}'s crazy action.");
        }
    }
}