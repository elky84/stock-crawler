using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class StockEvaluate
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> MovingAverageLines { get; set; } = new Dictionary<int, int>();

        public double Fluctuation { get; set; }

        public double TradeCount { get; set; }

        public double BuyStockValue { get; set; }

        public bool BuyStockTiming => BuyStockValue > 0;

        public double TransactionPrice { get; set; }

    }
}
