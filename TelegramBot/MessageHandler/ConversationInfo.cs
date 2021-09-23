using System;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal class ConversationInfo
  {
    public ConversationInfo(IMessageHandler messageHandler)
    {
      Handler = messageHandler;
    }

    public DateTime LastMessageDate { get; set; } = DateTime.Now;

    public IMessageHandler Handler { get; set; }

    public void Handle(Message message)
    {
      LastMessageDate = message.Date;

      var nextHandler = Handler.Handle(message);

      Handler = nextHandler.OnEnter(message);
    }

    internal void Active(IMessageHandler messageHandler, Message message)
    {
      Handler = messageHandler;

      Handler = Handler.OnEnter(message);
    }
  }
}