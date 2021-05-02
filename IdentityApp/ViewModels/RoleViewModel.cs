using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.ViewModels
{
    public class RoleViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage ="Role ismi zorunludur.")]
        [Display(Name = "Rol İsmi")]
        public string Name { get; set; }
    }
}
