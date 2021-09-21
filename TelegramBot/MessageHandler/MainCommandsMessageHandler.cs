using System;

namespace TelegramBot.MessageHandler
{
  internal class MainCommandsMessageHandler : CommandsMessageHandler
  {
    public MainCommandsMessageHandler(ITelegramBotClient telegramBotClient,
      Func<MainCommandsMessageHandler, IShowUserInfoMessageHandler> createShowUserInfoMessageHandler,
      Func<MainCommandsMessageHandler, IAssignInstitutionMessageHandler> createAssignInstitutionMessageHandler,
      Func<MainCommandsMessageHandler, IRemoveInstitutionMessageHandler> createRemoveInstitutionMessageHandler,
      Func<MainCommandsMessageHandler, IShowStatisticMessageHandler> createShowStatisticMessageHandler) : base(
      telegramBotClient)
    {
      MessageHandlers.Add(InformationCommand, createShowUserInfoMessageHandler(this));
      MessageHandlers.Add(AssignInstitutionCommand, createAssignInstitutionMessageHandler.Invoke(this));
      MessageHandlers.Add(RemoveInstitutionCommand, createRemoveInstitutionMessageHandler.Invoke(this));
      MessageHandlers.Add(StatusCommand, createShowStatisticMessageHandler.Invoke(this));
    }

    public static string InformationCommand => "Meine Informationen";

    public static string AssignInstitutionCommand => "Einrichtung hinzufügen";

    public static string RemoveInstitutionCommand => "Einrichtung entfernen";

    public static string StatusCommand => "Status";
  }
}