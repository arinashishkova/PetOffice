namespace PetOffice.Core.Models
{
    public static class OfficeGroups
    {
        public const string PawformanceHub = "Pawformance Hub";
        public const string CreativeCorner = "Creative Corner";
        public const string TechDen = "Tech Den";

        public static IReadOnlyList<string> All { get; } = new[]
        {
            PawformanceHub,
            CreativeCorner,
            TechDen
        };
    }
}