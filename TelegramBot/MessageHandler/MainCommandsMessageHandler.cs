using System;
using Microsoft.VisualBasic;

namespace TelegramBot.MessageHandler
{
  internal class MainCommandsMessageHandler : CommandsMessageHandler
  {
    public MainCommandsMessageHandler(
      ITelegramBotClient telegramBotClient,
      IShowUserInfoMessageHandler showUserInfo,
      Func<MainCommandsMessageHandler, IAssignInstitutionMessageHandler> createAssignInstitutionMessageHandler,
      Func<MainCommandsMessageHandler, IRemoveInstitutionMessageHandler> createRemoveInstitutionMessageHandler,
      IShowStatisticMessageHandler showStatistic) : base(
      telegramBotClient)
    {
      AddCommand("info", InformationCommand, showUserInfo.Show);
      AddCommand("status", StatusCommand, showStatistic.Show);
      AddCommand("addinstitution", AssignInstitutionCommand, createAssignInstitutionMessageHandler(this));
      AddCommand("removeinstitution", RemoveInstitutionCommand, createRemoveInstitutionMessageHandler(this));
    }

    public static string InformationCommand => "Meine Informationen";

    public static string AssignInstitutionCommand => "Einrichtung hinzufügen";

    public static string RemoveInstitutionCommand => "Einrichtung entfernen";

    public static string StatusCommand => "Status";
  }
}