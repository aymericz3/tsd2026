using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoldSavings.App.Client;
using GoldSavings.App.Model;

namespace GoldSavings.App.Services
{
    public class GoldDataService
    {
        private readonly GoldClient _goldClient;

        public GoldDataService()
        {
            _goldClient = new GoldClient();
        }

        public async Task<List<GoldPrice>> GetGoldPrices(DateTime startDate, DateTime endDate)
        {
            var prices = await _goldClient.GetGoldPrices(startDate, endDate);
            return prices ?? new List<GoldPrice>();  // Prevent null values
        }

        // The NBP API rejects requests spanning more than 367 days.
        // This method splits a long date range into 366-day chunks and merges the results.
        public async Task<List<GoldPrice>> GetGoldPricesLongRange(DateTime startDate, DateTime endDate)
        {
            var allPrices = new List<GoldPrice>();
            var current = startDate;

            while (current <= endDate)
            {
                // Calculate the end of the current chunk (max 366 days ahead)
                var chunkEnd = current.AddDays(366);

                // If the chunk overshoots the target end date, clamp it
                if (chunkEnd > endDate) chunkEnd = endDate;

                // Fetch prices for this chunk and add them to the full list
                var chunk = await _goldClient.GetGoldPrices(current, chunkEnd);
                if (chunk != null) allPrices.AddRange(chunk);

                // Move to the day after the end of the current chunk
                current = chunkEnd.AddDays(1);
            }

            return allPrices;
        }
    }
}
