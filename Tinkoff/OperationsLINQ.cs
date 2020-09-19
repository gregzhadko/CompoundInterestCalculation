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
                return operation.Currency switch
                {
                    Currency.Rub => operation,
                    Currency.Usd => operation.ConvertToRub(usdRates),
                    Currency.Eur => operation.ConvertToRub(eurRates),
                    _ => throw new Exception($"Currency {operation.Currency} is not supported")
                };
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
        
        public static List<MutableOperation> JoinAtTheSameDate(this IEnumerable<MutableOperation> input)
        {
            var operations = input.ToList();
            var joined = new List<MutableOperation>();

            var j = 0;
            var current = operations[0];
            while (j < operations.Count - 1)
            {
                if (operations[j + 1].Date.Date == current.Date.Date)
                {
                    current.Payment += operations[j + 1].Payment;
                    j++;
                    continue;
                }

                j++;
                joined.Add(current);
                current = operations[j];
            }

            joined.Add(current);
            return joined;
        }

        public static List<MutableOperation> ReverseList(this List<MutableOperation> list)
        {
            list.Reverse();
            return list;
        }
    }
}