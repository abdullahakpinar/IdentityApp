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
using Microsoft.AspNetCore.Mvc.Rendering;
using IdentityApp.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;
using IdentityApp.Services.TwoFactorServices;

namespace IdentityApp.Controllers
{
    [Authorize]
    public class MemberController : BaseController
    {
        private readonly TwoFactorService _twoFactorService;
        public MemberController(UserManager<UserApp> userManager, SignInManager<UserApp> signInManager, TwoFactorService twoFactorService) : base(userManager, signInManager)
        {
            _twoFactorService = twoFactorService;
        }

        public IActionResult Index()
        {
            UserApp user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {

            if (ModelState.IsValid)
            {
                if (changePasswordViewModel.PasswordNew != changePasswordViewModel.PasswordOld)
                {
                    UserApp user = CurrentUser;
                    if (user != null)
                    {
                        bool checkPassword = UserManager.CheckPasswordAsync(user, changePasswordViewModel.PasswordOld).Result;
                        if (checkPassword)
                        {
                            IdentityResult result = UserManager.ChangePasswordAsync(user, changePasswordViewModel.PasswordOld, changePasswordViewModel.PasswordNew).Result;
                            if (result.Succeeded)
                            {
                                await UserManager.UpdateSecurityStampAsync(user);
                                //SignOut After LogIn
                                await SignInManager.SignOutAsync();
                                await SignInManager.PasswordSignInAsync(user.UserName, changePasswordViewModel.PasswordNew, true, false);

                                ViewBag.Status = true;
                            }
                            else
                            {
                                AddModelError(result);
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
            UserApp user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));


            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserViewModel userViewModel, IFormFile userPicture)
        {
            ModelState.Remove("Password");
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            if (ModelState.IsValid)
            {
                UserApp user = CurrentUser;

                if (user.PhoneNumber != userViewModel.PhoneNumber)
                {
                    if (UserManager.Users.Any(u => u.PhoneNumber == userViewModel.PhoneNumber))
                    {
                        ModelState.AddModelError(userViewModel.PhoneNumber, "Telefon numarası sistemde kayıtlıdır.");
                        return View(userViewModel);
                    }
                }

                if (userPicture != null && userPicture.Length > 0)
                {
                    string picturePath = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/UserImages", picturePath);
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(fileStream);
                        user.PictureURL = "/Images/UserImages/" + picturePath;
                    }
                }

                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;
                user.City = userViewModel.City;
                user.BirthDate = userViewModel.BirthDate;
                user.Gender = (int)userViewModel.Gender;

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
                    AddModelError(result);
                }

            }

            return View(userViewModel);
        }

        public void LogOut()
        {
            SignInManager.SignOutAsync();
        }

        public IActionResult AccessDenied(string returnUrl)
        {
            if (returnUrl.ToLower().Contains("violence"))
            {
                ViewBag.Message = "Erişilmek istenen sayfa için yaş sınırından dolayı erişim izniniz yok.";
            }
            else if (returnUrl.ToLower().Contains("exchange"))
            {
                ViewBag.Message = "Ücretsiz deneme süreniz sona ermiştir.";
            }

            return View();
        }


        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Editor()
        {
            return View();
        }

        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Manager()
        {
            return View();
        }

        [Authorize(Policy = "SomeCityPolicy")]
        public IActionResult OnlySomeCity()
        {
            return View();
        }

        [Authorize(Policy = "ViolencePolicy")]
        public IActionResult Violence()
        {
            return View();
        }

        public async Task<IActionResult> ExchangeRedirect()
        {
            bool result = User.HasClaim(c => c.Type == "ExpireDateExchange");
            if (!result)
            {
                Claim exchangeClaim = new Claim("ExpireDateExchange", DateTime.Now.AddDays(30).ToShortDateString(), ClaimValueTypes.String, "Internal");
                await UserManager.AddClaimAsync(CurrentUser, exchangeClaim);
                await SignInManager.SignOutAsync();
                await SignInManager.SignInAsync(CurrentUser, true);
            }
            return RedirectToAction("Exchange");
        }

        [Authorize(Policy = "ExchangePolicy")]
        public IActionResult Exchange()
        {
            return View();
        }

        public IActionResult TwoFactorAuthentication()
        {
            return View(new AuthenticatorViewModel() { TwoFactorTypes = (TwoFactorAuthTypes)CurrentUser.TwoFactorAuthType });
        }

        [HttpPost]
        public async Task<IActionResult> TwoFactorAuthentication(AuthenticatorViewModel authenticatorViewModel)
        {
            switch (authenticatorViewModel.TwoFactorTypes)
            {
                case TwoFactorAuthTypes.None:
                    CurrentUser.TwoFactorEnabled = false;
                    TempData["TwoFactorTypeMessage"] = "İki adımlı kimlik doğrulama aktif değildir.";
                    break;
                case TwoFactorAuthTypes.SMS:
                    if (string.IsNullOrEmpty(CurrentUser.PhoneNumber))
                    {
                        CurrentUser.TwoFactorEnabled = false;
                        ViewBag.Message = "Telefon numaranız sistemde kayıtlı olmadığı için SMS ile doğrulama yapamazsınız.";
                    }
                    else
                    {
                        CurrentUser.TwoFactorEnabled = true;
                        TempData["TwoFactorTypeMessage"] = "İki adımlı kimlik doğrulama SMS olarak aktif hale getirilmiştir.";
                    }
                    break;
                case TwoFactorAuthTypes.Email:
                    CurrentUser.TwoFactorEnabled = true;
                    TempData["TwoFactorTypeMessage"] = "İki adımlı kimlik doğrulama email olarak aktif hale getirilmiştir.";
                    break;
                case TwoFactorAuthTypes.MicrosoftGoogle:
                    return RedirectToAction("TwoFactorWithAuthenticator");
                default:
                    break;
            } 
            CurrentUser.TwoFactorAuthType = (sbyte)authenticatorViewModel.TwoFactorTypes;
            IdentityResult result = await UserManager.UpdateAsync(CurrentUser);
            if (!result.Succeeded)
            {
                AddModelError(result);
            }
            return View(authenticatorViewModel);
        }

        public async Task<IActionResult> TwoFactorWithAuthenticator()
        {
            string unFormattedKey = await UserManager.GetAuthenticatorKeyAsync(CurrentUser);
            if (string.IsNullOrEmpty(unFormattedKey))
            {
                await UserManager.ResetAuthenticatorKeyAsync(CurrentUser);
                unFormattedKey = await UserManager.GetAuthenticatorKeyAsync(CurrentUser);
            }
            AuthenticatorViewModel authenticatorViewModel = new AuthenticatorViewModel()
            {
                SharedKey = unFormattedKey,
                AuthenticatorUri = _twoFactorService.GenerateQRCodeUri(CurrentUser.Email, unFormattedKey)
            };

            return View(authenticatorViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> TwoFactorWithAuthenticator(AuthenticatorViewModel authenticatorViewModel)
        {

            var verificationCode = authenticatorViewModel.VerificationCode.Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty);

            bool is2FATokenValid = await UserManager.VerifyTwoFactorTokenAsync(CurrentUser, UserManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (is2FATokenValid)
            {
                CurrentUser.TwoFactorEnabled = true;
                CurrentUser.TwoFactorAuthType = (sbyte)TwoFactorAuthTypes.MicrosoftGoogle;
                var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(CurrentUser, 5);
                TempData["TwoFactorRecoveryCodes"] = recoveryCodes;
                TempData["TwoFactorTypeMessage"] = "İki adımlı doğrulama başarı ile sağlanmıştır.";
                return RedirectToAction("TwoFactorAuthentication");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Girilen doğrulama kodu yanlıştır.");
            }
            return View();
        }
    }
}
