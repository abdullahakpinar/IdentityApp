using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.ViewModels
{
    public class ResetUserPasswordByAdminViewModel
    {
        public string UserId { get; set; }
        [Display(Name ="Yeni Şifre")]
        [DataType(DataType.Password)]
        public string PasswordNew { get; set; }
    }
}
