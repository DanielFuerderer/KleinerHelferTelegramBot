using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace TelegramBot.MessageHandler
{
  internal class CommandsMessageHandler : IMessageHandler
  {
    private readonly ITelegramBotClient _telegramBotClient;

    public readonly MessageHandlerMapping MessageHandlers = new MessageHandlerMapping();
    protected readonly Dictionary<string, Action<Message>> Actions = new Dictionary<string, Action<Message>>();

    public CommandsMessageHandler(ITelegramBotClient telegramBotClient)
    {
      _telegramBotClient = telegramBotClient;
    }

    public IMessageHandler Handle(Message message)
    {
      var input = message.Text.Trim(' ').ToLower();
      if (Commands.TryGetValue(input, out var command))
      {
        return command.Action(message);
      }

      return this;
    }

    public IMessageHandler OnEnter(Message message)
    {
      _telegramBotClient.ShowMenu(
        message.Chat,
        @"Was möchtest du tun?",
        Commands.Values.Select<Command, (string, string, Action<Message>)>(c =>
          (c.Text, c.Id, m => { c.Action(m); }
          )));

      return this;
    }

    public Dictionary<string, Command> Commands { get; } = new Dictionary<string, Command>();

    protected void AddCommand(string command, string text, Action<Message> action)
    {
      Commands.Add(command, new Command(text, command, m =>
      {
        action(m);
        return this;
      }));
    }

    protected void AddCommand(string command, string text, IMessageHandler messageHandler)
    {
      Commands.Add(command, new Command(text, command, m => messageHandler));
    }
  }

  internal class Command
  {
    public Command(string text, string id, Func<Message, IMessageHandler> action)
    {
      Text = text;
      Id = id;
      Action = action;
    }

    public string Id { get; }

    public string Text { get; }

    public Func<Message, IMessageHandler> Action { get; }
  }
}