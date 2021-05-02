using IdentityApp.Models;
using IdentityApp.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityApp.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(UserManager<UserApp> userManager, SignInManager<UserApp> signInManager) : base(userManager, signInManager)
        {
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
                    if (!UserManager.IsEmailConfirmedAsync(user).Result)
                    {
                        ModelState.AddModelError(string.Empty, "E-postanızı doğrulanmamıştır. Lütfen e-postanızı doğrulayınız.");
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
                if (UserManager.Users.Any(u => u.PhoneNumber == user.PhoneNumber))
                {
                    ModelState.AddModelError(user.PhoneNumber, "Telefon numarası sistemde kayıtlıdır.");
                    return View(user);
                }

                UserApp appUser = new UserApp
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                };

                IdentityResult result = await UserManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    string confirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(appUser);
                    string emailConfirmURL = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = appUser.Id,
                        token = confirmationToken
                    }, protocol: HttpContext.Request.Scheme);
                    Helpers.MailSender.SendMail(user.Email, "Email Doğrulama", emailConfirmURL);
                    return RedirectToAction("LogIn");
                }
                else
                {
                    AddModelError(result);
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
                Helpers.MailSender.SendMail(user.Email, "Şifre Yenileme", passwordResetLink);
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
        public async Task<IActionResult> ResetPasswordConfirm([Bind("Password")] ResetPasswordViewModel resetPasswordViewModel)
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
                        AddModelError(result);
                    }
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Linkin süresi dolmuştur. Yeni link alınız.");
            }
            return View(resetPasswordViewModel);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            UserApp user = await GetUserByIdAsync(userId);

            IdentityResult result = await UserManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.Status = "Email adresiniz doğrulanmıştır.";
            }
            else
            {
                ViewBag.Status = "Email adresini doğrulanamamıştır.";
            }
            return View();
        }


        public IActionResult LoginFacebook(string returnURL)
        {
            string redirectURL = Url.Action("ExternalResponse", "Home", new { returnURL = returnURL });
            var property = SignInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectURL);

            return new ChallengeResult("Facebook", property);
        }

        public IActionResult LoginGoogle(string returnURL)
        {
            string redirectURL = Url.Action("ExternalResponse", "Home", new { returnURL = returnURL });
            var property = SignInManager.ConfigureExternalAuthenticationProperties("Google", redirectURL);

            return new ChallengeResult("Google", property);
        }

        public IActionResult LoginMicrosoft(string returnURL)
        {
            string redirectURL = Url.Action("ExternalResponse", "Home", new { returnURL = returnURL });
            var property = SignInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectURL);

            return new ChallengeResult("Microsoft", property);
        }


        public async Task<IActionResult> ExternalResponse(string returnURL = "/") 
        {
            ExternalLoginInfo userInfo = await SignInManager.GetExternalLoginInfoAsync();
            if (userInfo == null)
            {
                return RedirectToAction("LogIn");
            }
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await SignInManager.ExternalLoginSignInAsync(userInfo.LoginProvider, userInfo.ProviderKey, true);
                if (result.Succeeded)
                {
                    //TODO EmailConfirm
                    return Redirect(returnURL);
                }
                else
                {
                    string externalUserName = userInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    UserApp user = new UserApp()
                    {
                        Email = userInfo.Principal.FindFirst(ClaimTypes.Email).Value,
                        BirthDate = Convert.ToDateTime(userInfo.Principal.FindFirst(ClaimTypes.DateOfBirth)?.Value)
                    };
                    if (userInfo.Principal.HasClaim(c => c.Type == ClaimTypes.Name))
                    {
                        string userName = userInfo.Principal.FindFirst(ClaimTypes.Name).Value.Replace(" ", string.Empty).ToLower() + externalUserName.Substring(0, 5);
                        user.UserName = userName;
                    }
                    else
                    {
                        user.UserName = user.Email;
                    }

                    UserApp userApp = await GetUserByEmailAsync(user.Email);
                    if (userApp == null)
                    {
                        IdentityResult createResult = await UserManager.CreateAsync(user);

                        if (createResult.Succeeded)
                        {
                            IdentityResult loginResult = await UserManager.AddLoginAsync(user, userInfo);
                            if (loginResult.Succeeded)
                            {
                                //TODO EmailConfirm
                                await SignInManager.ExternalLoginSignInAsync(userInfo.LoginProvider, userInfo.ProviderKey, true);

                                return Redirect(returnURL);
                            }
                            else
                            {
                                AddModelError(loginResult);
                            }
                        }
                        else
                        {
                            AddModelError(createResult);
                        }
                    }
                    else
                    {
                        IdentityResult loginResult = await UserManager.AddLoginAsync(userApp, userInfo);
                        if (loginResult.Succeeded)
                        {
                            //TODO EmailConfirm
                            await SignInManager.ExternalLoginSignInAsync(userInfo.LoginProvider, userInfo.ProviderKey, true);

                            return Redirect(returnURL);
                        }
                        else
                        {
                            AddModelError(loginResult);
                        }
                    }
                }
            }
            List<string> errors = ModelState.Values.SelectMany(e => e.Errors).Select(r => r.ErrorMessage).ToList();
            return View("Error", errors);
        }

        public ActionResult Error()
        {
            return View();
        }

    }
}
