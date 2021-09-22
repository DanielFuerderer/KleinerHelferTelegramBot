using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities
{
  internal class UserMessageStatisticFile
  {
    public List<UserMessageStatistic> UserMessageStatistics { get; set; } = new List<UserMessageStatistic>();
  }

  internal class UserMessageStatistic
  {
    public string UserId { get; set; }

    public List<GroupMessageStatistic> Groups { get; set; } = new List<GroupMessageStatistic>();
  }

  internal class GroupMessageStatistic
  {
    public long ChatId { get; set; }

    public List<DayMessageStatistics> DayStatistics { get; set; } = new List<DayMessageStatistics>();
  }

  internal class DayMessageStatistics
  {
    public string Date { get; set; }

    public MessageStatistic MessageStatistic { get; set; }
  }

  internal class MessageStatistic
  {
    public uint Total { get; set; }

    public uint Links { get; set; }

    public uint Forwards { get; set; }
  }
}
