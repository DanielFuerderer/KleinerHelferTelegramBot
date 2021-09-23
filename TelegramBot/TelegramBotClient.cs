﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#pragma warning disable 618

namespace TelegramBot
{
  internal class TelegramBotClient : ITelegramBotClient
  {
    public delegate void MessageAction(User user, Chat chat, Message message);

    public delegate void UserGroupAction(User user, Chat chat);

    private readonly Telegram.Bot.TelegramBotClient _telegramBotClient;
    private Message _lastMenuMessage;
    private Chat _lastMenuChat;
    private Dictionary<(long, string), Action<Message>> _commands = new Dictionary<(long, string), Action<Message>>();
    private Action _lastMenu;

    public TelegramBotClient(string token)
    {
      _telegramBotClient = new Telegram.Bot.TelegramBotClient(token);

      Me = _telegramBotClient.BotId;

      _telegramBotClient.OnMessage += TelegramBotClientOnMessage;
      _telegramBotClient.OnUpdate += TelegramBotClientOnUpdate;

      _telegramBotClient.StartReceiving();

      //_telegramBotClient.SetMyCommandsAsync(new[]
      //{
      //  new BotCommand { Command = "info", Description = "Meine Informationen" },
      //  new BotCommand { Command = "addinstitution", Description = "Einrichtung zuweisen" },
      //  new BotCommand { Command = "removeinstitution", Description = "Einrichtung entfernen" },
      //  new BotCommand { Command = "status", Description = "Status" }
      //}).Wait(1000);
    }

    public long? Me { get; set; }

    public event UserGroupAction OnUserLeftGroup;

    public event UserGroupAction OnUserJoinedGroup;

    public event MessageAction OnPrivateMessage;

    public event MessageAction OnGroupMessage;

    public void Write(Chat chat, string text)
    {
      if (_lastMenuMessage != null)
      {
        _telegramBotClient.DeleteMessageAsync(_lastMenuChat.Id, _lastMenuMessage.MessageId).Wait(1000);
        _lastMenuMessage = null;
        _lastMenuChat = null;
      }

      _telegramBotClient.SendTextMessageAsync(chat.Id, text).Wait(1500);
    }

    public void Write(Chat chat, string text, params string[] replyOptions)
    {
      Write(chat, text, ParseMode.Default, replyOptions);
    }

    public void Write(Chat chat, string text, ParseMode parseMode = default, params string[] replyOptions)
    {
      if (_lastMenuMessage != null)
      {
        _telegramBotClient.DeleteMessageAsync(_lastMenuChat.Id, _lastMenuMessage.MessageId).Wait(1000);
        _lastMenuMessage = null;
        _lastMenuChat = null;
      }

      _telegramBotClient.SendTextMessageAsync(chat.Id, text, parseMode,
        replyMarkup: new ReplyKeyboardMarkup(replyOptions.Select(t => new KeyboardButton(t)))
        { OneTimeKeyboard = true })
        .Wait(1500);
    }

    public void ShowMenu(Chat chat, string text, IEnumerable<(string, string, Action<Message>)> buttons)
    {
      if (_lastMenuMessage != null)
      {
        _telegramBotClient.DeleteMessageAsync(_lastMenuChat.Id, _lastMenuMessage.MessageId).Wait(1000);
        _lastMenuMessage = null;
        _lastMenuChat = null;
      }

      var menu = buttons
        .Select(Create)
        .Select(s => new[] { s })
        .ToArray();

      foreach (var button in buttons)
      {
        RegisterCommands(chat, button.Item2, button.Item3);
      }

      var inlineKeyboard = new InlineKeyboardMarkup(menu);

      _lastMenu = () =>
      {
        _lastMenuMessage = _telegramBotClient.SendTextMessageAsync(
            chat.Id,
            text,
            replyMarkup: inlineKeyboard)
          .Result;
        _lastMenuChat = chat;
      };

      _lastMenu();
    }

    private void RegisterCommands(Chat chat, string data, Action<Message> action)
    {
      _commands.Add((chat.Id, data), action);
    }

    private InlineKeyboardButton Create((string text, string data, Action<Message>) button)
    {
      return InlineKeyboardButton.WithCallbackData(text: button.text, button.data); //, button.data.ToLower());
    }

