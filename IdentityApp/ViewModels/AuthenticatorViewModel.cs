using IdentityApp.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.ViewModels
{
    public class AuthenticatorViewModel
    {
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
        [Display(Name = "Doğrulama Kodu")]
        [Required(ErrorMessage = "Doğrulama Kodu Zorunludur.")]
        public string VerificationCode { get; set; }

        [Display(Name ="İki Adımlı Kimlik Doğrulama Türü")]
        public TwoFactorAuthTypes TwoFactorTypes { get; set; }
    }
}
