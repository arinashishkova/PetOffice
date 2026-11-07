using PetOffice.Core.Models;
using PetOffice.Core.Persistence.Dto;

namespace PetOffice.Core.Persistence
{
    public static class PetDtoMapper
    {
        public static PetDto ToDto(
            OfficePet pet,
            string repositoryName)
        {
            ArgumentNullException.ThrowIfNull(pet);

            if (string.IsNullOrWhiteSpace(repositoryName))
            {
                throw new ArgumentException(
                    "Repository name is required.",
                    nameof(repositoryName));
            }

            return new PetDto
            {
                Id = pet.Id,
                Type = pet.GetType().Name,
                Name = pet.Name,
                Energy = pet.Energy,
                TasksCompleted = pet.TasksCompleted,
                ReactionsCount = pet.ReactionsCount,
                RepositoryName = repositoryName
            };
        }

        public static OfficePet FromDto(
            PetDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.Id == Guid.Empty)
            {
                throw new InvalidDataException(
                    "Pet ID is missing.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new InvalidDataException(
                    "Pet name is missing.");
            }

            if (string.IsNullOrWhiteSpace(dto.Type))
            {
                throw new InvalidDataException(
                    "Pet type is missing.");
            }

            if (string.IsNullOrWhiteSpace(dto.RepositoryName))
            {
                throw new InvalidDataException(
                    "Repository name is missing.");
            }

            OfficePet pet = dto.Type switch
            {
                nameof(CatManager) =>
                    new CatManager(dto.Name),

                nameof(DogIntern) =>
                    new DogIntern(dto.Name),

                nameof(HamsterIT) =>
                    new HamsterIT(dto.Name),

                nameof(ParrotReceptionist) =>
                    new ParrotReceptionist(dto.Name),

                nameof(RabbitAccountant) =>
                    new RabbitAccountant(dto.Name),

                nameof(RaccoonBarista) =>
                    new RaccoonBarista(dto.Name),

                nameof(OwlSysAdmin) =>
                    new OwlSysAdmin(dto.Name),

                _ => throw new InvalidDataException(
                    $"Unknown pet type: {dto.Type}")
            };

            pet.RestoreState(
                dto.Id,
                dto.Energy,
                dto.TasksCompleted,
                dto.ReactionsCount);

            return pet;
        }
    }
}