    private void TelegramBotClientOnUpdate(object sender, UpdateEventArgs e)
    {
      Console.WriteLine($"============  OnUpdate [{e.Update.Id}] =============");
      ShowProperties(e.Update, nameof(e.Update.Id), nameof(e.Update.Type));

      Show("Message", e.Update.Message);
      Show("ChannelPost", e.Update.ChannelPost);
      Show("ChatMember", e.Update.ChatMember);
      Show("MyChatMember", e.Update.MyChatMember);

      Console.WriteLine($"===========  OnUpdateEnd [{e.Update.Id}] ===========");

      if (e.Update.Type == UpdateType.CallbackQuery)
      {
        var message = e.Update.CallbackQuery.Message;

        if (_commands.TryGetValue((message.Chat.Id, e.Update.CallbackQuery.Data), out var action))
        {
          action(message);
          _lastMenu();
        }
        else
        {
          foreach (var key in _commands
            .Keys
            .Where(k => k.Item1 == message.Chat.Id).ToList())
          {
            _commands.Remove(key);
          }
        }
      }
    }

    private void Show(string title, Message message)
    {
      if (message == null)
      {
        return;
      }

      Console.WriteLine(title + ": ");

      Show(message);
    }

    private void Show(Message message)
    {
      ShowProperties(message, nameof(message.Date), nameof(message.MessageId), nameof(message.Type),
        nameof(message.Text));

      Show("Chat", message.Chat);
      Show("From", message.From);
      Show("LeftChatMember", message.LeftChatMember);
      if (message.NewChatMembers != null)
      {
        Console.WriteLine("NewChatMembers: ");
        foreach (var newChatMember in message.NewChatMembers)
        {
          Show(newChatMember);
        }
      }
    }

    private void Show(string title, ChatMemberUpdated updateMyChatMember)
    {
      if (updateMyChatMember == null)
      {
        return;
      }

      Console.WriteLine(title + ": ");

      Show(updateMyChatMember);
    }

    private void Show(ChatMemberUpdated chatMemberUpdated)
    {
      ShowProperties(chatMemberUpdated, nameof(ChatMemberUpdated.Date));

      Show("Chat", chatMemberUpdated.Chat);
      Show("From", chatMemberUpdated.From);
      Show("NewChatMember", chatMemberUpdated.NewChatMember);
      Show("OldChatMember", chatMemberUpdated.OldChatMember);
    }

    private void Show(string title, ChatMember chatMember)
    {
      if (chatMember == null)
      {
        return;
      }

      Console.WriteLine(title + ": ");

      Show(chatMember.User);
    }

    private void Show(string title, User user)
    {
      if (user == null)
      {
        return;
      }

      Console.WriteLine(title + ": ");

      Show(user);
    }

    private void Show(User user)
    {
      ShowProperties(user, nameof(user.Id), nameof(user.FirstName), nameof(user.LastName), nameof(user.Username));
    }

    private void Show(string title, Chat chat)
    {
      if (chat == null)
      {
        return;
      }

      Console.WriteLine(title + ": ");

      Show(chat);
    }

    private void Show(Chat chat)
    {
      ShowProperties(chat, nameof(Chat.Id), nameof(Chat.Type), nameof(Chat.FirstName), nameof(Chat.LastName),
        nameof(Chat.Username), nameof(Chat.Title));
    }

    private static void ShowProperties(object instance, params string[] propertyNames)
    {
      var propsLookUp = instance.GetType().GetProperties().ToDictionary(p => p.Name);

      var propertiesToDisplay = propertyNames.Select(pn => propsLookUp[pn]);

      Console.WriteLine(
        string.Join(", ", propertiesToDisplay.Select(prop => $"{prop.Name}: {prop.GetValue(instance)}")));
    }

    private void TelegramBotClientOnMessage(object sender, MessageEventArgs e)
    {
      Console.WriteLine($"============  OnMessage [{e.Message.MessageId}] ============");

      Show("OnMessage", e.Message);

      var chat = e.Message.Chat;

      if (e.Message.Type == MessageType.ChatMemberLeft)
      {
        OnMemberLeftChat(e.Message.LeftChatMember, chat);
      }
      else if (e.Message.Type == MessageType.ChatMembersAdded)
      {
        foreach (var newChatMember in e.Message.NewChatMembers)
        {
          OnMemberJoinedChat(newChatMember, chat);
        }
      }
      else if (e.Message.Type == MessageType.Text)
      {
        OnTextMessage(e.Message);
      }

      Console.WriteLine($"==========  OnMessageEnd [{e.Message.MessageId}] ===========");
    }

    private void OnTextMessage(Message message)
    {
      if (message.Chat.Type == ChatType.Private)
      {
        OnPrivateMessage?.Invoke(message.From, message.Chat, message);
      }
      else if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
      {
        OnGroupMessage?.Invoke(message.From, message.Chat, message);
      }
    }

    private void OnMemberJoinedChat(User newChatMember, Chat chat)
    {
      if (newChatMember.Id == Me)
      {
        return;
      }

      OnUserJoinedGroup?.Invoke(newChatMember, chat);
    }

    private void OnMemberLeftChat(User member, Chat chat)
    {
      OnUserLeftGroup?.Invoke(member, chat);
    }
  }
}