using System.Collections.Generic;
using Data.Data;

namespace KleinerHelferBot.Data
{
  internal class CommunityStatistic
  {
    public CommunityStatistic(Community community, IReadOnlyList<InstitutionStatistic> institutionStatistics,
      IReadOnlyList<UserInformation> users)
    {
      InstitutionStatistics = institutionStatistics;
      Users = users;
      Community = community;
    }

    public Community Community { get; }

    public IReadOnlyList<UserInformation> Users { get; }

    public IReadOnlyList<InstitutionStatistic> InstitutionStatistics { get; }
  }
}