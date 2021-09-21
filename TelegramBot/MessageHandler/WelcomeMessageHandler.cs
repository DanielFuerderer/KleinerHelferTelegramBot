using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal class WelcomeMessageHandler : IMessageHandler
  {
    private readonly ITelegramBotClient _telegramBotClient;

    public WelcomeMessageHandler(ITelegramBotClient telegramBotClient, IMessageHandler nextHandler)
    {
      _telegramBotClient = telegramBotClient;

      NextHandler = nextHandler;
    }

    public IMessageHandler NextHandler { get; }

    public IMessageHandler Handle(Message message)
    {
      _telegramBotClient.Write(message.Chat, $@"Hallo {GetDisplayName(message.From)}, wir kennen uns noch nicht.");

      return NextHandler;
    }

    public IMessageHandler OnEnter(Message message)
    {
      return Handle(message);
    }

    private static string GetDisplayName(User user)
    {
      return user.FirstName;
    }
  }
}