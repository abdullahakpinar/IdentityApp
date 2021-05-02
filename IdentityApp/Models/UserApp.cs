using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.Models
{
    public class UserApp : IdentityUser
    {
        [MaxLength(75)]
        public string City { get; set; }
        public string PictureURL { get; set; }
        public DateTime? BirthDate { get; set; }
        public int Gender { get; set; }

        public sbyte TwoFactorAuthType { get; set; }

    }
}
