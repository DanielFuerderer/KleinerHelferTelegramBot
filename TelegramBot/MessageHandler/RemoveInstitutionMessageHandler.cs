using System;
using System.Linq;
using Data;
using Telegram.Bot.Types;

namespace KleinerHelferBot.MessageHandler
{
  internal class RemoveInstitutionMessageHandler : IRemoveInstitutionMessageHandler
  {
    private const string Cancel = "Abbrechen";
    private readonly IMessageHandler _mainMenuMessageHandler;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUserRepository _userRepository;

    public RemoveInstitutionMessageHandler(IUserRepository userRepository, ITelegramBotClient telegramBotClient,
      MainCommandsMessageHandler mainMenuMessageHandler)
    {
      _userRepository = userRepository;
      _telegramBotClient = telegramBotClient;
      _mainMenuMessageHandler = mainMenuMessageHandler;
    }

    public IMessageHandler Handle(Message message)
    {
      var userId = message.From.IsBot ? message.Chat.Id : message.From.Id;

      var userInformation = _userRepository[userId.ToString()];

      var input = message.Text.Trim(' ');

      if (string.Equals(input, Cancel, StringComparison.CurrentCultureIgnoreCase))
      {
        return _mainMenuMessageHandler;
      }

      var institution = userInformation.AssociatedInstitutions.FirstOrDefault(ai =>
        string.Equals(ai.Name, message.Text, StringComparison.InvariantCultureIgnoreCase));
      if (institution == null)
      {
        _telegramBotClient.Write(message.Chat,
          "Ich konnte die Institution bei dir nicht finden. Bitte versuche es erneut.", GetCommands(message));

        return this;
      }

      _userRepository.StartUpdate(userInformation).RemoveInstitution(institution).Commit();

      _userRepository.Save();

      _telegramBotClient.Write(message.Chat, "Institution entfernt.");

      return _mainMenuMessageHandler;
    }

    public IMessageHandler OnEnter(Message message)
    {
      _telegramBotClient.Write(message.Chat, "Welche Institution willst du entfernen?", GetCommands(message));

      return this;
    }

    private string[] GetCommands(Message message)
    {
      var userId = message.From.IsBot ? message.Chat.Id : message.From.Id;

      var userInformation = _userRepository[userId.ToString()];

      return userInformation.AssociatedInstitutions.Select(ai => ai.Name).OrderBy(name => name).Concat(new[] { Cancel })
        .ToArray();
    }
  }

  internal interface IRemoveInstitutionMessageHandler : IMessageHandler { }
}