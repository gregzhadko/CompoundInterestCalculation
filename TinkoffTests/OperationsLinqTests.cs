using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffTests
{
    public class Tests
    {
        [Test]
        public void ConvertOperationToRub_ValueExists_True()
        {
            var operation = new MutableOperation { Currency = Currency.Usd, Date = new DateTime(2000, 10, 10, 5, 10, 10), Payment = 10 };
            var rates = new Dictionary<DateTime, decimal> { { new DateTime(2000, 10, 10), 30 } };
            operation.ConvertToRub(rates);
            Assert.AreEqual(300, operation.Payment);
            Assert.AreEqual(Currency.Rub, operation.Currency);
        }

        [Test]
        public void ConvertListOfOperationsToRub_ValueExists_True()
        {
            var operations = new List<MutableOperation>
            {
                new MutableOperation { Currency = Currency.Usd, Date = new DateTime(2000, 10, 10, 5, 10, 10), Payment = 10 },
                new MutableOperation { Currency = Currency.Usd, Date = new DateTime(2000, 10, 11, 5, 10, 10), Payment = 100 },
                new MutableOperation { Currency = Currency.Eur, Date = new DateTime(2000, 10, 10, 5, 10, 10), Payment = 10 },
                new MutableOperation { Currency = Currency.Eur, Date = new DateTime(2000, 10, 11, 5, 10, 10), Payment = 1000 },
                new MutableOperation { Currency = Currency.Rub, Date = new DateTime(2000, 10, 11, 5, 10, 10), Payment = 1000 }
            };
            var usdRates = new Dictionary<DateTime, decimal> { { new DateTime(2000, 10, 10), 70 }, { new DateTime(2000, 10, 11), 80 } };
            var eurRates = new Dictionary<DateTime, decimal> { { new DateTime(2000, 10, 10), 90 }, { new DateTime(2000, 10, 11), 100 } };

            operations = operations.ConvertToRub(usdRates, eurRates).ToList();
            Assert.AreEqual(110600, operations.Sum(o => o.Payment));
            Assert.AreEqual(Currency.Rub, operations[0].Currency);
            Assert.AreEqual(1, operations.Select(o => o.Currency).Distinct().Count());
        }

        [Test]
        public void FilterByPayInAndPayOut_WithIncorrectOperations_CorrectList()
        {
            var operations = new List<MutableOperation>
            {
                new MutableOperation { Id = "1", OperationType = ExtendedOperationType.PayIn },
                new MutableOperation { Id = "2", OperationType = ExtendedOperationType.PayOut },
                new MutableOperation { Id = "3", OperationType = ExtendedOperationType.Coupon },
                new MutableOperation { Id = "4", OperationType = ExtendedOperationType.PayIn },
                new MutableOperation { Id = "5", OperationType = ExtendedOperationType.OtherCommission },
            };

            var actual = operations.FilterByPayInAndPayOut();
            var expected = new List<MutableOperation> { operations[0], operations[1], operations[3] };
            CollectionAssert.AreEqual(expected.Select(o => o.Id), actual.Select(o => o.Id));
        }

        [Test]
        public void JoinOperations_CorrectJoin()
        {
            var operations = new List<MutableOperation>
            {
                new MutableOperation { Payment = 100, Date = new DateTime(2020, 09, 08), OperationType = ExtendedOperationType.PayIn },
                new MutableOperation { Payment = 100, Date = new DateTime(2020, 09, 09), OperationType = ExtendedOperationType.PayIn },
                new MutableOperation { Payment = 10, Date = new DateTime(2020, 09, 09), OperationType = ExtendedOperationType.PayIn },
                new MutableOperation { Payment = 1000, Date = new DateTime(2020, 09, 09), OperationType = ExtendedOperationType.PayIn },
                new MutableOperation { Payment = -100, Date = new DateTime(2020, 09, 09), OperationType = ExtendedOperationType.PayOut },
                new MutableOperation { Payment = 1000, Date = new DateTime(2020, 09, 10), OperationType = ExtendedOperationType.PayIn },
                new MutableOperation { Payment = -100, Date = new DateTime(2020, 09, 10), OperationType = ExtendedOperationType.PayOut },
                new MutableOperation { Payment = -100, Date = new DateTime(2020, 09, 11), OperationType = ExtendedOperationType.PayOut },
            };

            operations = operations.JoinAtTheSameDate();
            Assert.AreEqual(4, operations.Count);
            Assert.AreEqual(100, operations[0].Payment);
            Assert.AreEqual(1010, operations[1].Payment);
            Assert.AreEqual(900, operations[2].Payment);
            Assert.AreEqual(-100, operations[3].Payment);
        }
    }
}