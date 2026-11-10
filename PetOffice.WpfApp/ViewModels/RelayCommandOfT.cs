using System.Windows.Input;

namespace PetOffice.WpfApp.ViewModels
{
    public class RelayCommand<T> : ICommand
        where T : class
    {
        private readonly Action<T?> execute;
        private readonly Func<T?, bool>? canExecute;

        public RelayCommand(
            Action<T?> execute,
            Func<T?, bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke(parameter as T) ?? true;
        }

        public void Execute(object? parameter)
        {
            execute(parameter as T);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(
                this,
                EventArgs.Empty);
        }
    }
}