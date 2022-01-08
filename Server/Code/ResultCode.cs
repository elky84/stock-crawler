using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Code
{
    public class ResultCode : EzAspDotNet.Code.ResultCode
    {
        public ResultCode(int id, string name) : base(id, name)
        {
        }

        public static ResultCode UsingUserId = new(10000, "UsingUserId");

        public static ResultCode NotEnoughBalance = new(10001, "NotEnoughBalance");
        public static ResultCode UsingAutoTradeId = new(10002, "UsingAutoTradeId");
        public static ResultCode CannotOverHaveStockAmount = new(10003, "CannotOverHaveStockAmount");
        public static ResultCode NotFoundStockCodeData = new(10004, "NotFoundStockCodeData");
        public static ResultCode NotFoundStockData = new(10005, "NotFoundStockData");

        public static ResultCode AnalysisNeedComparable2DaysData = new(10006, "AnalysisNeedComparable2DaysData");
        public static ResultCode UsingNotificationId = new(10007, "UsingNotificationId");
        public static ResultCode BuyAmountGreaterThanZero = new(10008, "BuyAmountGreaterThanZero");
        public static ResultCode InvestAlertCompany = new(10009, "InvestAlertCompany");
    }
}
