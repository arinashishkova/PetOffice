using PetOffice.Core.Persistence.Dto;

namespace PetOffice.Core.Persistence
{
    public interface IWorldStorage
    {
        Task SaveAsync(
            WorldDto world,
            string path,
            CancellationToken token = default);

        Task<WorldDto> LoadAsync(
            string path,
            CancellationToken token = default);
    }
}