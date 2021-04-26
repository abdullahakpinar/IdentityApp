using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Yeni Şifre alanı zorunludur.")]
        [Display(Name = "Yeni Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
