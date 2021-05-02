using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.Controllers
{
    public class BaseController : Controller
    {
        protected UserManager<UserApp> UserManager { get; }
        protected RoleManager<RoleApp> RoleManager  { get; }
        protected SignInManager<UserApp> SignInManager { get; }
        protected UserApp CurrentUser => UserManager.FindByNameAsync(User.Identity.Name).Result;
        protected IList<string> CurrentUserRoles => UserManager.GetRolesAsync(CurrentUser).Result;
        public BaseController(UserManager<UserApp> userManager, SignInManager<UserApp> signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public BaseController(UserManager<UserApp> userManager, SignInManager<UserApp> signInManager, RoleManager<RoleApp> roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }

        public BaseController(UserManager<UserApp> userManager, RoleManager<RoleApp> roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }
        protected void AddModelError(IdentityResult result)
        {
            foreach (IdentityError ex in result.Errors)
            {
                ModelState.AddModelError(string.Empty, ex.Description);
            }
        }
        protected UserApp GetUserById(string id)
        {
            return UserManager.FindByIdAsync(id).Result;
        }

        protected UserApp GetUserByUserName(string userName)
        {
            return UserManager.FindByNameAsync(userName).Result;
        }
        protected UserApp GetUserByEmail(string email)
        {
            return UserManager.FindByEmailAsync(email).Result;
        }
        protected RoleApp GetRoleById(string id)
        {
            return RoleManager.FindByIdAsync(id).Result;
        }

        protected RoleApp GetRoleByName(string name)
        {
            return RoleManager.FindByNameAsync(name).Result;
        }

        protected async Task<UserApp> GetUserByIdAsync(string id)
        {
            return await UserManager.FindByIdAsync(id);
        }

        protected async Task<UserApp> GetUserByUserNameAsync(string userName)
        {
            return await UserManager.FindByNameAsync(userName);
        }
        protected async Task<UserApp> GetUserByEmailAsync(string email)
        {
            return await UserManager.FindByEmailAsync(email);
        }

        protected async Task<RoleApp> GetRoleByIdAsync(string id)
        {
            return await RoleManager.FindByIdAsync(id);
        }

        protected async Task<RoleApp> GetRoleByNameAsync(string name)
        {
            return await RoleManager.FindByNameAsync(name);
        }
    }
}
