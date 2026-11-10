using System;
using System.Windows;
using PetOffice.WpfApp.ViewModels;

namespace PetOffice.WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow(
            MainViewModel viewModel)
        {
            InitializeComponent();

            // Do not allow the initial window
            // to grow beyond the usable screen area.
            double availableHeight =
                SystemParameters.WorkArea.Height - 24;

            double availableWidth =
                SystemParameters.WorkArea.Width - 24;

            MaxHeight =
                Math.Max(
                    MinHeight,
                    availableHeight);

            MaxWidth =
                Math.Max(
                    MinWidth,
                    availableWidth);

            if (Width > availableWidth)
            {
                Width =
                    Math.Max(
                        MinWidth,
                        availableWidth);
            }

            DataContext =
                viewModel;
        }


        protected override void OnContentRendered(
            EventArgs e)
        {
            base.OnContentRendered(e);

            // SizeToContent is needed only for startup.
            // After that the user can resize the window normally.
            SizeToContent =
                System.Windows.SizeToContent.Manual;
        }
    }
}