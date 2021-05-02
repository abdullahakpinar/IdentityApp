using IdentityApp.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.ViewModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage ="Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }
        
        [RegularExpression(pattern: @"^(0(\d{3}) (\d{3}) (\d{2}) (\d{2}))$", ErrorMessage = "Telefon numarası uygun formatta değildir.")]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Email zorunludur.")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage ="Email adres formatı yanlıştır.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name ="Şehir")]
        public string City { get; set; }
        [Display(Name = "Resim")]
        public string PictureURL { get; set; }
        [Display(Name ="Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        [Display(Name = "Cinsiyet")]
        public Gender Gender { get; set; }

    }
}
