using IdentityApp.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.ViewModels
{
    public class LoginTwoFactorViewModel
    {
        [Display(Name = "Doğrulama Kodu")]
        [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
        [StringLength(maximumLength: 8, ErrorMessage = "Maximum karakter sayısını aştınız.")]
        public string VerificationCode { get; set; }

        public bool IsRememberMe { get; set; }
        public bool IsRecoverCode { get; set; }
        public TwoFactorAuthTypes TwoFactorType { get; set; }

    }
}
