using System;
using System.Linq;
using System.Text.RegularExpressions;
using Data;
using Data.Data;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  class GetCommunityMessageHandler : IMessageHandler
  {
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ICommunityRepository _communityRepository;
    private readonly IMessageHandler messageHandler;
    private readonly Func<Community, IMessageHandler> _onReceivedCommunity;

    public GetCommunityMessageHandler(
      ITelegramBotClient telegramBotClient,
      ICommunityRepository communityRepository,
      IMessageHandler messageHandler,
      Func<Community, IMessageHandler> onReceivedCommunity)
    {
      _telegramBotClient = telegramBotClient;
      this._communityRepository = communityRepository;
      this.messageHandler = messageHandler;
      _onReceivedCommunity = onReceivedCommunity;
    }

    public IMessageHandler Handle(Message message)
    {
      var input = message.Text.Trim(' ').ToLower();
      Community community;

      if (int.TryParse(input, out var zipCode))
      {
        if (!Regex.IsMatch(input, "^[0-9]{5}$"))
        {
          _telegramBotClient.Write(message.Chat,
            "Ungültige Postleitzahl. Die Postleitzahl muss genau aus 5 Zahlen bestehen.");

          return this;
        }

        if (!_communityRepository.TryGetCommunity(zipCode, out community))
        {
          _telegramBotClient.Write(message.Chat, @"Ich konnte die Postleitzahl nicht zuordnen.

Um einen neuen Ort hinzuzufügen schreibe mir bitte noch den Namen des Ortes: ");

          return new CreateCommunityWithZipCodeMessageHandler(
            _communityRepository,
            _telegramBotClient,
            zipCode,
            c => AssignCommunity(message.From.Id, c));
        }
      }
      else
      {
        community = _communityRepository.Communities.FirstOrDefault(c => c.Name.ToLower().Contains(input));

        if (community == null)
        {
          _telegramBotClient.Write(message.Chat, @"Ich konnte den Ort nicht finden.

Bitte pass die Eingabe an oder füge einen neuen Gemeinde hinzu indem du mir noch die Postleitzahl nennst: ");

          return new CreateCommunityWithNameMessageHandler(
            _telegramBotClient,
            _communityRepository,
            message.Text,
            c => AssignCommunity(message.From.Id, c));
        }
      }

      return AssignCommunity(message.From.Id, community);
    }

    private IMessageHandler AssignCommunity(long id, Community community)
    {
      return _onReceivedCommunity(community);
    }

    public IMessageHandler OnEnter(Message message)
    {
      return this;
    }
  }
}