using System.Collections.Generic;

namespace KleinerHelferBot.Data
{
  internal class UserStatistic
  {
    internal UserStatistic(int countOfUsers, IReadOnlyList<CommunityStatistic> communities)
    {
      NumberOfUsers = countOfUsers;
      Communities = communities;
    }

    public int NumberOfUsers { get; }

    public IReadOnlyList<CommunityStatistic> Communities { get; }
  }
}