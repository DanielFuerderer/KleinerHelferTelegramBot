using System.Collections.Generic;

namespace TelegramBot.MessageHandler
{
  internal class TrimmedCaseInsensitiveEqualityComparer : IEqualityComparer<string>
  {
    public bool Equals(string x, string y)
    {
      return string.Equals(Normalize(x), Normalize(y));
    }

    public int GetHashCode(string obj)
    {
      return Normalize(obj).GetHashCode();
    }

    private string Normalize(string s)
    {
      return s.Trim(' ').ToLower();
    }
  }
}