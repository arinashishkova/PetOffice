using PetOffice.Core.Models;

namespace PetOffice.WpfApp.ViewModels
{
    public class AnalysisPetItem : ViewModelBase
    {
        private bool isSelected;

        public OfficePet Pet { get; }

        public bool IsSelected
        {
            get => isSelected;

            set
            {
                if (isSelected == value)
                {
                    return;
                }

                isSelected = value;
                OnPropertyChanged();
            }
        }

        public string DisplayName =>
            Pet.DisplayName;

        public AnalysisPetItem(OfficePet pet)
        {
            Pet = pet;
        }
    }
}