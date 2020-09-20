using System;
using Tinkoff.Trading.OpenApi.Models;

namespace Tinkoff
{
    public class MutableOperation
    {
        public string? Id { get; set; }
        public decimal Payment { get; set; }
        public Currency Currency { get; set; }

        public DateTime Date { get; set; }
        public ExtendedOperationType OperationType { get; set; }

        public static explicit operator MutableOperation(Operation operation) => new MutableOperation
            { Id = operation.Id, Payment = operation.Payment, Currency = operation.Currency, Date = operation.Date, OperationType = operation.OperationType };

        // public static explicit operator Operation(MutableOperation operation) =>
        //     new Operation(operation.Id, default, default, default, operation.Currency, operation.Payment, default, default, default, default, default, operation.Date, operation.OperationType);
    }
}