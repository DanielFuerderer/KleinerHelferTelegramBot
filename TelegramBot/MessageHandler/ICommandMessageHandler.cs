using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal interface ICommandMessageHandler : IMessageHandler
  {
    public string CommandName { get; }

    public bool Match(Message message);
  }
}