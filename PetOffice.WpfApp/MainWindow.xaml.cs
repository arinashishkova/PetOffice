using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using PetOffice.Core.Models;
using PetOffice.Core.Interfaces;

namespace PetOffice.WpfApp
{
    public partial class MainWindow : Window
    {
        // Shared collection for all office pets
        private ObservableCollection<OfficePet> pets =
            new ObservableCollection<OfficePet>();

        public MainWindow()
        {
            InitializeComponent();

            // Pet types available in the application
            PetTypeComboBox.Items.Add("CatManager");
            PetTypeComboBox.Items.Add("DogIntern");
            PetTypeComboBox.Items.Add("HamsterIT");

            PetTypeComboBox.SelectedIndex = 0;

            // Show the shared collection in the ListBox
            PetsListBox.ItemsSource = pets;

            UpdateSelectedPetInfo();
            UpdateAbilityButtons();
        }

        // Adds a new office pet
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = NameTextBox.Text;

                OfficePet pet;

                switch (PetTypeComboBox.SelectedItem?.ToString())
                {
                    case "CatManager":
                        pet = new CatManager(name);
                        break;

                    case "DogIntern":
                        pet = new DogIntern(name);
                        break;

                    case "HamsterIT":
                        pet = new HamsterIT(name);
                        break;

                    default:
                        MessageBox.Show("Please select a pet type.");
                        return;
                }

                pets.Add(pet);

                LogListBox.Items.Add(
                    $"Added: {pet.DisplayName}");

                NameTextBox.Clear();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Runs when the selected pet changes
        private void PetsListBox_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateSelectedPetInfo();
            UpdateAbilityButtons();
        }

        // Shows information about the selected pet
        private void UpdateSelectedPetInfo()
        {
            if (PetsListBox.SelectedItem is not OfficePet pet)
            {
                SelectedPetTextBlock.Text = "No pet selected";

                EnergyProgressBar.Value = 0;
                EnergyTextBlock.Text = "0 / 100";
                TasksTextBlock.Text = "Tasks completed: 0";

                return;
            }

            SelectedPetTextBlock.Text = pet.DisplayName;

            EnergyProgressBar.Value = pet.Energy;
            EnergyTextBlock.Text = $"{pet.Energy} / 100";

            TasksTextBlock.Text =
                $"Tasks completed: {pet.TasksCompleted}";
        }

        // Runs the normal work action
        private void WorkButton_Click(object sender, RoutedEventArgs e)
        {
            if (PetsListBox.SelectedItem is not OfficePet pet)
            {
                MessageBox.Show("Please select a pet.");
                return;
            }

            try
            {
                string message = pet.Work();

                LogListBox.Items.Add(message);
                UpdateSelectedPetInfo();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        // Runs the special action of the selected pet
        private void CrazyButton_Click(object sender, RoutedEventArgs e)
        {
            if (PetsListBox.SelectedItem is not OfficePet pet)
            {
                MessageBox.Show("Please select a pet.");
                return;
            }

            try
            {
                string message = pet.CrazyAction();

                LogListBox.Items.Add(message);
                UpdateSelectedPetInfo();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CommunicateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (PetsListBox.SelectedItem is not ICommunicate communicator)
            {
                MessageBox.Show("This pet cannot communicate.");
                return;
            }

            try
            {
                string message = communicator.Communicate();

                LogListBox.Items.Add(message);
                UpdateSelectedPetInfo();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Enables buttons depending on the selected pet's abilities
        private void UpdateAbilityButtons()
        {
            OfficePet? pet = PetsListBox.SelectedItem as OfficePet;

            RemoveButton.IsEnabled = pet is not null;

            CommunicateButton.IsEnabled =
                pet is ICommunicate;

            CoffeeButton.IsEnabled =
                pet is IMakeCoffee;

            FixButton.IsEnabled =
                pet is ITechSupport;
            WorkButton.IsEnabled =
                pet is not null;

            CrazyButton.IsEnabled =
                pet is not null;
        }
        private void CoffeeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (PetsListBox.SelectedItem is not IMakeCoffee coffeeMaker)
            {
                MessageBox.Show("This pet cannot make coffee.");
                return;
            }

            try
            {
                string message = coffeeMaker.MakeCoffee();

                LogListBox.Items.Add(message);
                UpdateSelectedPetInfo();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FixButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (PetsListBox.SelectedItem is not ITechSupport techSupport)
            {
                MessageBox.Show("This pet cannot fix computers.");
                return;
            }

            try
            {
                string message = techSupport.FixComputer();

                LogListBox.Items.Add(message);
                UpdateSelectedPetInfo();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Removes the selected pet from the collection
        private void RemoveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (PetsListBox.SelectedItem is not OfficePet pet)
            {
                MessageBox.Show("Please select a pet.");
                return;
            }

            pets.Remove(pet);

            LogListBox.Items.Add(
                $"Removed: {pet.DisplayName}");

            UpdateSelectedPetInfo();
            UpdateAbilityButtons();
        }

    }
}