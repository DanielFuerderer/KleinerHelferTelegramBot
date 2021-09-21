using System.Collections.Generic;
using Data;
using Data.Data;
using Telegram.Bot.Types;
using TelegramBot.Data;

namespace TelegramBot.MessageHandler
{
  internal class ConversationManager
  {
    private readonly ICommunityRepository _communityRepository;

    private readonly Dictionary<User, ConversationInfo> _handlers = new Dictionary<User, ConversationInfo>();

    private readonly MainCommandsMessageHandler _mainMenuMessageHandler;

    private readonly ITelegramBotClient _telegramBotClient;

    private readonly IUserRepository _userRepository;

    public ConversationManager(ITelegramBotClient telegramBotClient, IUserRepository userRepository,
      ICommunityRepository communityRepository)
    {
      _telegramBotClient = telegramBotClient;
      _userRepository = userRepository;
      _communityRepository = communityRepository;

      _mainMenuMessageHandler = new MainCommandsMessageHandler(_telegramBotClient,
        mmh => new ShowUserInfoMessageHandler(_telegramBotClient, mmh, _userRepository),
        mmh => new AssignInstitutionMessageHandler(_communityRepository, _userRepository, _telegramBotClient, mmh),
        mmh => new RemoveInstitutionMessageHandler(_userRepository, _telegramBotClient, mmh),
        mmh => new ShowStatisticMessageHandler(_telegramBotClient, new UserStatisticService(_userRepository), mmh));
    }

    public ConversationInfo Get(User user)
    {
      if (_handlers.TryGetValue(user, out var conversationInfo))
      {
        return conversationInfo;
      }

      IMessageHandler handler;

      if (_userRepository.Exists(user.Id.ToString()))
      {
        handler = _mainMenuMessageHandler;
      }
      else
      {
        _userRepository.AddUser(new UserInformation(user.Id.ToString(), user.FirstName));
        _userRepository.Save();

        handler = new WelcomeMessageHandler(_telegramBotClient,
          new AssignZipCodeMessageHandler(_telegramBotClient, _userRepository, _communityRepository,
            _mainMenuMessageHandler));
      }

      conversationInfo = new ConversationInfo(handler);

      _handlers.Add(user, conversationInfo);

      return conversationInfo;
    }

    public void Handle(Message message)
    {
      var handler = Get(message.From);

      handler.Handle(message);
    }
  }
}