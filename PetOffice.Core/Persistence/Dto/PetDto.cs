namespace PetOffice.Core.Persistence.Dto
{
    public class PetDto
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = "";

        public string Name { get; set; } = "";

        public string GroupName { get; set; } = "";

        public int Energy { get; set; }

        public int TasksCompleted { get; set; }

        public int ReactionsCount { get; set; }

        public string RepositoryName { get; set; } = "";
    }
}