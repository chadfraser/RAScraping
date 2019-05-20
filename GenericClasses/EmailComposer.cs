using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security;

namespace Fraser.GenericMethods
{
    public class EmailComposer
    {
        private string fromEmail;
        private List<string> toEmails;
        //SecureString emailPassword;
        private string emailPassword;
        private SmtpClient client;

        public void InitializeGmailComposer()
        {
            client = new SmtpClient("smtp.gmail.com");
            InitializeMailComposer();
        }

        public void InitializeGmailComposer(string toEmail)
        {
            toEmails = new List<string> { toEmail };
            InitializeGmailComposer();
        }

        public void InitializeOutlookComposer()
        {
            client = new SmtpClient("smtp-mail.outlook.com");
            InitializeMailComposer();
        }

        public void InitializeOutlookComposer(string toEmail)
        {
            toEmails = new List<string> { toEmail };
            InitializeOutlookComposer();
        }

        private void InitializeMailComposer()
        {
            InitializeProperties();

            client.Port = 587;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(fromEmail, emailPassword);
        }

        public void InitializeProperties()
        {
            if (fromEmail != null && emailPassword != null && toEmails.Count == 0)
            {
                return;
            }

            DirectoryBuilder.InitializeMainAsGrandparentDirectory();
            try
            {
                using (StreamReader r = new StreamReader(
                    Path.Combine(DirectoryBuilder.mainDirectory, "email_settings.json")))
                {
                    ReadProperties(r);
                }
            }
            catch (FileNotFoundException)
            {
                UpdateEmailIfNull();
                UpdatePasswordIfNull();
                UpdateToEmailsIfNullOrEmpty();
            }
            SaveEmailProperties();
        }

        private void ReadProperties(StreamReader r)
        {
            var json = r.ReadToEnd();
            var jsonDataObject = JsonConvert.DeserializeObject<EmailJsonDataObject>(json);
            fromEmail = jsonDataObject.EmailAddress;
            emailPassword = jsonDataObject.Password;
            toEmails = jsonDataObject.RecepientEmailAddresses;
            
            if (jsonDataObject.EmailAddress is null)
            {
                UpdateEmailIfNull();
            }
            if (jsonDataObject.Password is null)
            {
                UpdatePasswordIfNull();
            }
            if (toEmails is null || toEmails.Count == 0)
            {
                UpdateToEmailsIfNullOrEmpty();
            }
        }

        private void UpdateEmailIfNull()
        {
            if (fromEmail is null)
            {
                Console.WriteLine("Please enter your email address that we will be sending email from.");
                fromEmail = Console.ReadLine();
            }
        }

        private void UpdatePasswordIfNull()
        {
            if (emailPassword is null)
            {
                Console.WriteLine($"Please enter the password to the email address {fromEmail}");
                //GetUserInputPassword();
                emailPassword = Console.ReadLine();
            }
        }

        private void UpdateToEmailsIfNullOrEmpty()
        {
            string input;
            if (toEmails is null || toEmails.Count == 0)
            {
                toEmails = new List<string>();
                Console.WriteLine("Please enter all emails that you wish to send the message to. End with a blank line.");
                do
                {
                    input = Console.ReadLine();
                    toEmails.Add(input);
                }
                while (!string.IsNullOrEmpty(input));

                if (toEmails.Count > 0 && string.IsNullOrEmpty(toEmails[toEmails.Count - 1]))
                {
                    toEmails.RemoveAt(toEmails.Count - 1);
                }
            }
        }

        private void GetUserInputPassword()
        {
            //ConsoleKeyInfo key;

            //do
            //{
            //    key = Console.ReadKey(true);

            //    // Implement backspace capabilities
            //    if (((int)key.Key) == 8)
            //    {
            //        var passwordLength = emailPassword.Length;
            //        if (passwordLength > 0)
            //        {
            //            emailPassword.RemoveAt(passwordLength - 1);
            //        }
            //    }

            //    // Ignore any key out of range.
            //    if (((int)key.Key) >= 32 && ((int)key.Key <= 165))
            //    {
            //        // Append the character to the password.
            //        emailPassword.AppendChar(key.KeyChar);
            //        Console.Write("*");
            //    }

            //    // Exit if Enter key is pressed.
            //} while (key.Key != ConsoleKey.Enter);
        }

        public void SendEmail(string subject, string message)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = message
            };
            foreach (var email in toEmails)
            {
                mail.To.Add(new MailAddress(email));
            }

            try
            {
                client.Send(mail);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine("We have run into an exception. Either your email and password are incorrect, " +
                    "or your email is not set up to send messages through SMTP.");
                throw ex;
            }
        }

        private void SaveEmailProperties()
        {
            var jsonDataObject = new EmailJsonDataObject { EmailAddress = fromEmail,
                Password = emailPassword,
                RecepientEmailAddresses = toEmails };
            string jsonSerialize = JsonConvert.SerializeObject(jsonDataObject, Formatting.Indented);
            File.WriteAllText(Path.Combine(DirectoryBuilder.mainDirectory, "email_settings.json"), jsonSerialize);
        }
    }

    public class EmailJsonDataObject
    {
        public string EmailAddress;
        public string Password;
        public List<string> RecepientEmailAddresses;
        //public SecureString password;
    }
}
