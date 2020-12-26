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
        private readonly List<Portfolio.Position> _positions;
        private readonly Dictionary<DateTime, decimal> _usdRates;
        private readonly Dictionary<DateTime, decimal> _eurRates;

        public Calculator(List<MutableOperation> operations, List<Portfolio.Position> positions, Dictionary<DateTime, decimal> usdRates, Dictionary<DateTime, decimal> eurRates)
        {
            _operations = operations;
            _positions = positions;
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

            var sumWithNoProfit = _operations.Sum(o => o.Payment);
            var sumByDaysList = FindSumByDaysList();

            CalculateTotalCompoundInterest(sumByDaysList, currentBalance, sumWithNoProfit);
            //WriteOperations(_operations);
        }

        private List<SumByDays> FindSumByDaysList()
        {
            var j = 0;
            decimal currentSum = 0;
            int days;
            var sumByDaysList = new List<SumByDays>();
            while (j < _operations.Count - 1)
            {
                var sum = currentSum + _operations[j].Payment;
                days = (_operations[j + 1].Date.Date - _operations[j].Date.Date).Days;
                sumByDaysList.Add(new SumByDays(sum, days));
                currentSum = sum;
                j++;
            }

            days = (DateTime.Now.Date - _operations[j].Date.Date).Days;
            sumByDaysList.Add(new SumByDays(currentSum + _operations[j].Payment, days));
            return sumByDaysList;
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
            return _positions.Sum(position => CalculatePositionBalance(position, usdRate, eurRate));
        }

        private static decimal CalculatePositionBalance(Portfolio.Position position, decimal usdRate, decimal eurRate)
        {
            //This case can happen when the stock was sold on weekend and the position is still represented in the portfolio, but the average price is null (probably a bug in SDK).
            if (position.AveragePositionPrice == null)
            {
                return 0;
            }

            var balance = position.Balance * position.AveragePositionPrice.Value + position.ExpectedYield.Value;

            return position.AveragePositionPrice.Currency switch
            {
                Currency.Usd => balance * usdRate,
                Currency.Eur => balance * eurRate,
                Currency.Rub => balance,
                _ => throw new Exception("Wow, you have stocks in unusual currency")
            };
        }

        private static void CalculateTotalCompoundInterest(IReadOnlyCollection<SumByDays> sumByDaysList, decimal currentBalance, decimal sumWithNoProfit)
        {
            decimal calculatedBalance = 0;
            const double accuracy = 0.0005; //0.1%
            var inaccuracy = (decimal)accuracy * currentBalance;

            var dailyRate = 0.01; //yearlyRate / 365.0;
            const double rateStep = 0.0001;

            do
            {
                if (calculatedBalance > currentBalance)
                {
                    dailyRate -= rateStep;
                }
                else
                {
                    dailyRate += rateStep;
                }

                calculatedBalance = CalculateProfit(sumByDaysList, dailyRate) + sumWithNoProfit;
            } while (Math.Abs(calculatedBalance - currentBalance) > inaccuracy);

            WriteLine($"Calculated balance: {calculatedBalance}. Rate: {dailyRate * 365}");
        }

        private static decimal CalculateProfit(IEnumerable<SumByDays> sumByDaysList, double dailyRate)
        {
            return sumByDaysList.Sum(s => s.CalculateProfitByRate(dailyRate));
        }
    }
}