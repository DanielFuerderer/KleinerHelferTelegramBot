using Data;
using Data.Data;
using KleinerHelferBot;
using KleinerHelferBot.MessageStatistic;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;

namespace TelegramBotTests.MessageStatistic
{
  [TestFixture]
  public class MessageStatisticServiceTests
  {
    [SetUp]
    public void SetUp()
    {
      _textMessage = new Message { Text = "simple" };

      _mockTelegramBotClient = new Mock<ITelegramBotClient>();
      _mockUserRepository = new Mock<IUserRepository>();
      _mockMessageStatisticRepository = new Mock<IMessageStatisticRepository>();
    }

    private Message _textMessage;

    internal MessageStatisticService Create()
    {
      return new MessageStatisticService(
        _mockTelegramBotClient.Object,
        _mockMessageStatisticRepository.Object,
        _mockUserRepository.Object);
    }

    private Mock<ITelegramBotClient> _mockTelegramBotClient;
    private Mock<IMessageStatisticRepository> _mockMessageStatisticRepository;
    private Mock<IUserRepository> _mockUserRepository;

    [Test]
    public void OnGroupMessage_FirstMessageOfUserTextMessage_TotalIsOne()
    {
      // arrange
      var messageStatisticService = Create();

      var userId = 1;
      var chatId = 2;

      // act
      _mockTelegramBotClient.Raise(m => m.OnGroupMessage += null,
        new User { Id = userId },
        new Chat { Id = chatId },
        _textMessage);

      // assert
      Assert.AreEqual(1, messageStatisticService.GetStatistic(userId.ToString(), chatId).Total);
    }

    [Test]
    public void AnalyzeMessage_NoMessageForUser_AllValuesAreZero()
    {
      // arrange
      var messageStatisticService = Create();

      // act

      // assert
      var statistic = messageStatisticService.GetStatistic(1.ToString(), 2);

      Assert.Zero(statistic.Total);
      Assert.Zero(statistic.Forwards);
      Assert.Zero(statistic.Links);
    }

    [Test]
    public void AnalyzeMessage_FirstMessageOfUserSimpleMessage_TotalIsOne()
    {
      // arrange
      var messageStatisticService = Create();

      var userInformation = new UserInformation("1", "");
      var chatId = 2;

      // act
      messageStatisticService.AnalyzeMessage(userInformation, chatId, new Message { Text = "simple" });

      // assert
      Assert.AreEqual(1, messageStatisticService.GetStatistic(userInformation.Id, chatId).Total);
    }
  }
}