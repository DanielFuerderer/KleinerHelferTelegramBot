using System;
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
    private string _enteredInstitution;

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

      _telegramBotClient.Write(message.Chat, text);

      return this;
    }

    public IMessageHandler Handle(Message message)
    {
      var input = message.Text.Trim(' ');

      if (input.Equals(Cancel, StringComparison.CurrentCultureIgnoreCase))
      {
        return _mainMenuMessageHandler;
      }

      var institutions = _communityRepository.Institutions
        .Where(c => string.Equals(c.Name, message.Text, StringComparison.CurrentCultureIgnoreCase)).ToArray();

      if (institutions.Length > 1)
        // Multiple institution with same name is not supported
      {
        return this;
      }

      var institution = institutions.FirstOrDefault();

      if (!string.IsNullOrEmpty(_enteredInstitution) && message.Text.StartsWith("Neue Institution") &&
          message.Text.EndsWith("anlegen."))
      {
        var userInformation = _userRepository[message.From.Id.ToString()];
        var userCommunity = userInformation.Community;

        institution = new Institution(_enteredInstitution, userCommunity);
        _communityRepository.AddInstitution(institution);
        _communityRepository.Save();

        _userRepository.StartUpdate(userInformation).AddInstitution(institution).Commit();

        _userRepository.Save();

        _telegramBotClient.Write(message.Chat,
          $"Institution '{institution.Name}' wurde deiner Gemeinde '{institution.Community.Name}' und dir hinzugefügt.");

        return _mainMenuMessageHandler;
      }

      if (institution == null)
      {
        var similarMatches = _communityRepository.Institutions.Select(i => new
        {
          Distance = TextHelper.Compute(i.Name, message.Text), Value = i.Name,
          Threshold = Math.Round(message.Text.Length * 0.2, MidpointRounding.AwayFromZero)
        }).OrderBy(ri => ri.Distance).Where(ri => ri.Distance <= ri.Threshold).Take(3).ToArray();

        var similarInstitutions = similarMatches.Select(sm => sm.Value).ToArray();

        var text = $"Ich konnte die Institution {message.Text} nicht eindeutig zuordnen.";
        if (similarMatches.Any())
        {
          text += $@"

Meintest du vielleicht eine der folgenden Einrichtung? 

{string.Join(Environment.NewLine, similarInstitutions)}";
        }

        var communityName = _userRepository[message.From.Id.ToString()].Community.Name;
        _enteredInstitution = message.Text;
        _telegramBotClient.Write(message.Chat, text, new[]
        {
          $"Neue Institution '{_enteredInstitution}' für deine Gemeinde {communityName} anlegen.",
          Cancel
        }.Concat(similarInstitutions).ToArray());

        return this;
      }

      var user = _userRepository[message.From.Id.ToString()];
      var institutionUsers = _userRepository.GetUsersFrom(institution).ToArray();

      if (user.AssociatedInstitutions.Contains(institution))
      {
        _telegramBotClient.Write(message.Chat, "Die Institution ist dir bereits zugewiesen.");
        return _mainMenuMessageHandler;
      }

      _userRepository.StartUpdate(user).AddInstitution(institution).Commit();

      _userRepository.Save();

      _telegramBotClient.Write(message.Chat, $"Ich habe dir die Institution '{institution.Name}' zugewiesen.");

      if (institutionUsers.Any())
      {
        _telegramBotClient.Write(message.Chat,
          $"Für diese Institution gibt es bereits {institution.Name} interessierte. Du bist nicht allein!");
      }

      return _mainMenuMessageHandler;
    }
  }
}