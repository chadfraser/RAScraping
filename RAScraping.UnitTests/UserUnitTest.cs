using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using System.IO;
using System.Linq;

namespace RAScraping.UnitTests
{
    [TestClass]
    public class UserTest
    {
        public static string mainDirectory;
        public static string testDataDirectory;
        public static Dictionary<string, object> propertyDict = new Dictionary<string, object>
        {
            ["Username"] = "foo",
            ["UrlSuffix"] = "bar",
            ["Points"] = 10,
            ["RetroRatioPoints"] = 100,
            //["CompletedGamesData"] = new Dictionary<string, string>(),
            //["PlayedGamesData"] = new Dictionary<string, string>(),
            //["PlayedGamesEarnedAchievements"] = new Dictionary<string, HashSet<string>>()
        };

        [TestMethod]
        public void UserEqualityTest_SameData_AreEqual()
        {
            // Arrange
            var userA = new User();
            var userB = new User();

            // Act
            InitializeUserData(new User[] { userA, userB });

            // Assert
            Assert.AreEqual(userA, userB);
        }

        [TestMethod]
        public void UserEqualityTest_DifferentIrrelevantData_AreEqual()
        {
            var userA = new User();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            userB.RetroRatioPoints = 100000;
            userB.PlayedGamesData = new Dictionary<string, string>();

            Assert.AreEqual(userA, userB);
        }

        [TestMethod]
        public void UserEqualityTest_DifferentRelevantData_AreNotEqual()
        {
            var userA = new User();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            userB.UrlSuffix = "foo";

            Assert.AreNotEqual(userA, userB);
        }

        [TestMethod]
        public void UserEqualityTest_PuttingSameDataIntoDicts_AreEqual()
        {
            var userA = new User();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            userA.CompletedGamesData["test url"] = "test name";
            userB.CompletedGamesData["test url"] = "test name";
            userA.PlayedGamesData["new test url"] = "new test name";
            userB.PlayedGamesData["new test url"] = "new test name";
            userA.PlayedGamesEarnedAchievements["third test url"] = new HashSet<string> { "a test name",
                "second test name" };
            userB.PlayedGamesEarnedAchievements["third test url"] = new HashSet<string> { "a test name",
                "second test name" };

            Assert.AreEqual(userA, userB);
        }

        [TestMethod]
        public void UserEqualityTest_PuttingDifferentDataIntoCompletedGamesDict_AreNotEqual()
        {
            var userA = new User();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            userA.CompletedGamesData["test url"] = "test name";
            userB.CompletedGamesData["other test url"] = "test name";

            Assert.AreNotEqual(userA, userB);
        }

        [TestMethod]
        public void UserEqualityTest_PuttingDifferentDataIntoPlayedGamesDict_AreNotEqual()
        {
            var userA = new User();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            userA.PlayedGamesData["test url"] = "test name";
            userB.PlayedGamesData["test url"] = "new test name";

            Assert.AreNotEqual(userA, userB);
        }

        [TestMethod]
        public void UserHashCodeTest_PuttingDifferentDataIntoEarnedAchievementsDict_AreNotEqual()
        {
            var userA = new User();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            userA.PlayedGamesEarnedAchievements["test url"] = new HashSet<string> { "a test name",
                "second test name" };
            userB.PlayedGamesEarnedAchievements["test url"] = new HashSet<string> { "a test name",
                "changed test name" };

            Assert.AreNotEqual(userA, userB);
        }

