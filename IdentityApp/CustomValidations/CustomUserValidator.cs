using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdentityApp.CustomValidations
{
    public class CustomUserValidator : IUserValidator<UserApp>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<UserApp> manager, UserApp user)
        {
            List<IdentityError> errors = new List<IdentityError>();
            if (user.UserName.Length < 3)
            {
                errors.Add(new IdentityError { Code = "UserNameLength", Description = "Kullanıcı adı 3 karakterden az olamaz." });
            }
            if (IsFirstLetterNumber(user.UserName))
            {
                errors.Add(new IdentityError { Code = "UserFirsLetterNumber", Description = "Kullanıcı adı rakam ile başlayamaz." });
            }
            if (!IsValidateEmailFormat(user.Email))
            {
                errors.Add(new IdentityError { Code = "UserEmailFormat", Description = "Email uygun formatta değil." });
            }
            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }
        }

        private static bool IsFirstLetterNumber(string value)
        {
            return int.TryParse(value[0].ToString(), out _);
        }

        private static bool IsValidateEmailFormat(string emailAddress)
        {
            return Regex.IsMatch(emailAddress, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }
    }
}
