using Microsoft.Win32;

namespace PetOffice.WpfApp.Services
{
    public class WpfFileDialogService
        : IFileDialogService
    {
        public string? GetSaveFilePath(
            string filter,
            string defaultExtension)
        {
            SaveFileDialog dialog = new()
            {
                Filter = filter,
                DefaultExt = defaultExtension,
                AddExtension = true,
                OverwritePrompt = true
            };

            bool? result =
                dialog.ShowDialog();

            return result == true
                ? dialog.FileName
                : null;
        }

        public string? GetOpenFilePath(
            string filter)
        {
            OpenFileDialog dialog = new()
            {
                Filter = filter,
                CheckFileExists = true,
                Multiselect = false
            };

            bool? result =
                dialog.ShowDialog();

            return result == true
                ? dialog.FileName
                : null;
        }
    }
}