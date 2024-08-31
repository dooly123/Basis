using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class BasisStorageManagement : MonoBehaviour
{
    public const string AvatarDirectory = "Avatars";
    public const string WorldDirectory = "Worlds";
    public static double MaxSizeBeforeNuking = 25L; // 25GB in bytes
    public float TotalSizeBeforeNuking; // This is calculated in bytes
    public static double SizeInBytesBeforeNuking()
    {
        return (double)(MaxSizeBeforeNuking * 1024 * 1024 * 1024); // Cast the result to double
    }
    public static void CalculateContentSize()
    {
        string folderPathAvatar = Path.Combine(Application.persistentDataPath, AvatarDirectory);
        if (Directory.Exists(folderPathAvatar) == false)
        {
            Directory.CreateDirectory(folderPathAvatar);
        }

        string folderPathWorld = Path.Combine(Application.persistentDataPath, WorldDirectory);
        if (Directory.Exists(folderPathWorld) == false)
        {
            Directory.CreateDirectory(folderPathWorld);
        }

        // Calculate total size
        double totalSize = GetDirectorySize(folderPathAvatar) + GetDirectorySize(folderPathWorld);

        Debug.Log($"Total storage size: {totalSize / (1024 * 1024 * 1024)} GB");

        // Check if total size exceeds the limit and clean up if necessary
        if (totalSize > SizeInBytesBeforeNuking())
        {
            Debug.Log("Storage size exceeds the limit. Cleaning up oldest files...");
            CleanUpOldestFiles();
        }
    }

    public static void CleanUpOldestFiles()
    {
        string folderPathAvatar = Path.Combine(Application.persistentDataPath, AvatarDirectory);
        string folderPathWorld = Path.Combine(Application.persistentDataPath, WorldDirectory);

        // Get files from both directories, sorted by last access time
        var avatarFiles = new DirectoryInfo(folderPathAvatar).GetFiles().OrderBy(f => f.LastAccessTime).ToList();
        var worldFiles = new DirectoryInfo(folderPathWorld).GetFiles().OrderBy(f => f.LastAccessTime).ToList();

        // Combine the lists
        var allFiles = avatarFiles.Concat(worldFiles).OrderBy(f => f.LastAccessTime).ToList();

        double totalSize = GetDirectorySize(folderPathAvatar) + GetDirectorySize(folderPathWorld);

        for (int Index = 0; Index < allFiles.Count; Index++)
        {
            FileInfo file = allFiles[Index];
            if (totalSize <= SizeInBytesBeforeNuking())
            {
                break;
            }

            try
            {
                totalSize -= file.Length;
                Debug.Log($"Deleting file: {file.FullName}, Size: {file.Length / (1024 * 1024)} MB");
                file.Delete();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete file: {file.FullName}. Error: {e.Message}");
            }
        }
    }

    private static double GetDirectorySize(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return 0;
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        return directoryInfo.GetFiles().Sum(file => file.Length);
    }
}