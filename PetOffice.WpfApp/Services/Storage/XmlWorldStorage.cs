using PetOffice.Core.Persistence;
using PetOffice.Core.Persistence.Dto;
using System.IO;
using System.Xml.Linq;
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
                throw new ArgumentException("File path is required.", nameof(path));

            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                using FileStream stream = File.Create(path);
                serializer.Serialize(stream, world);
            }, token);
        }

        public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
        {
            // Validate the file path before starting the operation.
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("File path is required.", nameof(path));

            // Run synchronous XML operations outside the UI thread.
            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                using FileStream stream = File.OpenRead(path);
                XDocument document = XDocument.Load(stream);

                // Distinguish an invalid file from a valid empty world.
                // The Pets element must exist, even if it contains no pets.
                if (document.Root?.Element(nameof(WorldDto.Pets)) is null)
                    throw new InvalidDataException("The XML file is missing the required Pets element.");

                token.ThrowIfCancellationRequested();

                // Deserialize the validated XML structure into a DTO.
                using var reader = document.CreateReader();
                if (serializer.Deserialize(reader) is not WorldDto world)
                    throw new InvalidDataException("The XML file does not contain a valid world.");

                return world;
            }, token);
        }
    }
}