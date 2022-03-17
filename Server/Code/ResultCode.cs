namespace Server.Code
{
    public class ResultCode : EzAspDotNet.Protocols.Code.ResultCode
    {
        public ResultCode(int id, string name) : base(id, name)
        {
        }

        public readonly static ResultCode UsingUserId = new(10000, "UsingUserId");

        public readonly static ResultCode NotEnoughBalance = new(10001, "NotEnoughBalance");
        public readonly static ResultCode UsingAutoTradeId = new(10002, "UsingAutoTradeId");
        public readonly static ResultCode CannotOverHaveStockAmount = new(10003, "CannotOverHaveStockAmount");
        public readonly static ResultCode NotFoundStockCodeData = new(10004, "NotFoundStockCodeData");
        public readonly static ResultCode NotFoundStockData = new(10005, "NotFoundStockData");

        public readonly static ResultCode AnalysisNeedComparable2DaysData = new(10006, "AnalysisNeedComparable2DaysData");
        public readonly static ResultCode UsingNotificationId = new(10007, "UsingNotificationId");
        public readonly static ResultCode BuyAmountGreaterThanZero = new(10008, "BuyAmountGreaterThanZero");
        public readonly static ResultCode InvestAlertCompany = new(10009, "InvestAlertCompany");
    }
}
