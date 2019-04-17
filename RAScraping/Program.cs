﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using Newtonsoft.Json;
//using testing;

namespace RAScraping
{
    class Program
    {
        private static readonly string userDataFilepath = "../../data/ra_user_data.json";
        private static readonly string absoluteUserDataPath = Path.GetFullPath(userDataFilepath);
        private static readonly string absoluteUserDataDirectory = Path.GetDirectoryName(absoluteUserDataPath);
        private static Boolean oneUserPerFile = true;

        static void Main(string[] args)
        {
            //TestData.Test();
            try
            {
                using (StreamReader r = new StreamReader("../../data/usernames.json"))
                {
                    var json = r.ReadToEnd();
                    RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(json);
                    if (rootObject.Usernames is null)
                    {
                        Console.WriteLine("The list of usernames in the usernames.json file is empty.");
                        Environment.Exit(0);
                    }
                    CreateAndWriteUserData(rootObject);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("The file usernames.json was not found in the appropriate data folder.");
                Environment.Exit(0);
            }
        }

        public static HtmlDocument LoadDocument(string url)
        {
            var website = new HtmlWeb();
            HtmlDocument doc = website.Load(url);
            return doc;
        }

        static void CreateAndWriteUserData(RootObject rootObject)
        {
            List<User> users = new List<User>();

            foreach (string username in rootObject.Usernames)
            {
                User newUser = BuildSingleUserData(username);

                if (oneUserPerFile)
                {
                    var newUserFullPath = Path.GetFullPath($"../../data/users/{newUser.Username}.json");
                    var newUserDataDirectory = Path.GetDirectoryName(newUserFullPath);
                    Directory.CreateDirectory(newUserDataDirectory);
                    if (!File.Exists($"../../data/users/{username}.json"))
                    {
                        WriteSingleUserData(newUser);
                    }
                    else
                    {
                        CompareSingleUserData(newUser);
                    }
                }
                users.Add(newUser);
            }

            if (!oneUserPerFile)
            {
                Directory.CreateDirectory(absoluteUserDataPath);
                if (!File.Exists(userDataFilepath))
                {
                    WriteAllUserData(users);
                }
                else
                {
                    CompareAllUserData(users);
                }
            }
        }

        static User BuildSingleUserData(string username)
        {
            var newUser = new User(username);

            HtmlDocument doc = LoadDocument(newUser.Url);
            newUser.FillCompletedGames(doc);
            return newUser;
        }

        static void WriteSingleUserData(User newUser)
        {

            string jsonSerialize = JsonConvert.SerializeObject(newUser, Formatting.Indented);
            File.WriteAllText($"../../data/users/{newUser.Username}.json", jsonSerialize);
        }

        static void CompareSingleUserData(User newUser)
        {
            User tempUser;
            using (StreamReader r = new StreamReader($"../../data/users/{newUser.Username}.json"))
            {
                var json = r.ReadToEnd();
                tempUser = JsonConvert.DeserializeObject<User>(json);
            }

            if (newUser != tempUser)
            {
                string jsonSerialize = JsonConvert.SerializeObject(newUser, Formatting.Indented);
                Console.WriteLine($"Some information on the user '{newUser.Username}' has changed since this program was last run.");
                File.WriteAllText($"../../data/users/{newUser.Username}.json", jsonSerialize);
            }
        }

        static void WriteAllUserData(List<User> users)
        {
            string jsonSerialize = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(userDataFilepath, jsonSerialize);
        }

        static void CompareAllUserData(List<User> newUsers)
        {
            HashSet<User> currentUsers;
            var finalUsers = new List<User>();

            using (StreamReader r = new StreamReader(userDataFilepath))
            {
                var json = r.ReadToEnd();
                currentUsers = new HashSet<User>(JsonConvert.DeserializeObject<HashSet<User>>(json));
            }

            foreach (var user in newUsers)
            {
                if (!currentUsers.Contains(user))
                {
                    Console.WriteLine($"{user.Username} is either newly added to our tracker, or has updated data.");
                }
                finalUsers.Add(user);
            }

            WriteAllUserData(finalUsers);
        }
    }
}

public class RootObject
{
    public List<string> Usernames;
}
