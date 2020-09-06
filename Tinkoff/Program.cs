using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using static Tinkoff.Utils;

namespace Tinkoff
{
    class Program
    {
        private static IConfiguration _configuration;

        static async Task Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                             .AddJsonFile("appsettings.json")
                             .Build();

            var token = _configuration["Token"];
            var connection = ConnectionFactory.GetConnection(token);
            var context = connection.Context;

            WriteLine("Loading the operations");
            var operations = (await context.OperationsAsync(DateTime.MinValue, DateTime.MaxValue, "")).Where(o => o.Status == OperationStatus.Done).ToList();

            WriteLine("Calculating rates");
            var ratesLoader = new RatesLoader(operations.Last().Date, DateTime.Now);
            await ratesLoader.LoadAsync();

            WriteLine("Downloading portfolio");
            var portfolio = await context.PortfolioAsync();

            var calculator = new Calculator(operations, portfolio, ratesLoader.UsdRates, ratesLoader.EurRates);
            calculator.Calculate();

            Console.WriteLine("The End");
        }
    }
}