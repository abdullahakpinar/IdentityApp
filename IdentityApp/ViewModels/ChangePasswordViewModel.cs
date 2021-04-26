using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Eski Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Eski Şifre")]
        public string PasswordOld { get; set; }
        [Required(ErrorMessage = "Yeni Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [MinLength(length: 5, ErrorMessage = "Şifre uzunluğu en az 5 karakter olmalıdır.")]
        public string PasswordNew { get; set; }
        [Required(ErrorMessage = "Onay Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Onay Şifre")]
        [MinLength(length: 5, ErrorMessage = "Şifre uzunluğu en az 5 karakter olmalıdır.")]
        [Compare("PasswordNew", ErrorMessage = "Yeni şifreniz ve onay şifreniz uyuşmamaktadır.")]
        public string PasswordConfirm { get; set; }
    }
}
