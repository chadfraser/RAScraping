using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
//using testing;

namespace RAScraping
{
    class Program
    {
        private static string mainDirectory;
        private static string dataDirectory;
        public static string userDataDirectory;
        public static string gameDataDirectory;
        private static bool oneUserPerFile = true;

        static void Main(string[] args)
        {
            var checkedGamesData = new Dictionary<string, string>();
            var changedGamesData = new Dictionary<string, string>();

            //TestData.Test();
            InitializePaths();
            UpdateTrackedGameData(ref checkedGamesData, ref changedGamesData);

            try
            {
                using (StreamReader r = new StreamReader(Path.Combine(dataDirectory, "main_data.json")))
                {
                    var json = r.ReadToEnd();
                    RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(json);
                    if (rootObject.Usernames is null)
                    {
                        Console.WriteLine("The list of usernames in the 'main_data.json' file is empty.");
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                    UpdateTrackedUserData(rootObject, ref checkedGamesData, ref changedGamesData);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("The file 'main_data.json' was not found in the appropriate data folder.");
                Console.ReadLine();
                Environment.Exit(0);
            }
            Console.ReadLine();
        }

        static void InitializePaths()
        {
            mainDirectory = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory.ToString()).ToString()).ToString();
            dataDirectory = Path.Combine(mainDirectory, "data");
            userDataDirectory = Path.Combine(dataDirectory, "users");
            gameDataDirectory = Path.Combine(dataDirectory, "games");
            Directory.CreateDirectory(dataDirectory);
            Directory.CreateDirectory(userDataDirectory);
            Directory.CreateDirectory(gameDataDirectory);
        }

        public static HtmlDocument LoadDocument(string url)
        {
            var website = new HtmlWeb();
            HtmlDocument doc = website.Load(url);
            return doc;
        }

        static void UpdateTrackedGameData(ref Dictionary<string, string> checkedGamesData, ref Dictionary<string, string> changedGamesData)
        {
            string[] fileArray = Directory.GetFiles(gameDataDirectory, "*.json");
            foreach (var filename in fileArray)
            {
                var urlNumber = filename.Split(' ').Last();
                var url = $"/Game/{urlNumber}";
                var newGame = new Game(url);

                Game oldGame;
                using (StreamReader r = new StreamReader(Path.Combine(gameDataDirectory, filename)))
                {
                    var json = r.ReadToEnd();
                    oldGame = JsonConvert.DeserializeObject<Game>(json);
                }
                if (!oldGame.Equals(newGame))
                {
                    newGame.WriteDifferencesInGames(oldGame);
                    changedGamesData[url] = newGame.Name;
                }
                else if (newGame.TotalRetroRatioPoints != oldGame.TotalRetroRatioPoints)
                {
                    newGame.SaveData();
                }

                checkedGamesData[url] = newGame.Name;
            }
        }

        static void UpdateTrackedUserData(RootObject rootObject, ref Dictionary<string, string> checkedGamesData, ref Dictionary<string, string> changedGamesData)
        {
            var users = new List<User>();

            foreach (string username in rootObject.Usernames)
            {
                User newUser = BuildSingleUserData(username, ref checkedGamesData);

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
            newUser.FillPlayerData(ref checkedGamesData);
            return newUser;
        }

        static void WriteSingleUserData(User newUser)
        {
            string jsonSerialize = JsonConvert.SerializeObject(newUser, Formatting.Indented);
            File.WriteAllText(Path.Combine(userDataDirectory, $"{newUser.Username}.json"), jsonSerialize);
        }

        static void CompareSingleUserData(User newUser, Dictionary<string, string> changedGamesData)
        {
            User tempUser;
            using (StreamReader r = new StreamReader(Path.Combine(userDataDirectory, $"{newUser.Username}.json")))
            {
                var json = r.ReadToEnd();
                tempUser = JsonConvert.DeserializeObject<User>(json);
            }

            if (!newUser.Equals(tempUser))
            {
                newUser.WriteDifferencesInUsers(tempUser, changedGamesData);
                WriteSingleUserData(newUser);
            }
            else if (newUser.RetroRatioPoints != tempUser.RetroRatioPoints)
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
    }
}

public class RootObject
{
    public List<string> Usernames;
}
