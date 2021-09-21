using System;
using System.Linq;
using System.Text.RegularExpressions;
using Data;
using Data.Data;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal class AssignZipCodeMessageHandler : IMessageHandler
  {
    private readonly ICommunityRepository _communityRepository;
    private readonly MainCommandsMessageHandler _mainMenuMessageHandler;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUserRepository _userRepository;
    private string _enteredCommunityName;

    private int? _enteredZipCode;
    private Community _selectedCommunity;

    public AssignZipCodeMessageHandler(ITelegramBotClient telegramBotClient, IUserRepository userRepository,
      ICommunityRepository communityRepository, MainCommandsMessageHandler mainMenuMessageHandler)
    {
      _telegramBotClient = telegramBotClient;
      _userRepository = userRepository;
      _communityRepository = communityRepository;
      _mainMenuMessageHandler = mainMenuMessageHandler;
    }

    public IMessageHandler Handle(Message message)
    {
      Community community = null;
      var input = message.Text.Trim(' ');

      if (_selectedCommunity != null && input.ToLower() == "ja")
      {
        community = _selectedCommunity;
      }
      else if (int.TryParse(input, out var zipCode))
      {
        if (!Regex.IsMatch(input, "^[0-9]{5}$"))
        {
          _telegramBotClient.Write(message.Chat,
            "Ungültige Postleitzahl. Die Postleitzahl muss genau aus 5 Zahlen bestehen.");

          return this;
        }

        if (_enteredCommunityName != null)
        {
          community = new Community(_enteredCommunityName, zipCode);

          _communityRepository.AddCommunity(community, existingCommunity => _telegramBotClient.Write(message.Chat,
            @$"Es existiert bereits eine Gemeinde mit dieser Postleitzahl.

Soll ich dich der Gemeinde {existingCommunity.Name} zuordnen?", "Ja", "Nein"));

          _selectedCommunity = community;

          return this;
        }

        if (!_communityRepository.TryGetCommunity(zipCode, out community))
        {
          _telegramBotClient.Write(message.Chat, @"Ich konnte die Postleitzahl nicht zuordnen.

Um einen neuen Ort hinzuzufügen schreibe mir bitte noch den Namen des Ortes: ");

          _enteredZipCode = zipCode;

          return this;
        }
      }
      else
      {
        if (_enteredZipCode != null)
        {
          community = new Community(input, _enteredZipCode.Value);
          _communityRepository.AddCommunity(community, existingCommunity => _telegramBotClient.Write(message.Chat,
            @$"Es existiert bereits eine Gemeinde mit dieser Postleitzahl.

Soll ich dich der Gemeinde {existingCommunity.Name} zuordnen?", "Ja", "Nein"));

          _selectedCommunity = community;

          return this;
        }

        community = _communityRepository.Communities.FirstOrDefault(c => c.Name.ToLower().Contains(input.ToLower()));

        if (community == null)
        {
          _telegramBotClient.Write(message.Chat, @"Ich konnte den Ort nicht finden.

Bitte pass die Eingabe an oder füge einen neuen Gemeinde hinzu indem du mir noch die Postleitzahl nennst: ");

          _enteredCommunityName = input;

          return this;
        }
      }

      var usersOfCommunity = _userRepository.GetUsersFrom(community).ToList();

      _userRepository.StartUpdate(_userRepository[message.From.Id.ToString()]).SetCommunity(community).Commit();

      //_userRepository.UpdateCommunity(message.From.Id, community);
      //_userRepository.Save();

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

      _telegramBotClient.Write(message.Chat, response);

      return _mainMenuMessageHandler;
    }

    public IMessageHandler OnEnter(Message message)
    {
      _telegramBotClient.Write(message.Chat, "Bitte teile mir deine Postleitzahl oder Name deiner Gemeinde mit: ");

      return this;
    }
  }
}