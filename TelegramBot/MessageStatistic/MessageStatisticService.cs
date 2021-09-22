
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using MessageStatisticData = Data.Data.DayGroupMessageStatistic;
using MessageStatisticRepository = Data.MessageStatisticRepository;

namespace TelegramBot.MessageStatistic
{
  internal class MessageStatisticService
  {
    private readonly Dictionary<long, MessageStatisticData> _messageStatistics = new Dictionary<long, MessageStatisticData>();
    private readonly MessageStatisticRepository _messageStatisticRepository;

    public MessageStatisticService(ITelegramBotClient telegramBotClient, MessageStatisticRepository messageStatisticRepository)
    {
      telegramBotClient.OnGroupMessage += (user, chat, message) => AnalyzeMessage(user.Id, chat.Id, message);
      _messageStatisticRepository = messageStatisticRepository;
    }


    internal MessageStatisticService()
    {
    }

    internal void AnalyzeMessage(long userId, ChatId chatId, Message message)
    {
      var messageStatistic = GetStatistic(userId, chatId);

      AnalyzeMessage(message, messageStatistic);

      _messageStatisticRepository.Update(userId.ToString(), messageStatistic);

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

    internal MessageStatisticData GetStatistic(long userId, ChatId chatId)
    {
      if (!_messageStatistics.TryGetValue(userId, out var messageStatistic))
      {
        _messageStatistics.Add(userId, messageStatistic = new MessageStatisticData(DateTime.Now, chatId.Identifier));
      }

      return messageStatistic;
    }
  }
}
