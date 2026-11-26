using System.Collections.ObjectModel;
using System.Diagnostics;
using PetOffice.Core.Analysis;
using PetOffice.Core.Models;

namespace PetOffice.WpfApp.ViewModels
{
    public partial class MainViewModel
    {
        // Analysis state
        private string selectedWorkload = "Medium";
        private CancellationTokenSource? analysisCancellationTokenSource;
        private bool isAnalysisRunning;

        private string sequentialTotalTimeText = "Sequential total: -";
        private string parallelTotalTimeText = "Parallel total: -";
        private string resultsComparisonText = "Results comparison: -";
        private string analysisWarningText = "";

        // Pets selected for CPU analysis
        public ObservableCollection<AnalysisPetItem> AnalysisPets { get; } = new();

        public ObservableCollection<string> WorkloadOptions { get; } = new()
        {
            "Small", "Medium", "Large"
        };

        // Separate reports make the two modes easy to compare
        public ObservableCollection<PetAnalysisResult> SequentialResults { get; } = new();
        public ObservableCollection<PetAnalysisResult> ParallelResults { get; } = new();

        public AsyncCommand RunAnalysisCommand { get; private set; } = null!;
        public RelayCommand CancelAnalysisCommand { get; private set; } = null!;
        public RelayCommand<string> SelectWorkloadCommand { get; private set; } = null!;

        public string SelectedWorkload
        {
            get => selectedWorkload;
            set
            {
                if (selectedWorkload == value) return;
                selectedWorkload = value;
                OnPropertyChanged();
            }
        }

        public bool IsAnalysisRunning
        {
            get => isAnalysisRunning;
            private set
            {
                if (isAnalysisRunning == value) return;
                isAnalysisRunning = value;
                OnPropertyChanged();

                RunAnalysisCommand.RaiseCanExecuteChanged();
                CancelAnalysisCommand.RaiseCanExecuteChanged();
                SelectWorkloadCommand.RaiseCanExecuteChanged();
            }
        }

        public string SequentialTotalTimeText
        {
            get => sequentialTotalTimeText;
            private set
            {
                if (sequentialTotalTimeText == value) return;
                sequentialTotalTimeText = value;
                OnPropertyChanged();
            }
        }

        public string ParallelTotalTimeText
        {
            get => parallelTotalTimeText;
            private set
            {
                if (parallelTotalTimeText == value) return;
                parallelTotalTimeText = value;
                OnPropertyChanged();
            }
        }

        public string ResultsComparisonText
        {
            get => resultsComparisonText;
            private set
            {
                if (resultsComparisonText == value) return;
                resultsComparisonText = value;
                OnPropertyChanged();
            }
        }

        public string AnalysisWarningText
        {
            get => analysisWarningText;
            private set
            {
                if (analysisWarningText == value) return;
                analysisWarningText = value;
                OnPropertyChanged();
            }
        }

        // Creates the commands used by the Vol.3 analysis section
        private void InitializeAnalysis()
        {
            RunAnalysisCommand = new AsyncCommand(
                RunAnalysisAsync,
                () => !IsAnalysisRunning);

            CancelAnalysisCommand = new RelayCommand(
                CancelAnalysis,
                () => IsAnalysisRunning);

            SelectWorkloadCommand = new RelayCommand<string>(
                workload =>
                {
                    if (!string.IsNullOrWhiteSpace(workload))
                    {
                        SelectedWorkload = workload;
                    }
                },
                workload => !IsAnalysisRunning && !string.IsNullOrWhiteSpace(workload));
        }

        // Keeps the analysis selection list in sync with repository pets
        private void RefreshAnalysisPets()
        {
            AnalysisPets.Clear();

            foreach (OfficePet pet in GetAllPets())
            {
                AnalysisPets.Add(new AnalysisPetItem(pet));
            }
        }

