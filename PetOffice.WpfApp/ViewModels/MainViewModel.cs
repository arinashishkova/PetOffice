using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using PetOffice.Core.Interfaces;
using PetOffice.Core.Models;
using PetOffice.Core.Persistence;
using PetOffice.Core.Repositories;
using PetOffice.WpfApp.Services;
using PetOffice.WpfApp.Presentation;

namespace PetOffice.WpfApp.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        // Repositories for each pet type
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

        public ObservableCollection<string> RepositoryNames { get; } = new()
        {
            "CatManager", "DogIntern", "HamsterIT", "ParrotReceptionist",
            "RabbitAccountant", "RaccoonBarista", "OwlSysAdmin"
        };

        public ObservableCollection<string> SortOptions { get; } = new()
        {
            "Name A-Z", "Name Z-A", "Energy high to low", "Energy low to high"
        };

        public IReadOnlyList<OfficeGroupOption> OfficeGroupOptions { get; } = new List<OfficeGroupOption>
        {
            new(OfficeGroups.PawformanceHub, AppIconPaths.PawformanceHub),
            new(OfficeGroups.CreativeCorner, AppIconPaths.CreativeCorner),
            new(OfficeGroups.TechDen, AppIconPaths.TechDen)
        };

        // Pets currently shown in UI
        public ObservableCollection<OfficePet> VisiblePets { get; } = new();
        public LogEntryCollection Log { get; } = new();

        // GroupBy result
        public ObservableCollection<string> Summary { get; } = new();

        public RelayCommand AddCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand WorkCommand { get; }
        public RelayCommand CrazyCommand { get; }
        public RelayCommand RestoreEnergyCommand { get; }
        public RelayCommand CommunicateCommand { get; }
        public RelayCommand CoffeeCommand { get; }
        public RelayCommand FixCommand { get; }


        public string SelectedRepository
        {
            get => selectedRepository;
            set
            {
                if (selectedRepository == value) return;
                selectedRepository = value;
                OnPropertyChanged();
            }
        }

        public OfficePet? SelectedPet
        {
            get => selectedPet;
            set
            {
                if (selectedPet == value) return;
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
                if (newPetName == value) return;
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
                if (searchText == value) return;
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
                if (selectedSortOption == value) return;
                selectedSortOption = value;
                OnPropertyChanged();
                RefreshVisiblePets();
            }
        }

        public string? SelectedPetGroup
        {
            get => SelectedPet?.GroupName;
            set
            {
                if (SelectedPet is null || string.IsNullOrWhiteSpace(value) || SelectedPet.GroupName == value) return;

                OfficePet pet = SelectedPet;
                string oldGroup = pet.GroupName;

                pet.ChangeGroup(value);

                OnPropertyChanged();
                Log.Insert(0, $"{pet.DisplayName} moved from {oldGroup} to {value}.", pet);

                RefreshOfficeGroupFilters();
                filteredPetsView?.Refresh();
            }
        }


        // Selected pet state
        public string SelectedPetDisplayName => SelectedPet?.DisplayName ?? "No pet selected";
        public string SelectedPetName => SelectedPet?.Name ?? "No pet selected";

        public string SelectedPetType => SelectedPet switch
        {
            CatManager => "Cat Manager",
            DogIntern => "Dog Intern",
            HamsterIT => "Hamster IT",
            ParrotReceptionist => "Parrot Receptionist",
            RabbitAccountant => "Rabbit Accountant",
            RaccoonBarista => "Raccoon Barista",
            OwlSysAdmin => "Owl SysAdmin",
            _ => ""
        };

        public string SelectedPetIdText => SelectedPet is null ? "" : $"ID: {SelectedPet.Id:D}";

        public string? SelectedPetIconPath => SelectedPet switch
        {
            CatManager => AppIconPaths.Cat,
            DogIntern => AppIconPaths.Dog,
            HamsterIT => AppIconPaths.Hamster,
            ParrotReceptionist => AppIconPaths.Parrot,
            RabbitAccountant => AppIconPaths.Rabbit,
            RaccoonBarista => AppIconPaths.Raccoon,
            OwlSysAdmin => AppIconPaths.Owl,
            _ => null
        };

        public int SelectedPetEnergy => SelectedPet?.Energy ?? 0;
        public string SelectedPetEnergyText => $"{SelectedPet?.Energy ?? 0} / 100";
        public string SelectedPetTasksText => $"Tasks completed: {SelectedPet?.TasksCompleted ?? 0}";
        public string SelectedPetReactionsText => $"Reactions: {SelectedPet?.ReactionsCount ?? 0}";


        public MainViewModel(
            [FromKeyedServices("json")] IWorldStorage jsonStorage,
            [FromKeyedServices("xml")] IWorldStorage xmlStorage,
            [FromKeyedServices("localdb")] IWorldStorage localDbStorage,
            IFileDialogService fileDialogService)
        {
            this.jsonStorage = jsonStorage;
            this.xmlStorage = xmlStorage;
            this.localDbStorage = localDbStorage;
            this.fileDialogService = fileDialogService;

            AddCommand = new RelayCommand(AddPet, CanAddPet);
            RemoveCommand = new RelayCommand(RemoveSelectedPet, () => SelectedPet is not null);
            WorkCommand = new RelayCommand(Work, () => SelectedPet is not null);
            CrazyCommand = new RelayCommand(PerformCrazyAction, () => SelectedPet is not null);
            RestoreEnergyCommand = new RelayCommand(RestoreSelectedPetEnergy, () => SelectedPet is not null && SelectedPet.Energy < 100);
            CommunicateCommand = new RelayCommand(Communicate, () => SelectedPet is ICommunicate);
            CoffeeCommand = new RelayCommand(MakeCoffee, () => SelectedPet is IMakeCoffee);
            FixCommand = new RelayCommand(FixComputer, () => SelectedPet is ITechSupport);

            InitializeAnalysis();
            InitializeStorage();

            // Listen for reaction messages
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
            RefreshAnalysisPets();
        }


        private void Repository_ReactionOccurred(object? sender, ReactionEventArgs e)
        {
            Log.InsertReactionGroup(0, e.Source, e.Reactors, e.Kind);
            UpdateSelectedPetState();
        }

        private bool CanAddPet() => !string.IsNullOrWhiteSpace(NewPetName);

        private void AddPet()
        {
            try
            {
                OfficePet pet;
                switch (SelectedRepository)
                {
                    case "CatManager": var cat = new CatManager(NewPetName); catRepository.Add(cat); pet = cat; break;
                    case "DogIntern": var dog = new DogIntern(NewPetName); dogRepository.Add(dog); pet = dog; break;
                    case "HamsterIT": var hamster = new HamsterIT(NewPetName); hamsterRepository.Add(hamster); pet = hamster; break;
                    case "ParrotReceptionist": var parrot = new ParrotReceptionist(NewPetName); parrotRepository.Add(parrot); pet = parrot; break;
                    case "RabbitAccountant": var rabbit = new RabbitAccountant(NewPetName); rabbitRepository.Add(rabbit); pet = rabbit; break;
                    case "RaccoonBarista": var raccoon = new RaccoonBarista(NewPetName); raccoonRepository.Add(raccoon); pet = raccoon; break;
                    case "OwlSysAdmin": var owl = new OwlSysAdmin(NewPetName); owlRepository.Add(owl); pet = owl; break;
                    default: return;
                }

                Log.Insert(0, $"Added: {pet.DisplayName}", pet);
                RefreshVisiblePets();
                RefreshSummary();
                RefreshAnalysisPets();
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
            if (SelectedPet is null) return;

            OfficePet pet = SelectedPet;
            bool removed = pet switch
            {
                CatManager cat => catRepository.Remove(cat),
                DogIntern dog => dogRepository.Remove(dog),
                HamsterIT hamster => hamsterRepository.Remove(hamster),
                ParrotReceptionist parrot => parrotRepository.Remove(parrot),
                RabbitAccountant rabbit => rabbitRepository.Remove(rabbit),
                RaccoonBarista raccoon => raccoonRepository.Remove(raccoon),
                OwlSysAdmin owl => owlRepository.Remove(owl),
                _ => false
            };

            if (!removed) return;

            Log.Insert(0, $"Removed: {pet.DisplayName}", pet);
            SelectedPet = null;
            RefreshVisiblePets();
            RefreshSummary();
            RefreshAnalysisPets();
        }

        private void Work()
        {
            if (SelectedPet is null) return;

            OfficePet pet = SelectedPet;
            try
            {
                string message = pet.Work();
                Log.Insert(0, message, pet);
                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void PerformCrazyAction()
        {
            if (SelectedPet is null) return;

            OfficePet pet = SelectedPet;
            try
            {
                string message = pet switch
                {
                    CatManager cat => catRepository.PerformCrazyAction(cat),
                    DogIntern dog => dogRepository.PerformCrazyAction(dog),
                    HamsterIT hamster => hamsterRepository.PerformCrazyAction(hamster),
                    ParrotReceptionist parrot => parrotRepository.PerformCrazyAction(parrot),
                    RabbitAccountant rabbit => rabbitRepository.PerformCrazyAction(rabbit),
                    RaccoonBarista raccoon => raccoonRepository.PerformCrazyAction(raccoon),
                    OwlSysAdmin owl => owlRepository.PerformCrazyAction(owl),
                    _ => ""
                };

                if (!string.IsNullOrWhiteSpace(message))
                {
                    Log.Insert(0, message, pet);
                }

                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void RestoreSelectedPetEnergy()
        {
            if (SelectedPet is null) return;

            OfficePet pet = SelectedPet;
            string message = pet.RestoreToFullEnergy();
            Log.Insert(0, message, pet);
            RefreshAfterAction(pet);
        }

        private void Communicate()
        {
            if (SelectedPet is not ICommunicate communicator) return;

            OfficePet pet = SelectedPet;
            try
            {
                string message = communicator.Communicate();
                Log.Insert(0, message, pet);
                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void MakeCoffee()
        {
            if (SelectedPet is not IMakeCoffee coffeeMaker) return;

            OfficePet pet = SelectedPet;
            try
            {
                string message = coffeeMaker.MakeCoffee();
                Log.Insert(0, message, pet);
                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void FixComputer()
        {
            if (SelectedPet is not ITechSupport techSupport) return;

            OfficePet pet = SelectedPet;
            try
            {
                string message = techSupport.FixComputer();
                Log.Insert(0, message, pet);
                RefreshAfterAction(pet);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert(0, ex.Message);
            }
        }

        private void RefreshAfterAction(OfficePet pet)
        {
            RefreshVisiblePets();
            SelectedPet = pet;
            UpdateSelectedPetState();
            UpdateCommandStates();
            RefreshSummary();
            RefreshAnalysisPets();
        }

        private void RefreshVisiblePets()
        {
            VisiblePets.Clear();
            IEnumerable<OfficePet> pets = GetAllPets();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                pets = pets.Where(pet => pet.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            pets = SelectedSortOption switch
            {
                "Name Z-A" => pets.OrderByDescending(pet => pet.Name),
                "Energy high to low" => pets.OrderByDescending(pet => pet.Energy),
                "Energy low to high" => pets.OrderBy(pet => pet.Energy),
                _ => pets.OrderBy(pet => pet.Name)
            };

            foreach (OfficePet pet in pets) VisiblePets.Add(pet);
        }

        private IEnumerable<OfficePet> GetAllPets()
        {
            return catRepository.GetAll().Cast<OfficePet>()
                .Concat(dogRepository.GetAll())
                .Concat(hamsterRepository.GetAll())
                .Concat(parrotRepository.GetAll())
                .Concat(rabbitRepository.GetAll())
                .Concat(raccoonRepository.GetAll())
                .Concat(owlRepository.GetAll());
        }

        private void RefreshSummary()
        {
            Summary.Clear();

            var groups = GetAllPets()
                .GroupBy(pet => pet.GetType().Name)
                .OrderBy(group => group.Key);

            foreach (var group in groups) Summary.Add($"{group.Key}: {group.Count()}");

            if (Summary.Count == 0) Summary.Add("No pets");
        }

        private void UpdateSelectedPetState()
        {
            OnPropertyChanged(nameof(SelectedPetDisplayName));
            OnPropertyChanged(nameof(SelectedPetGroup));
            OnPropertyChanged(nameof(SelectedPetIconPath));
            OnPropertyChanged(nameof(SelectedPetEnergy));
            OnPropertyChanged(nameof(SelectedPetEnergyText));
            OnPropertyChanged(nameof(SelectedPetTasksText));
            OnPropertyChanged(nameof(SelectedPetReactionsText));
            OnPropertyChanged(nameof(SelectedPetIdText));
            OnPropertyChanged(nameof(SelectedPetName));
            OnPropertyChanged(nameof(SelectedPetType));
        }

        private void UpdateCommandStates()
        {
            RemoveCommand.RaiseCanExecuteChanged();
            WorkCommand.RaiseCanExecuteChanged();
            CrazyCommand.RaiseCanExecuteChanged();
            RestoreEnergyCommand.RaiseCanExecuteChanged();
            CommunicateCommand.RaiseCanExecuteChanged();
            CoffeeCommand.RaiseCanExecuteChanged();
            FixCommand.RaiseCanExecuteChanged();
        }
    }
}