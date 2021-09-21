using System;
using System.Text;
using Data;
using Telegram.Bot.Types;
using TelegramBot.MessageHandler;

namespace TelegramBot
{
  internal class Program
  {
    private const string DataFile = @"Data\data.json";
    private const string UserFile = @"Data\user.json";
    private const string BotToken = @"1953528295:AAEnLBu_KVzYMbAx17L3_1Ujow9uO-dbUiE";

    private static void Main(string[] args)
    {
      var communityRepository = new CommunityRepository(DataFile);
      var userRepository = new UserRepository(UserFile, communityRepository);
      var telegramBotClient = new TelegramBotClient(BotToken);
      var conversationManager = new ConversationManager(telegramBotClient, userRepository, communityRepository);
      var backupService = new BackupService(userRepository, communityRepository);

      telegramBotClient.OnUserJoinedGroup += (user, group) =>
      {
        Console.WriteLine($"User '{GetDisplayName(user)}' (@{user.Username}) joined group '{group.Title}'");
      };

      telegramBotClient.OnUserLeftGroup += (user, group) =>
      {
        Console.WriteLine($"User '{GetDisplayName(user)}' (@{user.Username}) left group '{group.Title}'");
      };

      telegramBotClient.OnPrivateMessage += (user, chat, message) => { conversationManager.Handle(message); };

      Console.ReadLine();
    }

    private static string GetDisplayName(User user)
    {
      var name = new StringBuilder();

      if (!string.IsNullOrEmpty(user.FirstName))
      {
        name.Append(user.FirstName);
      }

      if (!string.IsNullOrEmpty(user.LastName))
      {
        if (name.Length != 0)
        {
          name.Append(", ");
        }

        name.Append(user.LastName);
      }

      return name.ToString();
    }
  }
}