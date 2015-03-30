using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;


namespace SalesForceApiUpdater.Console
{
    class SendAlert
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public string MessagePrefix { get; set; }
        public List<String> AlertCollection { get; set; }

        public void SendEmail(string EmailAddressList)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = this.Subject + " - " + DateTime.Now.ToString("MMMM dd, yyyy H:mm:ss");

            // Get the email addresses that want to be notified
            Array emailAddressArray = EmailAddressList.Split(',');
            foreach (var emailAddress in emailAddressArray)
            {
                mailMessage.To.Add(new MailAddress(emailAddress.ToString()));
            }

            // Add error messages to body of email
            StringBuilder sb = new StringBuilder();
            sb.Append(AlertCollection.Count + " " + this.MessagePrefix + " occurred</b><hr/></p>");
            foreach (var alertMessage in AlertCollection)
            {
                sb.Append(alertMessage);
                sb.Append("<hr/>");
            }
            mailMessage.Body = sb.ToString();

            // Send the email
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Send(mailMessage);
        }
    }
}
