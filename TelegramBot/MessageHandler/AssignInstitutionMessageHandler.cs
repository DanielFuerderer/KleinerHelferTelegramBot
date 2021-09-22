using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Data;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal interface IAssignInstitutionMessageHandler : IMessageHandler { }

  internal class AssignInstitutionMessageHandler : IAssignInstitutionMessageHandler
  {
    private const string Cancel = "Abbrechen";
    private readonly ICommunityRepository _communityRepository;
    private readonly MainCommandsMessageHandler _mainMenuMessageHandler;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUserRepository _userRepository;

    public AssignInstitutionMessageHandler(ICommunityRepository communityRepository, IUserRepository userRepository,
      ITelegramBotClient telegramBotClient, MainCommandsMessageHandler mainMenuMessageHandler)
    {
      _communityRepository = communityRepository;
      _userRepository = userRepository;
      _telegramBotClient = telegramBotClient;
      _mainMenuMessageHandler = mainMenuMessageHandler;
    }

    public IMessageHandler OnEnter(Message message)
    {
      var institutions = _communityRepository
        .GetInstitutions(_userRepository[message.From.Id.ToString()].Community.ZipCode).OrderBy(i => i.Name).ToArray();

      string text;
      if (institutions.Length == 0)
      {
        text = @"Es ist noch keine Einrichtung in deiner Gemeinde zugeordnet.

Bitte schreibe mir den Namen deiner Einrichtung und ich füge sie zu deiner Gemeinde hinzu.";
      }
      else
      {
        text = $@"Folgende Einrichtungen sind deiner Gemeinde zugeordnet: 

{string.Join(Environment.NewLine, institutions.Select(i => i.Name))}

Ist die gewünschte Einrichtung nicht aufgelistet, kannst du sie durch freie Eingabe hinzufügen.";
      }

      _telegramBotClient.Write(
        message.Chat,
        text,
        institutions.Select(i => i.Name).Concat(new[] { Cancel }).ToArray());

      return new GetInstitutionMessageHandler(
        _telegramBotClient,
        _communityRepository,
        _userRepository,
        _mainMenuMessageHandler,
        institution => AssignInstitution(message.From.Id, message.Chat, institution),
        () => OnAbort(message.Chat));
    }

    private IMessageHandler OnAbort(Chat chat)
    {
      _telegramBotClient.Write(chat, "Einrichtung zuweisen abgebrochen.");

      return _mainMenuMessageHandler;
    }

    private IMessageHandler AssignInstitution(long fromId, Chat chat, Institution institution)
    {
      var user = _userRepository[fromId.ToString()];
      var institutionUsers = _userRepository.GetUsersFrom(institution).ToArray();

      if (user.AssociatedInstitutions.Contains(institution, InstitutionComparer.Instance))
      {
        _telegramBotClient.Write(chat, "Die Institution ist dir bereits zugewiesen.");
        return _mainMenuMessageHandler;
      }

      _userRepository.StartUpdate(user).AddInstitution(institution).Commit();
      _userRepository.Save();

      _telegramBotClient.Write(chat, $"Ich habe dir die Institution '{institution.Name}' zugewiesen.");

      if (institutionUsers.Any())
      {
        _telegramBotClient.Write(chat,
          $"Für diese Institution gibt es bereits {institution.Name} interessierte. Du bist nicht allein!");
      }

      return _mainMenuMessageHandler;
    }

    public IMessageHandler Handle(Message message)
    {
      return this;
    }
  }

  internal class InstitutionComparer : IEqualityComparer<Institution>
  {
    public bool Equals(Institution x, Institution y)
    {
      if (ReferenceEquals(x, y)) return true;
      if (ReferenceEquals(x, null)) return false;
      if (ReferenceEquals(y, null)) return false;
      if (x.GetType() != y.GetType()) return false;

      return x.Name == y.Name
             && Equals(x.Community.ZipCode, y.Community.ZipCode);
    }

    public int GetHashCode(Institution obj)
    {
      return HashCode.Combine(obj.Name, obj.Community);
    }

    public static IEqualityComparer<Institution> Instance { get; } = new InstitutionComparer();
  }
}