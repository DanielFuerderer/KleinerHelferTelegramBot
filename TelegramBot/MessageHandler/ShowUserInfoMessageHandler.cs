using System;
using System.Linq;
using Data;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.MessageHandler
{
  internal interface IShowUserInfoMessageHandler //: IMessageHandler { }
  {
    public void Show(Message message);
  }

  internal class ShowUserInfoMessageHandler : IShowUserInfoMessageHandler
  {
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUserRepository _userRepository;

    public ShowUserInfoMessageHandler(ITelegramBotClient telegramBotClient, IUserRepository userRepository)
    {
      _telegramBotClient = telegramBotClient;
      _userRepository = userRepository;
    }

    //public IMessageHandler Handle(Message message)
    //{
    //  return this;
    //}

    public void Show(Message message)
    {
      var userId = message.From.IsBot ? message.Chat.Id : message.From.Id;

      var userInformation = _userRepository[userId.ToString()];

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

      //return _messageHandler;
    }

    //public IMessageHandler GetHandler(Message message)
    //{
    //  return this;
    //}
  }
}