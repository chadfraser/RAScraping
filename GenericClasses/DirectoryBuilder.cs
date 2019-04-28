using System;
using System.IO;

/// <summary>
/// Sets the main directory as the parent or grandparent of the initial directory and uses this directory to build
/// paths to other directories.
/// </summary>
namespace Fraser.GenericMethods
{
    public class DirectoryBuilder
    {
        public static string mainDirectory;

        public static void InitializeMainAsParentDirectory()
        {
            if (mainDirectory != null)
            {
                return;
            }
            SetMainAsParentDirectory();
        }

        public static void InitializeMainAsGrandparentDirectory()
        {
            if (mainDirectory != null)
            {
                return;
            }
            SetMainAsGrandparentDirectory();
        }

        public static void SetMainAsParentDirectory()
        {
            mainDirectory = Directory.GetParent(Environment.CurrentDirectory.ToString()).ToString();
        }

        public static void SetMainAsGrandparentDirectory()
        {
            mainDirectory = Directory.GetParent(Directory.GetParent(
                Environment.CurrentDirectory.ToString()).ToString()).ToString();
        }

        public static string BuildPathAndDirectoryFromMainDirectory(string subDirectory)
        {
            var fullPath = Path.Combine(mainDirectory, subDirectory);
            CreateDirectoryIfNotAlreadyExistent(fullPath);
            return fullPath;
        }

        public static string BuildPathFromMainDirectory(string subDirectory)
        {
            return Path.Combine(mainDirectory, subDirectory);
        }

        public static void CreateDirectoryIfNotAlreadyExistent(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}