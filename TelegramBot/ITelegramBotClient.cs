using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
  internal interface ITelegramBotClient
  {
    event TelegramBotClient.UserGroupAction OnUserLeftGroup;

    event TelegramBotClient.UserGroupAction OnUserJoinedGroup;

    event TelegramBotClient.MessageAction OnPrivateMessage;

    event TelegramBotClient.MessageAction OnGroupMessage;

    void Write(Chat chat, string text);
    void Write(Chat chat, string text, params string[] replyOptions);
    void Write(Chat chat, string text, ParseMode parseMode = default, params string[] replyOptions);
  }
}