using IdentityApp.Models;
using IdentityApp.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.Controllers
{
    public class HomeController : Controller
    {
        public UserManager<UserApp> UserManager { get; }

        public SignInManager<UserApp> SignInManager { get; }

        public HomeController(UserManager<UserApp> userManager, SignInManager<UserApp> signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Member");
            }
            return View();
        }

        public IActionResult LogIn(string returnUrl)
        {
            TempData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LogInViewModel logInViewModel)
        {
            if (ModelState.IsValid)
            {
                UserApp user = await UserManager.FindByEmailAsync(logInViewModel.Email);
                if (user != null)
                {

                    if (await UserManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, "Hesabınız bir süreliğine kilitlenmiştir.\nLütfen daha sonra tekrar deneyiniz.");
                        return View(logInViewModel);
                    }

                    await SignInManager.SignOutAsync();

                    Microsoft.AspNetCore.Identity.SignInResult result = await SignInManager.PasswordSignInAsync(user, logInViewModel.Password, logInViewModel.RememberMe, false);
                    if (result.Succeeded)
                    {
                        await UserManager.ResetAccessFailedCountAsync(user);

                        if (TempData["ReturnUrl"] != null)
                        {
                            return Redirect(TempData["ReturnUrl"].ToString());
                        }
                        return RedirectToAction("Index", "Member");
                    }
                    else
                    {
                        await UserManager.AccessFailedAsync(user);

                        if (await UserManager.GetAccessFailedCountAsync(user) >= 3)
                        {
                            await UserManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(45)));
                            ModelState.AddModelError(string.Empty, "Hesabınız başarısız giriş denemelerinden dolayı 45dk kilitlenmiştir.\nLütfen daha sonra tekrar deneyiniz.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Email adresiniz veya şifreniz yanlış.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Bu email adresine kayıtlı kullanıcı bulunamamıştır.");
                }
            }
            return View(logInViewModel);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel user)
        {
            if (ModelState.IsValid)
            {
                UserApp appUser = new UserApp
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                };

                IdentityResult result = await UserManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("LogIn");
                }
                else
                {
                    foreach (IdentityError ex in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, ex.Description);
                    }
                }
            }

            return View(user);
        }


        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            UserApp user = UserManager.FindByEmailAsync(resetPasswordViewModel.Email).Result;
            if (user != null)
            {
                string passwordResetToken = UserManager.GeneratePasswordResetTokenAsync(user).Result;
                string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new { userId = user.Id, token = passwordResetToken }, HttpContext.Request.Scheme);
                Helpers.ResetPassword.PasswordResetSendEmail(passwordResetLink);
                ViewBag.Status = "Successful";
            }
            else
            {
                ModelState.AddModelError(string.Empty, $"{resetPasswordViewModel.Email} adresi sistemde bulunamamıştır.");
            }

            return View(resetPasswordViewModel);
        }

        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["UserId"] = userId;
            TempData["Token"] = token;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("Password")]ResetPasswordViewModel resetPasswordViewModel)
        {
            string userId = TempData["UserId"].ToString();
            string token = TempData["Token"].ToString();
            UserApp user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                IdentityResult result = await UserManager.ResetPasswordAsync(user, token, resetPasswordViewModel.Password);
                if (result.Succeeded)
                {
                    IdentityResult stampResult = await UserManager.UpdateSecurityStampAsync(user);
                    if (stampResult.Succeeded)
                    {
                        ViewBag.Status = "Successful";
                    }
                    else
                    {
                        foreach (IdentityError ex in stampResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, ex.Description);
                        }
                    }
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
                ModelState.AddModelError(string.Empty, "Linkin süresi dolmuştur. Yeni link alınız.");
            }
            return View(resetPasswordViewModel);
        }


        
    }
}
