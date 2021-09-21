using System.Collections.Generic;
using Data.Data;

namespace TelegramBot.Data
{
  internal class InstitutionStatistic
  {
    public InstitutionStatistic(Institution institution, IReadOnlyList<UserInformation> users)
    {
      Institution = institution;
      Users = users;
    }

    public Institution Institution { get; set; }

    public IReadOnlyList<UserInformation> Users { get; set; }
  }
}