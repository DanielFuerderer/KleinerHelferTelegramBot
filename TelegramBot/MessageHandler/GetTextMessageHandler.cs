using System;
using Telegram.Bot.Types;

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
