﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public static class ModelsExtend
    {

        public static Protocols.Common.Analysis ToProtocol(this Analysis analysis)
        {
            return new Protocols.Common.Analysis
            {
                Id = analysis.Id,
                Code = analysis.Code,
                StockEvaluate = analysis.StockEvaluate?.ToProtocol(),
                Date = analysis.Date
            };
        }

        public static Analysis ToModel(this Protocols.Common.Analysis analysis)
        {
            return new Analysis
            {
                Id = analysis.Id,
                Code = analysis.Code,
                StockEvaluate = analysis.StockEvaluate?.ToModel(),
                Date = analysis.Date
            };
        }

        public static Protocols.Common.Code ToProtocol(this Code code)
        {
            return new Protocols.Common.Code
            {
                Id = code.Id,
                Value = code.Value,
                Name = code.Name,
                Type = code.Type
            };
        }

        public static Code ToModel(this Protocols.Common.Code code)
        {
            return new Code
            {
                Id = code.Id,
                Value = code.Value,
                Name = code.Name,
                Type = code.Type
            };
        }



        public static Protocols.Common.MockInvest ToProtocol(this MockInvest mockInvest)
        {
            return new Protocols.Common.MockInvest
            {
                Id = mockInvest.Id,
                UserId = mockInvest.UserId,
                Code = mockInvest.Code,
                Amount = mockInvest.Amount,
                BuyPrice = mockInvest.BuyPrice,
                CurrentPrice = mockInvest.Price
            };
        }


        public static MockInvest ToModel(this Protocols.Common.MockInvest mockInvest)
        {
            return new MockInvest
            {
                Id = mockInvest.Id,
                UserId = mockInvest.UserId,
                Code = mockInvest.Code,
                Amount = mockInvest.Amount,
                BuyPrice = mockInvest.BuyPrice,
                Price = mockInvest.CurrentPrice
            };
        }

        public static Protocols.Common.MockInvestHistory ToProtocol(this MockInvestHistory mockInvestHistory)
        {
            return new Protocols.Common.MockInvestHistory
            {
                Id = mockInvestHistory.Id,
                Type = mockInvestHistory.Type,
                UserId = mockInvestHistory.UserId,
                Code = mockInvestHistory.Code,
                Amount = mockInvestHistory.Amount,
                BuyPrice = mockInvestHistory.BuyPrice,
                Price = mockInvestHistory.Price
            };
        }


        public static MockInvestHistory ToModel(this Protocols.Common.MockInvestHistory mockInvestHistory)
        {
            return new MockInvestHistory
            {
                Id = mockInvestHistory.Id,
                Type = mockInvestHistory.Type,
                UserId = mockInvestHistory.UserId,
                Code = mockInvestHistory.Code,
                Amount = mockInvestHistory.Amount,
                BuyPrice = mockInvestHistory.BuyPrice,
                Price = mockInvestHistory.Price
            };
        }

        public static Protocols.Common.StockEvaluate ToProtocol(this StockEvaluate stockEvaluate)
        {
            return new Protocols.Common.StockEvaluate
            {
                MovingAverageLines = stockEvaluate.MovingAverageLines,
                TradeCount = stockEvaluate.TradeCount,
                BuyStockValue = stockEvaluate.BuyStockValue,
                TransactionPrice = stockEvaluate.TransactionPrice
            };
        }

        public static StockEvaluate ToModel(this Protocols.Common.StockEvaluate stockEvaluate)
        {
            return new StockEvaluate
            {
                MovingAverageLines = stockEvaluate.MovingAverageLines,
                TradeCount = stockEvaluate.TradeCount,
                BuyStockValue = stockEvaluate.BuyStockValue,
                TransactionPrice = stockEvaluate.TransactionPrice
            };
        }


        public static Protocols.Common.User ToProtocol(this User user)
        {
            return new Protocols.Common.User
            {
                Id = user.Id,
                UserId = user.UserId,
                Balance = user.Balance,
                OriginBalance = user.OriginBalance,
                AutoTrade = user.AutoTrade,
                AutoTradeCount = user.AutoTradeCount,
                AnalysisType = user.AnalysisType
            };
        }

        public static User ToModel(this Protocols.Common.User user)
        {
            return new User
            {
                Id = user.Id,
                UserId = user.UserId,
                Balance = user.Balance,
                OriginBalance = user.OriginBalance,
                AutoTrade = user.AutoTrade,
                AutoTradeCount = user.AutoTradeCount,
                AnalysisType = user.AnalysisType
            };
        }

    }
}
