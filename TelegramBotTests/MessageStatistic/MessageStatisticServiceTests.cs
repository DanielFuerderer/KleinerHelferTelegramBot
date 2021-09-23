using NUnit.Framework;
using Telegram.Bot.Types;
using TelegramBot.MessageStatistic;

namespace TelegramBot
{
  [TestFixture]
  public class MessageStatisticServiceTests
  {
    private Message _textMessage;

    [SetUp]
    public void SetUp()
    {
      _textMessage = new Message { Text = "simple" };
    }

    internal MessageStatisticService Create()
    {
      return new MessageStatisticService();
    }

    [Test]
    public void OnGroupMessage_FirstMessageOfUserTextMessage_TotalIsOne()
    {
      // arrange
      var mockTelegramBotClient = new Moq.Mock<ITelegramBotClient>();
      var messageStatisticService = new MessageStatisticService(mockTelegramBotClient.Object, null);

      var userId = 1;
      var chatId = 2;

      // act
      mockTelegramBotClient.Raise(m => m.OnGroupMessage += null, new User { Id = userId }, new Chat { Id = chatId }, _textMessage);

      // assert
      Assert.AreEqual(1, messageStatisticService.GetStatistic(userId, chatId).Total);
    }

    [Test]
    public void AnalyzeMessage_NoMessageForUser_AllValuesAreZero()
    {
      // arrange
      var messageStatisticService = Create();

      // act

      // assert
      var statistic = messageStatisticService.GetStatistic(1, 2);

      Assert.Zero(statistic.Total);
      Assert.Zero(statistic.Forwards);
      Assert.Zero(statistic.Links);
    }

    [Test]
    public void AnalyzeMessage_FirstMessageOfUserSimpleMessage_TotalIsOne()
    {
      // arrange
      var messageStatisticService = Create();

      int userId = 1;
      int chatId = 2;

      // act
      messageStatisticService.AnalyzeMessage(userId, chatId, new Message { Text = "simple" });

      // assert
      Assert.AreEqual(1, messageStatisticService.GetStatistic(userId, chatId).Total);
    }
  }
}
