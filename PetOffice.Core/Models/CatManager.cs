using System;
using System.Collections.Generic;
using System.Text;
using PetOffice.Core.Interfaces;

namespace PetOffice.Core.Models
{
    public class CatManager : OfficePet, ICommunicate
    {
        public CatManager(string name)
            : base(name)
        {
        }

        public override string Work()
        {
            SpendEnergy(10);
            CompleteTask();

            return $"{Name} conducted an important office meeting.";
        }

        public override string CrazyAction()
        {
            if (Energy < 30)
            {
                RestoreEnergy(25);

                return $"{Name} declared an emergency office nap.";
            }

            SpendEnergy(20);

            return $"{Name} scheduled an emergency meeting about the lack of meetings.";
        }

        public string Communicate()
        {
            SpendEnergy(5);

            return $"{Name} sent a very serious message to the whole office.";
        }
    }
}