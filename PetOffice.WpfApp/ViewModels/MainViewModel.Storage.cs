using PetOffice.Core.Persistence;
using PetOffice.Core.Persistence.Dto;

namespace PetOffice.WpfApp.ViewModels
{
    public partial class MainViewModel
    {
        // creates a snapshot of the current office for storage
        private WorldDto CreateWorldDto()
        {
            return WorldDtoMapper.ToDto(
                GetAllPets());
        }

        // validates the loaded world first,
        // then replaces all repositories
        private void ApplyWorldDto(
            WorldDto world)
        {
            RestoredWorld restored =
                WorldDtoMapper.FromDto(world);

            // only after successful validation
            // do we replace the current office
            catRepository.ReplaceAll(
                restored.Cats);

            dogRepository.ReplaceAll(
                restored.Dogs);

            hamsterRepository.ReplaceAll(
                restored.Hamsters);

            parrotRepository.ReplaceAll(
                restored.Parrots);

            rabbitRepository.ReplaceAll(
                restored.Rabbits);

            raccoonRepository.ReplaceAll(
                restored.Raccoons);

            owlRepository.ReplaceAll(
                restored.Owls);

            SelectedPet = null;

            RefreshVisiblePets();
            RefreshSummary();
            RefreshAnalysisPets();

            UpdateSelectedPetState();
            UpdateCommandStates();
        }
    }
}