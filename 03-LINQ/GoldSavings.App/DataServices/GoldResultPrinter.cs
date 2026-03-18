using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GoldSavings.App.Model;

namespace GoldSavings.App.Services
{
    public static class GoldResultPrinter
    {
        public static void PrintPrices(List<GoldPrice> prices, string title)
        {
            Console.WriteLine($"\n--- {title} ---");
            foreach (var price in prices)
            {
                Console.WriteLine($"{price.Date:yyyy-MM-dd} - {price.Price} PLN");
            }
        }

        public static void PrintSingleValue<T>(T value, string title)
        {
            Console.WriteLine($"\n{title}: {value}");
        }

        // Serializes a list of GoldPrice objects to an XML file at the given path.
        // Each price is stored as a <GoldPrice> element with Date and Price attributes.
        public static void SaveToXml(List<GoldPrice> prices, string filePath)
        {
            var doc = new XDocument(
                new XElement("GoldPrices",
                    prices.Select(p =>
                        new XElement("GoldPrice",
                            new XAttribute("Date", p.Date.ToString("yyyy-MM-dd")),
                            new XAttribute("Price", p.Price)
                        )
                    )
                )
            );
            doc.Save(filePath);
            Console.WriteLine($"\nPrices saved to XML: {filePath}");
        }
    }
}