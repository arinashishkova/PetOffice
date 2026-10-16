using PetOffice.Core.Interfaces;

namespace PetOffice.Core.Models
{
    public class RabbitAccountant : OfficePet, ICommunicate
    {
        public RabbitAccountant(string name)
            : base(name)
        {
        }

        public override string Work()
        {
            SpendEnergy(10);
            CompleteTask();

            return $"{Name} balanced the monthly budget and found two missing carrots.";
        }

        public override string CrazyAction()
        {
            SpendEnergy(20);

            return $"{Name} declared that all office expenses must be paid in carrots.";
        }

        public string Communicate()
        {
            SpendEnergy(5);

            return $"{Name} sent a report with 47 tabs to the whole office.";
        }
    }
}
