using GoldSavings.App.Model;
using GoldSavings.App.Client;
using GoldSavings.App.Services;
namespace GoldSavings.App;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, Gold Investor!");

        GoldDataService dataService = new GoldDataService();
        DateTime today = DateTime.Now;

        // Originally: startDate = new DateTime(2024, 09, 18) and endDate = DateTime.Now (hardcoded start date).
        // Changed to today.AddYears(-1) to make "last year" dynamic regardless of when the app runs.
        // Fetch last year data for step 2a
        List<GoldPrice> lastYearPrices = dataService
            .GetGoldPrices(today.AddYears(-1), today)
            .GetAwaiter().GetResult();

        // Fetch 2019–present data for steps 2b–2e (chunked, API limit = 367 days)
        List<GoldPrice> longRangePrices = dataService
            .GetGoldPricesLongRange(new DateTime(2019, 1, 1), today)
            .GetAwaiter().GetResult();

        Console.WriteLine($"Last year: {lastYearPrices.Count} records | Since 2019: {longRangePrices.Count} records.");

        if (lastYearPrices.Count == 0 || longRangePrices.Count == 0)
        {
            Console.WriteLine("Incomplete data. Exiting.");
            return;
        }

        // Originally: a single GoldAnalysisService instance was created with goldPrices.
        // Split into two instances — one per dataset — because step 2a uses last year only,
        // while steps 2b–2e need the full 2019–present range.
        GoldAnalysisService lastYearService   = new GoldAnalysisService(lastYearPrices);
        GoldAnalysisService longRangeService  = new GoldAnalysisService(longRangePrices);

        // Originally: only GetAveragePrice() was called and printed via PrintSingleValue().
        // Replaced by the full set of step 2 queries below. GetAveragePrice() is superseded
        // by 2d which already computes per-year averages for 2020, 2023 and 2024.

        // ── 2a ──────────────────────────────────────────────────────────────────
        Console.WriteLine("\n=== 2a: TOP 3 highest / lowest prices (last year) ===");
        GoldResultPrinter.PrintPrices(lastYearService.GetTop3HighestPricesMethodSyntax(), "TOP 3 Highest [method syntax]");
        GoldResultPrinter.PrintPrices(lastYearService.GetTop3LowestPricesMethodSyntax(),  "TOP 3 Lowest  [method syntax]");
        GoldResultPrinter.PrintPrices(lastYearService.GetTop3HighestPricesQuerySyntax(),  "TOP 3 Highest [query syntax]");
        GoldResultPrinter.PrintPrices(lastYearService.GetTop3LowestPricesQuerySyntax(),   "TOP 3 Lowest  [query syntax]");

        // ── 2b ──────────────────────────────────────────────────────────────────
        Console.WriteLine("\n=== 2b: >5% gain from January 2020 purchase ===");
        var gainDays = longRangeService.GetDaysWithMoreThan5PercentGainFromJan2020();
        if (gainDays.Count > 0)
        {
            Console.WriteLine($"YES — possible on {gainDays.Count} days. First and last opportunities:");
            GoldResultPrinter.PrintPrices(gainDays.Take(5).ToList(),  "First 5 days");
            GoldResultPrinter.PrintPrices(gainDays.TakeLast(5).ToList(), "Last 5 days");
        }
        else
            Console.WriteLine("NOT possible to earn more than 5% from January 2020.");

        // ── 2c ──────────────────────────────────────────────────────────────────
        Console.WriteLine("\n=== 2c: Ranks 11-13 in 2019-2022 price ranking ===");
        GoldResultPrinter.PrintPrices(longRangeService.GetSecondTenOpenerDates(), "Ranks 11-13 (second ten openers)");

        // ── 2d ──────────────────────────────────────────────────────────────────
        Console.WriteLine("\n=== 2d: Average gold prices for 2020, 2023, 2024 [query syntax] ===");
        foreach (var (year, avg) in longRangeService.GetAveragesByYears(new[] { 2020, 2023, 2024 }))
            Console.WriteLine($"  {year}: {Math.Round(avg, 2)} PLN");

        // ── 2e ──────────────────────────────────────────────────────────────────
        Console.WriteLine("\n=== 2e: Best buy / sell window (2020-2024) ===");
        var (buy, sell, roi) = longRangeService.GetBestBuySell();
        Console.WriteLine($"  Buy  on {buy.Date:yyyy-MM-dd}  at {buy.Price} PLN");
        Console.WriteLine($"  Sell on {sell.Date:yyyy-MM-dd}  at {sell.Price} PLN");
        Console.WriteLine($"  Return on investment: {Math.Round(roi, 2)}%");

        // ── 3: Save prices to XML ────────────────────────────────────────────
        Console.WriteLine("\n=== 3: Saving prices to XML ===");
        GoldResultPrinter.SaveToXml(longRangePrices, "gold_prices.xml");

        // ── 4: Read prices from XML (single instruction) ─────────────────────
        Console.WriteLine("\n=== 4: Reading prices from XML (one instruction) ===");
        var loadedPrices = GoldResultPrinter.LoadFromXml("gold_prices.xml");
        Console.WriteLine($"Loaded {loadedPrices.Count} records from XML.");
        GoldResultPrinter.PrintPrices(loadedPrices.Take(3).ToList(), "First 3 loaded entries");

        Console.WriteLine("\nGold Analysis Queries with LINQ Completed.");
    }
}
