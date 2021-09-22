using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;
using TelegramBot.MessageHandler;

namespace KleinerHelferBot.MessageHandler
{
  class GetTextMessageHandler : IMessageHandler
  {
    private readonly Func<string, IMessageHandler> _onSuccess;

    public GetTextMessageHandler(Func<string, IMessageHandler> onSuccess)
    {
      _onSuccess = onSuccess;
    }

    public IMessageHandler Handle(Message message)
    {
      return _onSuccess(message.Text);
    }

    public IMessageHandler OnEnter(Message message)
    {
      return this;
    }
  }
}
