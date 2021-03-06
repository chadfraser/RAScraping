﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Fraser.GenericMethods;
using System.Net.Mail;
using HtmlAgilityPack;

namespace RAScraping
{
    class Program
    {
        private static string dataDirectory;
        public static string userDataDirectory;
        public static string gameDataDirectory;
        public static string gameSystemDataDirectory;
        private static bool oneUserPerFile = true;

        static void Main(string[] args)
        {
            var rootObject = new RootObject();
            var checkedGamesData = new Dictionary<string, string>();
            var changedGamesData = new Dictionary<string, string>();

            var mail = new EmailComposer();
            mail.InitializeOutlookComposer();

            var outputHandler = new OutputHandler();
            Console.SetOut(outputHandler);

            InitializePaths();
            UpdateTrackedGameData(ref checkedGamesData, ref changedGamesData);
            UpdateTrackedGameSystemData(checkedGamesData);

            try
            {
                using (StreamReader r = new StreamReader(Path.Combine(dataDirectory, "main_data.json")))
                {
                    var json = r.ReadToEnd();
                    rootObject = JsonConvert.DeserializeObject<RootObject>(json);
                    if (rootObject.Usernames is null)
                    {
                        Console.WriteLine("The list of usernames in the 'main_data.json' file is empty.");
                    }
                    else
                    {
                        UpdateTrackedUserData(rootObject, ref checkedGamesData, changedGamesData);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //FileNotFoundHandler.AbortProgramToDueMissingCriticalFile("main_data.json", dataDirectory);
            }

            WriteRecentChanges(outputHandler, rootObject);
            rootObject.LastAccess = DateTime.Now;
            rootObject.SaveProperties(Path.Combine(dataDirectory, "main_data.json"));
            try
            {
                mail.SendEmail($"RA Scraping Results, " +
                    $"{rootObject.LastAccess.ToString("yyyy/MM/dd, H:mm tt")}",
                    outputHandler.Output.ToString());
            }
            catch (SmtpException)
            {
                File.WriteAllText(
                    Path.Combine(dataDirectory,
                    $"error_{rootObject.LastAccess.ToString("yyyy-MM-dd  H,mm tt")}.txt"),
                    outputHandler.Output.ToString());
            }
        }

        static void InitializePaths()
        {
            DirectoryBuilder.InitializeMainAsGrandparentDirectory();
            dataDirectory = DirectoryBuilder.BuildPathAndDirectoryFromMainDirectory("data");
            userDataDirectory = DirectoryBuilder.BuildPathAndDirectoryFromMainDirectory(
                new string[] { "data", "users" });
            gameDataDirectory = DirectoryBuilder.BuildPathAndDirectoryFromMainDirectory(
                new string[] { "data", "games" });
            gameSystemDataDirectory = DirectoryBuilder.BuildPathAndDirectoryFromMainDirectory(
                new string[] { "data", "systems" });
        }

        static void UpdateTrackedGameData(ref Dictionary<string, string> checkedGamesData,
            ref Dictionary<string, string> changedGamesData)
        {
            string[] fileArray = Directory.GetFiles(gameDataDirectory, "*.json");
            foreach (var absoluteFileName in fileArray)
            {
                var urlNumber = absoluteFileName.Split(' ').Last().Replace(".json", "");
                var baseFileName = Path.GetFileName(absoluteFileName);
                var url = $"/Game/{urlNumber}";
                var newGame = new Game(url);
                Game oldGame;

                try
                {
                    using (StreamReader r = new StreamReader(Path.Combine(gameDataDirectory, absoluteFileName)))
                    {
                        var json = r.ReadToEnd();
                        oldGame = JsonConvert.DeserializeObject<Game>(json);
                    }
                    if (!oldGame.Equals(newGame))
                    {
                        var newFileLocation = Path.Combine(gameDataDirectory, "outdated", baseFileName);
                        if (File.Exists(newFileLocation))
                        {
                            File.Delete(newFileLocation);
                            Console.WriteLine($"The file {newFileLocation} has been updated, even though it should no " +
                                $"longer be relevant. Please ensure that this file is not relevant.");
                        }
                        File.Move(absoluteFileName, newFileLocation);
                        newGame.WriteDifferencesInGames(oldGame);
                        changedGamesData[url] = newGame.Name;
                        newGame.SaveData();
                    }
                    else if (newGame.TotalRetroRatioPoints != oldGame.TotalRetroRatioPoints)
                    {
                        newGame.SaveData();
                    }
                }
                catch (FileNotFoundException)
                {
                    FileNotFoundHandler.AbortProgramToDueMissingCriticalFile(absoluteFileName, gameDataDirectory);
                }

                checkedGamesData[url] = newGame.Name;
            }
        }

        static void UpdateTrackedGameSystemData(Dictionary<string, string> checkedGamesData)
        {
            string[] fileArray = Directory.GetFiles(gameSystemDataDirectory, "*.json");
            foreach (var filename in fileArray)
            {
                string urlSuffix;
                GameSystem newGameSystem;
                GameSystem oldGameSystem;

                try
                {
                    using (StreamReader r = new StreamReader(Path.Combine(gameDataDirectory, filename)))
                    {
                        var json = r.ReadToEnd();
                        oldGameSystem = JsonConvert.DeserializeObject<GameSystem>(json);
                        urlSuffix = oldGameSystem.UrlSuffix;
                        newGameSystem = new GameSystem(urlSuffix, checkedGamesData);
                    }
                    if (!oldGameSystem.Equals(newGameSystem))
                    {
                        newGameSystem.WriteDifferencesInGames(oldGameSystem);
                        newGameSystem.SaveData();
                    }
                }
                catch (FileNotFoundException)
                {
                    FileNotFoundHandler.AbortProgramToDueMissingCriticalFile(filename, gameDataDirectory);
                }
            }
        }

        static void UpdateTrackedUserData(RootObject rootObject, ref Dictionary<string, string> checkedGamesData,
            Dictionary<string, string> changedGamesData)
        {
            var users = new List<User>();

            foreach (string username in rootObject.Usernames)
            {
                User newUser = BuildSingleUserData(username, ref checkedGamesData);
                if (newUser is null)
                {
                    continue;
                }

                if (oneUserPerFile)
                {
                    if (!File.Exists(Path.Combine(userDataDirectory, $"{username}.json")))
                    {
                        WriteSingleUserData(newUser);
                    }
                    else
                    {
                        CompareSingleUserData(newUser, changedGamesData);
                    }
                }
                users.Add(newUser);
            }

            if (!oneUserPerFile)
            {
                if (!File.Exists(Path.Combine(dataDirectory, "ra_user_data.json")))
                {
                    WriteAllUserData(users);
                }
                else
                {
                    CompareAllUserData(users, changedGamesData);
                }
            }
        }

        static User BuildSingleUserData(string username, ref Dictionary<string, string> checkedGamesData)
        {
            var newUser = new User(username);
            try
            {
                newUser.FillUserData(ref checkedGamesData);
            }
            catch (HtmlWebException)
            {
                return null;
            }
            return newUser;
        }

        static void WriteSingleUserData(User newUser)
        {
            string jsonSerialize = JsonConvert.SerializeObject(newUser, Formatting.Indented);
            File.WriteAllText(Path.Combine(userDataDirectory, $"{newUser.Username}.json"), jsonSerialize);
        }

        static void CompareSingleUserData(User newUser, Dictionary<string, string> changedGamesData)
        {
            User oldUser;
            try
            {
                using (StreamReader r = new StreamReader(Path.Combine(userDataDirectory, $"{newUser.Username}.json")))
                {
                    var json = r.ReadToEnd();
                    oldUser = JsonConvert.DeserializeObject<User>(json);
                }
                if (!newUser.Equals(oldUser))
                {
                    newUser.WriteDifferencesInUsers(oldUser, changedGamesData);
                    WriteSingleUserData(newUser);
                }
                else if (newUser.RetroRatioPoints != oldUser.RetroRatioPoints)
                {
                    WriteSingleUserData(newUser);
                }
            }
            catch (FileNotFoundException)
            {
                WriteSingleUserData(newUser);
            }
        }

        static void WriteAllUserData(List<User> users)
        {
            string jsonSerialize = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(Path.Combine(dataDirectory, "ra_user_data.json"), jsonSerialize);
        }

        static void CompareAllUserData(List<User> newUsers, Dictionary<string, string> changedGamesData)
        {
            Dictionary<string, User> currentUsers;
            var finalUsers = new List<User>();

            using (StreamReader r = new StreamReader(Path.Combine(dataDirectory, "ra_user_data.json")))
            {
                var json = r.ReadToEnd();
                var tempList = new HashSet<User>(JsonConvert.DeserializeObject<List<User>>(json));
                currentUsers = tempList.ToDictionary(x => x.UrlSuffix, x => x);
            }

            foreach (var user in newUsers)
            {
                if (!currentUsers.ContainsKey(user.UrlSuffix))
                {
                    Console.WriteLine($"{user.Username} is newly added to our tracker.");
                }
                else if (!currentUsers[user.UrlSuffix].Equals(user))
                {
                    user.WriteDifferencesInUsers(currentUsers[user.UrlSuffix], changedGamesData);
                }
                finalUsers.Add(user);
            }

            WriteAllUserData(finalUsers);
        }

        static void WriteRecentChanges(OutputHandler outputHandler, RootObject rootObject)
        {
            string message;
            var messageSuffix = (rootObject.LastAccess == null) ? "." : $" on {rootObject.ParseLastAccess()}.\n";

            if (string.IsNullOrWhiteSpace(outputHandler.Output.ToString()))
            {
                message = "No changes have been made to any tracked files since the program was last run";
            }
            else
            {
                message = "The following changes have been made since the program was last run";
            }
            outputHandler.Output.Insert(0, $"{message}{messageSuffix}");
        }

        /// <summary>
        /// Tests if two dictionaries are functionally equal. This means that they have the same keys, and for every
        /// key their values are equal according to the <c>.Equals()</c> method.
        /// </summary>
        /// <typeparam name="K">Any type of object as a key, though only strings are currently implemented.</typeparam>
        /// <typeparam name="V">Any type of object as a value, though only strings are currently implemented.</typeparam>
        /// <param name="dict1">The first dictionary to compare.</param>
        /// <param name="dict2">The second dictionary to compare.</param>
        /// <returns>A boolean variable stating whether the two dictionaries are functionally equal.</returns>
        public static bool AreDictsEqual<K, V>(Dictionary<K, V> dict1, Dictionary<K, V> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }
            return dict1.Keys.All(k => dict2.ContainsKey(k) && dict1[k].Equals(dict2[k]));
        }

        /// <summary>
        /// Tests if two dictionaries are functionally equal, given that their values are HashSets.
        /// This means that they have the same keys, and for every key their values are equal according to the 
        /// <c>.SetEquals()</c> method.
        /// </summary>
        /// <typeparam name="K">Any type of object as a key, though only strings are currently implemented.</typeparam>
        /// <typeparam name="V">
        /// Any type of object to go in the HashSet stored as a value, though only strings are currently implemented.
        /// </typeparam>
        /// <param name="dict1">The first dictionary to compare.</param>
        /// <param name="dict2">The second dictionary to compare.</param>
        /// <returns>A boolean variable stating whether the two dictionaries are functionally equal.</returns>
        public static bool AreDictsEqual<K, V>(Dictionary<K, HashSet<V>> dict1, Dictionary<K, HashSet<V>> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }
            return dict1.Keys.All(k => dict2.ContainsKey(k) && dict1[k].SetEquals(dict2[k]));
        }
    }

    public class RootObject
    {
        public List<string> Usernames;
        public DateTime LastAccess;

        public string ParseLastAccess()
        {
            if (LastAccess == null)
            {
                return "";
            }
            return $"{LastAccess.ToLongDateString()} at {LastAccess.ToLongTimeString()}";
        }

        public void SaveProperties(string givenPath)
        {
            string jsonSerialize = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(givenPath, jsonSerialize);
        }

    }
}
