using System.Linq;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal class CommandsMessageHandler : IMessageHandler
  {
    private readonly ITelegramBotClient _telegramBotClient;

    public readonly MessageHandlerMapping MessageHandlers = new MessageHandlerMapping();

    public CommandsMessageHandler(ITelegramBotClient telegramBotClient)
    {
      _telegramBotClient = telegramBotClient;
    }

    public IMessageHandler Handle(Message message)
    {
      return MessageHandlers.GetHandler(message.Text, this);
    }

    public IMessageHandler OnEnter(Message message)
    {
      _telegramBotClient.Write(message.Chat, @"Was möchtest du tun?", MessageHandlers.AvailableCommands.ToArray());

      return this;
    }
  }
}