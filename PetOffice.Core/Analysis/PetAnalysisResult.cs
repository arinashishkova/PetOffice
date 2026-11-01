namespace PetOffice.Core.Analysis
{
    public enum PetAnalysisStatus
    {
        Success,
        Error,
        Cancelled
    }

    public class PetAnalysisResult
    {
        public string PetName { get; }
        public string PetType { get; }
        public PetAnalysisStatus Status { get; }
        public string ResultText { get; }
        public TimeSpan Duration { get; }

        public PetAnalysisResult(
            string petName,
            string petType,
            PetAnalysisStatus status,
            string resultText,
            TimeSpan duration)
        {
            PetName = petName;
            PetType = petType;
            Status = status;
            ResultText = resultText;
            Duration = duration;
        }
    }
}