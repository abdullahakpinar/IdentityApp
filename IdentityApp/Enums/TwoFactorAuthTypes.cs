using System.ComponentModel.DataAnnotations;

namespace IdentityApp.Enums
{
    public enum TwoFactorAuthTypes
    {
        [Display(Name ="Kullanılmayacak")]
        None = 0,
        [Display(Name ="SMS ile kimlik doğrulama")]
        SMS = 1,
        [Display(Name ="Email ile kimlik doğrulama")]
        Email = 2,
        [Display(Name ="Microsoft / Google Authenticator ile kimlik doğrulama")]
        MicrosoftGoogle = 3
    }
}
