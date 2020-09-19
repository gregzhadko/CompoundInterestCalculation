using System;

namespace Tinkoff
{
    public class SumByDays
    {
        public SumByDays(decimal sum, int days)
        {
            Sum = sum;
            Days = days;
        }

        public decimal Sum { get; set; }
        public int Days { get; set; }

        public decimal ProfitByRate(double rate) => (Sum * (decimal)Math.Pow(1 + rate / 100, Days)) - Sum;
    }
}