using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
  internal interface ITelegramBotClient
  {
    TelegramBotClient.UserGroupAction OnUserLeftGroup { get; set; }
    TelegramBotClient.UserGroupAction OnUserJoinedGroup { get; set; }
    TelegramBotClient.MessageAction OnPrivateMessage { get; set; }
    void Write(Chat chat, string text);
    void Write(Chat chat, string text, params string[] replyOptions);
    void Write(Chat chat, string text, ParseMode parseMode = default, params string[] replyOptions);
  }
}