using IdentityApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdentityApp.Helpers
{
    public class AkpinarMailSender
    {
        private string _hostAddress;
        private int _hostPost;
        public AkpinarMailSender(string hostAddress, int hostPost)
        {
            _hostAddress = hostAddress;
            _hostPost = hostPost;
        }
        private SmtpClient MailSenderCredential(string userName, string password, bool enableSSL)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(_hostAddress, _hostPost);
                smtpClient.Credentials = new NetworkCredential(userName, password);
                smtpClient.EnableSsl = enableSSL;
                return smtpClient;
            }
            catch (Exception ex)
            {
                throw new Exception($"(MailSenderCredential)Mail gönderici ayarları yapılırken hata oluştu.\nHata : {ex.Message}");
            }
        }
        private void CheckRequiredField(SendMailRequest request)
        {
            if (string.IsNullOrEmpty(request.SenderMailAddress))
            {
                throw new Exception("SenderMailAddress alanını giriniz.");
            }
            else
            {
                if (!IsValidEmailWithRegex(request.SenderMailAddress))
                {
                    throw new Exception("SenderMailAddress alanındaki mail adresi formata uygun değildir.");
                }
            }
            if (string.IsNullOrEmpty(request.SenderPassword))
            {
                throw new Exception("SenderPassword alanını giriniz.");
            }
            if (string.IsNullOrEmpty(request.MailSubject))
            {
                throw new Exception("MailSubject alanını giriniz.");
            }
            if (string.IsNullOrEmpty(request.MailBody))
            {
                throw new Exception("MailBody alanını giriniz.");
            }
            if (request.MailTo == null)
            {
                throw new Exception("MailTo alanında en az 1 adet mail adresi olmak zorundadır.");
            }
            else
            {
                foreach (string mailTo in request.MailTo)
                {
                    if (!IsValidEmailWithRegex(mailTo))
                    {
                        throw new Exception($"MailTo alanındaki {mailTo} mail adresi formata uygun değildir.");
                    }
                }
            }
            if (request.MailCC != null)
            {
                foreach (string mailCC in request.MailCC)
                {
                    if (!IsValidEmailWithRegex(mailCC))
                    {
                        throw new Exception($"MailCC alanındaki { mailCC } mail adresi formata uygun değildir.");
                    }
                }
            }
            if (request.MailBCC != null)
            {
                foreach (string mailBCC in request.MailBCC)
                {
                    if (!IsValidEmailWithRegex(mailBCC))
                    {
                        throw new Exception($"MailBCC alanındaki {mailBCC} mail adresi formata uygun değildir.");
                    }
                }
            }
        }
        private bool IsValidEmailWithRegex(string mailAddress)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(mailAddress);
            return match.Success;
        }
        private MailMessage CreateMessage(SendMailRequest request)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(request.SenderMailAddress);
                foreach (string mailTo in request.MailTo)
                {
                    mailMessage.To.Add(mailTo);
                }
                if (request.MailCC != null)
                {
                    foreach (string mailCC in request.MailCC)
                    {
                        mailMessage.CC.Add(mailCC);
                    }
                }
                if (request.MailBCC != null)
                {
                    foreach (string mailBCC in request.MailBCC)
                    {
                        mailMessage.To.Add(mailBCC);
                    }
                }
                mailMessage.Subject = request.MailSubject;
                mailMessage.Body = request.MailBody;
                mailMessage.IsBodyHtml = request.IsBodyHtml;
                if (!string.IsNullOrEmpty(request.MailSignature))
                {
                    mailMessage.Body += request.MailSignature;
                }
                return mailMessage;
            }
            catch (Exception ex)
            {

                throw new Exception($"(CreateMessage)Mail oluşturulurken hata oluştu.\nHata : {ex.Message}");
            }
        }
        private void AddToNotepad(string filePath, string content)
        {
            try
            {
                using (StreamWriter sWrite = new StreamWriter(filePath, true))
                {
                    sWrite.WriteLine(content);
                    sWrite.WriteLine("\n");
                    sWrite.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("(AddToNotepad)Not deftarine kayıt yapılırken hata oluştu.\nHata : " + ex.Message);
            }
        }
        public void SendMail(SendMailRequest request)
        {
            try
            {
                CheckRequiredField(request);
                SmtpClient smtp = MailSenderCredential(request.SenderMailAddress, request.SenderPassword, request.EnableSSL);
                MailMessage mailMessage = CreateMessage(request);
                smtp.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"(SendMail)Mail gönderilirken hata oluştu.\nHata : {ex.Message}");
            }
        }
    }
}
