using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffTests
{
    public class CalculatorTests
    {
        [Test]
        public void CalculateCurrentBalance_CurrenciesOnlyPositions_Correct()
        {
            var positions = new List<Portfolio.Position>
            {
                new Portfolio.Position(default, default, default, default, InstrumentType.Stock, 100, default, new MoneyAmount(Currency.Rub, 100), 0, new MoneyAmount(Currency.Rub, 70), null)
            };
            var rates = new Dictionary<DateTime, decimal> { { DateTime.Now.Date, 1 } };
            var calculator = new Calculator(new List<MutableOperation>(), positions, rates, rates);
            var actual = calculator.CalculateCurrentBalance();
            Assert.AreEqual(7100, actual);
        }
    }
}