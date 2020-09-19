using System;
using System.Collections.Generic;
using System.Linq;
using Tinkoff.Trading.OpenApi.Models;
using static Tinkoff.Utils;

namespace Tinkoff
{
    public class Calculator
    {
        private List<MutableOperation> _operations;
        private readonly Portfolio _portfolio;
        private readonly Dictionary<DateTime, decimal> _usdRates;
        private readonly Dictionary<DateTime, decimal> _eurRates;

        public Calculator(List<Operation> operations, Portfolio portfolio, Dictionary<DateTime, decimal> usdRates, Dictionary<DateTime, decimal> eurRates)
        {
            _operations = operations.ConvertToMutable().ToList();
            _portfolio = portfolio;
            _usdRates = usdRates;
            _eurRates = eurRates;
        }

        public void Calculate()
        {
            var currentBalance = CalculateCurrentBalance();
            WriteLine($"Current Balance:{currentBalance}");

            _operations = _operations
                          .FilterByPayInAndPayOut()
                          .ConvertToRub(_usdRates, _eurRates)
                          .JoinAtTheSameDate()
                          .ReverseList();
            _operations.Reverse();

            //_operations = JoinOperations(_operations.Select(o => (Operation)o).ToList()).Select(o => (MutableOperation)o).ToList();
            //_operations.Reverse();

            var j = 0;
            decimal currentSum = 0;
            int days;
            var tuples = new List<(decimal sum, int days)>();
            while (j < _operations.Count - 1)
            {
                var sum = currentSum + _operations[j].Payment;
                days = (_operations[j + 1].Date.Date - _operations[j].Date.Date).Days;
                tuples.Add((sum, days));
                currentSum = sum;
                j++;
            }

            days = (DateTime.Now.Date - _operations[j].Date.Date).Days;
            tuples.Add((currentSum + _operations[j].Payment, days));

            CalculateTotalCompoundInterest(tuples, currentBalance);
            //WriteOperations(_operations);
        }

        // private void WriteOperations(List<Operation> operations)
        // {
        //     foreach (var operation in operations)
        //     {
        //         Console.WriteLine($"{(operation.Payment >= 0 ? " " : "")}{operation.Payment:0.00}\t\t{operation.Date.Date:dd.MM.yyyy}");
        //     }
        // }

        public decimal CalculateCurrentBalance()
        {
            var usdRate = _usdRates[DateTime.Now.Date];
            var eurRate = _eurRates[DateTime.Now.Date];
            return _portfolio.Positions.Sum(position => CalculatePositionBalance(position, usdRate, eurRate));
        }

        private static decimal CalculatePositionBalance(Portfolio.Position position, decimal usdRate, decimal eurRate)
        {
            var balance = position.Balance * position.AveragePositionPrice.Value + position.ExpectedYield.Value;

            return position.AveragePositionPrice.Currency switch
            {
                Currency.Usd => balance * usdRate,
                Currency.Eur => balance * eurRate,
                Currency.Rub => balance,
                _ => throw new Exception("Wow, you have stocks in unusual currency")
            };
        }

        private static void CalculateTotalCompoundInterest(IReadOnlyCollection<(decimal sum, int days)> tuples, decimal portfolioFund)
        {
            //var originalFund = tuples.Last().sum;
            //var inaccuracy = (decimal)0.01 * portfolioFund;
            //double yearlyRate = 21.0;
            double dailyRate = 0.058; //yearlyRate / 365.0;
            // decimal profit = 0;
            // int i = 0;
            var calculatedFund = CalculateProfit(tuples, dailyRate);
            WriteLine($"Calculated balance: {calculatedFund}. Rate: {dailyRate * 366}");
            //var calculatedFund = originalFund + profit;
            // while (i < 100 && Math.Abs(portfolioFund - calculatedFund) > inaccuracy)
            // {
            //     if (portfolioFund > calculatedFund)
            //     {
            //         dailyRate += 0.0001;
            //     }
            //     else
            //     {
            //         dailyRate -= 0.0001;
            //     }
            //
            //     profit = CalculateProfit(tuples, profit, dailyRate);
            //     calculatedFund = originalFund + profit;
            //     WriteLine($"Calculated balance: {calculatedFund}. Rate: {dailyRate * 366}");
            //     i++;
            // }
        }

        private static decimal CalculateProfit(IEnumerable<(decimal sum, int days)> tuples, double dailyRate)
        {
            decimal currentSum = 0;
            foreach (var (sum, days) in tuples)
            {
                currentSum += CalculateProfitByCompoundInterest(sum, days, dailyRate);
            }

            return currentSum;
        }

        private static decimal CalculateProfitByCompoundInterest(decimal sum, int days, double rate)
        {
            return (sum * (decimal)Math.Pow(1 + rate / 100, days)) - sum;
        }
    }
}