using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.Models
{
    public class SendMailRequest
    {
        public SendMailRequest()
        {
        }
        public string SenderMailAddress { get; set; }
        public string SenderPassword { get; set; }
        public List<string> MailTo { get; set; }
        public List<string> MailCC { get; set; }
        public List<string> MailBCC { get; set; }
        public string MailSubject { get; set; }
        public string MailBody { get; set; }
        public bool IsBodyHtml { get; set; }
        public bool EnableSSL { get; set; }
        public string MailSignature { get; set; }
    }
}
