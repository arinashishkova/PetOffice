using System.Windows;
using PetOffice.WpfApp.ViewModels;

namespace PetOffice.WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }
    }
}