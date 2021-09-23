using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Data;

namespace TelegramBot.Data
{
  internal class UserStatisticService
  {
    private readonly IUserRepository _userRepository;

    public UserStatisticService(IUserRepository userRepository)
    {
      _userRepository = userRepository;
    }

    public UserStatistic Get()
    {
      var assignedUsers = _userRepository.GetAssignedUsers().ToList();

      var usersPerCommunity = assignedUsers.GroupBy(ui => ui.Community);

      var usersInstitutions = assignedUsers
        .SelectMany(ui => ui.AssociatedInstitutions.Select(ai => (ai, ui))).ToArray();

      var usersPerInstitution = usersInstitutions.GroupBy(ui => ui.ai, ui => ui.ui);

      var institutionStatisticsPerCommunity = new Dictionary<Community, List<InstitutionStatistic>>();

      foreach (var userInstitution in usersPerInstitution)
      {
        if (!institutionStatisticsPerCommunity.TryGetValue(userInstitution.Key.Community,
          out var institutionStatistics))
        {
          institutionStatistics = new List<InstitutionStatistic>();
          institutionStatisticsPerCommunity.Add(userInstitution.Key.Community, institutionStatistics);
        }

        institutionStatistics.Add(new InstitutionStatistic(userInstitution.Key, userInstitution.ToList()));
      }

      var institutionPerCommunities = new List<CommunityStatistic>();

      foreach (var userPerCommunity in usersPerCommunity)
      {
        institutionPerCommunities.Add(
          institutionStatisticsPerCommunity.TryGetValue(userPerCommunity.Key, out var institutionPerCommunity)
            ? new CommunityStatistic(userPerCommunity.Key, institutionPerCommunity, userPerCommunity.ToList())
            : new CommunityStatistic(userPerCommunity.Key, new List<InstitutionStatistic>(),
              userPerCommunity.ToList()));
      }

      return new UserStatistic(assignedUsers.Count, institutionPerCommunities);
    }
  }
}