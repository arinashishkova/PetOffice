using System.Collections.ObjectModel;
using PetOffice.Core.Interfaces;
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
        private readonly Repository<RabbitAccountant> rabbitRepository = new();
        private readonly Repository<RaccoonBarista> raccoonRepository = new();
        private readonly Repository<OwlSysAdmin> owlRepository = new();
        private string selectedRepository = "CatManager";
        private OfficePet? selectedPet;
        private string newPetName = "";
        private string searchText = "";
        private string selectedSortOption = "Name A-Z";

        public ObservableCollection<string> RepositoryNames { get; }
            = new()
            {
                "CatManager",
                "DogIntern",
                "HamsterIT",
                "ParrotReceptionist",
                "RabbitAccountant",
                "RaccoonBarista",
                "OwlSysAdmin"
            };

        public ObservableCollection<string> SortOptions { get; }
            = new()
            {
                "Name A-Z",
                "Name Z-A",
                "Energy high to low",
                "Energy low to high"
            };

        // pets currently shown in ui
        public ObservableCollection<OfficePet> VisiblePets { get; }
            = new();

        public ObservableCollection<string> Log { get; }
            = new();

        // groupby result
        public ObservableCollection<string> Summary { get; }
            = new();

        public RelayCommand AddCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand WorkCommand { get; }
        public RelayCommand CrazyCommand { get; }
        public RelayCommand CommunicateCommand { get; }
        public RelayCommand CoffeeCommand { get; }
        public RelayCommand FixCommand { get; }


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

                UpdateSelectedPetState();
                UpdateCommandStates();
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

        public string SearchText
        {
            get => searchText;

            set
            {
                if (searchText == value)
                {
                    return;
                }

                searchText = value;

                OnPropertyChanged();

                RefreshVisiblePets();
            }
        }

        public string SelectedSortOption
        {
            get => selectedSortOption;

            set
            {
                if (selectedSortOption == value)
                {
                    return;
                }

                selectedSortOption = value;

                OnPropertyChanged();

                RefreshVisiblePets();
            }
        }

        // selected pet state
        public string SelectedPetDisplayName =>
            SelectedPet?.DisplayName ?? "No pet selected";

        public int SelectedPetEnergy =>
            SelectedPet?.Energy ?? 0;

        public string SelectedPetEnergyText =>
            $"{SelectedPet?.Energy ?? 0} / 100";

        public string SelectedPetTasksText =>
            $"Tasks completed: {SelectedPet?.TasksCompleted ?? 0}";

        public string SelectedPetReactionsText =>
            $"Reactions: {SelectedPet?.ReactionsCount ?? 0}";

        public MainViewModel()
        {
            AddCommand = new RelayCommand(
                AddPet,
                CanAddPet);

            RemoveCommand = new RelayCommand(
                RemoveSelectedPet,
                () => SelectedPet is not null);

            WorkCommand = new RelayCommand(
                Work,
                () => SelectedPet is not null);

            CrazyCommand = new RelayCommand(
                PerformCrazyAction,
                () => SelectedPet is not null);

            CommunicateCommand = new RelayCommand(
                Communicate,
                () => SelectedPet is ICommunicate);

            CoffeeCommand = new RelayCommand(
                MakeCoffee,
                () => SelectedPet is IMakeCoffee);

            FixCommand = new RelayCommand(
                FixComputer,
                () => SelectedPet is ITechSupport);

            // listen for reaction messages
            catRepository.ReactionOccurred += Repository_ReactionOccurred;
            dogRepository.ReactionOccurred += Repository_ReactionOccurred;
            hamsterRepository.ReactionOccurred += Repository_ReactionOccurred;
            parrotRepository.ReactionOccurred += Repository_ReactionOccurred;
            rabbitRepository.ReactionOccurred += Repository_ReactionOccurred;
            raccoonRepository.ReactionOccurred += Repository_ReactionOccurred;
            owlRepository.ReactionOccurred += Repository_ReactionOccurred;

            rabbitRepository.Add(new RabbitAccountant("Bunny"));
            raccoonRepository.Add(new RaccoonBarista("Rocky"));
            owlRepository.Add(new OwlSysAdmin("Hoot"));

            RefreshVisiblePets();
            RefreshSummary();
        }

        private void Repository_ReactionOccurred(
            object? sender,
            ReactionEventArgs e)
        {
            Log.Insert(0, e.Message);

            UpdateSelectedPetState();
        }

        private bool CanAddPet()
        {
            return !string.IsNullOrWhiteSpace(NewPetName);
        }

        private void AddPet()
        {
            try
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
                    case "RabbitAccountant":
                        RabbitAccountant rabbit =
                            new RabbitAccountant(NewPetName);

                        rabbitRepository.Add(rabbit);
                        pet = rabbit;
                        break;

                    case "RaccoonBarista":
                        RaccoonBarista raccoon =
                            new RaccoonBarista(NewPetName);

                        raccoonRepository.Add(raccoon);
                        pet = raccoon;
                        break;

                    case "OwlSysAdmin":
                        OwlSysAdmin owl =
                            new OwlSysAdmin(NewPetName);

                        owlRepository.Add(owl);
                        pet = owl;
                        break;

                    default:
                        return;
                }

                Log.Insert(
                    0,
                    $"Added: {pet.DisplayName}");

                RefreshVisiblePets();
                RefreshSummary();

                SelectedPet = pet;

                NewPetName = "";
            }
            catch (ArgumentException ex)
            {
                Log.Insert(0, ex.Message);
            }
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

                RabbitAccountant rabbit =>
                    rabbitRepository.Remove(rabbit),

                RaccoonBarista raccoon =>
                    raccoonRepository.Remove(raccoon),

                OwlSysAdmin owl =>
                    owlRepository.Remove(owl),

                _ => false
            };

            if (!removed)
            {
                return;
            }

            Log.Insert(
                0,
                $"Removed: {pet.DisplayName}");

            SelectedPet = null;

            RefreshVisiblePets();
            RefreshSummary();
        }

        private void Work()
        {
            if (SelectedPet is null)
            {
                return;
            }

            OfficePet pet = SelectedPet;

            try
            {
                string message = pet.Work();

                Log.Insert(0, message);

                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void PerformCrazyAction()
        {
            if (SelectedPet is null)
            {
                return;
            }

            OfficePet pet = SelectedPet;

            try
            {
                string message = pet switch
                {
                    CatManager cat =>
                        catRepository.PerformCrazyAction(cat),

                    DogIntern dog =>
                        dogRepository.PerformCrazyAction(dog),

                    HamsterIT hamster =>
                        hamsterRepository.PerformCrazyAction(hamster),

                    ParrotReceptionist parrot =>
                        parrotRepository.PerformCrazyAction(parrot),

                    RabbitAccountant rabbit =>
                        rabbitRepository.PerformCrazyAction(rabbit),

                    RaccoonBarista raccoon =>
                        raccoonRepository.PerformCrazyAction(raccoon),

                    OwlSysAdmin owl =>
                        owlRepository.PerformCrazyAction(owl),

                    _ => ""
                };

                if (!string.IsNullOrWhiteSpace(message))
                {
                    Log.Insert(0, message);
                }

                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void Communicate()
        {
            if (SelectedPet is not ICommunicate communicator)
            {
                return;
            }

            OfficePet pet = SelectedPet!;

            try
            {
                string message =
                    communicator.Communicate();

                Log.Insert(0, message);

                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void MakeCoffee()
        {
            if (SelectedPet is not IMakeCoffee coffeeMaker)
            {
                return;
            }

            OfficePet pet = SelectedPet!;

            try
            {
                string message =
                    coffeeMaker.MakeCoffee();

                Log.Insert(0, message);

                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void FixComputer()
        {
            if (SelectedPet is not ITechSupport techSupport)
            {
                return;
            }

            OfficePet pet = SelectedPet!;

            try
            {
                string message =
                    techSupport.FixComputer();

                Log.Insert(0, message);

                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void RefreshAfterAction(OfficePet pet)
        {
            // refresh sorting after state changes
            RefreshVisiblePets();

            SelectedPet = pet;

            UpdateSelectedPetState();
            RefreshSummary();
        }

        private void RefreshVisiblePets()
        {
            VisiblePets.Clear();

            IEnumerable<OfficePet> pets =
                GetAllPets();

            // where
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search =
                    SearchText.Trim();

                pets = pets.Where(
                    pet =>
                        pet.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase));
            }

            // orderby
            pets = SelectedSortOption switch
            {
                "Name Z-A" =>
                    pets.OrderByDescending(
                        pet => pet.Name),

                "Energy high to low" =>
                    pets.OrderByDescending(
                        pet => pet.Energy),

                "Energy low to high" =>
                    pets.OrderBy(
                        pet => pet.Energy),

                _ =>
                    pets.OrderBy(
                        pet => pet.Name)
            };

            foreach (OfficePet pet in pets)
            {
                VisiblePets.Add(pet);
            }
        }

        private IEnumerable<OfficePet> GetAllPets()
        {
            return catRepository
                .GetAll()
                .Cast<OfficePet>()

                .Concat(
                    dogRepository.GetAll())

                .Concat(
                    hamsterRepository.GetAll())

                .Concat(
                    parrotRepository.GetAll())

                 .Concat(
                    parrotRepository.GetAll())

                .Concat(
                    rabbitRepository.GetAll())

                .Concat(
                    raccoonRepository.GetAll())

                .Concat(
                    owlRepository.GetAll());
        }
        
        private void RefreshSummary()
        {
            Summary.Clear();

            // groupby
            var groups = GetAllPets()
                .GroupBy(
                    pet => pet.GetType().Name)
                .OrderBy(
                    group => group.Key);

            foreach (var group in groups)
            {
                Summary.Add(
                    $"{group.Key}: {group.Count()}");
            }

            if (Summary.Count == 0)
            {
                Summary.Add("No pets");
            }
        }

        private void UpdateSelectedPetState()
        {
            OnPropertyChanged(
                nameof(SelectedPetDisplayName));

            OnPropertyChanged(
                nameof(SelectedPetEnergy));

            OnPropertyChanged(
                nameof(SelectedPetEnergyText));

            OnPropertyChanged(
                nameof(SelectedPetTasksText));

            OnPropertyChanged(
                nameof(SelectedPetReactionsText));
        }

        private void UpdateCommandStates()
        {
            RemoveCommand.RaiseCanExecuteChanged();
            WorkCommand.RaiseCanExecuteChanged();
            CrazyCommand.RaiseCanExecuteChanged();
            CommunicateCommand.RaiseCanExecuteChanged();
            CoffeeCommand.RaiseCanExecuteChanged();
            FixCommand.RaiseCanExecuteChanged();
        }
    }
}