using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IdentityApp.Helpers
{
    public static class ResetPassword
    {
        public static void PasswordResetSendEmail(string link)
        {
           AkpinarMailSender sender = new AkpinarMailSender("",2);
        }
    }
}
