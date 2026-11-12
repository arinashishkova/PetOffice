using PetOffice.Core.Persistence;
using PetOffice.Core.Persistence.Dto;
using System.IO;
using System.Xml.Serialization;

namespace PetOffice.WpfApp.Services.Storage
{
    public class XmlWorldStorage : IWorldStorage
    {
        private readonly XmlSerializer serializer = new(typeof(WorldDto));

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

            await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                using FileStream stream = File.Create(path);
                serializer.Serialize(stream, world);
            }, token);
        }

        public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("File path is required.", nameof(path));
            }

            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                using FileStream stream = File.OpenRead(path);
                object? result = serializer.Deserialize(stream);

                if (result is not WorldDto world)
                {
                    throw new InvalidDataException("The XML file does not contain a valid world.");
                }

                return world;
            }, token);
        }
    }
}