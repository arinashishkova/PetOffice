using System;
using System.Collections.Generic;
using System.Text;


namespace PetOffice.Core.Models
{
    public abstract class OfficePet
    {
        public string Name { get; }
        public int Energy { get; private set; }
        public int TasksCompleted { get; private set; }
        public string DisplayName => $"{Name} - {GetType().Name}";

        protected OfficePet(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name is required.");
            }

            Name = name.Trim();
            Energy = 100;
            TasksCompleted = 0;
        }

        public abstract string Work();

        public abstract string CrazyAction();

        protected void SpendEnergy(int amount)
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

        protected void RestoreEnergy(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Energy amount must be greater than 0.");
            }

            Energy = Math.Min(100, Energy + amount);
        }

        protected void CompleteTask()
        {
            TasksCompleted++;
        }
    }
}