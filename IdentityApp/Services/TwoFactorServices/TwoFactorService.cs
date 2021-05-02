using IdentityApp.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace IdentityApp.Services.TwoFactorServices
{
    public class TwoFactorService
    {
        private readonly UrlEncoder _urlEncoder;
        private readonly TwoFactorOptions _twoFactorOptions;
        public TwoFactorService(UrlEncoder urlEncoder, IOptions<TwoFactorOptions> options)
        {
            _urlEncoder = urlEncoder;
            _twoFactorOptions = options.Value;
        }

        public string GenerateQRCodeUri(string email, string unFormattedKey)
        {
            const string format = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(format, _urlEncoder.Encode("www.website.com"), _urlEncoder.Encode(email), unFormattedKey);
        }

        public string GetCodeVerification()
        {
            return new Random().Next(1000, 9999).ToString();
        }

        public string SendMail(string email)
        {
            string createdCode = GetCodeVerification();
            //TODO MailSender.SendMail(email, "İki Adımlı Kimlik Doğrulama Kodu", "Kod : " + createdCode);
            return createdCode;
        }

        public string SendSMS(string phoneNumber)
        {
            string createdCode = GetCodeVerification();
            SMSSender.SendSMS(phoneNumber,"Doğrulama Kodu : " + createdCode);
            return "4141";
        }

        public int TimeLeft(HttpContext httpContext)
        {
            if (string.IsNullOrEmpty(httpContext.Session.GetString("CurrentTime")))
            {
                httpContext.Session.SetString("CurrentTime", DateTime.Now.AddSeconds(_twoFactorOptions.CodeTimeExpire).ToString());
            }
            DateTime currentTime = DateTime.Parse(httpContext.Session.GetString("CurrentTime"));
            int timeLeft = (int)(currentTime - DateTime.Now).TotalSeconds;
            if (timeLeft <= 0)
            {
                httpContext.Session.Remove("CurrentTime");
                return 0;
            }
            return timeLeft;
        }
    }
}
