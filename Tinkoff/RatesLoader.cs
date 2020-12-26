using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Tinkoff
{
    public class RatesLoader
    {
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;
        public Dictionary<DateTime, decimal> UsdRates { get; private set; } = new Dictionary<DateTime, decimal>();
        public Dictionary<DateTime, decimal> EurRates { get; private set; } = new Dictionary<DateTime, decimal>();

        public RatesLoader(DateTime fromDate, DateTime toDate)
        {
            _fromDate = fromDate.Date;
            _toDate = toDate.Date;
        }

        public async Task LoadAsync()
        {
            const string usdCode = "R01235";
            const string eurCode = "R01239";
            UsdRates = await GetRateAsync(usdCode);
            EurRates = await GetRateAsync(eurCode);
        }

        private async Task<Dictionary<DateTime, decimal>> GetRateAsync(string currency)
        {
            var result = new Dictionary<DateTime, decimal>();
            var uri = $"http://www.cbr.ru/scripts/XML_dynamic.asp?date_req1={_fromDate:dd/MM/yyyy}&date_req2={_toDate:dd/MM/yyyy}&VAL_NM_RQ={currency}";
            var client = new WebClient();
            var xml = await client.DownloadStringTaskAsync(uri);
            var xDoc = new XmlDocument();
            xDoc.LoadXml(xml);

            if (xDoc.DocumentElement?.ChildNodes != null)
            {
                foreach (XmlNode? node in xDoc.DocumentElement.ChildNodes)
                {
                    if (node == null)
                    {
                        throw new FormatException("The format of response is incorrect");
                    }

                    var date = Convert.ToDateTime(node.Attributes?["Date"].Value).Date;
                    var rateString = node.ChildNodes.Cast<XmlNode>().First(n => n.Name == "Value").InnerText;
                    var rate = Convert.ToDecimal(rateString);
                    result.Add(date, rate);
                }
            }

            var currentDate = _fromDate;
            while (currentDate <= _toDate)
            {
                if (!result.ContainsKey(currentDate))
                {
                    result.Add(currentDate, result[currentDate.AddDays(-1)]);
                }

                currentDate = currentDate.AddDays(1);
            }

            return result;
        }
    }
}