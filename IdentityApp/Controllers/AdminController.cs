using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private UserManager<UserApp> UserManager { get; }

        public AdminController(UserManager<UserApp> userManager)
        {
            UserManager = userManager;
        }

        public IActionResult Index()
        {
            return View(UserManager.Users.ToList());
        }
    }
}
