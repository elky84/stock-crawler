using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Code
{
    public enum ResultCode
    {
        Success,
        UsingUserId,
        UsingAutoTradeId,
        NotEnoughBalance,
        CannotOverHaveStockAmount,
        NotFoundStockCodeData,
        NotImplementedYet,
        NotFoundStockData,
        AnalysisNeedComparable2DaysData,
        UsingNotificationId,
        BuyAmountGreaterThanZero,
        InvestAlertCompany,
        UnknownException
    }
}
