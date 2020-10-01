using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using WebUtil.Models;

namespace Server.Models
{
    public static class ModelsExtend
    {
        public static T ToProtocol<T>(this T t, MongoDbHeader header)
            where T : Protocols.Common.Header
        {
            t.Id = header.Id;
            t.Created = header.Created;
            t.Updated = header.Updated;
            return t;
        }

        public static Protocols.Common.Analysis ToProtocol(this Analysis analysis)
        {
            return new Protocols.Common.Analysis
            {
                Code = analysis.Code,
                StockEvaluate = analysis.StockEvaluate?.ToProtocol(),
                Date = analysis.Date
            }.ToProtocol(analysis);
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
            }.ToProtocol(code);
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
                UserId = mockInvest.UserId,
                Code = mockInvest.Code,
                Amount = mockInvest.Amount,
                BuyPrice = mockInvest.BuyPrice,
                CurrentPrice = mockInvest.Price,
                Date = mockInvest.Date
            }.ToProtocol(mockInvest);
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
                Price = mockInvest.CurrentPrice,
                Date = mockInvest.Date
            };
        }

        public static Protocols.Common.MockInvestHistory ToProtocol(this MockInvestHistory mockInvestHistory)
        {
            return new Protocols.Common.MockInvestHistory
            {
                Type = mockInvestHistory.Type,
                UserId = mockInvestHistory.UserId,
                Code = mockInvestHistory.Code,
                Amount = mockInvestHistory.Amount,
                BuyPrice = mockInvestHistory.BuyPrice,
                Price = mockInvestHistory.Price,
            }.ToProtocol(mockInvestHistory);
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
                UserId = user.UserId,
                Balance = user.Balance,
                OriginBalance = user.OriginBalance
            };
        }

        public static User ToModel(this Protocols.Common.User user)
        {
            return new User
            {
                Id = user.Id,
                UserId = user.UserId,
                Balance = user.Balance,
                OriginBalance = user.OriginBalance
            };
        }

        public static Protocols.Common.AutoTrade ToProtocol(this AutoTrade autoTrade)
        {
            return new Protocols.Common.AutoTrade
            {
                UserId = autoTrade.UserId,
                Balance = autoTrade.Balance,
                Code = autoTrade.Code,
                BuyCondition = autoTrade.BuyCondition,
                SellCondition = autoTrade.SellCondition,
            }.ToProtocol(autoTrade);
        }

        public static AutoTrade ToModel(this Protocols.Common.AutoTrade autoTrade)
        {
            return new AutoTrade
            {
                UserId = autoTrade.UserId,
                Balance = autoTrade.Balance,
                Code = autoTrade.Code,
                BuyCondition = autoTrade.BuyCondition,
                SellCondition = autoTrade.SellCondition,
            };
        }

        public static Protocols.Common.Notification ToProtocol(this Notification notification)
        {
            return new Protocols.Common.Notification
            {
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                InvestType = notification.InvestType,
            }.ToProtocol(notification);
        }

        public static Notification ToModel(this Protocols.Common.Notification notification)
        {
            return new Notification
            {
                Id = notification.Id,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                InvestType = notification.InvestType
            };
        }

        public static Notification ToModel(this Protocols.Common.NotificationCreate notification)
        {
            return new Notification
            {
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                InvestType = notification.InvestType,
            };
        }
    }
}
