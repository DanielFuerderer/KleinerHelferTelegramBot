using System.Text;
using KleinerHelferBot.Data;
using Telegram.Bot.Types;

namespace KleinerHelferBot.MessageHandler
{
  internal class ShowStatisticMessageHandler : IShowStatisticMessageHandler
  {
    private readonly UserStatisticService _statisticService;

    private readonly ITelegramBotClient _telegramBotClient;

    public ShowStatisticMessageHandler(ITelegramBotClient telegramBotClient, UserStatisticService statisticService)
    {
      _telegramBotClient = telegramBotClient;
      _statisticService = statisticService;
    }

    public void Show(Message message)
    {
      var statistic = _statisticService.Get();

      var statisticBuilder = new StringBuilder();

      statisticBuilder.AppendLine($"Benutzer: {statistic.NumberOfUsers}");

      foreach (var communityStatistic in statistic.Communities)
      {
        statisticBuilder.AppendLine();
        statisticBuilder.AppendLine($"{communityStatistic.Community.Name} ({communityStatistic.Users.Count})");

        foreach (var institution in communityStatistic.InstitutionStatistics)
        {
          statisticBuilder.AppendLine($"\t {institution.Institution.Name} ({institution.Users.Count})");
        }
      }

      _telegramBotClient.Write(message.Chat, statisticBuilder.ToString());

      //return _mainMenuMessageHandler;
    }

    //public IMessageHandler GetHandler(Message message)
    //{
    //  return this;
    //}

    //public bool Match(Message message)
    //{
    //  var input = message.Text.Trim(' ');

    //  return string.Equals(input, CommandName, StringComparison.CurrentCultureIgnoreCase);
    //}
  }

  internal interface IShowStatisticMessageHandler
  {
    public void Show(Message message);
  }
}