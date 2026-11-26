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

        public string Name =>
            Pet.Name;

        public string Role =>
            Pet switch
            {
                CatManager => "Cat Manager",
                DogIntern => "Dog Intern",
                HamsterIT => "Hamster IT",
                ParrotReceptionist => "Parrot Receptionist",
                RabbitAccountant => "Rabbit Accountant",
                RaccoonBarista => "Raccoon Barista",
                OwlSysAdmin => "Owl SysAdmin",
                _ => Pet.GetType().Name
            };

        public string? IconPath =>
            Pet switch
            {
                CatManager =>
                    "/PetOffice.WpfApp;component/Assets/cat.png",

                DogIntern =>
                    "/PetOffice.WpfApp;component/Assets/dog.png",

                HamsterIT =>
                    "/PetOffice.WpfApp;component/Assets/hamster.png",

                ParrotReceptionist =>
                    "/PetOffice.WpfApp;component/Assets/parrot.png",

                RabbitAccountant =>
                    "/PetOffice.WpfApp;component/Assets/rabbit.png",

                RaccoonBarista =>
                    "/PetOffice.WpfApp;component/Assets/raccoon.png",

                OwlSysAdmin =>
                    "/PetOffice.WpfApp;component/Assets/owl.png",

                _ => null
            };

        public AnalysisPetItem(OfficePet pet)
        {
            Pet = pet;
        }
    }
}