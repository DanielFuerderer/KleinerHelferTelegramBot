using System;
using System.Text.RegularExpressions;
using Data;
using Data.Data;
using Telegram.Bot.Types;

namespace KleinerHelferBot.MessageHandler
{

  internal abstract class CreateCommunityMessageHandler : IMessageHandler
  {
    private readonly ICommunityRepository _communityRepository;
    private readonly Func<Community, IMessageHandler> _onCreated;

    public CreateCommunityMessageHandler(
      ICommunityRepository communityRepository,
      Func<Community, IMessageHandler> onCreated)
    {
      this._communityRepository = communityRepository;
      this._onCreated = onCreated;
    }

    public IMessageHandler Handle(Message message)
    {
      var community = GetCommunity(message);
      if(community == null)
      {
        return this;
      }

      _communityRepository.AddCommunity(community, null);
      _communityRepository.Save();

      return _onCreated?.Invoke(community);
    }

    protected abstract Community GetCommunity(Message message);

    public IMessageHandler OnEnter(Message message)
    {
      return this;
    }
  }

  internal class CreateCommunityWithZipCodeMessageHandler : CreateCommunityMessageHandler
  {
    private readonly int _zipCode;
    private readonly ITelegramBotClient _telegramBotClient;

    public CreateCommunityWithZipCodeMessageHandler(
      ICommunityRepository communityRepository,
      ITelegramBotClient telegramBotClient,
      int zipCode,
      Func<Community, IMessageHandler> onCreated)
      : base(communityRepository, onCreated)
    {
      this._zipCode = zipCode;
      this._telegramBotClient = telegramBotClient;
    }

    protected override Community GetCommunity(Message message)
    {
      var input = message.Text.Trim(' ');

      if(string.IsNullOrEmpty(input))
      {
        return null;
      }

      return new Community(input, this._zipCode);
    }
  }

  internal class CreateCommunityWithNameMessageHandler : CreateCommunityMessageHandler
  {
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ICommunityRepository _communityRepository;
    private readonly string _name;

    public CreateCommunityWithNameMessageHandler(
      ITelegramBotClient telegramBotClient,
      ICommunityRepository communityRepository, 
      string name,
      Func<Community, IMessageHandler> onCreated) : base(communityRepository, onCreated)
    {
      this._telegramBotClient = telegramBotClient;
      this._communityRepository = communityRepository;
      this._name = name;
    }

    protected override Community GetCommunity(Message message)
    {
      var input = message.Text.Trim(' ');

      if (int.TryParse(input, out var zipCode))
      {
        if (!Regex.IsMatch(input, "^[0-9]{5}$"))
        {
          _telegramBotClient.Write(message.Chat,
            "Ungültige Postleitzahl. Die Postleitzahl muss genau aus 5 Zahlen bestehen.");

          return null;
        }

        if (_communityRepository.TryGetCommunity(zipCode, out var community))
        {
          _telegramBotClient.Write(message.Chat, $@"Gemeinde {community.Name} hat bereits die Postleitzahl und wird verwendet.");

          return community;
        }

        return new Community(_name, zipCode);
      }

      return null;
    }
  }
}