        // Uses one snapshot list for both modes so the input is identical
        private async Task RunAnalysisAsync()
        {
            List<AnalysisPetItem> selectedItems = AnalysisPets
                .Where(item => item.IsSelected)
                .ToList();

            // Show the warning directly in the CPU panel
            if (selectedItems.Count < 3)
            {
                AnalysisWarningText = "Please select at least 3 pets before starting the analysis.";
                ResultsComparisonText = "Results comparison: -";
                Log.Insert(0, "Select at least three pets for analysis.");
                return;
            }

            AnalysisWarningText = "";

            // Snapshot keeps the input unchanged during calculation
            List<PetAnalysisSnapshot> snapshots = selectedItems
                .Select(item => new PetAnalysisSnapshot(
                    item.Pet.Name,
                    item.Pet.GetType().Name,
                    item.Pet.Energy,
                    item.Pet.TasksCompleted,
                    item.Pet.ReactionsCount))
                .ToList();

            int workload = GetWorkloadIterations();

            analysisCancellationTokenSource?.Dispose();
            analysisCancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = analysisCancellationTokenSource.Token;

            IsAnalysisRunning = true;
            SequentialResults.Clear();
            ParallelResults.Clear();

            SequentialTotalTimeText = "Sequential total: running...";
            ParallelTotalTimeText = "Parallel total: waiting...";
            ResultsComparisonText = "Results comparison: waiting...";

            try
            {
                PetPerformanceAnalyzer analyzer = new();

                // Sequential analysis
                List<PetAnalysisResult> sequentialResults = await RunSequentialAsync(
                    snapshots, workload, token, analyzer);

                foreach (PetAnalysisResult result in sequentialResults)
                {
                    SequentialResults.Add(result);
                }

                // If the user cancelled the sequential run, do not start another workload afterwards
                if (token.IsCancellationRequested)
                {
                    ParallelTotalTimeText = "Parallel total: not started";
                    ResultsComparisonText = "Results comparison: cancelled";
                    return;
                }

                // Parallel analysis
                List<PetAnalysisResult> parallelResults = await RunParallelAsync(
                    snapshots, workload, token, analyzer);

                foreach (PetAnalysisResult result in parallelResults)
                {
                    ParallelResults.Add(result);
                }

                ResultsComparisonText = ResultsMatch(sequentialResults, parallelResults)
                    ? "Results comparison: match"
                    : "Results comparison: different";
            }
            catch (OperationCanceledException)
            {
                if (SequentialResults.Count == 0)
                {
                    SequentialTotalTimeText = "Sequential total: cancelled";
                }
                if (ParallelResults.Count == 0)
                {
                    ParallelTotalTimeText = "Parallel total: cancelled";
                }
                ResultsComparisonText = "Results comparison: cancelled";
            }
            finally
            {
                IsAnalysisRunning = false;
                analysisCancellationTokenSource?.Dispose();
                analysisCancellationTokenSource = null;
            }
        }

        // One pet finishes before the next one starts
        private async Task<List<PetAnalysisResult>> RunSequentialAsync(
            IReadOnlyList<PetAnalysisSnapshot> snapshots,
            int workload,
            CancellationToken token,
            PetPerformanceAnalyzer analyzer)
        {
            List<PetAnalysisResult> results = new();
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (PetAnalysisSnapshot snapshot in snapshots)
            {
                token.ThrowIfCancellationRequested();

                PetAnalysisResult result = await Task.Run(
                    () => analyzer.Analyze(snapshot, workload, token),
                    token);

                results.Add(result);
            }

            stopwatch.Stop();
            SequentialTotalTimeText = $"Sequential total: {stopwatch.Elapsed.TotalSeconds:F2} s";

            return results;
        }

        // All pet calculations start first and Task.WhenAll waits for all
        private async Task<List<PetAnalysisResult>> RunParallelAsync(
            IReadOnlyList<PetAnalysisSnapshot> snapshots,
            int workload,
            CancellationToken token,
            PetPerformanceAnalyzer analyzer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Task<PetAnalysisResult>[] tasks = snapshots
                .Select(snapshot => Task.Run(
                    () => analyzer.Analyze(snapshot, workload, token),
                    token))
                .ToArray();

            PetAnalysisResult[] results = await Task.WhenAll(tasks);

            stopwatch.Stop();
            ParallelTotalTimeText = $"Parallel total: {stopwatch.Elapsed.TotalSeconds:F2} s";

            return results.ToList();
        }

        // Durations may differ, but statuses and calculated results should match
        private static bool ResultsMatch(
            IReadOnlyList<PetAnalysisResult> sequentialResults,
            IReadOnlyList<PetAnalysisResult> parallelResults)
        {
            if (sequentialResults.Count != parallelResults.Count) return false;

            for (int i = 0; i < sequentialResults.Count; i++)
            {
                PetAnalysisResult sequential = sequentialResults[i];
                PetAnalysisResult parallel = parallelResults[i];

                if (sequential.PetName != parallel.PetName
                    || sequential.PetType != parallel.PetType
                    || sequential.Status != parallel.Status
                    || sequential.ResultText != parallel.ResultText)
                {
                    return false;
                }
            }

            return true;
        }

        private void CancelAnalysis()
        {
            analysisCancellationTokenSource?.Cancel();
        }

        private int GetWorkloadIterations() => SelectedWorkload switch
        {
            "Small" => 1_000_000,
            "Large" => 15_000_000,
            _ => 5_000_000
        };
    }
}