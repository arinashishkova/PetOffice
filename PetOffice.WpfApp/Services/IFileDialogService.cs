namespace PetOffice.WpfApp.Services
{
    public interface IFileDialogService
    {
        string? GetSaveFilePath(
            string filter,
            string defaultExtension);

        string? GetOpenFilePath(
            string filter);
    }
}