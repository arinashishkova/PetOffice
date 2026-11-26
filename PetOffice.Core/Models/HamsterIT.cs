using System;
using System.Collections.Generic;
using System.Text;
using PetOffice.Core.Interfaces;

namespace PetOffice.Core.Models
{
    public class HamsterIT : OfficePet, ITechSupport
    {
        public HamsterIT(string name)
            : base(name)
        {
        }

        public override string Work()
        {
            SpendEnergy(12);
            CompleteTask();

            return $"{Name} checked the office computers.";
        }

        public override string CrazyAction()
        {
            SpendEnergy(25);
            CompleteTask();
            CompleteTask();

            return $"{Name} fixed the server by running at maximum speed in the wheel.";
        }

        public string FixComputer()
        {
            SpendEnergy(10);
            CompleteTask();

            return $"{Name} fixed a computer in the office.";
        }

        protected override string GetRestoreEnergyMessage()
        {
            return
                $"{Name} filled its cheeks with seeds, rested in the wheel, and restored energy to 100.";
        }
    }
}