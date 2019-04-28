using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security;

namespace Fraser.GenericMethods
{
    public class EmailComposer
    {
        private string fromEmail;
        //SecureString emailPassword;
        private string emailPassword;

        public void InitializeGmailComposer(string toEmail, string subject, string message)
        {
            var mail = new MailMessage();
            var SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress(fromEmail);
            mail.To.Add(new MailAddress(toEmail));
            mail.Subject = subject;
            mail.Body = message;

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new NetworkCredential(fromEmail, emailPassword);
        }

        public void InitializeEmailAndPassword()
        {
            if (fromEmail != null && emailPassword != null)
            {
                return;
            }

            DirectoryBuilder.InitializeMainAsParentDirectory();
            try
            {
                using (StreamReader r = new StreamReader(
                    Path.Combine(DirectoryBuilder.mainDirectory, "email_setting.json")))
                {
                    var json = r.ReadToEnd();
                    EmailJsonDataObject jsonDataObject = JsonConvert.DeserializeObject<EmailJsonDataObject>(json);
                    if (jsonDataObject.FromEmail is null)
                    {
                        UpdateEmailIfNull();
                    }
                    if (jsonDataObject.password is null)
                    {
                        UpdatePasswordIfNull();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                UpdateEmailIfNull();
                UpdatePasswordIfNull();
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

        public void SaveEmailAndPassword()
        {
            var jsonDataObject = new EmailJsonDataObject { FromEmail = fromEmail, password = emailPassword };
            string jsonSerialize = JsonConvert.SerializeObject(jsonDataObject, Formatting.Indented);
            File.WriteAllText(Path.Combine(DirectoryBuilder.mainDirectory, "email_settings.json"), jsonSerialize);
        }
    }

    public class EmailJsonDataObject
    {
        public string FromEmail;
        public string password;
        //public SecureString password;
    }
}
