using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.CustomTagHelpers
{
    [HtmlTargetElement("td", Attributes ="user-roles")]
    public class UserRolesName : TagHelper
    {
        public UserManager<UserApp>  UserManager { get; set; }

        public UserRolesName(UserManager<UserApp> userManager)
        {
            UserManager = userManager;
        }
        [HtmlAttributeName("user-roles")]
        public string UserId { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            UserApp user = await UserManager.FindByIdAsync(UserId);
            IList<string> roles = await UserManager.GetRolesAsync(user);

            string html = string.Empty;
            roles.ToList().ForEach(r =>
            {
                html += $"<span class='badge badge-info'>{r}</span>";
            });

            output.Content.SetHtmlContent(html);
        }
    }
}
