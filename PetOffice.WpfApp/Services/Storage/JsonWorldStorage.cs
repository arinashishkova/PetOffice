using PetOffice.Core.Persistence;
using PetOffice.Core.Persistence.Dto;
using System.IO;
using System.Text.Json;

namespace PetOffice.WpfApp.Services.Storage
{
    public class JsonWorldStorage : IWorldStorage
    {
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("File path is required.", nameof(path));
            }

            string? directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream stream = File.Create(path);

            await JsonSerializer.SerializeAsync(stream, world, options, token);
        }

        public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("File path is required.", nameof(path));
            }

            await using FileStream stream = File.OpenRead(path);

            WorldDto? world = await JsonSerializer.DeserializeAsync<WorldDto>(stream, options, token);

            if (world is null)
            {
                throw new InvalidDataException("The JSON file does not contain a valid world.");
            }

            return world;
        }
    }
}