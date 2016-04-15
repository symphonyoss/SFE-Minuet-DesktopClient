using System;
using System.IO;
using System.Reflection;

namespace Paragon.AppPackager
{
    internal class PathValidation
    {
        private static string _jsonFilePath;

        public static string GetValidInputFolder(string inputPath)
        {
            var inputFolder = inputPath;
            if (!Path.IsPathRooted(inputFolder))
            {
                inputFolder = Path.Combine(Environment.CurrentDirectory, inputFolder);
            }

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("App folder not found: " + inputFolder);
                return null;
            }

            _jsonFilePath = Path.Combine(inputFolder, "manifest.json");

            if (!File.Exists(_jsonFilePath))
            {
                Console.WriteLine("manifest.json file was not found. Failed to package the App");
                return null;
            }
            return inputFolder;
        }
    }
}