namespace PetOffice.Core.Analysis
{
    public class PetAnalysisSnapshot
    {
        public string Name { get; }
        public string Type { get; }
        public int Energy { get; }
        public int TasksCompleted { get; }
        public int ReactionsCount { get; }

        public PetAnalysisSnapshot(
            string name,
            string type,
            int energy,
            int tasksCompleted,
            int reactionsCount)
        {
            Name = name;
            Type = type;
            Energy = energy;
            TasksCompleted = tasksCompleted;
            ReactionsCount = reactionsCount;
        }
    }
}