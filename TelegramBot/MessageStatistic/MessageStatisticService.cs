using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Data;
using Data.Data;
using Telegram.Bot.Types;
using MessageStatisticData = Data.Data.DayGroupMessageStatistic;

namespace KleinerHelferBot.MessageStatistic
{
  internal class MessageStatisticService
  {
    private readonly Dictionary<string, MessageStatisticData> _messageStatistics = new Dictionary<string, MessageStatisticData>();
    private readonly IMessageStatisticRepository _messageStatisticRepository;
    private readonly IUserRepository _userRepository;

    public MessageStatisticService(ITelegramBotClient telegramBotClient, IMessageStatisticRepository messageStatisticRepository, IUserRepository userRepository)
    {
      telegramBotClient.OnGroupMessage += (user, chat, message) =>
      {
        var userInformation = GetUser(user);

        AnalyzeMessage(userInformation, chat.Id, message);
      };

      _messageStatisticRepository = messageStatisticRepository;
      this._userRepository = userRepository;
    }

    private UserInformation GetUser(User user)
    {
      return NewUser(user)
        ? CreateUser(user)
        : GetUserFromRepository(user);
    }

    private UserInformation GetUserFromRepository(User user)
    {
      return _userRepository[user.Id.ToString()];
    }

    private UserInformation CreateUser(User user)
    {
      var userInformation = new UserInformation(user.Id.ToString(), user.FirstName);

      _userRepository.AddUser(userInformation);
      _userRepository.Save();

      return userInformation;
    }

    private bool NewUser(User user)
    {
      return !_userRepository.Exists(user.Id.ToString());
    }

    internal void AnalyzeMessage(UserInformation user, ChatId chatId, Message message)
    {
      var messageStatistic = GetStatistic(user.Id, chatId);

      AnalyzeMessage(message, messageStatistic);

      _messageStatisticRepository.Update(user, messageStatistic);

      _messageStatisticRepository.Save();
    }

    private void AnalyzeMessage(Message message, MessageStatisticData messageStatistic)
    {
      messageStatistic.Total += 1;

      if (message.ForwardFrom != null || message.ForwardFromChat != null || message.ForwardFromMessageId != 0)
      {
        messageStatistic.Forwards += 1;
      }
      else if (ContainsLink(message.Text))
      {
        messageStatistic.Links += 1;
      }
    }

    private static bool ContainsLink(string text)
    {
      return Regex.IsMatch(
        text,
        @"((http|https)://)?([a-zA-Z0-9\-]+\.)+[a-zA-Z0-9]+(/[a-zA-Z0-9\-/]+(\.[a-zA-Z0-9\-/]+)?)?(\?[a-zA-Z0-9\-/=&]+)?(#.*)?");
    }

    internal MessageStatisticData GetStatistic(string userId, ChatId chatId)
    {
      if (!_messageStatistics.TryGetValue(userId, out var messageStatistic))
      {
        _messageStatistics.Add(userId, messageStatistic = new MessageStatisticData(DateTime.Now, chatId.Identifier));
      }

      return messageStatistic;
    }
  }
}
