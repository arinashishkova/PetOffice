
using System.Text.Json.Serialization;

namespace PetOffice.Core.Persistence.Dto
{
    public class WorldDto
    {
        [JsonRequired]
        public List<PetDto> Pets { get; set; } = new();
    }
}
