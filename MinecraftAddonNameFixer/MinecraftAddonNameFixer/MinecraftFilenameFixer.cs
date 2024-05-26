using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace MinecraftAddonNameFixer;

public class MinecraftFilenameFixer
{
    public static Stream CleanFile(Stream sourceFile)
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        ZipFile.ExtractToDirectory(sourceFile, tempDirectory);

        // Recursively process all directories and files in the temporary directory
        ProcessDirectory(tempDirectory);

        // Create a new zip file with the cleaned contents
        Stream destinationStream = new MemoryStream();
        ZipFile.CreateFromDirectory(tempDirectory, destinationStream);

        // Delete the temporary directory
        Directory.Delete(tempDirectory, true);
        return destinationStream;
    }

    static void Main(string[] args)
    {
        string sourcePath = args[0];
        string destinationPath = args.Length > 1 ? args[1] : "cleaned.mcaddon";

        // Unzip the mcaddon file to a temporary directory
        string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        ZipFile.ExtractToDirectory(sourcePath, tempDirectory);

        // Recursively process all directories and files in the temporary directory
        ProcessDirectory(tempDirectory);

        // Create a new zip file with the cleaned contents
        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }
        ZipFile.CreateFromDirectory(tempDirectory, destinationPath);

        // Delete the temporary directory
        Directory.Delete(tempDirectory, true);
    }

    static void ProcessDirectory(string directoryPath)
    {
        // Get the parent directory and the directory name
        string parentDirectory = Directory.GetParent(directoryPath)!.FullName;
        string directoryName = Path.GetFileName(directoryPath);

        // Clean the directory name
        string cleanedDirectoryName = CleanFileName(directoryName);

        // If the directory name has changed, rename the directory
        if (!directoryName.Equals(cleanedDirectoryName, StringComparison.Ordinal))
        {
            string cleanedDirectoryPath = Path.Combine(parentDirectory, cleanedDirectoryName);
            Directory.Move(directoryPath, cleanedDirectoryPath);
            directoryPath = cleanedDirectoryPath;  // Use the new directory path for further processing
        }

        // Process all files in the current directory
        foreach (string filePath in Directory.GetFiles(directoryPath))
        {
            ProcessFile(filePath);
        }

        // Recursively process all subdirectories
        foreach (string subdirectoryPath in Directory.GetDirectories(directoryPath))
        {
            ProcessDirectory(subdirectoryPath);
        }
    }

    static void ProcessFile(string filePath)
    {
        // Get the directory name and the file name
        string directoryName = Path.GetDirectoryName(filePath)!;
        string fileName = Path.GetFileName(filePath);

        // Clean the file name
        string cleanedFileName = CleanFileName(fileName);

        // If the file name has changed, rename the file
        if (!fileName.Equals(cleanedFileName, StringComparison.Ordinal))
        {
            string cleanedFilePath = Path.Combine(directoryName, cleanedFileName);
            File.Move(filePath, cleanedFilePath);
        }
    }

    static string CleanFileName(string fileName)
    {
        // Define a list of valid characters
        string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~!@#$%^&*()_+-={}|[]\\:\";'<>?,./";

        // Create a new string with only the valid characters
        string cleanedFileName = new string(fileName.Where(c => validChars.Contains(c)).ToArray());

        return cleanedFileName;
    }
}
