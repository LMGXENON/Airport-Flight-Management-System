namespace AFMS.Utilities
{
    /// <summary>
    /// Performance analysis utility for data structure operations
    /// Measures and logs execution time for search, insert, and delete operations
    /// </summary>
    public class PerformanceAnalyzer
    {
        private readonly ILogger<PerformanceAnalyzer> _logger;
        private Dictionary<string, List<long>> _timings;

        public PerformanceAnalyzer(ILogger<PerformanceAnalyzer> logger)
        {
            _logger = logger;
            _timings = new Dictionary<string, List<long>>();
        }

        /// <summary>
        /// Measures the time taken to execute an operation
        /// </summary>
        public long MeasureOperation(string operationName, Action operation)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            operation();
            stopwatch.Stop();

            if (!_timings.ContainsKey(operationName))
                _timings[operationName] = new List<long>();

            _timings[operationName].Add(stopwatch.ElapsedMilliseconds);
            _logger.LogInformation($"{operationName} completed in {stopwatch.ElapsedMilliseconds}ms");

            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Gets statistics for an operation
        /// </summary>
        public OperationStatistics GetStatistics(string operationName)
        {
            if (!_timings.ContainsKey(operationName) || _timings[operationName].Count == 0)
                return null;

            var times = _timings[operationName];
            return new OperationStatistics
            {
                OperationName = operationName,
                AverageMs = times.Average(),
                MinMs = times.Min(),
                MaxMs = times.Max(),
                Count = times.Count
            };
        }

        /// <summary>
        /// Generates a performance report
        /// </summary>
        public string GenerateReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Performance Analysis Report ===");

            foreach (var operation in _timings.Keys)
            {
                var stats = GetStatistics(operation);
                sb.AppendLine($"\nOperation: {stats.OperationName}");
                sb.AppendLine($"  Count: {stats.Count}");
                sb.AppendLine($"  Average: {stats.AverageMs:F2}ms");
                sb.AppendLine($"  Min: {stats.MinMs}ms");
                sb.AppendLine($"  Max: {stats.MaxMs}ms");
            }

            return sb.ToString();
        }
    }

    public class OperationStatistics
    {
        public string OperationName { get; set; }
        public double AverageMs { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
        public int Count { get; set; }
    }
}
