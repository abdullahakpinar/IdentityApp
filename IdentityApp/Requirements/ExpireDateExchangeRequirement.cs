using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.Requirements
{
    public class ExpireDateExchangeRequirement : IAuthorizationRequirement
    {

    }

    public class ExpireDateExchangeHandler : AuthorizationHandler<ExpireDateExchangeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ExpireDateExchangeRequirement requirement)
        {
            if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var userClaim = context.User.Claims.Where(c => c.Type == "ExpireDateExchange" && c.Value != null).FirstOrDefault();
                if (userClaim != null)
                {
                    if (DateTime.Now < Convert.ToDateTime(userClaim.Value))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
