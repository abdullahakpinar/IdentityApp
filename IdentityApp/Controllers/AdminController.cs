using IdentityApp.Models;
using IdentityApp.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(UserManager<UserApp> userManager, RoleManager<RoleApp> roleManager) : base(userManager, roleManager)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View(UserManager.Users.ToList());
        }

        public IActionResult Roles()
        {
            return View(RoleManager.Roles.ToList());
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateRole(RoleViewModel roleViewModel)
        {
            RoleApp role = new RoleApp
            {
                Name = roleViewModel.Name
            };
            IdentityResult result = RoleManager.CreateAsync(role).Result;
            if (result.Succeeded)
            {
                ViewBag.Success = true;
                return RedirectToAction("Roles");
            }
            else
            {
                AddModelError(result);
            }
            return View();
        }

        public IActionResult DeleteRole(string id)
        {
            RoleApp role = GetRoleById(id);
            if (role != null)
            {
                IdentityResult result = RoleManager.DeleteAsync(role).Result;
                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
            }
            return RedirectToAction("Roles");
        }

        public IActionResult UpdateRole(string id)
        {
            RoleApp role = GetRoleById(id);
            if (role != null)
            {
                return View(role.Adapt<RoleViewModel>());
            }
            return RedirectToAction("Roles");
        }

        [HttpPost]
        public IActionResult UpdateRole(RoleViewModel roleViewModel)
        {
            RoleApp role = GetRoleById(roleViewModel.Id);
            if (role != null)
            {
                role.Name = roleViewModel.Name;
                IdentityResult result = RoleManager.UpdateAsync(role).Result;
                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Rol güncellenemedi.");
            }
            return View(roleViewModel);
        }

        public IActionResult AssignRole(string id)
        {
            TempData["UserId"] = id;
            UserApp user = GetUserById(id);
            ViewBag.UserName = user.UserName;

            IQueryable<RoleApp> roles = RoleManager.Roles;

            List<string> userRoles = UserManager.GetRolesAsync(user).Result as List<string>;

            List<AssignRoleViewModel> assignRoleViewModels = new List<AssignRoleViewModel>();
            foreach (RoleApp role in roles)
            {
                AssignRoleViewModel assignRoleVM = new AssignRoleViewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsExist = false
                };
                if (userRoles.Contains(role.Name))
                {
                    assignRoleVM.IsExist = true;
                }
                assignRoleViewModels.Add(assignRoleVM);
            }

            return View(assignRoleViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(List<AssignRoleViewModel> assignRoleViewModels)
        {
            UserApp user = GetUserById(TempData["UserId"].ToString());
            foreach (AssignRoleViewModel assignRole in assignRoleViewModels)
            {
                if (assignRole.IsExist)
                {
                    await UserManager.AddToRoleAsync(user, assignRole.RoleName);
                }
                else
                {
                    await UserManager.RemoveFromRoleAsync(user, assignRole.RoleName);
                }
            }
            return RedirectToAction("Users");
        }

        public IActionResult Claims()
        {
            return View(User.Claims.ToList());
        }

        public async Task<IActionResult> ResetUserPassword(string id)
        {
            UserApp user = await GetUserByIdAsync(id);
            if (user != null)
            {
                ResetUserPasswordByAdminViewModel resetUserPassword = new ResetUserPasswordByAdminViewModel();
                resetUserPassword.UserId = user.Id;
                return View(resetUserPassword);
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(ResetUserPasswordByAdminViewModel resetViewModel)
        {
            UserApp user = await GetUserByIdAsync(resetViewModel.UserId);
            string token = await UserManager.GeneratePasswordResetTokenAsync(user);

            IdentityResult result = await UserManager.ResetPasswordAsync(user, token, resetViewModel.PasswordNew);
            if (!result.Succeeded)
            {
                AddModelError(result);
                return View(resetViewModel);
            }
            await UserManager.UpdateSecurityStampAsync(user);
            return RedirectToAction("Users");
        }
    }

}
