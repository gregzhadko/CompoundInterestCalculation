using System;
using System.Collections.Generic;
using System.Linq;
using Tinkoff.Trading.OpenApi.Models;

namespace Tinkoff
{
    public static class OperationsLinq
    {
        public static IEnumerable<MutableOperation> ConvertToMutable(this IEnumerable<Operation> operations)
        {
            return operations.Select(o => (MutableOperation)o);
        }

        public static IEnumerable<MutableOperation> FilterByPayInAndPayOut(this IEnumerable<MutableOperation> operations)
        {
            return operations.Where(o => o.OperationType == ExtendedOperationType.PayIn || o.OperationType == ExtendedOperationType.PayOut);
        }
        
        public static IEnumerable<MutableOperation> ConvertToRub(this IEnumerable<MutableOperation> operations, Dictionary<DateTime, decimal> usdRates, Dictionary<DateTime, decimal> eurRates)
        {
            return operations.Select(operation =>
            {
                Dictionary<DateTime, decimal> rates = operation.Currency switch
                {
                    Currency.Usd => usdRates,
                    Currency.Eur => eurRates,
                    _ => throw new Exception($"Currency {operation.Currency} is not supported")
                };
                
                return operation.ConvertToRub(rates);
            });
        }
        
        /// <summary>
        /// Converts operation from the existing currency to RUB based on the currency dictionary
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static MutableOperation ConvertToRub(this MutableOperation operation, IReadOnlyDictionary<DateTime, decimal> dictionary)
        {
            var payment = dictionary[operation.Date.Date] * operation.Payment;
            operation.Currency = Currency.Rub;
            operation.Payment = payment;
            return operation;
        }
    }
}