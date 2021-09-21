
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Data.Data;

namespace Data
{
  public class MessageStatisticRepository
  {
    private readonly Dictionary<UserInformation, HashSet<DayGroupMessageStatistic>> _userMessageStatistics = new Dictionary<UserInformation, HashSet<DayGroupMessageStatistic>>();
    private readonly UserRepository _userRepository;

    public MessageStatisticRepository(string fileName, UserRepository userRepository)
    {
      FileName = fileName;

      _userRepository = userRepository;

      if (File.Exists(fileName))
      {
        var serializedData = File.ReadAllText(fileName);

        var result = JsonSerializer.Deserialize<Entities.UserMessageStatisticFile>(serializedData);

        foreach (var userMessageStatistic in result.UserMessageStatistics)
        {
          var user = userRepository[userMessageStatistic.UserId];

          var dayGroupMessageStatistic = new HashSet<DayGroupMessageStatistic>();
          _userMessageStatistics.Add(user, dayGroupMessageStatistic);

          foreach (var group in userMessageStatistic.Groups)
          {
            foreach (var day in group.DayStatistics)
            {
              dayGroupMessageStatistic.Add(new DayGroupMessageStatistic(
                DateTime.ParseExact(day.Date, PersistenceDateTimeFormat, null),
                group.ChatId)
              {
                Forwards = day.MessageStatistic.Forwards,
                Links = day.MessageStatistic.Links,
                Total = day.MessageStatistic.Total
              });
            }
          }
        }
      }
    }

    public void Save()
    {
      var userMessageStatisticFile = new Entities.UserMessageStatisticFile();

      foreach (var u in _userMessageStatistics)
      {
        var groups = new List<Entities.GroupMessageStatistic>();

        foreach (var group in u.Value.GroupBy(v => v.ChatId))
        {
          var dayStatistics = new List<Entities.DayMessageStatistics>();
          foreach (var date in group.ToDictionary(g => g.DateTime))
          {
            dayStatistics.Add(new Entities.DayMessageStatistics
            {
              Date = date.Key.ToString(PersistenceDateTimeFormat),
              MessageStatistic = new Entities.MessageStatistic
              {
                Forwards = date.Value.Forwards,
                Links = date.Value.Links,
                Total = date.Value.Total
              }
            });
          }

          groups.Add(new Entities.GroupMessageStatistic
          {
            ChatId = group.Key,
            DayStatistics = dayStatistics
          });
        }

        userMessageStatisticFile.UserMessageStatistics.Add(
          new Entities.UserMessageStatistic
          {
            UserId = u.Key.Id,
            Groups = groups
          });
      }

      var serializedData = JsonSerializer.Serialize(userMessageStatisticFile);

      Directory.CreateDirectory(Path.GetDirectoryName(FileName));

      File.WriteAllText(FileName, serializedData);
    }

    public string FileName { get; private set; }
    private const string PersistenceDateTimeFormat = @"yyyyMMdd";

    public void Update(string userId, DayGroupMessageStatistic dayGroupMessageStatistic)
    {
      var user = _userRepository[userId];

      if (!_userMessageStatistics.TryGetValue(user, out var userDayGroupMessageStatistic))
      {
        _userMessageStatistics.Add(user, userDayGroupMessageStatistic = new HashSet<DayGroupMessageStatistic>());
      }

      userDayGroupMessageStatistic.Remove(dayGroupMessageStatistic);
      userDayGroupMessageStatistic.Add(dayGroupMessageStatistic);
    }
  }
}
