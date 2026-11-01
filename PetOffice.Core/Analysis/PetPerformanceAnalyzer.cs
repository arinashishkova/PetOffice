using System.Diagnostics;

namespace PetOffice.Core.Analysis
{
    public class PetPerformanceAnalyzer
    {
        public PetAnalysisResult Analyze(
            PetAnalysisSnapshot snapshot,
            int workload,
            CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            if (workload <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(workload),
                    "Workload must be greater than 0.");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                if (snapshot.Energy < 20)
                {
                    throw new InvalidOperationException(
                        "Not enough energy for analysis.");
                }

                double total = 0;

                for (int i = 1; i <= workload; i++)
                {
                    token.ThrowIfCancellationRequested();

                    double value =
                        Math.Sqrt(
                            snapshot.Energy * i
                            + snapshot.TasksCompleted
                            + snapshot.ReactionsCount
                            + 1);

                    total +=
                        Math.Sin(value)
                        * Math.Cos(value / 2.0);
                }

                double score =
                    Math.Abs(total / workload) * 100.0;

                stopwatch.Stop();

                return new PetAnalysisResult(
                    snapshot.Name,
                    snapshot.Type,
                    PetAnalysisStatus.Success,
                    $"Productivity score: {score:F2}",
                    stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();

                return new PetAnalysisResult(
                    snapshot.Name,
                    snapshot.Type,
                    PetAnalysisStatus.Cancelled,
                    "Analysis cancelled.",
                    stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                return new PetAnalysisResult(
                    snapshot.Name,
                    snapshot.Type,
                    PetAnalysisStatus.Error,
                    ex.Message,
                    stopwatch.Elapsed);
            }
        }
    }
}