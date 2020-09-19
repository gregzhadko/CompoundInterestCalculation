using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Tinkoff;

namespace TinkoffTests
{
    [Category("Integration Tests")]
    public class IntegrationTests
    {
        private RatesLoader _ratesLoader = null!;
        private DateTime _date;

        [OneTimeSetUp]
        public async Task OneTimeSetUpAsync()
        {
            _date = new DateTime(2010, 1, 1);
            _ratesLoader = new RatesLoader(_date, _date);
            await _ratesLoader.LoadAsync();
        }
        
        [Test]
        public void UsdRateTest()
        {
            Assert.AreEqual(30.1851, _ratesLoader.UsdRates[_date]);
        }
        
        [Test]
        public void EurRateTest()
        {
            Assert.AreEqual(43.4605, _ratesLoader.EurRates[_date]);
        }
    }
}