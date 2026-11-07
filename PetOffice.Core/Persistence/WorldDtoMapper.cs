using PetOffice.Core.Models;
using PetOffice.Core.Persistence.Dto;

namespace PetOffice.Core.Persistence
{
    public static class WorldDtoMapper
    {
        public static WorldDto ToDto(
            IEnumerable<OfficePet> pets)
        {
            ArgumentNullException.ThrowIfNull(pets);

            WorldDto world = new();

            foreach (OfficePet pet in pets)
            {
                string repositoryName =
                    pet.GetType().Name;

                world.Pets.Add(
                    PetDtoMapper.ToDto(
                        pet,
                        repositoryName));
            }

            return world;
        }

        public static RestoredWorld FromDto(
            WorldDto world)
        {
            ArgumentNullException.ThrowIfNull(world);

            if (world.Pets is null)
            {
                throw new InvalidDataException(
                    "World does not contain a pets collection.");
            }

            RestoredWorld result = new();

            HashSet<Guid> ids = new();

            foreach (PetDto dto in world.Pets)
            {
                if (!ids.Add(dto.Id))
                {
                    throw new InvalidDataException(
                        $"Duplicate pet ID: {dto.Id}");
                }

                OfficePet pet =
                    PetDtoMapper.FromDto(dto);

                switch (pet)
                {
                    case CatManager cat:
                        EnsureRepository(
                            dto,
                            nameof(CatManager));

                        result.Cats.Add(cat);
                        break;

                    case DogIntern dog:
                        EnsureRepository(
                            dto,
                            nameof(DogIntern));

                        result.Dogs.Add(dog);
                        break;

                    case HamsterIT hamster:
                        EnsureRepository(
                            dto,
                            nameof(HamsterIT));

                        result.Hamsters.Add(hamster);
                        break;

                    case ParrotReceptionist parrot:
                        EnsureRepository(
                            dto,
                            nameof(ParrotReceptionist));

                        result.Parrots.Add(parrot);
                        break;

                    case RabbitAccountant rabbit:
                        EnsureRepository(
                            dto,
                            nameof(RabbitAccountant));

                        result.Rabbits.Add(rabbit);
                        break;

                    case RaccoonBarista raccoon:
                        EnsureRepository(
                            dto,
                            nameof(RaccoonBarista));

                        result.Raccoons.Add(raccoon);
                        break;

                    case OwlSysAdmin owl:
                        EnsureRepository(
                            dto,
                            nameof(OwlSysAdmin));

                        result.Owls.Add(owl);
                        break;

                    default:
                        throw new InvalidDataException(
                            $"Unsupported pet type: {pet.GetType().Name}");
                }
            }

            return result;
        }

        private static void EnsureRepository(
            PetDto dto,
            string expectedRepository)
        {
            if (!string.Equals(
                    dto.RepositoryName,
                    expectedRepository,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"Pet '{dto.Name}' belongs to an invalid repository.");
            }
        }
    }
}