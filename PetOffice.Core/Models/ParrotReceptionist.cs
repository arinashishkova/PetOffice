using System;
using System.Collections.Generic;
using System.Text;
using PetOffice.Core.Interfaces;

namespace PetOffice.Core.Models
{
    public class ParrotReceptionist : OfficePet, ICommunicate
    {
        public ParrotReceptionist(string name)
            : base(name)
        {
        }

        public override string Work()
        {
            SpendEnergy(10);
            CompleteTask();

            return $"{Name} welcomed visitors at the reception desk.";
        }

        public override string CrazyAction()
        {
            if (Energy < 30)
            {
                RestoreEnergy(25);

                return $"{Name} took a break and listened to office gossip.";
            }

            SpendEnergy(20);

            return $"{Name} started repeating every conversation in the office.";
        }

        public string Communicate()
        {
            SpendEnergy(5);

            return $"{Name} greeted everyone with great enthusiasm.";
        }
    }
}
