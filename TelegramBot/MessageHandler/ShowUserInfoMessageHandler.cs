using System;
using System.Linq;
using Data;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.MessageHandler
{
  internal interface IShowUserInfoMessageHandler : IMessageHandler { }

  internal class ShowUserInfoMessageHandler : IShowUserInfoMessageHandler
  {
    private readonly IMessageHandler _messageHandler;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUserRepository _userRepository;

    public ShowUserInfoMessageHandler(ITelegramBotClient telegramBotClient, IMessageHandler messageHandler,
      IUserRepository userRepository)
    {
      _telegramBotClient = telegramBotClient;
      _messageHandler = messageHandler;
      _userRepository = userRepository;
    }

    public IMessageHandler Handle(Message message)
    {
      return this;
    }

    public IMessageHandler OnEnter(Message message)
    {
      var userInformation = _userRepository[message.From.Id.ToString()];

      var usersOfCommunity = _userRepository.GetUsersFrom(userInformation.Community);

      var associatedInstitutions = string.Empty;
      if (userInformation.AssociatedInstitutions.Any())
      {
        associatedInstitutions = string.Join(Environment.NewLine,
          userInformation.AssociatedInstitutions.Select(ai =>
            $"{ai.Name} ({_userRepository.GetUsersFrom(ai).Count()} insgesamt)"));
      }

      _telegramBotClient.Write(message.Chat,
        @$"Gemeinde:    *{userInformation.Community?.ZipCode?.Value} {userInformation.Community?.Name}* ({usersOfCommunity.Count()} insgesamt)

Einrichtungen:
{associatedInstitutions} ", ParseMode.Markdown);

      return _messageHandler;
    }

    public IMessageHandler GetHandler(Message message)
    {
      return this;
    }
  }
}