        [TestMethod]
        public void UserHashCodeTest_FindingUserInSet_IsTrue()
        {
            var userA = new User();
            var testSet = new HashSet<User>();

            InitializeUserData(new User[] { userA });
            testSet.Add(userA);
            var result = testSet.Contains(userA);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UserHashCodeTest_FindingUserInDict_IsTrue()
        {
            var userA = new User();
            var testDict = new Dictionary<User, string>();

            InitializeUserData(new User[] { userA });
            testDict[userA] = "correct";
            var result = testDict.ContainsKey(userA);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UserHashCodeTest_FindingSameUserWithUpdatedRelevantDataInSet_IsFalse()
        {
            var userA = new User();
            var testSet = new HashSet<User>();

            InitializeUserData(new User[] { userA });
            testSet.Add(userA);
            userA.Points = 100000;
            var result = testSet.Contains(userA);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithUpdatedRelevantDataInDict_IsFalse()
        {
            var userA = new User();
            var testDict = new Dictionary<User, string>();

            InitializeUserData(new User[] { userA });
            testDict[userA] = "correct";
            userA.PlayedGamesEarnedAchievements["test"] = new HashSet<string> { "sample" };

            try
            {
                var result = (testDict[userA] == "correct");
                Assert.IsFalse(result);
            }
            catch (KeyNotFoundException)
            {
            }
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithUpdatedIdenticalDataInSet_IsTrue()
        {
            var userA = new User();
            var testSet = new HashSet<User>();

            InitializeUserData(new User[] { userA });
            userA.CompletedGamesData["example"] = "pass";
            testSet.Add(userA);
            userA.CompletedGamesData["example"] = "pass";
            var result = testSet.Contains(userA);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithUpdatedIdenticalDataInDict_IsTrue()
        {
            var userA = new User();
            var testDict = new Dictionary<User, string>();

            InitializeUserData(new User[] { userA });
            userA.PlayedGamesEarnedAchievements["test"] = new HashSet<string> { "sample" };
            testDict[userA] = "correct";
            userA.PlayedGamesEarnedAchievements["test"] = new HashSet<string> { "sample" };

            try
            {
                var result = (testDict[userA] == "correct");
                Assert.IsTrue(result);
            }
            catch (KeyNotFoundException)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithSameDataInSet_IsTrue()
        {
            var userA = new User();
            var testSet = new HashSet<User>();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            testSet.Add(userA);
            var result = testSet.Contains(userB);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithSameDataInDict_IsTrue()
        {
            var userA = new User();
            var testDict = new Dictionary<User, string>();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            testDict[userA] = "correct";

            try
            {
                var result = (testDict[userB] == "correct");
                Assert.IsTrue(result);
            }
            catch (KeyNotFoundException)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithDifferentIrrelevantDataInSet_IsTrue()
        {
            var userA = new User();
            var testSet = new HashSet<User>();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            testSet.Add(userA);
            userB.RetroRatioPoints = 100000;
            var result = testSet.Contains(userB);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithDifferentIrrelevantDataInDict_IsTrue()
        {
            var userA = new User();
            var testDict = new Dictionary<User, string>();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            testDict[userA] = "correct";
            userB.RetroRatioPoints = 100000;

            try
            {
                var result = (testDict[userB] == "correct");
                Assert.IsTrue(result);
            }
            catch (KeyNotFoundException)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithDifferentRelevantDataInSet_IsFalse()
        {
            var userA = new User();
            var testSet = new HashSet<User>();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            testSet.Add(userA);
            userB.Points = 100000;
            var result = testSet.Contains(userB);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UserHashCodeTest_FindingNewUserWithDifferentRelevantDataInDict_IsFalse()
        {
            var userA = new User();
            var testDict = new Dictionary<User, string>();
            var userB = new User();

            InitializeUserData(new User[] { userA, userB });
            testDict[userA] = "correct";
            userB.Points = 100000;

            try
            {
                var result = (testDict[userB] == "correct");
                Assert.IsFalse(result);
            }
            catch (KeyNotFoundException)
            {
            }
        }

        [TestMethod]
        public void UserDataFillTest_FillPointsWithControlDoc_IsTrue()
        {
            InitializeDirectories();

            var userTest = new User();
            var userA = new PrivateObject(userTest);
            var controlDoc = LoadControlDoc();
            var controlArray = new object[] { controlDoc };

            userA.Invoke("FillPoints", controlArray);
            var result = (((int)userA.GetProperty("Points") == 962) &&
                ((int)userA.GetProperty("RetroRatioPoints") == 1328));

            Assert.IsTrue(result);
        }

        //[TestMethod]
        //public void UserDataFillTest_FillCompletedGamesDataWithControlDoc_AreEqual()
        //{
        //    InitializeDirectories();

        //    var userTest = new User();
        //    var userA = new PrivateObject(userTest);
        //    var controlDoc = LoadControlDoc();
        //    var controlCompletedGamesData = new Dictionary<string, string>
        //    {
        //        ["/Game/1627"] = "Color a Dinosaur (NES)",
        //        ["/Game/1623"] = "Clu Clu Land (NES)"
        //    };
        //    var controlDict = controlCompletedGamesData.ToDictionary(entry => entry.Key,
        //        entry => entry.Value); ;
        //    var controlArray = new object[] { controlDoc, controlDict };

        //    userA.Invoke("FillCompletedGames", controlArray);
        //    var completedGamesResult = ((Dictionary<string, string>)userA.GetProperty("CompletedGamesData"));
        //    var result = User.AreDictsEqual(completedGamesResult, controlCompletedGamesData);

        //    Assert.IsTrue(result);
        //}

        //[TestMethod]
        //public void UserDataFillTest_FillPlayedGamesDataWithControlDoc_AreEqual()
        //{
        //    InitializeDirectories();

        //    var userTest = new User();
        //    var userA = new PrivateObject(userTest);
        //    var controlDoc = LoadControlDoc();
        //    var controlPlayedGamesData = new Dictionary<string, string>
        //    {
        //        ["/Game/1720"] = "Goonies, The",
        //        ["/Game/1474"] = "Friday the 13th"
        //    };

        //    var controlDict = controlPlayedGamesData.ToDictionary(entry => entry.Key,
        //        entry => entry.Value); ;
        //    controlDict["/Game/1627"] = "Color a Dinosaur (NES)";
        //    controlDict["/Game/1623"] = "Clu Clu Land (NES)";
        //    var controlArray = new object[] { controlDoc, controlDict };

        //    userA.SetProperty("CompletedGamesData", new Dictionary<string, string>
        //    {
        //        ["/Game/1627"] = "Color a Dinosaur (NES)",
        //        ["/Game/1623"] = "Clu Clu Land (NES)"
        //    });
        //    userA.Invoke("FillPlayedGames", controlArray);
        //    var playedGamesResult = ((Dictionary<string, string>)userA.GetProperty("PlayedGamesData"));
        //    var result = User.AreDictsEqual(playedGamesResult, controlPlayedGamesData);

        //    Assert.IsTrue(result);
        //}

        //[TestMethod]
        //public void UserDataFillTest_FillEarnedAchievementsDataWithControlDoc_AreEqual()
        //{
        //    InitializeDirectories();

        //    var userTest = new User();
        //    var userA = new PrivateObject(userTest);
        //    var controlDoc = LoadControlDoc();
        //    var controlEarnedAchievementsData = new Dictionary<string, HashSet<string>>
        //    {
        //        ["/Game/1720"] = new HashSet<string>{ "/Achievement/61502", "/Achievement/61339" },
        //        ["/Game/1474"] = new HashSet<string> { "/Achievement/3231", "/Achievement/3241", "/Achievement/3246" }
        //    };
        //    var controlArray = new object[] { controlDoc };

        //    userA.SetProperty("PlayedGamesData", new Dictionary<string, string>
        //    {
        //        ["/Game/1720"] = "Goonies, The",
        //        ["/Game/1474"] = "Friday the 13th"
        //    });
        //    userA.Invoke("FillPlayedGamesEarnedAchievements", controlArray);
        //    var earnedAchievementsResult = ((Dictionary<string, HashSet<string>>)userA.GetProperty("PlayedGamesEarnedAchievements"));
        //    var result = User.AreDictsEqual(earnedAchievementsResult, controlEarnedAchievementsData);

        //    Assert.IsTrue(result);
        //}

        private void InitializeUserData(User[] users)
        {
            foreach (var currentUser in users)
            {
                foreach (var key in propertyDict.Keys)
                {
                    var property = currentUser.GetType().GetProperty(key);
                    var convertedValue = Convert.ChangeType(propertyDict[key], property.PropertyType);
                    property.SetValue(currentUser, convertedValue);
                }
            }
        }

        private static void InitializeDirectories()
        {
            if (testDataDirectory != null)
            {
                return;
            }
            mainDirectory = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory.ToString()).ToString()).ToString();
            testDataDirectory = Path.Combine(mainDirectory, "testData");
        }

        private HtmlDocument LoadControlDoc()
        {
            InitializeDirectories();
            var controlDoc = new HtmlDocument();
            controlDoc.Load(Path.Combine(testDataDirectory, "test_user_a.txt"));
            return controlDoc;
        }
    }
}

// Unequal tests for achievements and game dicts
// differences in users (url error, points, game dicts, earned achievements, games with differences)