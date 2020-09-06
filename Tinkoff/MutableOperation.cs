using System;
using Tinkoff.Trading.OpenApi.Models;

namespace Tinkoff
{
    public class MutableOperation
    {
        public decimal Payment { get; set; }
        public Currency Currency { get; set; }

        public DateTime Date { get; set; }

        public static explicit operator MutableOperation(Operation operation) => new MutableOperation { Payment = operation.Payment, Currency = operation.Currency, Date = operation.Date };

        public static explicit operator Operation(MutableOperation operation) =>
            new Operation(default, default, default, default, operation.Currency, operation.Payment, default, default, default, default, default, operation.Date, default);
    }
}