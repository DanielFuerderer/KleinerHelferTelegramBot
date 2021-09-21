using System;

namespace Data.Data
{
  public class DayGroupMessageStatistic
  {
    public DayGroupMessageStatistic(DateTime dateTime, long chatId)
    {
      DateTime = dateTime;
      ChatId = chatId;
    }

    public DateTime DateTime { get; }

    public long ChatId { get; }

    public uint Total { get; set; }

    public uint Forwards { get; set; }

    public uint Links { get; set; }

    public override bool Equals(object obj)
    {
      return Equals(obj as DayGroupMessageStatistic);
    }

    public bool Equals(DayGroupMessageStatistic other)
    {
      if (other == null)
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return ChatId == other.ChatId &&
        DateTime.Equals(other.DateTime);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(DateTime, ChatId);
    }
  }
}
