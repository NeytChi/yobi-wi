using System;
using System.Net;
using System.Net.Mail;
using Newtonsoft.Json.Linq;

namespace Common
{
    public static class MailF
    {
        private static string GmailServer = "smtp.gmail.com";
        private static int GmailPort = 587;
        private static string ip = "127.0.0.1";
        private static string domen = "minimessanger";
        private static string mailAddress = "test6574839210@gmail.com";
        private static string mailPassword = "GmailPassword1234";
        private static MailAddress from = new MailAddress(mailAddress, domen);
        private static SmtpClient smtp;

        public static void Init()
        {
            ip = Config.IP;
            domen = Config.Domen;
            mailAddress = Config.GetServerConfigValue("mail_address", JTokenType.String);
            mailPassword = Config.GetServerConfigValue("mail_password", JTokenType.String);
            GmailServer = Config.GetServerConfigValue("smtp_server", JTokenType.String);
            GmailPort = Config.GetServerConfigValue("smtp_port", JTokenType.Integer);
            if (ip != null && mailAddress != null)
            {
                smtp = new SmtpClient(GmailServer, GmailPort);
                smtp.Credentials = new NetworkCredential(mailAddress, mailPassword);
                from = new MailAddress(mailAddress, domen);
                smtp.EnableSsl = true;
            }
        }
        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="emailAddress">Email address.</param>
        /// <param name="subject">Subject.</param>
        /// <param name="message">Message.</param>
        public static async void SendEmail(string emailAddress, string subject, string text)
        {
            MailAddress to = new MailAddress(emailAddress);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = text;
            message.IsBodyHtml = true;
            try
            {
                if (smtp == null)
                {
                    smtp = new SmtpClient(GmailServer, GmailPort);
                    smtp.Credentials = new NetworkCredential(mailAddress, mailPassword);
                    from = new MailAddress(mailAddress, domen);
                    smtp.EnableSsl = true;
                }
                await smtp.SendMailAsync(message);
                Log.Info("Send message to " + emailAddress);
            }
            catch (Exception e)
            {
                Log.Error("Error SendEmail, email ->" + emailAddress + " , Message:" + e.Message);
            }
        }
    }
}
