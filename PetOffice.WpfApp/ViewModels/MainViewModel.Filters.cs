using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using PetOffice.Core.Models;

namespace PetOffice.WpfApp.ViewModels
{
    public class PetGroupFilterItem
    {
        public PetGroupFilterItem(
            string typeName, string displayName,
            string iconPath, int count, bool isSelected)
        {
            TypeName = typeName;
            DisplayName = displayName;
            IconPath = iconPath;
            Count = count;
            IsSelected = isSelected;
        }

        public string TypeName { get; }
        public string DisplayName { get; }
        public string IconPath { get; }
        public int Count { get; }
        public bool IsSelected { get; }
    }

    public sealed record OfficeGroupFilterItem(
        string Name,
        string IconPath,
        int Count,
        bool IsSelected);


    public partial class MainViewModel
    {
        private readonly ObservableCollection<PetGroupFilterItem> groupFilters = new();
        private readonly HashSet<string> selectedGroupFilters = new();

        private readonly ObservableCollection<OfficeGroupFilterItem> officeGroupFilters = new();
        private readonly HashSet<string> selectedOfficeGroupFilters = new();

        private RelayCommand<PetGroupFilterItem>? toggleGroupFilterCommand;
        private RelayCommand<OfficeGroupFilterItem>? toggleOfficeGroupFilterCommand;

        private ICollectionView? filteredPetsView;
        private bool petFiltersInitialized;


        public ObservableCollection<PetGroupFilterItem> GroupFilters
        {
            get
            {
                EnsurePetFiltersInitialized();
                return groupFilters;
            }
        }

        public ObservableCollection<OfficeGroupFilterItem> OfficeGroupFilters
        {
            get
            {
                EnsurePetFiltersInitialized();
                return officeGroupFilters;
            }
        }

        public ICollectionView FilteredPetsView
        {
            get
            {
                EnsurePetFiltersInitialized();
                return filteredPetsView!;
            }
        }

        public int TotalPetsCount => GetAllPets().Count();

        public RelayCommand<PetGroupFilterItem> ToggleGroupFilterCommand =>
            toggleGroupFilterCommand ??= new RelayCommand<PetGroupFilterItem>(ToggleGroupFilter);

        public RelayCommand<OfficeGroupFilterItem> ToggleOfficeGroupFilterCommand =>
            toggleOfficeGroupFilterCommand ??= new RelayCommand<OfficeGroupFilterItem>(ToggleOfficeGroupFilter);


        private void EnsurePetFiltersInitialized()
        {
            if (petFiltersInitialized) return;
            petFiltersInitialized = true;

            filteredPetsView = CollectionViewSource.GetDefaultView(VisiblePets);
            filteredPetsView.Filter = PetMatchesFilters;
            VisiblePets.CollectionChanged += VisiblePets_CollectionChanged;

            RefreshGroupFilters();
            RefreshOfficeGroupFilters();
        }


        private bool PetMatchesFilters(object item)
        {
            if (item is not OfficePet pet) return false;

            bool matchesType =
                selectedGroupFilters.Count == 0 ||
                selectedGroupFilters.Contains(pet.GetType().Name);

            bool matchesOfficeGroup =
                selectedOfficeGroupFilters.Count == 0 ||
                selectedOfficeGroupFilters.Contains(pet.GroupName);

            return matchesType && matchesOfficeGroup;
        }


        private void ToggleGroupFilter(PetGroupFilterItem? item)
        {
            if (item is null) return;

            if (!selectedGroupFilters.Add(item.TypeName))
            {
                selectedGroupFilters.Remove(item.TypeName);
            }

            SelectedPet = null;

            RefreshGroupFilters();
            filteredPetsView?.Refresh();
        }

        private void ToggleOfficeGroupFilter(OfficeGroupFilterItem? item)
        {
            if (item is null) return;

            if (!selectedOfficeGroupFilters.Add(item.Name))
            {
                selectedOfficeGroupFilters.Remove(item.Name);
            }

            SelectedPet = null;

            RefreshOfficeGroupFilters();
            filteredPetsView?.Refresh();
        }


        private void VisiblePets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshGroupFilters();
            RefreshOfficeGroupFilters();
        }


        private void RefreshGroupFilters()
        {
            Dictionary<string, int> counts = GetAllPets()
                .GroupBy(pet => pet.GetType().Name)
                .ToDictionary(group => group.Key, group => group.Count());

            selectedGroupFilters.RemoveWhere(typeName => !counts.ContainsKey(typeName));

            var definitions = new[]
            {
                new { TypeName = nameof(CatManager), DisplayName = "Cats", IconPath = "/PetOffice.WpfApp;component/Assets/cat.png" },
                new { TypeName = nameof(DogIntern), DisplayName = "Dogs", IconPath = "/PetOffice.WpfApp;component/Assets/dog.png" },
                new { TypeName = nameof(HamsterIT), DisplayName = "Hamsters", IconPath = "/PetOffice.WpfApp;component/Assets/hamster.png" },
                new { TypeName = nameof(ParrotReceptionist), DisplayName = "Parrots", IconPath = "/PetOffice.WpfApp;component/Assets/parrot.png" },
                new { TypeName = nameof(RabbitAccountant), DisplayName = "Rabbits", IconPath = "/PetOffice.WpfApp;component/Assets/rabbit.png" },
                new { TypeName = nameof(RaccoonBarista), DisplayName = "Raccoons", IconPath = "/PetOffice.WpfApp;component/Assets/raccoon.png" },
                new { TypeName = nameof(OwlSysAdmin), DisplayName = "Owls", IconPath = "/PetOffice.WpfApp;component/Assets/owl.png" }
            };

            groupFilters.Clear();

            foreach (var definition in definitions)
            {
                if (!counts.TryGetValue(definition.TypeName, out int count)) continue;

                groupFilters.Add(new PetGroupFilterItem(
                    definition.TypeName,
                    definition.DisplayName,
                    definition.IconPath,
                    count,
                    selectedGroupFilters.Contains(definition.TypeName)));
            }

            OnPropertyChanged(nameof(TotalPetsCount));
        }


        private void RefreshOfficeGroupFilters()
        {
            Dictionary<string, int> counts = GetAllPets()
                .GroupBy(pet => pet.GroupName)
                .ToDictionary(group => group.Key, group => group.Count());

            officeGroupFilters.Clear();

            foreach (OfficeGroupOption option in OfficeGroupOptions)
            {
                counts.TryGetValue(option.Name, out int count);

                officeGroupFilters.Add(new OfficeGroupFilterItem(
                    option.Name,
                    option.IconPath,
                    count,
                    selectedOfficeGroupFilters.Contains(option.Name)));
            }
        }
    }
}