using PetOffice.Core.Interfaces;

namespace PetOffice.Core.Models
{
    public class RaccoonBarista : OfficePet, IMakeCoffee
    {
        public RaccoonBarista(string name)
            : base(name)
        {
        }

        public override string Work()
        {
            SpendEnergy(12);
            CompleteTask();

            return $"{Name} cleaned the coffee machine and kept the shiny parts.";
        }

        public override string CrazyAction()
        {
            if (Energy < 30)
            {
                RestoreEnergy(20);

                return $"{Name} drank the entire office coffee supply and got energy back.";
            }

            SpendEnergy(25);

            return $"{Name} opened a midnight coffee bar inside the office bin.";
        }

        public string MakeCoffee()
        {
            SpendEnergy(8);
            CompleteTask();

            return $"{Name} made a coffee with a suspicious amount of foam.";
        }
    }
}