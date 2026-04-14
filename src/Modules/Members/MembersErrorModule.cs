using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Platform.Errors;

namespace Zss.BilliardHall.Modules.Members;

public sealed class MembersErrorModule : IErrorModule
{
    public void Register()
    {
        ErrorRegistry.Register(new ErrorDescriptor(
            MemberErrorCodes.MemberEmailExists,
            "会员邮箱已存在",
            StatusCodes.Status409Conflict,
            "https://api.zss.com/problems/members/email-exists",
            LogLevel.Information));
    }
}
