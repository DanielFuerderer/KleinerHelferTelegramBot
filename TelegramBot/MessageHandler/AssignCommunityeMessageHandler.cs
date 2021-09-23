using System;
using System.Linq;
using Data;
using Data.Data;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal class AssignCommunityMessageHandler : IMessageHandler
  {
    private readonly ICommunityRepository _communityRepository;
    private readonly MainCommandsMessageHandler _mainMenuMessageHandler;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUserRepository _userRepository;
    private string _enteredCommunityName;

    private int? _enteredZipCode;
    private Community _selectedCommunity;

    public AssignCommunityMessageHandler(
      ITelegramBotClient telegramBotClient,
      IUserRepository userRepository,
      ICommunityRepository communityRepository,
      MainCommandsMessageHandler mainMenuMessageHandler)
    {
      _telegramBotClient = telegramBotClient;
      _userRepository = userRepository;
      _communityRepository = communityRepository;
      _mainMenuMessageHandler = mainMenuMessageHandler;
    }

    public IMessageHandler Handle(Message message)
    {
      return this;
    }

    public IMessageHandler OnEnter(Message message)
    {
      var userId = message.From.IsBot ? message.Chat.Id : message.From.Id;

      _telegramBotClient.Write(message.Chat, "Bitte teile mir deine Postleitzahl oder Name deiner Gemeinde mit: ");

      return new GetCommunityMessageHandler(
        _telegramBotClient,
        _communityRepository,
        this,
        c => OnGetCommunity(userId.ToString(), message.Chat, c));
    }

    private IMessageHandler OnGetCommunity(string userId, Chat chat, Community community)
    {
      var usersOfCommunity = _userRepository.GetUsersFrom(community).ToList();

      _userRepository
        .StartUpdate(_userRepository[userId])
        .SetCommunity(community)
        .Commit();

      _userRepository.Save();

      var response = $"Okay. Ich habe dich der Gemeinde {community.ZipCode} {community.Name} zugeordnet.";

      if (usersOfCommunity.Any())
      {
        response += Environment.NewLine + Environment.NewLine +
                    $"Es sind bereits {usersOfCommunity.Count} Personen deiner Gemeinde zugeorndet";
      }
      else
      {
        response += Environment.NewLine + Environment.NewLine +
                    "Du bist die erste Person aus dieser Gemeinde. Wir freuen uns darauf mehr aus deiner Gemeinde kennen zu lernen.";
      }

      _telegramBotClient.Write(chat, response);

      return _mainMenuMessageHandler;

    }
  }
}