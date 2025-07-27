namespace Karin;

public static class DirectoryStatistics
{
    public static int GetStatistics()
    {
        string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Console.WriteLine($"Scanning root folder: {rootPath}");

        if (!Directory.Exists(rootPath))
        {
            Console.WriteLine("Invalid directory path. Exiting...");
            return -1;
        }

        int numberOfFiles = GetTotalFiles(rootPath);
        return numberOfFiles;
    }

    static int GetTotalFiles(string path)
    {
        int numberOfFiles = Directory.GetFiles(path).Length;

        try
        {
            foreach (string subDir in Directory.GetDirectories(path))
            {
                numberOfFiles += GetTotalFiles(subDir);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to read directory: {path}");
        }

        return numberOfFiles;
    }
}