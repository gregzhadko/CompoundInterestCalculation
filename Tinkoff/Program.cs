using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using static Tinkoff.Utils;

namespace Tinkoff
{
    internal static class Program
    {
        private static IConfiguration? _configuration;

        private static async Task Main()
        {
            _configuration = new ConfigurationBuilder()
                             .AddJsonFile("appsettings.json")
                             .Build();

            var token = _configuration["Token"];
            var connection = ConnectionFactory.GetConnection(token);
            var context = connection.Context;

            WriteLine("Loading the operations");
            var operations = (await context.OperationsAsync(DateTime.MinValue, DateTime.MaxValue, "")).Where(o => o.Status == OperationStatus.Done).ConvertToMutable().ToList();

            WriteLine("Calculating rates");
            var ratesLoader = new RatesLoader(operations.Last().Date, DateTime.Now);
            await ratesLoader.LoadAsync();

            WriteLine("Downloading portfolio");
            var portfolio = await context.PortfolioAsync();

            var calculator = new Calculator(operations, portfolio.Positions, ratesLoader.UsdRates, ratesLoader.EurRates);
            calculator.Calculate();

            Console.WriteLine("The End");
        }
    }
}