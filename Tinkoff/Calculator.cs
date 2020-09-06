using System;
using System.Collections.Generic;
using System.Linq;
using Tinkoff.Trading.OpenApi.Models;
using static Tinkoff.Utils;

namespace Tinkoff
{
    public class Calculator
    {
        private List<Operation> _operations;
        private readonly Portfolio _portfolio;
        private readonly Dictionary<DateTime, decimal> _usdRates;
        private readonly Dictionary<DateTime, decimal> _eurRates;

        public Calculator(List<Operation> operations, Portfolio portfolio, Dictionary<DateTime, decimal> usdRates, Dictionary<DateTime, decimal> eurRates)
        {
            this._operations = operations;
            _portfolio = portfolio;
            _usdRates = usdRates;
            _eurRates = eurRates;
        }

        public void Calculate()
        {
            var currentBalance = CalculateBalance(_portfolio);
            WriteLine($"Current Balance:{currentBalance}");

            _operations = FilterAndConvertOperations(_operations);
            _operations = JoinOperations(_operations);
            _operations.Reverse();

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
        }
        
        private static decimal CalculateBalance(Portfolio portfolio)
        {
            var usdRate = CalculateBalance(portfolio.Positions, "BBG0013HGFT4");
            var eurRate = CalculateBalance(portfolio.Positions, "BBG0013HJJ31");
            return portfolio.Positions.Sum(position => CalculatePositionBalance(position, usdRate, eurRate));
        }

        private static decimal CalculateBalance(IReadOnlyCollection<Portfolio.Position> positions, string figi)
        {
            var currency = positions.First(p => p.Figi == figi);
            var balance = currency.Balance * currency.AveragePositionPrice.Value + currency.ExpectedYield.Value;
            var rate = balance / currency.Balance;
            return rate;
        }

        private static decimal CalculatePositionBalance(Portfolio.Position position, decimal usdRate, decimal eurRate)
        {
            var balance = position.Balance * position.AveragePositionPrice.Value + position.ExpectedYield.Value;

            switch (position.AveragePositionPrice.Currency)
            {
                case Currency.Usd:
                    return balance * usdRate;
                case Currency.Eur:
                    return balance * eurRate;
                case Currency.Rub:
                    return balance;
                default:
                    throw new Exception("Wow, you have stock in unusual currency");
            }
        }

        private static void CalculateTotalCompoundInterest(IReadOnlyCollection<(decimal sum, int days)> tuples, decimal portfolioFund)
        {
            //var originalFund = tuples.Last().sum;
            //var inaccuracy = (decimal)0.01 * portfolioFund;
            //double yearlyRate = 21.0;
            double dailyRate = 0.06; //yearlyRate / 365.0;
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

        private static List<Operation> JoinOperations(IReadOnlyList<Operation> operations)
        {
            var optimized = new List<Operation>();

            var j = 0;
            var current = operations[0];
            while (j < operations.Count - 1)
            {
                if (operations[j + 1].Date.Date == current.Date.Date)
                {
                    var currentPayment = current.Payment + operations[j + 1].Payment;
                    current = new Operation(current.Id, current.Status, current.Trades, current.Commission, current.Currency, currentPayment, current.Price, current.Quantity, current.Figi, current.InstrumentType, current.IsMarginCall, current.Date,
                        current.OperationType);
                    j++;
                    continue;
                }

                j++;
                optimized.Add(current);
                current = operations[j];
            }

            optimized.Add(current);
            return optimized;
        }

        private List<Operation> FilterAndConvertOperations(IList<Operation> operations)
        {
            var convertedOperations = new List<Operation>();

            for (var i = 0; i < operations.Count; i++)
            {
                if (operations[i].OperationType != ExtendedOperationType.PayIn && operations[i].OperationType != ExtendedOperationType.PayOut)
                {
                    continue;
                }

                switch (operations[i].Currency)
                {
                    case Currency.Usd:
                        operations[i] = ConvertOperationToRub(operations[i], _usdRates, operations[i].Date);
                        break;
                    case Currency.Eur:
                        operations[i] = ConvertOperationToRub(operations[i], _eurRates, operations[i].Date);
                        break;
                }

                convertedOperations.Add(operations[i]);
            }

            return convertedOperations;
        }

        private static Operation ConvertOperationToRub(Operation o, IReadOnlyDictionary<DateTime, decimal> dictionary, DateTime date)
        {
            var payment = dictionary[date.Date] * o.Payment;
            var operation = new Operation(o.Id, o.Status, o.Trades, o.Commission, Currency.Rub, payment, o.Price, o.Quantity, o.Figi, o.InstrumentType, o.IsMarginCall, o.Date, o.OperationType);
            return operation;
        }

        private static string FormatOperation(Operation operation)
        {
            return $"Date: {operation.Date:dd.MM.yy}. {operation.Currency}, Payment: {operation.Payment,15} Type: {operation.OperationType}";
        }
    }
}