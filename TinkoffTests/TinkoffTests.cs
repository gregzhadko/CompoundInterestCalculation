using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ConvertOperationToRub_ValueExists_True()
        {
            var operation = new MutableOperation { Currency = Currency.Usd, Date = new DateTime(2000, 10, 10, 5, 10, 10), Payment = 10 };
            var rates = new Dictionary<DateTime, decimal> { { new DateTime(2000, 10, 10), 30 } };
            Calculator.ConvertOperationToRub(operation, rates);
            Assert.AreEqual(300, operation.Payment);
            Assert.AreEqual(Currency.Rub, operation.Currency);
        }

        [Test]
        public void CalculateCurrentBalance_CurrenciesOnlyPositions_Correct()
        {
            var positions = new List<Portfolio.Position>
            {
                new Portfolio.Position(default, default, default, default, InstrumentType.Stock, 100, default, new MoneyAmount(Currency.Rub, 100), 0, new MoneyAmount(Currency.Rub, 70), null)
            };
            var portfolio = new Portfolio(positions);
            var rates = new Dictionary<DateTime, decimal> { { DateTime.Now.Date, 1 } };
            var calculator = new Calculator(new List<Operation>(), portfolio, rates, rates);
            var actual = calculator.CalculateCurrentBalance();
            Assert.AreEqual(7100, actual);
        }
    }
}