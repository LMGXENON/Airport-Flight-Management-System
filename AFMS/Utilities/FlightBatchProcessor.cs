using AFMS.Models;
using System.Collections.Generic;

namespace AFMS.Utilities
{
    public class FlightBatchProcessor
    {
        private readonly ILogger<FlightBatchProcessor> _logger;

        public FlightBatchProcessor(ILogger<FlightBatchProcessor> logger)
        {
            _logger = logger;
        }

        public List<List<Flight>> BatchFlights(List<Flight> flights, int batchSize)
        {
            try
            {
                _logger.LogInformation("Processing {Count} flights in batches of {BatchSize}", flights.Count, batchSize);
                var batches = new List<List<Flight>>();

                for (int i = 0; i < flights.Count; i += batchSize)
                {
                    var batch = flights.Skip(i).Take(batchSize).ToList();
                    batches.Add(batch);
                }

                _logger.LogInformation("Created {BatchCount} batches", batches.Count);
                return batches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing flights in batches");
                throw;
            }
        }

        public async Task ProcessFlightsInBatchesAsync(List<Flight> flights, int batchSize, Func<List<Flight>, Task> processBatch)
        {
            try
            {
                _logger.LogInformation("Processing {Count} flights in batches of {BatchSize}", flights.Count, batchSize);
                var batches = BatchFlights(flights, batchSize);

                foreach (var batch in batches)
                {
                    await processBatch(batch);
                }

                _logger.LogInformation("Batch processing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing flights in batches");
                throw;
            }
        }
    }
}
