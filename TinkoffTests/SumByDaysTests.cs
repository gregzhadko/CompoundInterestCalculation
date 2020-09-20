using NUnit.Framework;
using Tinkoff;

namespace TinkoffTests
{
    public class SumByDaysTests
    {
        [Test]
        [TestCase(100, 1, 5, 5)]
        [TestCase(100, 2, 10, 21)]
        public void CalculateProfitByRateTests(int sum, int days, double rate, decimal profit)
        {
            var sumByDays = new SumByDays(sum, days);
            var actualProfit = sumByDays.CalculateProfitByRate(rate);
            Assert.AreEqual(profit, actualProfit);
        }
    }
}