using System;
using System.Collections.Generic;
using System.Text;
using PetOffice.Core.Interfaces;

namespace PetOffice.Core.Models
{
    public class DogIntern : OfficePet, ICommunicate, IMakeCoffee
    {
        public DogIntern(string name)
            : base(name)
        {
        }

        public override string Work()
        {
            SpendEnergy(8);
            CompleteTask();

            return $"{Name} delivered the office documents.";
        }

        public override string CrazyAction()
        {
            SpendEnergy(15);
            CompleteTask();

            return $"{Name} tried to complete every office task at the same time.";
        }

        public string Communicate()
        {
            SpendEnergy(5);

            return $"{Name} happily greeted everyone in the office.";
        }

        public string MakeCoffee()
        {
            SpendEnergy(6);

            return $"{Name} made coffee for the office team.";
        }
    }
}