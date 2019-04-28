using System;

namespace Fraser.GenericMethods
{
    public class FileNotFoundHandler
    {
        public static void AbortProgramToDueMissingCriticalFile(string filename, string path)
        {
            Console.WriteLine($"The file '{filename}' was not found in the appropriate folder, '{path}'.");
            Console.WriteLine("Press enter to end the program.");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
