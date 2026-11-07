using System.IO;
using PetOffice.Core.Persistence;
using PetOffice.Core.Persistence.Dto;
using PetOffice.WpfApp.Services;
using PetOffice.WpfApp.Services.Storage;

namespace PetOffice.WpfApp.ViewModels
{
    public partial class MainViewModel
    {
        private readonly IWorldStorage jsonStorage =
            new JsonWorldStorage();
        private readonly IWorldStorage xmlStorage =
            new XmlWorldStorage();

        private readonly IFileDialogService fileDialogService =
            new WpfFileDialogService();

        private string storageStatus =
            "No storage operation yet.";

        public AsyncCommand SaveJsonCommand { get; private set; } = null!;

        public AsyncCommand LoadJsonCommand { get; private set; } = null!;

        public AsyncCommand SaveXmlCommand { get; private set; } = null!;

        public AsyncCommand LoadXmlCommand { get; private set; } = null!;

        public string StorageStatus
        {
            get => storageStatus;

            private set
            {
                if (storageStatus == value)
                {
                    return;
                }

                storageStatus = value;

                OnPropertyChanged();
            }
        }

        private void InitializeStorage()
        {
            SaveJsonCommand =
                new AsyncCommand(SaveJsonAsync);

            LoadJsonCommand =
                new AsyncCommand(LoadJsonAsync);

            SaveXmlCommand =
                new AsyncCommand(SaveXmlAsync);

            LoadXmlCommand =
                new AsyncCommand(LoadXmlAsync);
        }

        private async Task SaveJsonAsync()
        {
            string? path =
                fileDialogService.GetSaveFilePath(
                    "JSON files (*.json)|*.json",
                    ".json");

            if (path is null)
            {
                StorageStatus =
                    "JSON save cancelled.";

                return;
            }

            try
            {
                WorldDto world =
                    CreateWorldDto();

                await jsonStorage.SaveAsync(
                    world,
                    path);

                StorageStatus =
                    $"JSON saved successfully: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                StorageStatus =
                    $"JSON save failed: {ex.Message}";
            }
        }

        private async Task LoadJsonAsync()
        {
            string? path =
                fileDialogService.GetOpenFilePath(
                    "JSON files (*.json)|*.json");

            if (path is null)
            {
                StorageStatus =
                    "JSON load cancelled.";

                return;
            }

            try
            {
                WorldDto world =
                    await jsonStorage.LoadAsync(path);

                ApplyWorldDto(world);

                StorageStatus =
                    $"JSON loaded successfully: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                StorageStatus =
                    $"JSON load failed: {ex.Message}";
            }
        }

        private async Task SaveXmlAsync()
        {
            string? path =
                fileDialogService.GetSaveFilePath(
                    "XML files (*.xml)|*.xml",
                    ".xml");

            if (path is null)
            {
                StorageStatus =
                    "XML save cancelled.";

                return;
            }

            try
            {
                WorldDto world =
                    CreateWorldDto();

                await xmlStorage.SaveAsync(
                    world,
                    path);

                StorageStatus =
                    $"XML saved successfully: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                StorageStatus =
                    $"XML save failed: {ex.Message}";
            }
        }

        private async Task LoadXmlAsync()
        {
            string? path =
                fileDialogService.GetOpenFilePath(
                    "XML files (*.xml)|*.xml");

            if (path is null)
            {
                StorageStatus =
                    "XML load cancelled.";

                return;
            }

            try
            {
                WorldDto world =
                    await xmlStorage.LoadAsync(path);

                ApplyWorldDto(world);

                StorageStatus =
                    $"XML loaded successfully: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                StorageStatus =
                    $"XML load failed: {ex.Message}";
            }
        }

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