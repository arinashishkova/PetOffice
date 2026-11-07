using PetOffice.Core.Models;

namespace PetOffice.Core.Persistence
{
    public class RestoredWorld
    {
        public List<CatManager> Cats { get; } = new();

        public List<DogIntern> Dogs { get; } = new();

        public List<HamsterIT> Hamsters { get; } = new();

        public List<ParrotReceptionist> Parrots { get; } = new();

        public List<RabbitAccountant> Rabbits { get; } = new();

        public List<RaccoonBarista> Raccoons { get; } = new();

        public List<OwlSysAdmin> Owls { get; } = new();
    }
}