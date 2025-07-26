using System.IO;
using System.Security.Cryptography;

class FolderSync
{
    private string masterPath;
    private string slavePath;
    private FileSystemWatcher watcher;

    public FolderSync(string master, string slave)
    {
        masterPath = master;
        slavePath = slave;
    }

    public void StartSync()
    {
        PerformInitialSync();
        SetupWatcher();
    }

    private void PerformInitialSync()
    {
        // Compare and sync files/directories
        SyncDirectories(masterPath, slavePath);
    }

    private void SyncDirectories(string masterDir, string slaveDir)
    {
        // Create slave directory if missing
        if (!Directory.Exists(slaveDir)) Directory.CreateDirectory(slaveDir);

        // Sync files
        foreach (var masterFile in Directory.GetFiles(masterDir))
        {
            string slaveFile = Path.Combine(slaveDir, Path.GetFileName(masterFile));
            if (!File.Exists(slaveFile) || !FilesAreEqual(masterFile, slaveFile))
                File.Copy(masterFile, slaveFile, true);
        }

        // Sync subdirectories
        foreach (var masterSubDir in Directory.GetDirectories(masterDir))
        {
            string slaveSubDir = Path.Combine(slaveDir, Path.GetFileName(masterSubDir));
            SyncDirectories(masterSubDir, slaveSubDir);
        }

        // Delete files/directories in slave not in master
        foreach (var slaveFile in Directory.GetFiles(slaveDir))
            if (!File.Exists(Path.Combine(masterDir, Path.GetFileName(slaveFile))))
                File.Delete(slaveFile);
        foreach (var slaveSubDir in Directory.GetDirectories(slaveDir))
            if (!Directory.Exists(Path.Combine(masterDir, Path.GetFileName(slaveSubDir))))
                Directory.Delete(slaveSubDir, true);
    }

    private bool FilesAreEqual(string file1, string file2)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hash1 = md5.ComputeHash(File.ReadAllBytes(file1));
            byte[] hash2 = md5.ComputeHash(File.ReadAllBytes(file2));
            return hash1.SequenceEqual(hash2);
        }
    }

    private void SetupWatcher()
    {
        watcher = new FileSystemWatcher(masterPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            IncludeSubdirectories = true
        };
        watcher.Changed += (s, e) => SyncFile(e.FullPath);
        watcher.Created += (s, e) => SyncFile(e.FullPath);
        watcher.Deleted += (s, e) => DeleteSlaveFile(e.FullPath);
        watcher.Renamed += (s, e) => RenameSlaveFile(e.OldFullPath, e.FullPath);
        watcher.EnableRaisingEvents = true;
    }

    private void SyncFile(string masterFile)
    {
        string relativePath = Path.GetRelativePath(masterPath, masterFile);
        string slaveFile = Path.Combine(slavePath, relativePath);
        File.Copy(masterFile, slaveFile, true);
    }

    private void DeleteSlaveFile(string masterFile)
    {
        string relativePath = Path.GetRelativePath(masterPath, masterFile);
        string slaveFile = Path.Combine(slavePath, relativePath);
        if (File.Exists(slaveFile)) File.Delete(slaveFile);
        else if (Directory.Exists(slaveFile)) Directory.Delete(slaveFile, true);
    }

    private void RenameSlaveFile(string oldMasterPath, string newMasterPath)
    {
        string oldRelative = Path.GetRelativePath(masterPath, oldMasterPath);
        string newRelative = Path.GetRelativePath(masterPath, newMasterPath);
        string oldSlavePath = Path.Combine(slavePath, oldRelative);
        string newSlavePath = Path.Combine(slavePath, newRelative);
        if (File.Exists(oldSlavePath)) File.Move(oldSlavePath, newSlavePath);
        else if (Directory.Exists(oldSlavePath)) Directory.Move(oldSlavePath, newSlavePath);
    }
}