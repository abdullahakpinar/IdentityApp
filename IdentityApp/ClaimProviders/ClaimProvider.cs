using IdentityApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityApp.ClaimProviders
{
    public class ClaimProvider : IClaimsTransformation
    {
        public UserManager<UserApp> UserManager { get; set; }

        public ClaimProvider(UserManager<UserApp> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal != null && principal.Identity.IsAuthenticated)
            {
                ClaimsIdentity claimsIdentity = principal.Identity as ClaimsIdentity;
                UserApp user = await UserManager.FindByNameAsync(claimsIdentity.Name);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.City))
                    {
                        if (!principal.HasClaim(c => c.Type == "city"))
                        {
                            Claim cityClaim = new Claim("city", user.City, ClaimValueTypes.String, "Internal");
                            claimsIdentity.AddClaim(cityClaim);
                        }
                    }

                    if (user.BirthDate.HasValue)
                    {
                        bool status = DateTime.Today.Year - user.BirthDate.Value.Year > 15;
                        if (status)
                        {
                            Claim violanceClaim = new Claim("violence", status.ToString(), ClaimValueTypes.Boolean, "Internal");
                            claimsIdentity.AddClaim(violanceClaim);
                        }                        
                    }
                }
            }
            return principal;
        }
    }
}
