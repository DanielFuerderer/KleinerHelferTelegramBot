using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace KleinerHelferBot.MessageHandler
{
  internal class CommandsMessageHandler : IMessageHandler
  {
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ConversationManager _conversationManager;

    public CommandsMessageHandler(ITelegramBotClient telegramBotClient, ConversationManager conversationManager)
    {
      _telegramBotClient = telegramBotClient;
      this._conversationManager = conversationManager;
    }

    public IMessageHandler Handle(Message message)
    {
      return this;
    }

    public IMessageHandler OnEnter(Message message)
    {
      _telegramBotClient.ShowMenu(
        message.Chat,
        @"Was möchtest du tun?",
        Commands.Values.Select<Command, (string, string, Func<Message, bool>)>(c => (c.Text, c.Id, c.Action)));

      return this;
    }

    public Dictionary<string, Command> Commands { get; } = new Dictionary<string, Command>();

    protected void AddCommand(string command, string text, Action<Message> action)
    {
      Commands.Add(command, new Command(text, command, m =>
      {
        action(m);

        return false;
      }));
    }

    protected void AddCommand(string command, string text, IMessageHandler messageHandler)
    {
      Commands.Add(command, new Command(text, command, m =>
      {
        _conversationManager.Activate(messageHandler, m);

        return true;
      }));
    }
  }

  internal class Command
  {
    public Command(string text, string id, Func<Message, bool> action)
    {
      Text = text;
      Id = id;
      Action = action;
    }

    public string Id { get; }

    public string Text { get; }

    public Func<Message, bool> Action { get; }
  }
}