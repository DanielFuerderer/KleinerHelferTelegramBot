using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Data;
using Data.Data;
using KleinerHelferBot.Data;
using Telegram.Bot.Types;

namespace KleinerHelferBot.MessageHandler
{
  internal class ConversationManager
  {
    private readonly ICommunityRepository _communityRepository;

    private readonly Dictionary<User, ConversationInfo> _handlers = new Dictionary<User, ConversationInfo>(IdUserEqualityComparer.Instance);

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
        new ShowUserInfoMessageHandler(_telegramBotClient, _userRepository),
        mmh => new AssignInstitutionMessageHandler(_communityRepository, _userRepository, _telegramBotClient, mmh),
        mmh => new RemoveInstitutionMessageHandler(_userRepository, _telegramBotClient, mmh),
        new ShowStatisticMessageHandler(_telegramBotClient, new UserStatisticService(_userRepository)),
        this);
    }

    public ConversationInfo Get(User user)
    {
      if (_handlers.TryGetValue(user, out var conversationInfo))
      {
        return conversationInfo;
      }

      if (NewUser(user))
      {
        _userRepository.AddUser(new UserInformation(user.Id.ToString(), user.FirstName));
        _userRepository.Save();
      }

      var handler = HasCommunityAssigned(user)
        ? (IMessageHandler)_mainMenuMessageHandler
        : CreateWelcomeMessageHandler();

      conversationInfo = new ConversationInfo(handler);

      _handlers.Add(user, conversationInfo);

      return conversationInfo;
    }

    private WelcomeMessageHandler CreateWelcomeMessageHandler()
    {
      return new WelcomeMessageHandler(
        _telegramBotClient,
        new AssignCommunityMessageHandler(
          _telegramBotClient,
          _userRepository,
          _communityRepository,
          _mainMenuMessageHandler));
    }

    private bool HasCommunityAssigned(User user)
    {
      return _userRepository[user.Id.ToString()].Community != null;
    }

    private bool NewUser(User user)
    {
      return !_userRepository.Exists(user.Id.ToString());
    }

    public void Handle(Message message)
    {
      var handler = Get(message.From);

      handler.Handle(message);
    }

    public void Activate(IMessageHandler messageHandler, Message message)
    {
      var user = message.From.IsBot
        ? new User
        {
          Id = message.Chat.Id,
          FirstName = message.Chat.FirstName,
          Username = message.Chat.Username
        }
        : message.From;

      var handler = Get(user);

      handler.Active(messageHandler, message);
    }
  }

  internal class IdUserEqualityComparer : IEqualityComparer<User>
  {
    public static IdUserEqualityComparer Instance = new IdUserEqualityComparer();

    public bool Equals([AllowNull] User x, [AllowNull] User y)
    {
      if (ReferenceEquals(x, y)) return true;
      if (x == null || y == null) return false;

      return x.Id == y.Id;
    }

    public int GetHashCode([DisallowNull] User obj)
    {
      return obj.Id.GetHashCode();
    }
  }
}