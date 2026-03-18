using System;
using System.Collections.Generic;
using System.Linq;
using GoldSavings.App.Model;

namespace GoldSavings.App.Services
{
    public class GoldAnalysisService
    {
        private readonly List<GoldPrice> _goldPrices;

        public GoldAnalysisService(List<GoldPrice> goldPrices)
        {
            _goldPrices = goldPrices;
        }
        public double GetAveragePrice()
        {
            return _goldPrices.Average(p => p.Price);
        }

        // 2a: TOP 3 highest/lowest prices — method syntax
        public List<GoldPrice> GetTop3HighestPricesMethodSyntax() =>
            _goldPrices.OrderByDescending(p => p.Price).Take(3).ToList();

        public List<GoldPrice> GetTop3LowestPricesMethodSyntax() =>
            _goldPrices.OrderBy(p => p.Price).Take(3).ToList();

        // 2a: TOP 3 highest/lowest prices — query syntax
        public List<GoldPrice> GetTop3HighestPricesQuerySyntax() =>
            (from p in _goldPrices orderby p.Price descending select p).Take(3).ToList();

        public List<GoldPrice> GetTop3LowestPricesQuerySyntax() =>
            (from p in _goldPrices orderby p.Price select p).Take(3).ToList();

        // 2b: Days with >5% gain from first available price in January 2020
        public List<GoldPrice> GetDaysWithMoreThan5PercentGainFromJan2020()
        {
            var buyPrice = _goldPrices
                .Where(p => p.Date.Year == 2020 && p.Date.Month == 1)
                .OrderBy(p => p.Date)
                .FirstOrDefault();

            if (buyPrice == null) return new List<GoldPrice>();

            double target = buyPrice.Price * 1.05;
            return _goldPrices
                .Where(p => p.Date > buyPrice.Date && p.Price > target)
                .OrderBy(p => p.Date)
                .ToList();
        }

        // 2c: Ranks 11-13 (second ten) in the 2019-2022 price ranking
        public List<GoldPrice> GetSecondTenOpenerDates()
        {
            return _goldPrices
                .Where(p => p.Date.Year >= 2019 && p.Date.Year <= 2022)
                .OrderByDescending(p => p.Price)
                .Skip(10)
                .Take(3)
                .ToList();
        }

        // 2d: Average prices for given years — query syntax
        public List<(int Year, double Average)> GetAveragesByYears(int[] years)
        {
            return (from p in _goldPrices
                    where years.Contains(p.Date.Year)
                    group p by p.Date.Year into g
                    orderby g.Key
                    select (Year: g.Key, Average: g.Average(p => p.Price))).ToList();
        }

        // 2e: Best buy/sell window between 2020 and 2024 (max profit)
        public (GoldPrice Buy, GoldPrice Sell, double ROI) GetBestBuySell()
        {
            var prices = _goldPrices
                .Where(p => p.Date.Year >= 2020 && p.Date.Year <= 2024)
                .OrderBy(p => p.Date)
                .ToList();

            if (prices.Count < 2) return (prices[0], prices[0], 0);

            GoldPrice bestBuy = prices[0], bestSell = prices[1], currentMin = prices[0];
            double bestProfit = 0;

            foreach (var price in prices.Skip(1))
            {
                double profit = price.Price - currentMin.Price;
                if (profit > bestProfit)
                {
                    bestProfit = profit;
                    bestBuy = currentMin;
                    bestSell = price;
                }
                if (price.Price < currentMin.Price)
                    currentMin = price;
            }

            double roi = (bestSell.Price - bestBuy.Price) / bestBuy.Price * 100;
            return (bestBuy, bestSell, roi);
        }
    }
}
