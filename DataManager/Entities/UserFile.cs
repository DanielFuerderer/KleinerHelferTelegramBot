using System.Collections.Generic;

namespace Data.Entities
{
  internal sealed class UserFile
  {
    public List<UserInformation> UsersInformation { get; set; } = new List<UserInformation>();
  }
}