using System;
using System.Linq;
using Data;
using Data.Data;
using KleinerHelferBot.MessageHandler;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal class GetInstitutionMessageHandler : IMessageHandler
  {
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ICommunityRepository _communityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageHandler _back;
    private readonly Func<Institution, IMessageHandler> _onReceiveInstitution;
    private readonly Func<IMessageHandler> _onAbort;
    private const string Cancel = "Abbrechen";

    public GetInstitutionMessageHandler(
      ITelegramBotClient telegramBotClient,
      ICommunityRepository communityRepository,
      IUserRepository userRepository,
      IMessageHandler back,
      Func<Institution, IMessageHandler> onReceiveInstitution,
      Func<IMessageHandler> onAbort)
    {
      _telegramBotClient = telegramBotClient;
      _communityRepository = communityRepository;
      _userRepository = userRepository;
      _back = back;
      _onReceiveInstitution = onReceiveInstitution;
      _onAbort = onAbort;
    }

    public IMessageHandler Handle(Message message)
    {
      var input = message.Text.Trim(' ');

      if (input.Equals(Cancel, StringComparison.CurrentCultureIgnoreCase))
      {
        return _back;
      }

      var institutions = _communityRepository.Institutions
        .Where(c => string.Equals(c.Name, message.Text, StringComparison.CurrentCultureIgnoreCase)).ToArray();

      if (institutions.Length > 1)
        // Multiple institution with same name is not supported
      {
        return this;
      }

      var institution = institutions.FirstOrDefault();

      if (institution == null)
      {
        var userInformation = _userRepository[message.From.Id.ToString()];

        var similarMatches = _communityRepository
          .GetInstitutions(userInformation.Community.ZipCode)
          .Select(i => new
        {
          Distance = TextHelper.Compute(i.Name, message.Text),
          Value = i.Name,
          Threshold = Math.Round(message.Text.Length * 0.4, MidpointRounding.AwayFromZero)
        }).OrderBy(ri => ri.Distance)
          .Where(ri => ri.Distance <= ri.Threshold).Take(3)
          .ToArray();

        var similarInstitutions = similarMatches.Select(sm => sm.Value).ToArray();

        var text = $"Ich konnte die Institution {message.Text} nicht eindeutig zuordnen.";
        if (similarMatches.Any())
        {
          text += $@"

Welche Einrichtung möchtest du hinzufügen? 

{string.Join(Environment.NewLine, similarInstitutions)}";
        }

        var communityName = _userRepository[message.From.Id.ToString()].Community.Name;

        _telegramBotClient.Write(message.Chat, text, new[]
        {
          $"Neue Institution '{input}' für deine Gemeinde {communityName} anlegen.",
          Cancel
        }.Concat(similarInstitutions).ToArray());

        return new GetTextMessageHandler(institutionName =>
        {
          if (string.Equals(institutionName, Cancel, StringComparison.CurrentCultureIgnoreCase))
          {
            return _onAbort();
          }

          if (institutionName.StartsWith("Neue Institution") &&
              institutionName.EndsWith("anlegen."))
          {
            // use original institution name
            institutionName = input;
          }

          var userCommunity = userInformation.Community;

          institution = new Institution(institutionName, userCommunity);

          _communityRepository.AddInstitution(institution);
          _communityRepository.Save();

          return _onReceiveInstitution(institution);
        });
      }

      return _onReceiveInstitution(institution);
    }

    public IMessageHandler OnEnter(Message message)
    {
      return this;
    }
  }
}