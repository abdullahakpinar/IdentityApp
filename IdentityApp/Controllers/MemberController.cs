using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using IdentityApp.ViewModels;

namespace IdentityApp.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        public UserManager<UserApp> UserManager { get; }

        public SignInManager<UserApp> SignInManager { get; }

        public MemberController(UserManager<UserApp> userManager, SignInManager<UserApp> signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public IActionResult Index()
        {
            UserApp user = UserManager.FindByNameAsync(User.Identity.Name).Result;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {

            if (ModelState.IsValid)
            {
                if (changePasswordViewModel.PasswordNew != changePasswordViewModel.PasswordOld)
                {
                    UserApp user = UserManager.FindByNameAsync(User.Identity.Name).Result;
                    if (user != null)
                    {
                        bool checkPassword = UserManager.CheckPasswordAsync(user, changePasswordViewModel.PasswordOld).Result;
                        if (checkPassword)
                        {
                            IdentityResult result = UserManager.ChangePasswordAsync(user, changePasswordViewModel.PasswordOld, changePasswordViewModel.PasswordNew).Result;
                            if (result.Succeeded)
                            {
                                UserManager.UpdateSecurityStampAsync(user);
                                //SignOut After LogIn
                                SignInManager.SignOutAsync();
                                SignInManager.PasswordSignInAsync(user.UserName, changePasswordViewModel.PasswordNew, true, false);

                                ViewBag.Status = true;
                            }
                            else
                            {
                                foreach (IdentityError ex in result.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, ex.Description);
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Eski şifrenizi kontrol ediniz.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Eski ve yeni şifreniz aynı olamaz.");
                }
            }
            return View(changePasswordViewModel);
        }

        public IActionResult EditUser()
        {
            UserApp user = UserManager.FindByNameAsync(User.Identity.Name).Result;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserViewModel userViewModel)
        {
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                UserApp user = await UserManager.FindByNameAsync(User.Identity.Name);
                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;

                IdentityResult result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await UserManager.UpdateSecurityStampAsync(user);

                    await SignInManager.SignOutAsync();
                    await SignInManager.SignInAsync(user, true);
                    ViewBag.Status = true;
                }
                else
                {
                    foreach (IdentityError ex in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, ex.Description);
                    }
                }

            }

            return View(userViewModel);
        }


        public void LogOut()
        {
            SignInManager.SignOutAsync();
        }

    }
}
