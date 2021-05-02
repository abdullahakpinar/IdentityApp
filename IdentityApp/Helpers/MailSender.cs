using IdentityApp.Models;
using System.Collections.Generic;

namespace IdentityApp.Helpers
{
    public static class MailSender
    {
        public static void SendMail(string mailTO, string mailSubject, string mailBody)
        {
            AkpinarMailSender sender = new AkpinarMailSender("", 587);
            SendMailRequest sendMailRequest = new SendMailRequest
            {
                EnableSSL = true,
                SenderMailAddress = "",
                SenderPassword = "",
                MailTo = new List<string>() { mailTO },
                MailSubject = @mailSubject,
                MailBody = @mailBody,
                IsBodyHtml = true
            };
            sender.SendMail(sendMailRequest);
        }
    }
}
