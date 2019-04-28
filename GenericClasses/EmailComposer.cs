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
        private static string fromEmail;
        private static List<string> toEmails;
        //SecureString emailPassword;
        private static string emailPassword;

        public static void InitializeGmailComposer(string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com");
            InitializeMailComposer(client, subject, message);
        }

        public static void InitializeOutlookComposer(string subject, string message)
        {
            var client = new SmtpClient("smtp-mail.outlook.com");
            InitializeMailComposer(client, subject, message);
        }

        public static void InitializeMailComposer(SmtpClient client, string subject, string message)
        {
            InitializeEmailAndPassword();

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

            client.Port = 587;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(fromEmail, emailPassword);
            //try
            //{
                client.Send(mail);
            //}
            //catch (SmtpException)
            //{
            //    Console.WriteLine("We have run into an exception.");
            //    Console.ReadLine();
            //}
        }

        public static void InitializeEmailAndPassword()
        {
            if (fromEmail != null && emailPassword != null)
            {
                return;
            }

            Console.WriteLine("AA");
            DirectoryBuilder.InitializeMainAsGrandparentDirectory();
            try
            {
                using (StreamReader r = new StreamReader(
                    Path.Combine(DirectoryBuilder.mainDirectory, "email_settings.json")))
                {
                    ReadEmailAndPassword(r);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine(DirectoryBuilder.mainDirectory);
                UpdateEmailIfNull();
                UpdatePasswordIfNull();
            }
            SaveEmailAndPassword();
        }

        private static void ReadEmailAndPassword(StreamReader r)
        {
            var json = r.ReadToEnd();
            var jsonDataObject = JsonConvert.DeserializeObject<EmailJsonDataObject>(json);
            fromEmail = jsonDataObject.EmailAddress;
            emailPassword = jsonDataObject.Password;
            Console.WriteLine(fromEmail + "  " + emailPassword);
            
            if (jsonDataObject.EmailAddress is null)
            {
                UpdateEmailIfNull();
            }
            if (jsonDataObject.Password is null)
            {
                UpdatePasswordIfNull();
            }
        }

        private static void UpdateEmailIfNull()
        {
            if (fromEmail is null)
            {
                Console.WriteLine("Please enter your email address that we will be sending email from.");
                fromEmail = Console.ReadLine();
            }
        }

        private static void UpdatePasswordIfNull()
        {
            if (emailPassword is null)
            {
                Console.WriteLine($"Please enter the password to the email address {fromEmail}");
                //GetUserInputPassword();
                emailPassword = Console.ReadLine();
            }
        }

        private static void GetUserInputPassword()
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

        private static void SaveEmailAndPassword()
        {
            var jsonDataObject = new EmailJsonDataObject { EmailAddress = fromEmail, Password = emailPassword };
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
