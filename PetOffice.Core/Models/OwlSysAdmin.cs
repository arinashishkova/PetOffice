using PetOffice.Core.Interfaces;

namespace PetOffice.Core.Models
{
    public class OwlSysAdmin : OfficePet, ITechSupport
    {
        public OwlSysAdmin(string name)
            : base(name)
        {
        }

        public override string Work()
        {
            SpendEnergy(15);
            CompleteTask();

            return $"{Name} monitored the servers all night without blinking.";
        }

        public override string CrazyAction()
        {
            SpendEnergy(25);

            return $"{Name} restarted the main server by hooting at it at 3 a.m.";
        }

        public string FixComputer()
        {
            SpendEnergy(10);
            CompleteTask();

            return $"{Name} fixed the computer by turning it off and on again.";
        }
    }
}
