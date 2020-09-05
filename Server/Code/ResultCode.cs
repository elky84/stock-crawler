﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Code
{
    public enum ResultCode
    {
        Success,
        UsingUserId,
        NotEnoughBalance,
        CannotOverHaveStockAmount,
        NotFoundStockCodeData,
        UnknownException
    }
}
