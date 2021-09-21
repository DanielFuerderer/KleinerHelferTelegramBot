using System;
using System.IO;
using System.Linq;
using System.Timers;
using Data;

namespace TelegramBot
{
  public class BackupService
  {
    private readonly CommunityRepository _communityRepository;
    private readonly UserRepository _userRepository;

    public BackupService(UserRepository userRepository, CommunityRepository communityRepository)
    {
      _userRepository = userRepository;
      _communityRepository = communityRepository;

      Start();
    }

    private void Start()
    {
      var timer = new Timer(TimeSpan.FromDays(0.5).TotalMilliseconds);

      timer.Elapsed += (sender, e) => CreateBackup();

      timer.Start();
    }

    private void CreateBackup()
    {
      Console.WriteLine("Doing backup ...");

      _userRepository.Save(GetBackupFileName(_userRepository.FileName));
      _communityRepository.Save(GetBackupFileName(_communityRepository.FileName));

      CleanDuplicates(_userRepository.FileName);
      CleanDuplicates(_communityRepository.FileName);
    }

    private static void CleanDuplicates(string fileName)
    {
      CleanDuplicates(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
    }

    private static void CleanDuplicates(string directory, string fileName)
    {
      var backupFiles = Directory.GetFiles(directory, $"Backup_*_{fileName}");
      var filesByContent = backupFiles.GroupBy(File.ReadAllText);

      foreach (var duplicates in filesByContent.Where(fbc => fbc.Count() > 1))
      {
        foreach (var duplicate in duplicates.OrderByDescending(f => f).Skip(1))
        {
          File.Delete(duplicate);
        }
      }
    }

    private static string GetBackupFileName(string fileName)
    {
      var directory = Path.GetDirectoryName(fileName);
      fileName = Path.GetFileName(fileName);

      var backupFileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}_{fileName}";

      return string.IsNullOrEmpty(directory) ? backupFileName : Path.Combine(directory, backupFileName);
    }
  }
}