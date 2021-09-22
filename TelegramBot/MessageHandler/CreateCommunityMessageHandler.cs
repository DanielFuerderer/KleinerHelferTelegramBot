using System;
using System.Text.RegularExpressions;
using Data;
using Data.Data;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{

  internal abstract class CreateCommunityMessageHandler : IMessageHandler
  {
    private readonly ICommunityRepository communityRepository;
    private readonly Func<Community, IMessageHandler> onCreated;

    public CreateCommunityMessageHandler(
      ICommunityRepository communityRepository,
      Func<Community, IMessageHandler> onCreated)
    {
      this.communityRepository = communityRepository;
      this.onCreated = onCreated;
    }

    public IMessageHandler Handle(Message message)
    {
      var community = GetCommunity(message);
      if(community == null)
      {
        return this;
      }

      communityRepository.AddCommunity(community, null);
      communityRepository.Save();

      return onCreated?.Invoke(community);
    }

    protected abstract Community GetCommunity(Message message);

    public IMessageHandler OnEnter(Message message)
    {
      return this;
    }
  }

  internal class CreateCommunityWithZipCodeMessageHandler : CreateCommunityMessageHandler
  {
    private readonly int zipCode;
    private readonly ITelegramBotClient _telegramBotClient;

    public CreateCommunityWithZipCodeMessageHandler(
      ICommunityRepository communityRepository,
      ITelegramBotClient telegramBotClient,
      int zipCode,
      Func<Community, IMessageHandler> onCreated)
      : base(communityRepository, onCreated)
    {
      this.zipCode = zipCode;
      this._telegramBotClient = telegramBotClient;
    }

    protected override Community GetCommunity(Message message)
    {
      var input = message.Text.Trim(' ');

      if(string.IsNullOrEmpty(input))
      {
        return null;
      }

      return new Community(input, this.zipCode);
    }
  }

  internal class CreateCommunityWithNameMessageHandler : CreateCommunityMessageHandler
  {
    private readonly ITelegramBotClient telegramBotClient;
    private readonly ICommunityRepository communityRepository;
    private readonly string name;

    public CreateCommunityWithNameMessageHandler(
      ITelegramBotClient telegramBotClient,
      ICommunityRepository communityRepository, 
      string name,
      Func<Community, IMessageHandler> onCreated) : base(communityRepository, onCreated)
    {
      this.telegramBotClient = telegramBotClient;
      this.communityRepository = communityRepository;
      this.name = name;
    }

    protected override Community GetCommunity(Message message)
    {
      var input = message.Text.Trim(' ');

      if (int.TryParse(input, out var zipCode))
      {
        if (!Regex.IsMatch(input, "^[0-9]{5}$"))
        {
          telegramBotClient.Write(message.Chat,
            "Ungültige Postleitzahl. Die Postleitzahl muss genau aus 5 Zahlen bestehen.");

          return null;
        }

        if (communityRepository.TryGetCommunity(zipCode, out var community))
        {
          telegramBotClient.Write(message.Chat, $@"Gemeinde {community.Name} hat bereits die Postleitzahl und wird verwendet.");

          return community;
        }

        return new Community(name, zipCode);
      }

      return null;
    }
  }
}