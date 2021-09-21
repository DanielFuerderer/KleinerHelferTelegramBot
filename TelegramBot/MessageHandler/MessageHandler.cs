using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal abstract class MessageHandler : IMessageHandler
  {
    public abstract IMessageHandler Handle(Message message);

    public abstract IMessageHandler OnEnter(Message message);
  }
}