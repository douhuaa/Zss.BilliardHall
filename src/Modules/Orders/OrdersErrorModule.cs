using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zss.BilliardHall.Platform.Errors;

namespace Zss.BilliardHall.Modules.Orders;

public sealed class OrdersErrorModule : IErrorModule
{
    public void Register()
    {
        ErrorRegistry.Register(new ErrorDescriptor(
            OrdersErrorCodes.AlreadyPaid,
            "订单已支付",
            StatusCodes.Status409Conflict,
            "https://api.zss.com/problems/orders/already-paid",
            LogLevel.Information));

        ErrorRegistry.Register(new ErrorDescriptor(
            OrdersErrorCodes.NotFound,
            "订单不存在",
            StatusCodes.Status404NotFound,
            "https://api.zss.com/problems/orders/not-found",
            LogLevel.Debug));
    }
}

