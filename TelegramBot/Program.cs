using System;
using System.IO;
using System.Text;
using Data;
using Telegram.Bot.Types;
using TelegramBot.MessageHandler;

namespace TelegramBot
{
  internal class Program
  {
    private static readonly string DataFile = Path.Combine("Data", "communities.json");
    private static readonly string UserFile = Path.Combine("Data", "users.json");
    private static readonly string UserStatistics = Path.Combine("Data", "userstatistics.json");
    private const string BotToken = @"1953528295:AAEnLBu_KVzYMbAx17L3_1Ujow9uO-dbUiE";

    private static void Main(string[] args)
    {
      var communityRepository = new CommunityRepository(DataFile);
      var userRepository = new UserRepository(UserFile, communityRepository);
      var telegramBotClient = new TelegramBotClient(BotToken);
      var conversationManager = new ConversationManager(telegramBotClient, userRepository, communityRepository);
      var backupService = new BackupService(userRepository, communityRepository);
      var messageStatisticRepository = new MessageStatisticRepository(UserStatistics, userRepository);
      var messageStatisticService = new MessageStatistic.MessageStatisticService(telegramBotClient, messageStatisticRepository);

      telegramBotClient.OnUserJoinedGroup += (user, group) =>
      {
        Console.WriteLine($"User '{GetDisplayName(user)}' (@{user.Username}) joined group '{group.Title}'");

        //telegramBotClient.Write(group, $"Hallo {GetDisplayName(user)}");  
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
        return user.FirstName;
      }
      else if (!string.IsNullOrEmpty(user.LastName))
      {
        return user.LastName;
      }
      else
      {
        return user.Username;
      }
    }
  }
}