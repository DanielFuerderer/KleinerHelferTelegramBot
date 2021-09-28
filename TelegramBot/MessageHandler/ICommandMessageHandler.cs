using Telegram.Bot.Types;

namespace KleinerHelferBot.MessageHandler
{
  internal interface ICommandMessageHandler : IMessageHandler
  {
    public string CommandName { get; }

    public bool Match(Message message);
  }
}