using System.Collections.ObjectModel;
using PetOffice.Core.Models;
using PetOffice.Core.Repositories;

namespace PetOffice.WpfApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // repositories for each pet type
        private readonly Repository<CatManager> catRepository = new();
        private readonly Repository<DogIntern> dogRepository = new();
        private readonly Repository<HamsterIT> hamsterRepository = new();
        private readonly Repository<ParrotReceptionist> parrotRepository = new();

        private string selectedRepository = "CatManager";
        private OfficePet? selectedPet;
        private string newPetName = "";

        public ObservableCollection<string> RepositoryNames { get; }
            = new()
            {
                "CatManager",
                "DogIntern",
                "HamsterIT",
                "ParrotReceptionist"
            };

        // pets currently shown in ui
        public ObservableCollection<OfficePet> VisiblePets { get; }
            = new();

        public ObservableCollection<string> Log { get; }
            = new();

        public RelayCommand AddCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand CrazyCommand { get; }

        public string SelectedRepository
        {
            get => selectedRepository;

            set
            {
                if (selectedRepository == value)
                {
                    return;
                }

                selectedRepository = value;

                OnPropertyChanged();

                SelectedPet = null;

                RefreshVisiblePets();
            }
        }

        public OfficePet? SelectedPet
        {
            get => selectedPet;

            set
            {
                if (selectedPet == value)
                {
                    return;
                }

                selectedPet = value;

                OnPropertyChanged();

                RemoveCommand.RaiseCanExecuteChanged();
                CrazyCommand.RaiseCanExecuteChanged();
            }
        }

        public string NewPetName
        {
            get => newPetName;

            set
            {
                if (newPetName == value)
                {
                    return;
                }

                newPetName = value;

                OnPropertyChanged();

                AddCommand.RaiseCanExecuteChanged();
            }
        }

        public MainViewModel()
        {
            AddCommand = new RelayCommand(
                AddPet,
                CanAddPet);

            RemoveCommand = new RelayCommand(
                RemoveSelectedPet,
                () => SelectedPet is not null);

            CrazyCommand = new RelayCommand(
                PerformCrazyAction,
                () => SelectedPet is not null);

            // listen for reaction messages
            catRepository.ReactionOccurred += Repository_ReactionOccurred;
            dogRepository.ReactionOccurred += Repository_ReactionOccurred;
            hamsterRepository.ReactionOccurred += Repository_ReactionOccurred;
            parrotRepository.ReactionOccurred += Repository_ReactionOccurred;

            RefreshVisiblePets();
        }

        private void Repository_ReactionOccurred(
            object? sender,
            ReactionEventArgs e)
        {
            Log.Insert(0, e.Message);
        }

        private bool CanAddPet()
        {
            return !string.IsNullOrWhiteSpace(NewPetName);
        }

        private void AddPet()
        {
            OfficePet pet;

            switch (SelectedRepository)
            {
                case "CatManager":
                    CatManager cat =
                        new CatManager(NewPetName);

                    catRepository.Add(cat);
                    pet = cat;
                    break;

                case "DogIntern":
                    DogIntern dog =
                        new DogIntern(NewPetName);

                    dogRepository.Add(dog);
                    pet = dog;
                    break;

                case "HamsterIT":
                    HamsterIT hamster =
                        new HamsterIT(NewPetName);

                    hamsterRepository.Add(hamster);
                    pet = hamster;
                    break;

                case "ParrotReceptionist":
                    ParrotReceptionist parrot =
                        new ParrotReceptionist(NewPetName);

                    parrotRepository.Add(parrot);
                    pet = parrot;
                    break;

                default:
                    return;
            }

            Log.Insert(0, $"Added: {pet.DisplayName}");

            RefreshVisiblePets();

            SelectedPet = pet;

            NewPetName = "";
        }

        private void RemoveSelectedPet()
        {
            if (SelectedPet is null)
            {
                return;
            }

            OfficePet pet = SelectedPet;

            bool removed = pet switch
            {
                CatManager cat =>
                    catRepository.Remove(cat),

                DogIntern dog =>
                    dogRepository.Remove(dog),

                HamsterIT hamster =>
                    hamsterRepository.Remove(hamster),

                ParrotReceptionist parrot =>
                    parrotRepository.Remove(parrot),

                _ => false
            };

            if (!removed)
            {
                return;
            }

            Log.Insert(0, $"Removed: {pet.DisplayName}");

            SelectedPet = null;

            RefreshVisiblePets();
        }

        private void PerformCrazyAction()
        {
            if (SelectedPet is null)
            {
                return;
            }

            try
            {
                string message = SelectedPet switch
                {
                    CatManager cat =>
                        catRepository.PerformCrazyAction(cat),

                    DogIntern dog =>
                        dogRepository.PerformCrazyAction(dog),

                    HamsterIT hamster =>
                        hamsterRepository.PerformCrazyAction(hamster),

                    ParrotReceptionist parrot =>
                        parrotRepository.PerformCrazyAction(parrot),

                    _ => ""
                };

                if (!string.IsNullOrWhiteSpace(message))
                {
                    Log.Insert(0, message);
                }

                OnPropertyChanged(nameof(SelectedPet));
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void RefreshVisiblePets()
        {
            VisiblePets.Clear();

            IEnumerable<OfficePet> pets =
                SelectedRepository switch
                {
                    "CatManager" => catRepository.GetAll(),
                    "DogIntern" => dogRepository.GetAll(),
                    "HamsterIT" => hamsterRepository.GetAll(),
                    "ParrotReceptionist" => parrotRepository.GetAll(),
                    _ => Enumerable.Empty<OfficePet>()
                };

            foreach (OfficePet pet in pets)
            {
                VisiblePets.Add(pet);
            }
        }
    }
}