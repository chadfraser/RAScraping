using System;
using RAScraping;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace testing
{
    public class TestData
    {
        public static void Test()
        {
            bool testResult;
            var testSet = new HashSet<User>();
            var testUserA = new User("foo");
            var testUserB = new User("bar");
            var testUserC = new User("foo");

            Console.WriteLine("STARTING TESTS");

            testResult = !testUserA.Equals(testUserB);
            if (!testResult)
            {
                Console.WriteLine("Distinct users were detemined to be equal.");
                Console.ReadLine();
            }

            testResult = testUserA.Equals(testUserC);
            if (!testResult)
            {
                Console.WriteLine("Identical users were detemined to be unequal.");
                Console.ReadLine();
            }

            string jsonSerialize = JsonConvert.SerializeObject(testUserC, Formatting.Indented);
            File.WriteAllText("../../data/test_data.json", jsonSerialize);

            using (StreamReader r = new StreamReader("../../data/test_data.json"))
            {
                var json = r.ReadToEnd();
                var tempUser = JsonConvert.DeserializeObject<User>(json);

                testResult = testUserA.Equals(tempUser);
                if (!testResult)
                {
                    Console.WriteLine("Equality of users lost in json reading/writing.");
                    Console.ReadLine();
                }
                testSet.Add(tempUser);
            }

            testResult = testSet.Contains(testUserA);
            if (!testResult)
            {
                Console.WriteLine("User contained in hashset not recognized.");
                Console.ReadLine();
            }

            testUserA.RetroRatioPoints = 10;
            testResult = testUserA.Equals(testUserC);
            if (!testResult)
            {
                Console.WriteLine("Editing irrelevant properties ruined equality.");
                Console.ReadLine();
            }

            testUserA.Url = "foo";
            testResult = !testUserA.Equals(testUserC);
            if (!testResult)
            {
                Console.WriteLine("Editing relevant properties did not ruin equality.");
                Console.ReadLine();
            }

            Console.WriteLine("TESTS CONCLUDED");
        }
    }
}