using System.Collections.Generic;
using System.Linq;

namespace TelegramBot.MessageHandler
{
  internal class MessageHandlerMapping
  {
    private readonly Dictionary<string, IMessageHandler> _mapping =
      new Dictionary<string, IMessageHandler>(new TrimmedCaseInsensitiveEqualityComparer());

    public IEnumerable<string> AvailableCommands => _mapping.Select(m => m.Key);

    public IMessageHandler GetHandler(string command, IMessageHandler fallback)
    {
      return _mapping.TryGetValue(command, out var messageHandler) ? messageHandler : fallback;
    }

    public void Add(string message, IMessageHandler handler)
    {
      _mapping.Add(message, handler);
    }
  }
}