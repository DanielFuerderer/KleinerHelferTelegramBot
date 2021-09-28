using Telegram.Bot.Types;

namespace KleinerHelferBot.MessageHandler
{
  internal interface IMessageHandler
  {
    public IMessageHandler Handle(Message message);

    public IMessageHandler OnEnter(Message message);
  }
}